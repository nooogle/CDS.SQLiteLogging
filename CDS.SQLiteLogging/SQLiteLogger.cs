using System.Collections.Immutable;

namespace CDS.SQLiteLogging;

/// <summary>
/// A facade for SQLite logging operations with caching and batching capabilities.
/// </summary>
class SQLiteLogger : IDisposable, ISQLiteLoggerUtilities
{
    private readonly ConnectionManager connectionManager;
    private readonly LogWriter writer;
    private readonly LogReader reader;
    private readonly LogHousekeeper housekeeper;
    private readonly BatchLogCache logCache;
    private bool disposed;


    /// <summary>
    /// Initializes a new instance of the <see cref="SQLiteLogger"/> class.
    /// </summary>
    /// <param name="fileName">The name of the SQLite database file.</param>
    /// <param name="batchingOptions">Options for configuring batch processing.</param>
    /// <param name="houseKeepingOptions">Options for configuring housekeeping.</param>
    public SQLiteLogger(
        string fileName,
        BatchingOptions batchingOptions,
        HouseKeepingOptions houseKeepingOptions)
    {
        // Initialize connection manager
        connectionManager = new ConnectionManager(fileName);

        // Create table schema
        var tableCreator = new TableCreator(connectionManager);
        string tableName = tableCreator.CreateTableForLogEntry();

        // Initialize writer and reader
        writer = new LogWriter(connectionManager, tableName);
        reader = new LogReader(connectionManager, tableName);

        // Setup batching with defaults if options not provided
        batchingOptions ??= new BatchingOptions();
        logCache = new BatchLogCache(writer, batchingOptions);

        // Initialize housekeeper with defaults if not specified
        houseKeepingOptions ??= new HouseKeepingOptions();
        housekeeper = new LogHousekeeper(
            connectionManager,
            tableName,
            houseKeepingOptions.RetentionPeriod,
            houseKeepingOptions.CleanupInterval);
    }

    /// <summary>
    /// Gets the log housekeeper instance.
    /// </summary>
    public LogHousekeeper Housekeeper => housekeeper;

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
        bool shouldIgnore = false;
        if (shouldIgnore)
        {
            return;
        }

        logCache.Add(entry);
    }

    /// <summary>
    /// Flushes all pending entries from the cache to the database.
    /// </summary>
    public Task FlushAsync() => logCache.FlushAllAsync();

    /// <summary>
    /// Flushes all pending entries from the cache to the database synchronously.
    /// </summary>
    public void Flush() => logCache.FlushAll();

    /// <summary>
    /// Reads and returns all log entries from the database.
    /// </summary>
    /// <returns>An immutable list of log entries.</returns>
    public ImmutableList<LogEntry> GetAllEntries()
    {
        // Flush pending entries first to ensure we get the most recent data
        Flush();
        return reader.GetAllEntries();
    }

    /// <summary>
    /// Reads and returns all log entries from the database asynchronously.
    /// </summary>
    /// <returns>An immutable list of log entries.</returns>
    public async Task<ImmutableList<LogEntry>> GetAllEntriesAsync()
    {
        // Flush pending entries first to ensure we get the most recent data
        await FlushAsync().ConfigureAwait(false);
        return await reader.GetAllEntriesAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Disposes resources used by the logger.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the resources used by the logger.
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
                    logCache.FlushAll();
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
        // Flush any pending entries first
        Flush();
        return housekeeper.DeleteAll();
    }

    /// <summary>
    /// Deletes all log entries from the database asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, with the number of entries deleted.</returns>
    public async Task<int> DeleteAllAsync()
    {
        // Flush any pending entries first
        await FlushAsync().ConfigureAwait(false);
        return await housekeeper.DeleteAllAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Reads and returns the most recent log entries from the database.
    /// </summary>
    /// <param name="maxCount">The maximum number of entries to return.</param>
    /// <returns>An immutable list of log entries, ordered by timestamp descending.</returns>
    public ImmutableList<LogEntry> GetRecentEntries(int maxCount)
    {
        // Flush any pending entries first to ensure we get the most recent data
        Flush();
        return reader.GetRecentEntries(maxCount);
    }

    /// <summary>
    /// Reads and returns the most recent log entries from the database asynchronously.
    /// </summary>
    /// <param name="maxCount">The maximum number of entries to return.</param>
    /// <returns>A task representing the asynchronous operation, with an immutable list of log entries.</returns>
    public async Task<ImmutableList<LogEntry>> GetRecentEntriesAsync(int maxCount)
    {
        await FlushAsync().ConfigureAwait(false);
        return await reader.GetRecentEntriesAsync(maxCount).ConfigureAwait(false);
    }

    /// <summary>
    /// Returns the size of the database file in bytes.
    /// </summary>
    /// <returns>The size of the database file in bytes.</returns>
    public long GetDatabaseFileSize() => connectionManager.GetDatabaseFileSize();

    /// <summary>
    /// Returns log entries that match the specified message parameter.
    /// </summary>
    /// <param name="key">The message parameter key to search for.</param>
    /// <param name="value">The message parameter value to search for.</param>
    /// <returns>A task representing the asynchronous operation, with an immutable list of log entries.</returns>
    public async Task<ImmutableList<LogEntry>> GetEntriesByMessageParamAsync(string key, object value)
    {
        await FlushAsync().ConfigureAwait(false);
        return await reader.GetEntriesByMessageParamAsync(key, value).ConfigureAwait(false);
    }

    /// <summary>
    /// Reads and returns log entries that contain a specific message parameter (synchronous version).
    /// </summary>
    /// <param name="key">The key of the message parameter to search for.</param>
    /// <param name="value">The value of the message parameter to search for.</param>
    /// <returns>An immutable list of log entries that match the specified message parameter.</returns>
    public ImmutableList<LogEntry> GetEntriesByMessageParam(string key, object value) => GetEntriesByMessageParamAsync(key, value).GetAwaiter().GetResult();

    /// <summary>
    /// Returns the number of log entries that have been discarded due to cache overflow.
    /// </summary>
    public int DiscardedEntriesCount => logCache.DiscardCount;
}

