namespace CDS.SQLiteLogging;

#if NET6_0_OR_GREATER

/// <summary>
/// Represents SQLite error codes. They don't seem to be defined in the SQLite library.
/// </summary>
public enum SqliteErrorCode
{
    /// <summary>
    /// The database file is locked/busy.
    /// </summary>
    Busy = 5,


    /// <summary>
    /// The database file is locked.
    /// </summary>
    Locked = 6,
    // Add additional codes if needed.
}

#endif
