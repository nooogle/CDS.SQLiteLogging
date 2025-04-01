using System.Collections.Concurrent;

namespace CDS.SQLiteLogging;

/// <summary>
/// Provides caching and batch processing capabilities for log entries.
/// </summary>
class BatchLogCache : IDisposable
{
    private readonly ConcurrentQueue<LogEntry> entryQueue = new ConcurrentQueue<LogEntry>();
    private readonly LogWriter logWriter;
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
    public BatchLogCache(LogWriter logWriter, BatchingOptions options)
    {
        this.logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));
        this.batchSize = options?.BatchSize ?? throw new ArgumentNullException(nameof(options));
        this.maxCacheSize = options.MaxCacheSize;

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
    /// Process all pending entries in the queue.
    /// </summary>
    private async Task ProcessPendingEntries()
    {
        if (pendingEntries == 0 || disposed)
        {
            return;
        }

        await flushLock.WaitAsync().ConfigureAwait(false);
        try
        {
            // Keep processing until all entries are handled or we're disposed
            while (pendingEntries > 0 && !disposed)
            {
                await FlushBatchAsync().ConfigureAwait(false);
            }
        }
        finally
        {
            flushLock.Release();
        }
    }

    /// <summary>
    /// Flush a batch of entries to the database.
    /// </summary>
    private async Task FlushBatchAsync()
    {
        // Check if there's anything to flush or if we're disposed
        if (pendingEntries == 0 || disposed)
        {
            return;
        }

        // Take a snapshot of entries to process
        var entries = new List<LogEntry>(Math.Min(batchSize, pendingEntries));
        int processed = 0;

        // Dequeue entries up to batch size
        while (processed < batchSize && entryQueue.TryDequeue(out var entry))
        {
            entries.Add(entry);
            processed++;
        }

        if (entries.Count > 0)
        {
            try
            {
                // Write entries to database in a transaction
                await logWriter.AddBatchAsync(entries).ConfigureAwait(false);

                // Update the counter
                Interlocked.Add(ref pendingEntries, -processed);
            }
            catch (Exception)
            {
                // If writing fails, put the entries back in the queue
                foreach (var entry in entries)
                {
                    entryQueue.Enqueue(entry);
                }

                // Don't re-adjust the counter since we put the entries back
                throw;
            }
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
    /// Flushes the cache to the database asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task FlushAsync()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(BatchLogCache));
        }

        // Signal the processing thread and wait for it to complete
        processEvent.Set();

        await flushLock.WaitAsync().ConfigureAwait(false);
        try
        {
            await FlushBatchAsync().ConfigureAwait(false);
        }
        finally
        {
            flushLock.Release();
        }
    }

    /// <summary>
    /// Flushes all remaining entries and waits for completion.
    /// </summary>
    public async Task FlushAllAsync()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(BatchLogCache));
        }

        if (pendingEntries == 0)
        {
            return;
        }

        // Signal the processing thread
        processEvent.Set();

        await flushLock.WaitAsync().ConfigureAwait(false);
        try
        {
            // Keep flushing until the queue is empty
            while (pendingEntries > 0 && !disposed)
            {
                await FlushBatchAsync().ConfigureAwait(false);
            }
        }
        finally
        {
            flushLock.Release();
        }
    }

    /// <summary>
    /// Flushes all remaining entries synchronously. 
    /// </summary>
    public void FlushAll()
    {
        if (disposed)
        {
            return;
        }

        // Signal the processing thread
        processEvent.Set();

        // No entries to process
        if (pendingEntries == 0)
        {
            return;
        }

        if (!flushLock.Wait(TimeSpan.FromSeconds(10)))
        {
            System.Diagnostics.Debug.WriteLine("Warning: Timed out waiting for flush lock during FlushAll");
            return;
        }

        try
        {
            // Keep flushing until the queue is empty or we time out
            var timeout = DateTime.UtcNow.AddSeconds(30);
            while (pendingEntries > 0 && !disposed && DateTime.UtcNow < timeout)
            {
                FlushBatchAsync().GetAwaiter().GetResult();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during synchronous flush: {ex.Message}");
        }
        finally
        {
            flushLock.Release();
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

            // Flush any remaining entries with timeout protection
            try
            {
                FlushAll();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during final log flush: {ex.Message}");
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
    /// </summary>
    private void WriteBatch()
    {
        // Check if there's anything to flush or if we're disposed
        if (pendingEntries == 0 || disposed)
        {
            return;
        }

        // Take a snapshot of entries to process
        var entries = new List<LogEntry>(Math.Min(batchSize, pendingEntries));
        int processed = 0;

        // Dequeue entries up to batch size
        while (processed < batchSize && entryQueue.TryDequeue(out var entry))
        {
            entries.Add(entry);
            processed++;
        }

        if (entries.Count > 0)
        {
            try
            {
                // Write entries to database in a transaction
                logWriter.AddBatch(entries);

                // Update the counter
                Interlocked.Add(ref pendingEntries, -processed);
            }
            catch (Exception)
            {
                // If writing fails, put the entries back in the queue
                foreach (var entry in entries)
                {
                    entryQueue.Enqueue(entry);
                }

                // Don't re-adjust the counter since we put the entries back
                throw;
            }
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
    /// </summary>
    /// <param name="timeout">Optional timeout in milliseconds. Default is 30 seconds.</param>
    /// <returns>True if all entries were written, false if timeout occurred.</returns>
    public bool WaitUntilCacheIsEmpty(TimeSpan timeout)
    {
        return WaitUntilCacheIsEmptyAsync(timeout).GetAwaiter().GetResult();
    }
}
