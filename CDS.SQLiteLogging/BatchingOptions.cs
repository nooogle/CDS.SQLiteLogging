namespace CDS.SQLiteLogging;

/// <summary>
/// Configuration options for batch processing of log entries.
/// </summary>
public class BatchingOptions
{
    /// <summary>
    /// Gets or sets the maximum number of entries to write in a single batch.
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the maximum number of entries to cache. Any items
    /// logged when the cache is full will be discarded.
    /// </summary>
    public int MaxCacheSize { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the interval in milliseconds between cache flushes.
    /// </summary>
    public TimeSpan FlushInterval { get; set; } = TimeSpan.FromSeconds(5);
}
