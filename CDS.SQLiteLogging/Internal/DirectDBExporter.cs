namespace CDS.SQLiteLogging.Internal;

/// <summary>
/// Provides high-performance direct database-to-database export functionality for log entries.
/// </summary>
internal static class DirectDBExporter
{
    private const int BatchSize = 500; // Avoids SQLite's parameter limit (999)

    /// <summary>
    /// Exports log entries from a source database to a destination database by their IDs.
    /// </summary>
    /// <param name="sourceConnectionManager">The source database connection manager.</param>
    /// <param name="destinationConnectionManager">The destination database connection manager.</param>
    /// <param name="idsToExport">The array of log entry IDs to export.</param>
    /// <param name="cancellationToken">Cancellation token for long-running operations.</param>
    /// <returns>A task representing the asynchronous export operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any connection manager is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the IDs array is null or empty.</exception>
    public static async Task ExportAsync(
        ConnectionManager sourceConnectionManager,
        ConnectionManager destinationConnectionManager,
        long[] idsToExport,
        CancellationToken cancellationToken = default)
    {
        if (sourceConnectionManager == null)
        {
            throw new ArgumentNullException(nameof(sourceConnectionManager));
        }

        if (destinationConnectionManager == null)
        {
            throw new ArgumentNullException(nameof(destinationConnectionManager));
        }

        if (idsToExport == null)
        {
            throw new ArgumentException("Value cannot be null or empty.", nameof(idsToExport));
        }

        var tableCreator = new TableCreator(destinationConnectionManager);
        string tableName = tableCreator.CreateTableForLogEntry();

        for (int i = 0; i < idsToExport.Length; i += BatchSize)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            int batchLength = Math.Min(BatchSize, idsToExport.Length - i);
            await ExportBatchAsync(
                sourceConnectionManager, 
                destinationConnectionManager, 
                tableName, 
                idsToExport,
                i,
                batchLength,
                cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Exports a batch of log entries.
    /// </summary>
    private static async Task ExportBatchAsync(
        ConnectionManager sourceConnectionManager,
        ConnectionManager destinationConnectionManager,
        string tableName,
        long[] idsToExport,
        int startIndex,
        int batchLength,
        CancellationToken cancellationToken)
    {
        await destinationConnectionManager.ExecuteInTransactionAsync(async transaction =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var selectCmd = BuildSelectCommand(sourceConnectionManager, tableName, idsToExport, startIndex, batchLength);
            using var reader = await Task.Run(() => selectCmd.ExecuteReader(), cancellationToken).ConfigureAwait(false);

            if (!reader.HasRows)
            {
                return;
            }

            // Cache column ordinals for performance
            var columnOrdinals = new ColumnOrdinals(reader);
            
            using var insertCmd = BuildInsertCommand(destinationConnectionManager, tableName, transaction);

            while (await Task.Run(() => reader.Read(), cancellationToken).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();
                PopulateInsertCommand(insertCmd, reader, columnOrdinals);
                await Task.Run(() => insertCmd.ExecuteNonQuery(), cancellationToken).ConfigureAwait(false);
            }
        }).ConfigureAwait(false);
    }

    /// <summary>
    /// Builds the SELECT command with parameterized IDs.
    /// </summary>
    private static SqliteCommand BuildSelectCommand(
        ConnectionManager sourceConnectionManager,
        string tableName,
        long[] idsToExport,
        int startIndex,
        int batchLength)
    {
        var cmd = new SqliteCommand { Connection = sourceConnectionManager.Connection };
        
        var parameterNames = new List<string>(batchLength);
        for (int i = 0; i < batchLength; i++)
        {
            string paramName = $"@id{i}";
            parameterNames.Add(paramName);
            cmd.Parameters.AddWithValue(paramName, idsToExport[startIndex + i]);
        }

        cmd.CommandText = $"SELECT * FROM {tableName} WHERE DbId IN ({string.Join(",", parameterNames)}) ORDER BY DbId";
        return cmd;
    }

    /// <summary>
    /// Builds the INSERT command for the destination database.
    /// </summary>
    private static SqliteCommand BuildInsertCommand(
        ConnectionManager destinationConnectionManager,
        string tableName,
        SqliteTransaction transaction)
    {
        string insertSql = $@"INSERT INTO {tableName} 
            (Category, EventId, EventName, Timestamp, Level, ManagedThreadId, MessageTemplate, Properties, RenderedMessage, ExceptionJson, ScopesJson) 
            VALUES 
            (@Category, @EventId, @EventName, @Timestamp, @Level, @ManagedThreadId, @MessageTemplate, @Properties, @RenderedMessage, @ExceptionJson, @ScopesJson)";

        return new SqliteCommand(insertSql, destinationConnectionManager.Connection, transaction);
    }

    /// <summary>
    /// Populates the INSERT command with data from the reader.
    /// </summary>
    private static void PopulateInsertCommand(
        SqliteCommand insertCmd,
        SqliteDataReader reader,
        ColumnOrdinals ordinals)
    {
        insertCmd.Parameters.Clear();
        
        insertCmd.Parameters.AddWithValue("@Category", GetValueOrDBNull(reader, ordinals.Category));
        insertCmd.Parameters.AddWithValue("@EventId", reader.GetInt32(ordinals.EventId));
        insertCmd.Parameters.AddWithValue("@EventName", GetValueOrDBNull(reader, ordinals.EventName));
        insertCmd.Parameters.AddWithValue("@Timestamp", reader.GetString(ordinals.Timestamp));
        insertCmd.Parameters.AddWithValue("@Level", reader.GetInt32(ordinals.Level));
        insertCmd.Parameters.AddWithValue("@ManagedThreadId", reader.GetInt32(ordinals.ManagedThreadId));
        insertCmd.Parameters.AddWithValue("@MessageTemplate", GetValueOrDBNull(reader, ordinals.MessageTemplate));
        insertCmd.Parameters.AddWithValue("@Properties", GetValueOrDBNull(reader, ordinals.Properties));
        insertCmd.Parameters.AddWithValue("@RenderedMessage", GetValueOrDBNull(reader, ordinals.RenderedMessage));
        insertCmd.Parameters.AddWithValue("@ExceptionJson", GetValueOrDBNull(reader, ordinals.ExceptionJson));
        insertCmd.Parameters.AddWithValue("@ScopesJson", GetValueOrDBNull(reader, ordinals.ScopesJson));
    }

    /// <summary>
    /// Gets the string value from the reader or DBNull if the value is null.
    /// </summary>
    private static object GetValueOrDBNull(SqliteDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? DBNull.Value : reader.GetString(ordinal);
    }

    /// <summary>
    /// Caches column ordinals for efficient data access.
    /// </summary>
    private sealed class ColumnOrdinals
    {
        public readonly int Category;
        public readonly int EventId;
        public readonly int EventName;
        public readonly int Timestamp;
        public readonly int Level;
        public readonly int ManagedThreadId;
        public readonly int MessageTemplate;
        public readonly int Properties;
        public readonly int RenderedMessage;
        public readonly int ExceptionJson;
        public readonly int ScopesJson;

        public ColumnOrdinals(SqliteDataReader reader)
        {
            Category = reader.GetOrdinal(nameof(LogEntry.Category));
            EventId = reader.GetOrdinal(nameof(LogEntry.EventId));
            EventName = reader.GetOrdinal(nameof(LogEntry.EventName));
            Timestamp = reader.GetOrdinal(nameof(LogEntry.Timestamp));
            Level = reader.GetOrdinal(nameof(LogEntry.Level));
            ManagedThreadId = reader.GetOrdinal(nameof(LogEntry.ManagedThreadId));
            MessageTemplate = reader.GetOrdinal(nameof(LogEntry.MessageTemplate));
            Properties = reader.GetOrdinal(nameof(LogEntry.Properties));
            RenderedMessage = reader.GetOrdinal(nameof(LogEntry.RenderedMessage));
            ExceptionJson = reader.GetOrdinal(nameof(LogEntry.ExceptionJson));
            ScopesJson = reader.GetOrdinal(nameof(LogEntry.ScopesJson));
        }
    }
}
