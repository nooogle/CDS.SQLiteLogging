using CDS.SQLiteLogging.Internal;

namespace CDS.SQLiteLogging;

/// <summary>
/// Provides writing capabilities for SQLite logging with caching, batching, and housekeeping.
/// </summary>
class Logger : IDisposable, ISQLiteWriterUtilities
{
    private readonly ConnectionManager connectionManager;
    private readonly LogWriter writer;
    private readonly Housekeeper housekeeper;
    private readonly BatchLogCache logCache;
    private bool disposed;


    /// <summary>
    /// Event that is raised when a log entry is received.
    /// </summary>
    public event LogEntryReceivedEvent? LogEntryReceived;


    /// <summary>
    /// Initializes a new instance of the <see cref="Logger"/> class.
    /// </summary>
    /// <param name="fileName">The name of the SQLite database file.</param>
    /// <param name="batchingOptions">Options for configuring batch processing.</param>
    /// <param name="houseKeepingOptions">Options for configuring housekeeping.</param>
    public Logger(
        string fileName,
        BatchingOptions batchingOptions,
        HouseKeepingOptions houseKeepingOptions,
        IDateTimeProvider dateTimeProvider)
    {
        // Initialize connection manager
        connectionManager = new ConnectionManager(fileName);

        // Create table schema
        var tableCreator = new TableCreator(connectionManager);
        string tableName = tableCreator.CreateTableForLogEntry();

        // Initialize writer
        writer = new LogWriter(connectionManager, tableName);

        // Setup batching with defaults if options not provided
        batchingOptions ??= new BatchingOptions();
        logCache = new BatchLogCache(writer, batchingOptions);

        // Initialize housekeeper with defaults if not specified
        houseKeepingOptions ??= new HouseKeepingOptions();

        housekeeper = new Housekeeper(
            connectionManager,
            houseKeepingOptions,
            dateTimeProvider);
    }

    /// <summary>
    /// Gets the log housekeeper instance.
    /// </summary>
    public Housekeeper Housekeeper => housekeeper;

    /// <summary>
    /// Gets the number of entries currently pending in the cache.
    /// </summary>
    public int PendingEntriesCount => logCache.PendingCount;

    /// <summary>
    /// Adds a new log entry to the cache for batch processing.
    /// </summary>
    /// <param name="entry">The log entry to add.</param>
    public void Add(LogEntry entry)
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(Logger));
        }

        logCache.Add(entry);
        LogEntryReceived?.Invoke(entry);
    }

    /// <summary>
    /// Disposes resources used by the writer.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the resources used by the writer.
    /// </summary>
    /// <param name="disposing">Indicates whether the method is called from Dispose.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                try
                {
                    // Try to flush any remaining entries
                    logCache.WaitUntilCacheIsEmpty(timeout: TimeSpan.FromSeconds(2));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error during final cache flush: {ex.Message}");
                }

                // Dispose components in the correct order
                logCache.Dispose();
                housekeeper.Dispose();
                connectionManager.Dispose();
            }
            disposed = true;
        }
    }

    /// <summary>
    /// Deletes all log entries from the database.
    /// </summary>
    /// <returns>The number of entries deleted.</returns>
    public int DeleteAll()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(Logger));
        }

        return housekeeper.DeleteAll();
    }


    /// <inheritdoc />
    public async Task DeleteByIds(long[] ids)
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(Logger));
        }

        await housekeeper.DeleteByIdsAsync(ids);
    }



    /// <summary>
    /// Deletes all log entries from the database asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, with the number of entries deleted.</returns>
    public Task<int> DeleteAllAsync()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(Logger));
        }

        return Task.Run(DeleteAll);
    }

    /// <summary>
    /// Returns the size of the database file in bytes.
    /// </summary>
    /// <returns>The size of the database file in bytes.</returns>
    public long GetDatabaseFileSize() => connectionManager.GetDatabaseFileSize();

    /// <summary>
    /// Returns the number of log entries that have been discarded due to cache overflow.
    /// </summary>
    public int DiscardedEntriesCount => logCache.DiscardCount;

    /// <summary>
    /// Resets the count of discarded log entries.
    /// </summary>
    public void ResetDiscardedEntriesCount() => logCache.ResetDiscardCount();

    /// <summary>
    /// Waits until all pending log entries have been written to the database.
    /// </summary>
    /// <param name="timeout">The maximum time to wait for the cache to empty.</param>
    public void WaitUntilCacheIsEmpty(TimeSpan timeout)
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(Logger));
        }

        logCache.WaitUntilCacheIsEmpty(timeout);
    }

    /// <summary>
    /// Waits asynchronously until all pending log entries have been written to the database.
    /// </summary>
    /// <param name="timeout">The maximum time to wait for the cache to empty.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task WaitUntilCacheIsEmptyAsync(TimeSpan timeout)
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(Logger));
        }

        return Task.Run(() => logCache.WaitUntilCacheIsEmpty(timeout));
    }
}
