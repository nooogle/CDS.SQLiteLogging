namespace CDS.SQLiteLogging.Internal;

/// <summary>
/// Creates tables in SQLite databases based on the <see cref="LogEntry"/> type definition.
/// </summary>
class TableCreator
{
    private readonly ConnectionManager connectionManager;

    /// <summary>
    /// Gets the name of the <see cref="LogEntry"/> table.
    /// </summary>
    public static string TableName => DatabaseSchema.Tables.LogEntry;

    /// <summary>
    /// Initializes a new instance of the <see cref="TableCreator"/> class.
    /// </summary>
    /// <param name="connectionManager">The SQLite connection manager.</param>
    public TableCreator(ConnectionManager connectionManager)
    {
        this.connectionManager = connectionManager;
    }

    /// <summary>
    /// Creates a table based on the <see cref="LogEntry"/> type.
    /// </summary>
    /// <returns>The name of the created table.</returns>
    public string CreateTableForLogEntry()
    {
        var columnDefinitions = DatabaseSchema.GetColumnDefinitions();
        string sql = $"CREATE TABLE IF NOT EXISTS {TableName} ({string.Join(", ", columnDefinitions)});";
        connectionManager.ExecuteNonQuery(sql);

        // Index supporting time-based housekeeping (WHERE Timestamp < @cutoffDate).
        // Without this the delete is a full table scan. CREATE INDEX IF NOT EXISTS is
        // idempotent so this is safe to run against existing databases.
        connectionManager.ExecuteNonQuery(
            $"CREATE INDEX IF NOT EXISTS IX_{TableName}_{DatabaseSchema.Columns.Timestamp} " +
            $"ON {TableName} ({DatabaseSchema.Columns.Timestamp});");

        return TableName;
    }
}
