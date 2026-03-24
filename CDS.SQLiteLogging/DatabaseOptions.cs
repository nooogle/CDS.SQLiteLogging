namespace CDS.SQLiteLogging;

/// <summary>
/// Configuration options for SQLite database behavior.
/// </summary>
public class DatabaseOptions
{
    /// <summary>
    /// Gets or sets the SQLite synchronous mode to apply to each connection.
    /// </summary>
    public SqliteSynchronousMode SynchronousMode { get; set; } = SqliteSynchronousMode.Normal;
}
