using CDS.SQLiteLogging;

namespace ConsoleTest.DIWriterDemos;

/// <summary>
/// Contains methods for getting custom options from the user.
/// </summary>
static class CustomOptionsEditor
{
    /// <summary>
    /// Gets the batching options from the user.
    /// </summary>
    /// <returns>The configured <see cref="BatchingOptions"/>.</returns>
    public static BatchingOptions GetBatchingOptions(BatchingOptions batchingOptions)
    {
        // Display existing options
        Console.WriteLine("Current batching options:");
        Console.WriteLine($"Batch size: {batchingOptions.BatchSize}");
        Console.WriteLine($"Cache size: {batchingOptions.MaxCacheSize}");
        Console.WriteLine($"Flush interval: {batchingOptions.FlushInterval}");
        Console.WriteLine();

        // Ask user for the batch size
        Console.Write("Enter the batch size: ");
        if (!int.TryParse(Console.ReadLine(), out int batchSize) || batchSize <= 0)
        {
            Console.WriteLine("Invalid batch size.");
            return batchingOptions;
        }

        // Ask user for the cache size
        Console.Write("Enter the cache size: ");
        if (!int.TryParse(Console.ReadLine(), out int cacheSize) || cacheSize <= 0)
        {
            Console.WriteLine("Invalid cache size.");
            return batchingOptions;
        }

        // Ask user for the flush interval in seconds
        Console.Write("Enter the flush interval in seconds: ");
        if (!int.TryParse(Console.ReadLine(), out int flushIntervalSeconds) || flushIntervalSeconds <= 0)
        {
            Console.WriteLine("Invalid flush interval.");
            return batchingOptions;
        }

        // Setup batching options
        batchingOptions = new()
        {
            BatchSize = batchSize,
            MaxCacheSize = cacheSize,
            FlushInterval = TimeSpan.FromSeconds(flushIntervalSeconds)
        };

        return batchingOptions;
    }

    /// <summary>
    /// Gets the housekeeping options from the user.
    /// </summary>
    /// <returns>The configured <see cref="HouseKeepingOptions"/>.</returns>
    public static HouseKeepingOptions GetHouseKeepingOptions(HouseKeepingOptions houseKeepingOptions)
    {
        // Display existing options
        Console.WriteLine("Current housekeeping options:");
        Console.WriteLine($"Retention period: {houseKeepingOptions.RetentionPeriod}");
        Console.WriteLine($"Cleanup interval: {houseKeepingOptions.CleanupInterval}");
        Console.WriteLine();

        // Ask user for the retention period in days
        Console.Write("Enter the retention period in days: ");
        if (!int.TryParse(Console.ReadLine(), out int retentionDays) || retentionDays <= 0)
        {
            Console.WriteLine("Invalid retention period.");
            return houseKeepingOptions;
        }

        // Ask user for the cleanup interval in hours
        Console.Write("Enter the cleanup interval in hours: ");
        if (!int.TryParse(Console.ReadLine(), out int cleanupIntervalHours) || cleanupIntervalHours <= 0)
        {
            Console.WriteLine("Invalid cleanup interval.");
            return houseKeepingOptions;
        }

        // Setup housekeeping options
        houseKeepingOptions = new()
        {
            RetentionPeriod = TimeSpan.FromDays(retentionDays),
            CleanupInterval = TimeSpan.FromHours(cleanupIntervalHours)
        };

        return houseKeepingOptions;
    }
}
