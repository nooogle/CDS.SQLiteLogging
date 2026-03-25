namespace CDS.SQLiteLogging;

/// <summary>
/// Supported values for SQLite <c>PRAGMA journal_mode</c>.
/// </summary>
public enum SqliteJournalMode
{
    /// <summary>
    /// The journal file is deleted after each transaction. This is the SQLite default.
    /// </summary>
    Delete,

    /// <summary>
    /// The journal file is truncated to zero length after each transaction rather than deleted.
    /// </summary>
    Truncate,

    /// <summary>
    /// The journal file is zeroed out at the header after each transaction but kept on disk.
    /// </summary>
    Persist,

    /// <summary>
    /// The journal is held entirely in memory. Faster but unsafe if the process crashes mid-transaction.
    /// </summary>
    Memory,

    /// <summary>
    /// Write-Ahead Log mode. Allows concurrent readers and a single writer.
    /// </summary>
    Wal,

    /// <summary>
    /// No journal is created. Fastest writes, but no rollback capability and no crash recovery.
    /// </summary>
    Off,
}
