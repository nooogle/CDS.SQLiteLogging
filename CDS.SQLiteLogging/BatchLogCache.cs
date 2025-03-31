using System.Collections.Concurrent;

namespace CDS.SQLiteLogging;

/// <summary>
/// Provides caching and batch processing capabilities for log entries.
/// </summary>
class BatchLogCache : IDisposable
{
    private readonly ConcurrentQueue<LogEntry> entryQueue = new ConcurrentQueue<LogEntry>();
    private readonly Timer flushTimer;
    private readonly LogWriter logWriter;
    private readonly int batchSize;
    private readonly int maxCacheSize;
    private readonly TimeSpan flushInterval;
    private bool disposed;
    private readonly SemaphoreSlim flushLock = new SemaphoreSlim(1, 1);
    private int pendingEntries;
    private readonly ManualResetEventSlim shutdownEvent = new ManualResetEventSlim(false);
    private readonly TaskCompletionSource<bool> disposedTcs = new TaskCompletionSource<bool>();

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchLogCache"/> class.
    /// </summary>
    /// <param name="logWriter">The SQLite log writer.</param>
    /// <param name="options">Options for configuring batch processing.</param>
    public BatchLogCache(LogWriter logWriter, BatchingOptions options)
    {
        this.logWriter = logWriter;
        this.flushInterval = options.FlushInterval;
        this.batchSize = options.BatchSize;
        this.maxCacheSize = options.MaxCacheSize;

        // Create a timer to regularly flush the cache
        flushTimer = new Timer(
            FlushCallback,
            null,
            flushInterval,
            flushInterval);
    }

    /// <summary>
    /// Gets the number of entries currently in the cache.
    /// </summary>
    public int PendingCount => pendingEntries;

    /// <summary>
    /// Gets the number of entries that have been discarded due to the cache being full.
    /// </summary>
    public int DiscardCount { get; private set; }

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

        if (pendingEntries >= maxCacheSize)
        {
            DiscardCount++;
        }
        else
        {
            entryQueue.Enqueue(entry);
            Interlocked.Increment(ref pendingEntries);
        }

        // If we've reached the batch size, trigger a flush asynchronously
        // But don't wait for it to complete
        if (pendingEntries >= batchSize)
        {
            // Use ConfigureAwait(false) to avoid capturing the synchronization context
            _ = TryFlushAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Timer callback that triggers a cache flush.
    /// </summary>
    private void FlushCallback(object? state)
    {
        // Don't start a flush operation if we're disposing
        if (disposed)
        {
            return;
        }

        // Fire and forget flush - we don't need to wait for it
        _ = TryFlushAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Tries to flush the cache, but doesn't wait if a flush is already in progress
    /// </summary>
    private async Task TryFlushAsync()
    {
        // Quick check if there's anything to flush
        if (pendingEntries == 0 || disposed)
        {
            return;
        }

        // Try to take the lock, but don't block if it's already taken
        if (!await flushLock.WaitAsync(0).ConfigureAwait(false))
        {
            return;
        }

        try
        {
            await FlushAsyncInternal().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Log the exception but don't crash
            System.Diagnostics.Debug.WriteLine($"Error during log batch flush: {ex.Message}");
        }
        finally
        {
            flushLock.Release();
        }
    }

    /// <summary>
    /// Internal method that handles the actual flush logic
    /// Assumes the flushLock is already acquired
    /// </summary>
    private async Task FlushAsyncInternal()
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
    /// Flushes the cache to the database asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task FlushAsync()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(BatchLogCache));
        }

        await flushLock.WaitAsync().ConfigureAwait(false);
        try
        {
            await FlushAsyncInternal().ConfigureAwait(false);
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

        await flushLock.WaitAsync().ConfigureAwait(false);
        try
        {
            // Keep flushing until the queue is empty
            while (pendingEntries > 0)
            {
                await FlushAsyncInternal().ConfigureAwait(false);
            }
        }
        finally
        {
            flushLock.Release();
        }
    }

    /// <summary>
    /// Flushes all remaining entries synchronously. Use with caution as this can cause deadlocks.
    /// Only safe to call from the Dispose method or in contexts where no other asynchronous operations
    /// are in progress on this cache instance.
    /// </summary>
    public void FlushAll()
    {
        // Only allowed during disposal or in contexts where deadlock won't occur
        if (disposed)
        {
            return;
        }

        // If we're on a background thread, we can safely block
        if (pendingEntries > 0)
        {
            if (!flushLock.Wait(TimeSpan.FromSeconds(10)))
            {
                System.Diagnostics.Debug.WriteLine("Warning: Timed out waiting for flush lock during disposal");
                return;
            }

            try
            {
                // Keep flushing until the queue is empty or we time out
                var timeout = DateTime.UtcNow.AddSeconds(30);
                while (pendingEntries > 0 && DateTime.UtcNow < timeout)
                {
                    // Use a separate task but wait synchronously - this avoids deadlocks
                    // that can occur when using GetAwaiter().GetResult() on the same method
                    Task.Run(() => FlushAsyncInternal()).Wait();
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
    }

    /// <summary>
    /// Disposes resources used by the batch log cache.
    /// </summary>
    public void Dispose()
    {
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

            // Stop the timer first to prevent new flush operations
            flushTimer.Dispose();

            // Flush any remaining entries with timeout protection
            try
            {
                FlushAll();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during final log flush: {ex.Message}");
            }

            flushLock.Dispose();
            shutdownEvent.Dispose();
        }

        disposed = true;
        disposedTcs.TrySetResult(true);
    }
}
