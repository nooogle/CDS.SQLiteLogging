using System.Collections.Concurrent;

namespace CDS.SQLiteLogging;

/// <summary>
/// Provides caching and batch processing capabilities for log entries.
/// </summary>
public class BatchLogCache : IDisposable
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
            return;
        }

        entryQueue.Enqueue(entry);
        Interlocked.Increment(ref pendingEntries);

        // If we've reached the batch size, trigger a flush
        if (pendingEntries >= batchSize)
        {
            // Try to flush but don't block if already flushing
            if (flushLock.Wait(0))
            {
                try
                {
                    // Await the flush operation to ensure the lock is held until it completes
                    FlushAsync().GetAwaiter().GetResult();
                }
                finally
                {
                    flushLock.Release();
                }
            }
        }
    }

    /// <summary>
    /// Timer callback that triggers a cache flush.
    /// </summary>
    private void FlushCallback(object state)
    {
        // Don't flush if there are no entries or we're already flushing
        if (pendingEntries == 0 || !flushLock.Wait(0))
        {
            return;
        }

        try
        {
            // Await the flush operation to ensure the lock is held until it completes
            FlushAsync().GetAwaiter().GetResult();
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
    /// Flushes the cache to the database asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task FlushAsync()
    {
        if (pendingEntries == 0)
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
                // Debug info
                System.Diagnostics.Debug.WriteLine($"Flushing {entries.Count} log entries");

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
                throw;
            }
        }
    }

    /// <summary>
    /// Flushes all remaining entries and waits for completion.
    /// </summary>
    public async Task FlushAllAsync()
    {
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
                await FlushAsync().ConfigureAwait(false);
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
        FlushAllAsync().GetAwaiter().GetResult();
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
        if (!disposed)
        {
            if (disposing)
            {
                // Stop the timer
                flushTimer.Dispose();

                // Flush any remaining entries
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
        }
    }
}
