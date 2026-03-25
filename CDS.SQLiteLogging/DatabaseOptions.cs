namespace CDS.SQLiteLogging;

/// <summary>
/// Configuration options for SQLite database behavior.
/// </summary>
public class DatabaseOptions
{
    /// <summary>
    /// Gets or sets the SQLite journal mode to apply to each connection.
    /// Defaults to <see cref="SqliteJournalMode.Delete"/>, which is the SQLite native default.
    /// </summary>
    public SqliteJournalMode JournalMode { get; set; } = SqliteJournalMode.Delete;

    /// <summary>
    /// Gets or sets the SQLite synchronous mode to apply to each connection.
    /// </summary>
    public SqliteSynchronousMode SynchronousMode { get; set; } = SqliteSynchronousMode.Normal;
}
