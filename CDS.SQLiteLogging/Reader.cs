using CDS.SQLiteLogging.Internal;
using System.Collections.Immutable;

namespace CDS.SQLiteLogging;

/// <summary>
/// Provides read-only access to SQLite log entries
/// </summary>
public class Reader : IDisposable
{
    private readonly ConnectionManager connectionManager;
    private readonly LogReader reader;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Reader"/> class.
    /// </summary>
    /// <param name="fileName">The name of the SQLite database file.</param>
    public Reader(ConnectionManager connectionManager)
    {
        this.connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));

        // Initialize reader with the existing log table
        reader = new LogReader(connectionManager, TableCreator.TableName);
    }

    /// <summary>
    /// Reads and returns all log entries from the database.
    /// </summary>
    /// <returns>An immutable list of log entries.</returns>
    public ImmutableList<LogEntry> GetAllEntries()
    {
        return reader.GetAllEntries();
    }

    /// <summary>
    /// Reads and returns all log entries from the database asynchronously.
    /// </summary>
    /// <returns>An immutable list of log entries.</returns>
    public async Task<ImmutableList<LogEntry>> GetAllEntriesAsync()
    {
        return await reader.GetAllEntriesAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Reads and returns the most recent log entries from the database.
    /// </summary>
    /// <param name="maxCount">The maximum number of entries to return.</param>
    /// <returns>An immutable list of log entries, ordered by timestamp descending.</returns>
    public ImmutableList<LogEntry> GetRecentEntries(int maxCount)
    {
        return reader.GetRecentEntries(maxCount);
    }

    /// <summary>
    /// Reads and returns the most recent log entries from the database asynchronously.
    /// </summary>
    /// <param name="maxCount">The maximum number of entries to return.</param>
    /// <returns>A task representing the asynchronous operation, with an immutable list of log entries.</returns>
    public async Task<ImmutableList<LogEntry>> GetRecentEntriesAsync(int maxCount)
    {
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
        return await reader.GetEntriesByMessageParamAsync(key, value).ConfigureAwait(false);
    }

    /// <summary>
    /// Reads and returns log entries that contain a specific message parameter (synchronous version).
    /// </summary>
    /// <param name="key">The key of the message parameter to search for.</param>
    /// <param name="value">The value of the message parameter to search for.</param>
    /// <returns>An immutable list of log entries that match the specified message parameter.</returns>
    public ImmutableList<LogEntry> GetEntriesByMessageParam(string key, object value) =>
        GetEntriesByMessageParamAsync(key, value).GetAwaiter().GetResult();

    /// <summary>
    /// Disposes resources used by the reader.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the resources used by the reader.
    /// </summary>
    /// <param name="disposing">Indicates whether the method is called from Dispose.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // Dispose connection manager
                connectionManager.Dispose();
            }
            disposed = true;
        }
    }


    /// <summary>
    /// Gets the number of entries in the database.
    /// </summary>
    public int GetNumberOfEntries()
    {
        return reader.GetEntryCount();
    }
}
