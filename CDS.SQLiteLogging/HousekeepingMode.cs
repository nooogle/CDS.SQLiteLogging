namespace CDS.SQLiteLogging;

/// <summary>
/// Configuration options for housekeeping of log entries.
/// </summary>
public enum HousekeepingMode
{
    /// <summary>
    /// Automatically clean up log entries.
    /// </summary>
    Automatic,


    /// <summary>
    /// Manually clean up log entries.
    /// </summary>
    Manual
}
