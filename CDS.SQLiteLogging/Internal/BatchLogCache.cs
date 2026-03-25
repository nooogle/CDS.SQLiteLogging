using System.Collections.Concurrent;

namespace CDS.SQLiteLogging.Internal;

/// <summary>
/// Provides caching and batch processing capabilities for log entries.
/// </summary>
class BatchLogCache : IDisposable
{
    private readonly ConcurrentQueue<LogEntry> entryQueue = new ConcurrentQueue<LogEntry>();
    private readonly LogWriter logWriter;
    private readonly LogPipeline? logPipeline;
    private readonly Action<LogEntry>? onEntryReady;
    private readonly int batchSize;
    private readonly int maxCacheSize;
    private bool disposed;
    private readonly SemaphoreSlim flushLock = new SemaphoreSlim(1, 1);
    private int pendingEntries;
    private readonly ManualResetEventSlim processEvent = new ManualResetEventSlim(false);
    private readonly ManualResetEventSlim shutdownEvent = new ManualResetEventSlim(false);
    private readonly Thread processingThread;

    /// <summary>
    /// Gets the number of entries currently in the cache.
    /// </summary>
    public int PendingCount => pendingEntries;

    /// <summary>
    /// Gets the number of entries that have been discarded due to the cache being full.
    /// </summary>
    public int DiscardCount { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchLogCache"/> class.
    /// </summary>
    /// <param name="logWriter">The SQLite log writer.</param>
    /// <param name="options">Options for configuring batch processing.</param>
    /// <param name="logPipeline">Optional pipeline applied on the caller thread inside <see cref="Add"/>, ensuring ambient context (e.g. <see cref="GlobalLogContext"/>) is captured at the moment of the log call.</param>
    /// <param name="onEntryReady">Optional callback invoked on the processing thread after the pipeline runs, before the DB write. Used to notify live UI subscribers.</param>
    public BatchLogCache(LogWriter logWriter, BatchingOptions options, LogPipeline? logPipeline = null, Action<LogEntry>? onEntryReady = null)
    {
        this.logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));
        batchSize = options?.BatchSize ?? throw new ArgumentNullException(nameof(options));
        maxCacheSize = options.MaxCacheSize;
        this.logPipeline = logPipeline;
        this.onEntryReady = onEntryReady;

        // Create and start the dedicated processing thread
        processingThread = new Thread(ProcessEntriesLoop)
        {
            Name = "SQLiteLogger_ProcessingThread",
            IsBackground = true // Make it a background thread so it doesn't prevent app exit
        };
        processingThread.Start();
    }

    /// <summary>
    /// The main processing loop that runs on the dedicated thread.
    /// </summary>
    private void ProcessEntriesLoop()
    {
        try
        {
            while (!shutdownEvent.IsSet)
            {
                // Wait for either new entries to process or shutdown signal
                WaitHandle.WaitAny(new[] { processEvent.WaitHandle, shutdownEvent.WaitHandle });

                // Reset the event before processing entries
                processEvent.Reset();

                if (shutdownEvent.IsSet)
                {
                    break;
                }

                try
                {
                    // Process all pending entries
                    WriteAllPending();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error processing log entries: {ex.Message}");
                }
            }
        }
        catch (ThreadAbortException)
        {
            // Handle thread abort if it occurs
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unhandled exception in log processing thread: {ex}");
        }
    }


    /// <summary>
    /// Adds a log entry to the cache for batch processing.
    /// </summary>
    /// <param name="entry">The log entry to add.</param>
    public void Add(LogEntry entry)
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(BatchLogCache));
        }

        if (entry == null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        // Apply the pipeline on the caller thread so that ambient context values such as
        // GlobalLogContext are captured at the moment of the log call, not when the
        // background thread dequeues the entry.
        if (!ApplyPipeline(entry))
        {
            // Pipeline rejected or threw — silently discard the entry.
            return;
        }

        if (pendingEntries >= maxCacheSize)
        {
            DiscardCount++;
        }
        else
        {
            entryQueue.Enqueue(entry);
            Interlocked.Increment(ref pendingEntries);

            // Signal the processing thread to wake up
            processEvent.Set();
        }
    }


    /// <summary>
    /// Disposes resources used by the batch log cache.
    /// </summary>
    public void Dispose()
    {
        if (!WaitUntilCacheIsEmpty(TimeSpan.FromSeconds(2000)))
        {
            System.Diagnostics.Debug.WriteLine("Warning: Not all log entries were written during shutdown");
        }

        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the resources used by the batch log cache.
    /// </summary>
    /// <param name="disposing">Indicates whether the method is called from Dispose.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            // Mark as disposed to prevent new entries
            disposed = true;

            // Signal the processing thread to shut down
            shutdownEvent.Set();
            processEvent.Set(); // Wake the thread if it's waiting

            // Give the thread a chance to exit gracefully
            if (!processingThread.Join(TimeSpan.FromSeconds(5)))
            {
                System.Diagnostics.Debug.WriteLine("Warning: Log processing thread did not exit gracefully");
            }

            // Clean up resources
            flushLock.Dispose();
            processEvent.Dispose();
            shutdownEvent.Dispose();
        }

        disposed = true;
    }

    /// <summary>
    /// Resets the discard count to zero.
    /// </summary>
    public void ResetDiscardCount()
    {
        DiscardCount = 0;
    }


    /// <summary>
    /// Processes all pending entries in the queue.
    /// </summary>
    private void WriteAllPending()
    {
        if (pendingEntries == 0 || disposed)
        {
            return;
        }

        flushLock.Wait(); // TODO do I need flushLock ?
        try
        {
            // Keep processing until all entries are handled or we're disposed
            while (pendingEntries > 0 && !disposed)
            {
                WriteBatch();
            }
        }
        finally
        {
            flushLock.Release();
        }
    }

    /// <summary>
    /// Writes a batch of entries to the database.
    /// The pipeline has already been applied on the caller thread (inside <see cref="Add"/>);
    /// this method only handles the live-UI callback and the DB write.
    /// </summary>
    private void WriteBatch()
    {
        if (pendingEntries == 0 || disposed)
        {
            return;
        }

        var toWrite = new List<LogEntry>(Math.Min(batchSize, pendingEntries));
        int dequeued = 0;

        while (dequeued < batchSize && entryQueue.TryDequeue(out var entry))
        {
            dequeued++;

            // Notify live UI subscribers (e.g. LogEntryUICache) before the DB write.
            onEntryReady?.Invoke(entry);
            toWrite.Add(entry);
        }

        if (toWrite.Count > 0)
        {
            try
            {
                logWriter.AddBatch(toWrite);
            }
            catch (Exception)
            {
                // DB write failed — put entries back for retry.
                foreach (var entry in toWrite)
                {
                    entryQueue.Enqueue(entry);
                }

                throw;
            }
        }

        // All dequeued entries are accounted for: written to DB or re-queued on failure.
        Interlocked.Add(ref pendingEntries, -dequeued);
    }

    /// <summary>
    /// Applies the pipeline to an entry. Returns <c>false</c> if the pipeline throws,
    /// in which case the entry is silently discarded.
    /// Called on the caller thread inside <see cref="Add"/> so that ambient context
    /// (e.g. <see cref="GlobalLogContext"/>) is captured at the moment of the log call.
    /// </summary>
    private bool ApplyPipeline(LogEntry entry)
    {
        if (logPipeline == null)
        {
            return true;
        }

        try
        {
            // .GetAwaiter().GetResult() is safe here because LogPipeline.ExecuteAsync
            // uses ConfigureAwait(false) throughout, so it never attempts to resume on
            // the caller's synchronisation context.
            logPipeline.ExecuteAsync(entry).GetAwaiter().GetResult();
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Log pipeline error — entry will be discarded: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Waits until all pending log entries have been written to the database.
    /// </summary>
    /// <param name="timeout">Optional timeout in milliseconds. Default is 30 seconds.</param>
    /// <returns>True if all entries were written, false if timeout occurred.</returns>
    public async Task<bool> WaitUntilCacheIsEmptyAsync(TimeSpan timeout)
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(BatchLogCache));
        }

        if (pendingEntries == 0)
        {
            return true;
        }

        // Signal the processing thread
        processEvent.Set();

        using var timeoutCts = new CancellationTokenSource(timeout);
        try
        {
            // Wait until the queue is empty or timeout occurs
            while (pendingEntries > 0 && !disposed)
            {
                if (timeoutCts.IsCancellationRequested)
                {
                    return false;
                }

                // Short delay to avoid tight loop
                await Task.Delay(10, timeoutCts.Token).ConfigureAwait(false);
            }

            return pendingEntries == 0;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    /// <summary>
    /// Synchronously waits until all pending log entries have been written to the database.
    /// Uses a true blocking poll so it is safe to call from any thread without deadlock risk.
    /// </summary>
    /// <param name="timeout">Maximum time to wait before returning <c>false</c>.</param>
    /// <returns><c>true</c> if all entries were written; <c>false</c> if the timeout elapsed.</returns>
    public bool WaitUntilCacheIsEmpty(TimeSpan timeout)
    {
        if (disposed)
        {
            return true;
        }

        // Signal the processing thread so it doesn't stay idle.
        processEvent.Set();

        var deadline = DateTime.UtcNow + timeout;
        while (pendingEntries > 0 && !disposed)
        {
            if (DateTime.UtcNow >= deadline)
            {
                return false;
            }

            Thread.Sleep(10);
        }

        return pendingEntries == 0;
    }
}
