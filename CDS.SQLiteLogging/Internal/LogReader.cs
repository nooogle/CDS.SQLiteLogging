using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Reflection;

namespace CDS.SQLiteLogging.Internal;


/// <summary>
/// Reads log entries from an SQLite database.
/// </summary>
class LogReader
{
    private readonly ConnectionManager connectionManager;
    private readonly string tableName;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogReader"/> class.
    /// </summary>
    /// <param name="connectionManager">The SQLite connection manager.</param>
    /// <param name="tableName">The name of the table to read from.</param>
    public LogReader(ConnectionManager connectionManager, string tableName)
    {
        this.connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        this.tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
    }

    /// <summary>
    /// Reads and returns all log entries from the database.
    /// </summary>
    /// <returns>An immutable list of log entries.</returns>
    public async Task<ImmutableList<LogEntry>> GetAllEntriesAsync()
    {
        var entries = ImmutableList.CreateBuilder<LogEntry>();

        await connectionManager.ExecuteWithRetryAsync(async () =>
        {
            string sql = $"SELECT * FROM {tableName};";
            using (var cmd = new SqliteCommand(sql, connectionManager.Connection))
            {
                using (var reader = await Task.Run(() => cmd.ExecuteReader()).ConfigureAwait(false))
                {
                    while (await Task.Run(() => reader.Read()).ConfigureAwait(false))
                    {
                        var entry = CreateLogEntryFromReader(reader);
                        entries.Add(entry);
                    }
                }
            }
        }).ConfigureAwait(false);

        return entries.ToImmutable();
    }

    /// <summary>
    /// Reads and returns all log entries from the database (synchronous version).
    /// </summary>
    /// <returns>An immutable list of log entries.</returns>
    public ImmutableList<LogEntry> GetAllEntries()
    {
        return GetAllEntriesAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Reads and returns the most recent log entries from the database.
    /// </summary>
    /// <param name="maxCount">The maximum number of entries to return.</param>
    /// <returns>An immutable list of log entries, ordered by timestamp descending.</returns>
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
            using (var cmd = new SqliteCommand(sql, connectionManager.Connection))
            {
                cmd.Parameters.AddWithValue("@maxCount", maxCount);
                using (var reader = await Task.Run(() => cmd.ExecuteReader()).ConfigureAwait(false))
                {
                    while (await Task.Run(() => reader.Read()).ConfigureAwait(false))
                    {
                        var entry = CreateLogEntryFromReader(reader);
                        entries.Add(entry);
                    }
                }
            }
        }).ConfigureAwait(false);

        return entries.ToImmutable();
    }

    /// <summary>
    /// Reads and returns the most recent log entries from the database (synchronous version).
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
    /// Reads and returns log entries that contain a specific message parameter.
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
}
