namespace CDS.SQLiteLogging;

/// <summary>
/// Configuration options for housekeeping of log entries.
/// </summary>
public class HouseKeepingOptions
{
    /// <summary>
    /// Gets or sets the housekeeping mode.
    /// </summary>
    public HousekeepingMode Mode { get; set; } = HousekeepingMode.Automatic;

    /// <summary>
    /// Gets or sets the retention period for log entries.
    /// </summary>
    public TimeSpan RetentionPeriod { get; set; } = TimeSpan.FromDays(30);

    /// <summary>
    /// Gets or sets the interval between cleanup operations.
    /// </summary>
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromHours(1);
}
