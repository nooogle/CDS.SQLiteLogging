using System.Collections.Immutable;
using System.Reflection;

namespace CDS.SQLiteLogging;


/// <summary>
/// Reads log entries from an SQLite database.
/// </summary>
/// <typeparam name="TLogEntry">The type of log entry to read.</typeparam>
public class LogReader<TLogEntry> where TLogEntry : ILogEntry, new()
{
    private readonly ConnectionManager connectionManager;
    private readonly string tableName;
    private readonly PropertyInfo[] properties;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogReader{TLogEntry}"/> class.
    /// </summary>
    /// <param name="connectionManager">The SQLite connection manager.</param>
    /// <param name="tableName">The name of the table to read from.</param>
    public LogReader(ConnectionManager connectionManager, string tableName)
    {
        this.connectionManager = connectionManager;
        this.tableName = tableName;
        this.properties = TableCreator.GetPublicNonStaticProperties<TLogEntry>();
    }

    /// <summary>
    /// Reads and returns all log entries from the database.
    /// </summary>
    /// <returns>An immutable list of log entries.</returns>
    public async Task<ImmutableList<TLogEntry>> GetAllEntriesAsync()
    {
        var entries = ImmutableList.CreateBuilder<TLogEntry>();

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
    public ImmutableList<TLogEntry> GetAllEntries()
    {
        return GetAllEntriesAsync().GetAwaiter().GetResult();
    }


    /// <summary>
    /// Reads and returns the most recent log entries from the database.
    /// </summary>
    /// <param name="maxCount">The maximum number of entries to return.</param>
    /// <returns>An immutable list of log entries, ordered by timestamp descending.</returns>
    public async Task<ImmutableList<TLogEntry>> GetRecentEntriesAsync(int maxCount)
    {
        if (maxCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCount), "Maximum count must be greater than zero");
        }

        var entries = ImmutableList.CreateBuilder<TLogEntry>();

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
                        TLogEntry entry = CreateLogEntryFromReader(reader);
                        entries.Add(entry);
                    }
                }
            }
        }).ConfigureAwait(false);

        return entries.ToImmutable();
    }



    /// <summary>
    /// Creates a log entry from the current row in the reader.
    /// </summary>
    /// <param name="reader">
    /// A reader that is positioned at the current row.
    /// </param>
    /// <returns>
    /// A log entry object.
    /// </returns>
    private TLogEntry CreateLogEntryFromReader(SqliteDataReader reader)
    {
        var entry = new TLogEntry();
        foreach (var prop in properties)
        {
            if (reader[prop.Name] != DBNull.Value)
            {
                SetLogEntryPropertyFromReaderField(reader, entry, prop);
            }
        }

        return entry;
    }


    /// <summary>
    /// Sets a property of a log entry from a field in the reader.
    /// </summary>
    /// <param name="reader">The DB reader</param>
    /// <param name="entry">The log entry</param>
    /// <param name="prop">The property to set</param>
    private static void SetLogEntryPropertyFromReaderField(SqliteDataReader reader, TLogEntry entry, PropertyInfo prop)
    {
        object value = reader[prop.Name];

        if (prop.Name.Equals(nameof(ILogEntry.Properties), StringComparison.OrdinalIgnoreCase))
        {
            entry.DeserializeMsgParams((string)value);
        }
        else
        {
            if (prop.PropertyType == typeof(DateTimeOffset))
            {
                value = DateTimeOffset.Parse((string)value);
            }
            else if (prop.PropertyType.IsEnum)
            {
                value = Enum.ToObject(prop.PropertyType, value);
            }
            else if (prop.PropertyType == typeof(int))
            {
                value = Convert.ToInt32(value);
            }

            prop.SetValue(entry, value);
        }
    }

    /// <summary>
    /// Reads and returns the most recent log entries from the database (synchronous version).
    /// </summary>
    /// <param name="maxCount">The maximum number of entries to return.</param>
    /// <returns>An immutable list of log entries, ordered by timestamp descending.</returns>
    public ImmutableList<TLogEntry> GetRecentEntries(int maxCount)
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
    public async Task<ImmutableList<TLogEntry>> GetEntriesByMessageParamAsync(string key, object value)
    {
        var entries = ImmutableList.CreateBuilder<TLogEntry>();

        await connectionManager.ExecuteWithRetryAsync(async () =>
        {
            string sql = $"SELECT * FROM {tableName} WHERE json_extract(MsgParams, '$.{key}') = @value;";
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
}
