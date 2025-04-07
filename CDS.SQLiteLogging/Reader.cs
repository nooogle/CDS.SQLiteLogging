using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace CDS.SQLiteLogging;

/// <summary>
/// Reads log entries from an SQLite database.
/// </summary>
public class Reader : IDisposable
{
    private readonly ConnectionManager connectionManager;
    private readonly string tableName;
    private bool disposed;

    /// <summary>
    /// Gets the name of the table to read from.
    /// </summary>
    public static string TableName => Internal.TableCreator.TableName;

    /// <summary>
    /// Initializes a new instance of the <see cref="Reader"/> class.
    /// </summary>
    /// <param name="dbPath">The path to the SQLite database file. This MUST exist!</param>
    /// <exception cref="FileNotFoundException">Thrown when the database file does not exist.</exception>"
    public Reader(string dbPath)
    {
        if (!File.Exists(dbPath))
        {
            throw new FileNotFoundException($"Database file not found: {dbPath}");
        }

        connectionManager = new ConnectionManager(dbPath);
        tableName = Internal.TableCreator.TableName;
    }

    /// <summary>
    /// Gets the database file size in bytes.
    /// </summary>
    public long GetDatabaseFileSize()
    {
        return connectionManager.GetDatabaseFileSize();
    }

    /// <summary>
    /// Reads and returns all log entries from the database asynchronously.
    /// </summary>
    /// <returns>An immutable list of log entries.</returns>
    public async Task<ImmutableList<LogEntry>> GetAllEntriesAsync()
    {
        var entries = ImmutableList.CreateBuilder<LogEntry>();

        await connectionManager.ExecuteWithRetryAsync(async () =>
        {
            string sql = $"SELECT * FROM {tableName} ORDER BY Timestamp DESC;";
            using var cmd = new SqliteCommand(sql, connectionManager.Connection);
            using var reader = await Task.Run(() => cmd.ExecuteReader()).ConfigureAwait(false);
            while (await Task.Run(() => reader.Read()).ConfigureAwait(false))
            {
                var entry = CreateLogEntryFromReader(reader);
                entries.Add(entry);
            }
        }).ConfigureAwait(false);

        return entries.ToImmutable();
    }

    /// <summary>
    /// Reads and returns all log entries from the database synchronously.
    /// </summary>
    /// <returns>An immutable list of log entries.</returns>
    public ImmutableList<LogEntry> GetAllEntries()
    {
        return GetAllEntriesAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Reads and returns the most recent log entries from the database asynchronously.
    /// </summary>
    /// <param name="maxCount">The maximum number of entries to return.</param>
    /// <returns>An immutable list of log entries, ordered by timestamp descending.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxCount"/> is less than or equal to zero.</exception>
    public async Task<ImmutableList<LogEntry>> GetRecentEntriesAsync(int maxCount)
    {
        if (maxCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCount), "Maximum count must be greater than zero");
        }

        var entries = ImmutableList.CreateBuilder<LogEntry>();

        await connectionManager.ExecuteWithRetryAsync(async () =>
        {
            string sql = $"SELECT * FROM {tableName} ORDER BY Timestamp DESC LIMIT @maxCount;";
            using var cmd = new SqliteCommand(sql, connectionManager.Connection);
            cmd.Parameters.AddWithValue("@maxCount", maxCount);
            using var reader = await Task.Run(() => cmd.ExecuteReader()).ConfigureAwait(false);
            while (await Task.Run(() => reader.Read()).ConfigureAwait(false))
            {
                var entry = CreateLogEntryFromReader(reader);
                entries.Add(entry);
            }
        }).ConfigureAwait(false);

        return entries.ToImmutable();
    }

    /// <summary>
    /// Reads and returns the most recent log entries from the database synchronously.
    /// </summary>
    /// <param name="maxCount">The maximum number of entries to return.</param>
    /// <returns>An immutable list of log entries, ordered by timestamp descending.</returns>
    public ImmutableList<LogEntry> GetRecentEntries(int maxCount)
    {
        return GetRecentEntriesAsync(maxCount).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets the count of log entries in the database.
    /// </summary>
    /// <returns>The count of log entries.</returns>
    public int GetEntryCount()
    {
        using var cmd = new SqliteCommand($"SELECT COUNT(*) FROM {tableName}", connectionManager.Connection);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    /// <summary>
    /// Reads and returns log entries that contain a specific message parameter asynchronously.
    /// </summary>
    /// <param name="key">The key of the message parameter to search for.</param>
    /// <param name="value">The value of the message parameter to search for.</param>
    /// <returns>An immutable list of log entries that match the specified message parameter.</returns>
    public async Task<ImmutableList<LogEntry>> GetEntriesByMessageParamAsync(string key, object value)
    {
        var entries = ImmutableList.CreateBuilder<LogEntry>();

        await connectionManager.ExecuteWithRetryAsync(async () =>
        {
            string sql = $"SELECT * FROM {tableName} WHERE json_extract(Properties, '$.{key}') = @value;";
            using var cmd = new SqliteCommand(sql, connectionManager.Connection);
            cmd.Parameters.AddWithValue("@value", value);
            using var reader = await Task.Run(() => cmd.ExecuteReader()).ConfigureAwait(false);
            while (await Task.Run(() => reader.Read()).ConfigureAwait(false))
            {
                var entry = CreateLogEntryFromReader(reader);
                entries.Add(entry);
            }
        }).ConfigureAwait(false);

        return entries.ToImmutable();
    }

    /// <summary>
    /// Executes a custom SQL select query and returns the resulting log entries asynchronously.
    /// </summary>
    /// <param name="sqlSelect">The SQL select query to execute.</param>
    /// <returns>An enumerable of log entries.</returns>
    public async Task<IEnumerable<LogEntry>> SelectAsync(string sqlSelect)
    {
        var entries = ImmutableList.CreateBuilder<LogEntry>();

        await connectionManager.ExecuteWithRetryAsync(async () =>
        {
            using var cmd = new SqliteCommand(sqlSelect, connectionManager.Connection);
            using var reader = await Task.Run(() => cmd.ExecuteReader()).ConfigureAwait(false);
            while (await Task.Run(() => reader.Read()).ConfigureAwait(false))
            {
                var entry = CreateLogEntryFromReader(reader);
                entries.Add(entry);
            }
        }).ConfigureAwait(false);

        return entries.ToImmutable();
    }

    /// <summary>
    /// Executes a custom SQL select query and returns the resulting log entries synchronously.
    /// </summary>
    /// <param name="sqlSelect">The SQL select query to execute.</param>
    /// <returns>An enumerable of log entries.</returns>
    public IEnumerable<LogEntry> Select(string sqlSelect)
    {
        return SelectAsync(sqlSelect).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Creates a log entry from the current row in the reader.
    /// </summary>
    /// <param name="reader">A reader that is positioned at the current row.</param>
    /// <returns>A log entry object.</returns>
    private LogEntry CreateLogEntryFromReader(SqliteDataReader reader)
    {
        var entry = new LogEntry
        {
            DbId = reader.GetInt64(reader.GetOrdinal(nameof(LogEntry.DbId))),
            Category = reader.GetString(reader.GetOrdinal(nameof(LogEntry.Category))),
            EventId = reader.GetInt32(reader.GetOrdinal(nameof(LogEntry.EventId))),
            EventName = reader.GetString(reader.GetOrdinal(nameof(LogEntry.EventName))),
            Timestamp = DateTimeOffset.Parse(reader.GetString(reader.GetOrdinal(nameof(LogEntry.Timestamp)))),
            Level = (LogLevel)reader.GetInt32(reader.GetOrdinal(nameof(LogEntry.Level))),
            MessageTemplate = reader.GetString(reader.GetOrdinal(nameof(LogEntry.MessageTemplate))),
            RenderedMessage = reader.GetString(reader.GetOrdinal(nameof(LogEntry.RenderedMessage))),
            ExceptionJson = reader.GetString(reader.GetOrdinal(nameof(LogEntry.ExceptionJson))),
            ScopesJson = reader.IsDBNull(reader.GetOrdinal(nameof(LogEntry.ScopesJson))) ? null : reader.GetString(reader.GetOrdinal(nameof(LogEntry.ScopesJson)))
        };

        entry.DeserializeMsgParams(reader.GetString(reader.GetOrdinal(nameof(LogEntry.Properties))));

        return entry;
    }

    /// <summary>
    /// Disposes resources used by the Reader.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the resources used by the Reader.
    /// </summary>
    /// <param name="disposing">Indicates whether the method is called from Dispose.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // Dispose the connection manager
                connectionManager?.Dispose();
            }
            disposed = true;
        }
    }
}
