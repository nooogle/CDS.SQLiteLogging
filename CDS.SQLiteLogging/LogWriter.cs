using System.Reflection;

namespace CDS.SQLiteLogging;

/// <summary>
/// Writes log entries to an SQLite database.
/// </summary>
/// <typeparam name="TLogEntry">The type of log entry to write.</typeparam>
public class LogWriter<TLogEntry> where TLogEntry : ILogEntry, new()
{
    private readonly ConnectionManager connectionManager;
    private readonly string tableName;
    private readonly List<PropertyInfo> insertionProperties;
    private readonly string sqlInsert;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogWriter{TLogEntry}"/> class.
    /// </summary>
    /// <param name="connectionManager">The SQLite connection manager.</param>
    /// <param name="tableName">The name of the table to write to.</param>
    public LogWriter(ConnectionManager connectionManager, string tableName)
    {
        this.connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        this.tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));

        var properties = TableCreator.GetPublicNonStaticProperties<TLogEntry>();

        // Exclude the Id property since it is auto-generated
        insertionProperties = properties
            .Where(p => !(p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) && p.PropertyType == typeof(int)))
            .ToList();

        // Build the SQL INSERT command
        string columns = string.Join(", ", insertionProperties.Select(p => p.Name));
        string parameters = string.Join(", ", insertionProperties.Select(p => "@" + p.Name));
        sqlInsert = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters});";
    }

    /// <summary>
    /// Adds a new log entry to the database using a transaction.
    /// </summary>
    /// <param name="entry">The log entry to add.</param>
    public void Add(TLogEntry entry)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));
        AddAsync(entry).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Adds a new log entry to the database asynchronously.
    /// </summary>
    /// <param name="entry">The log entry to add.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task AddAsync(TLogEntry entry)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));
        await connectionManager.ExecuteInTransactionAsync(async transaction =>
        {
            using (var cmd = new SqliteCommand(sqlInsert, connectionManager.Connection, transaction))
            {
                AddParametersToCommand(cmd, entry);
                await Task.Run(() => cmd.ExecuteNonQuery()).ConfigureAwait(false);
            }
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Adds multiple log entries to the database in a single transaction.
    /// </summary>
    /// <param name="entries">The log entries to add.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task AddBatchAsync(IEnumerable<TLogEntry> entries)
    {
        if (entries == null) throw new ArgumentNullException(nameof(entries));
        await connectionManager.ExecuteInTransactionAsync(async transaction =>
        {
            using (var cmd = new SqliteCommand(sqlInsert, connectionManager.Connection, transaction))
            {
                foreach (var entry in entries)
                {
                    if (entry == null) throw new ArgumentNullException(nameof(entries), "One of the entries is null.");
                    cmd.Parameters.Clear();
                    AddParametersToCommand(cmd, entry);
                    await Task.Run(() => cmd.ExecuteNonQuery()).ConfigureAwait(false);
                }
            }
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Adds parameters to the SQLite command based on the log entry properties.
    /// </summary>
    /// <param name="cmd">The SQLite command.</param>
    /// <param name="entry">The log entry.</param>
    private void AddParametersToCommand(SqliteCommand cmd, TLogEntry entry)
    {
        foreach (var prop in insertionProperties)
        {
            object value = prop.GetValue(entry);

            // For DateTimeOffset, store as an ISO 8601 string
            if (prop.PropertyType == typeof(DateTimeOffset) && value != null)
            {
                value = ((DateTimeOffset)value).ToString("o");
            }

            // For the MsgParams property, store as a JSON string
            if (prop.Name.Equals(nameof(ILogEntry.MsgParams), StringComparison.OrdinalIgnoreCase) && value != null)
            {
                value = entry.SerializeMsgParams();
            }

            cmd.Parameters.AddWithValue("@" + prop.Name, value ?? DBNull.Value);
        }
    }
}
