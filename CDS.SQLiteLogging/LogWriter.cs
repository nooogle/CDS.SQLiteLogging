using System.Reflection;
using System.Text;

namespace CDS.SQLiteLogging;

/// <summary>
/// Writes log entries to an SQLite database.
/// </summary>
public class LogWriter
{
    private readonly ConnectionManager connectionManager;
    private readonly string tableName;
    private readonly string sqlInsert;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogWriter"/> class.
    /// </summary>
    /// <param name="connectionManager">The SQLite connection manager.</param>
    /// <param name="tableName">The name of the table to write to.</param>
    public LogWriter(ConnectionManager connectionManager, string tableName)
    {
        this.connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        this.tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));

        // Predefined SQL INSERT command
        sqlInsert = $@"
            INSERT INTO {tableName} (
                Category, EventId, EventName, Timestamp, Level, MessageTemplate, Properties, RenderedMessage, ExceptionJson, ScopesJson
            ) VALUES (
                @Category, @EventId, @EventName, @Timestamp, @Level, @MessageTemplate, @Properties, @RenderedMessage, @ExceptionJson, @ScopesJson
            );";
    }

    /// <summary>
    /// Adds a new log entry to the database using a transaction.
    /// </summary>
    /// <param name="entry">The log entry to add.</param>
    public void Add(LogEntry entry)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));
        AddAsync(entry).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Adds a new log entry to the database asynchronously.
    /// </summary>
    /// <param name="entry">The log entry to add.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task AddAsync(LogEntry entry)
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
    public async Task AddBatchAsync(IEnumerable<LogEntry> entries)
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
    /// Adds multiple log entries to the database in a single transaction synchronously.
    /// </summary>
    /// <param name="entries">The log entries to add.</param>
    public void AddBatch(IEnumerable<LogEntry> entries)
    {
        if (entries == null) throw new ArgumentNullException(nameof(entries));

        connectionManager.ExecuteInTransaction(transaction =>
        {
            using var cmd = new SqliteCommand(sqlInsert, connectionManager.Connection, transaction);
            foreach (var entry in entries)
            {
                if (entry == null) throw new ArgumentNullException(nameof(entries), "One of the entries is null.");
                cmd.Parameters.Clear();
                AddParametersToCommand(cmd, entry);
                cmd.ExecuteNonQuery();
            }
        });
    }


    /// <summary>
    /// Adds parameters to the SQLite command based on the log entry properties.
    /// </summary>
    /// <param name="cmd">The SQLite command.</param>
    /// <param name="entry">The log entry.</param>
    private void AddParametersToCommand(SqliteCommand cmd, LogEntry entry)
    {
        cmd.Parameters.AddWithValue("@Category", entry.Category ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@EventId", entry.EventId);
        cmd.Parameters.AddWithValue("@EventName", entry.EventName ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Timestamp", entry.Timestamp.ToString("o"));
        cmd.Parameters.AddWithValue("@Level", entry.Level);
        cmd.Parameters.AddWithValue("@MessageTemplate", entry.MessageTemplate ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Properties", entry.SerializeMsgParams() ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@RenderedMessage", entry.RenderedMessage ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@ExceptionJson", entry.ExceptionJson ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@ScopesJson", entry.ScopesJson ?? (object)DBNull.Value);
    }
}
