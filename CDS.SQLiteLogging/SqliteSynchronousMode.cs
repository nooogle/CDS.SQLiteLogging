namespace CDS.SQLiteLogging;

/// <summary>
/// Supported values for SQLite <c>PRAGMA synchronous</c>.
/// </summary>
public enum SqliteSynchronousMode
{
    /// <summary>
    /// Disables extra synchronization for the lowest write overhead.
    /// </summary>
    Off,

    /// <summary>
    /// Uses the SQLite NORMAL durability mode.
    /// </summary>
    Normal,

    /// <summary>
    /// Uses the SQLite FULL durability mode.
    /// </summary>
    Full,

    /// <summary>
    /// Uses the SQLite EXTRA durability mode.
    /// </summary>
    Extra,
}
