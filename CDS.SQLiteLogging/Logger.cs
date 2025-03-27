using System.Collections.Immutable;

namespace CDS.SQLiteLogging;

/// <summary>
/// A facade for SQLite logging operations with caching and batching capabilities.
/// </summary>
/// <typeparam name="TLogEntry">A type that implements ILogEntry and has a parameterless constructor.</typeparam>
public class Logger<TLogEntry> : IDisposable where TLogEntry : ILogEntry, new()
{
    private readonly ConnectionManager connectionManager;
    private readonly LogWriter<TLogEntry> writer;
    private readonly LogReader<TLogEntry> reader;
    private readonly LogHousekeeper<TLogEntry> housekeeper;
    private readonly BatchLogCache<TLogEntry> logCache;
    private bool disposed;


    /// <summary>
    /// Optional callback for when a log entry is about to be added to the cache. 
    /// The client has an opportunity to ignore or modify the entry before it is added.
    /// </summary>
    public OnAboutToAddLogEntry<TLogEntry> OnAboutToAddLogEntry { get; set; }


    /// <summary>
    /// Optional callback for when a log entry has been added to the cache.
    /// </summary>
    public OnAddedLogEntry<TLogEntry> OnAddedLogEntry { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="Logger{TLogEntry}"/> class.
    /// </summary>
    /// <param name="folder">The folder where the SQLite database file will be located.</param>
    /// <param name="batchingOptions">Options for configuring batch processing.</param>
    /// <param name="houseKeepingOptions">Options for configuring housekeeping.</param>
    /// <param name="schemaVersion">The version of the log schema, used in the database filename to help avoid reading or writing to a mismatched schema.</param>
    public Logger(
        string folder,
        int schemaVersion,
        BatchingOptions batchingOptions = null,
        HouseKeepingOptions houseKeepingOptions = null)
    {
        // Initialize connection manager
        connectionManager = new ConnectionManager(folder, schemaVersion: schemaVersion);

        // Create type mapping
        var properties = typeof(TLogEntry).GetProperties();
        var typeMap = TypeMapper.CreateTypeToSqliteMap(properties);

        // Create table schema
        var tableCreator = new TableCreator(connectionManager, typeMap);
        string tableName = tableCreator.CreateTableForType<TLogEntry>();

        // Initialize writer and reader
        writer = new LogWriter<TLogEntry>(connectionManager, tableName);
        reader = new LogReader<TLogEntry>(connectionManager, tableName);

        // Setup batching with defaults if options not provided
        batchingOptions ??= new BatchingOptions();
        logCache = new BatchLogCache<TLogEntry>(
            writer,
            batchingOptions);

        // Initialize housekeeper with defaults if not specified
        houseKeepingOptions ??= new HouseKeepingOptions();
        housekeeper = new LogHousekeeper<TLogEntry>(
            connectionManager,
            tableName,
            houseKeepingOptions.RetentionPeriod,
            houseKeepingOptions.CleanupInterval);
    }

    /// <summary>
    /// Gets the log housekeeper instance.
    /// </summary>
    public LogHousekeeper<TLogEntry> Housekeeper => housekeeper;

    /// <summary>
    /// Gets the number of entries currently pending in the cache.
    /// </summary>
    public int PendingEntries => logCache.PendingCount;

    /// <summary>
    /// Adds a new log entry to the cache for batch processing.
    /// </summary>
    /// <param name="entry">The log entry to add.</param>
    public void Add(TLogEntry entry)
    {
        bool shouldIgnore = false;
        OnAboutToAddLogEntry?.Invoke(entry, ref shouldIgnore);
        if (shouldIgnore)
        {
            return;
        }

        logCache.Add(entry);

        OnAddedLogEntry?.Invoke(entry);
    }

    /// <summary>
    /// Flushes all pending entries from the cache to the database.
    /// </summary>
    public Task FlushAsync()
    {
        return logCache.FlushAllAsync();
    }

    /// <summary>
    /// Flushes all pending entries from the cache to the database synchronously.
    /// </summary>
    public void Flush()
    {
        logCache.FlushAll();
    }

    /// <summary>
    /// Reads and returns all log entries from the database.
    /// </summary>
    /// <returns>An immutable list of log entries.</returns>
    public ImmutableList<TLogEntry> GetAllEntries()
    {
        // Flush pending entries first to ensure we get the most recent data
        Flush();
        return reader.GetAllEntries();
    }

    /// <summary>
    /// Reads and returns all log entries from the database asynchronously.
    /// </summary>
    /// <returns>An immutable list of log entries.</returns>
    public async Task<ImmutableList<TLogEntry>> GetAllEntriesAsync()
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
    public ImmutableList<TLogEntry> GetRecentEntries(int maxCount)
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
    public async Task<ImmutableList<TLogEntry>> GetRecentEntriesAsync(int maxCount)
    {
        await FlushAsync().ConfigureAwait(false);
        return await reader.GetRecentEntriesAsync(maxCount).ConfigureAwait(false);
    }


    /// <summary>
    /// Returns the size of the database file in bytes.
    /// </summary>
    /// <returns>
    /// The size of the database file in bytes.
    /// </returns>
    public long GetDatabaseFileSize()
    {
        return connectionManager.GetDatabaseFileSize();
    }


    /// <summary>
    /// Returns log entries that match the specified message parameter.
    /// </summary>
    /// <param name="key">The message parameter key to search for.</param>
    /// <param name="value">The message parameter value to search for.</param>
    /// <returns>A task representing the asynchronous operation, with an immutable list of log entries.</returns>
    public async Task<ImmutableList<TLogEntry>> GetEntriesByMessageParamAsync(string key, object value)
    {
        await FlushAsync().ConfigureAwait(false);
        return await reader.GetEntriesByMessageParamAsync(key: key, value: value).ConfigureAwait(false);
    }


    /// <summary>
    /// Reads and returns log entries that contain a specific message parameter (synchronous version).
    /// </summary>
    /// <param name="key">The key of the message parameter to search for.</param>
    /// <param name="value">The value of the message parameter to search for.</param>
    /// <returns>An immutable list of log entries that match the specified message parameter.</returns>
    public ImmutableList<TLogEntry> GetEntriesByMessageParam(string key, object value)
    {
        return GetEntriesByMessageParamAsync(key, value).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Returns the number of log entries that have been discarded due to cache overflow.
    /// </summary>
    public int DiscardedEntries => logCache.DiscardCount;
}
