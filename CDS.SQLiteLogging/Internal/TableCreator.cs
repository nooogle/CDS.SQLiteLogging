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
    public static string TableName => nameof(LogEntry);


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
        var columnDefinitions = new List<string>
        {
            "DbId INTEGER PRIMARY KEY AUTOINCREMENT",
            "Category TEXT",
            "EventId INTEGER",
            "EventName TEXT",
            "Timestamp TEXT",
            "Level INTEGER",
            "ManagedThreadId INTEGER",
            "MessageTemplate TEXT",
            "Properties TEXT",
            "RenderedMessage TEXT",
            "ExceptionJson TEXT",
            "ScopesJson TEXT"
        };

        string sql = $"CREATE TABLE IF NOT EXISTS {TableName} ({string.Join(", ", columnDefinitions)});";
        connectionManager.ExecuteNonQuery(sql);

        return TableName;
    }
}
