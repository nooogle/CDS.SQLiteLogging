using CDS.SQLiteLogging;

namespace ConsoleTest;

/// <summary>
/// Contains ad-hoc tests for the SQLite Logger.
/// </summary>
static class AdHocTests
{
    /// <summary>
    /// Runs a basic test of the SQLite Logger.
    /// </summary>
    public static void RunBasicTest()
    {
        // Display the test header
        Console.Clear();
        Console.WriteLine("=== Basic SQLite Logger Test ===\n");

        // Get the standard log folder
        string folder = LogFolderManager.GetLogFolder(nameof(AdHocTests));
        Console.WriteLine($"Using log folder: {folder}");

        // Create a new instance of the SQLite Logger class
        using var logger = new Logger<MyLogEntry>(
            folder,
            schemaVersion: MyLogEntry.Version,
            new BatchingOptions(),
            new HouseKeepingOptions());

        // Clear existing entries (optional)
        int deletedCount = logger.DeleteAll();
        Console.WriteLine($"Deleted {deletedCount} existing entries.");

        // Add some sample log entries
        DateTimeOffset now = DateTimeOffset.Now;
        for (int i = 1; i <= 100; i++)
        {
            AddSampleLogEntry(logger, now.AddSeconds(i), i);
        }

        // Force flush to ensure all entries are written
        logger.Flush();
        Console.WriteLine("Added 100 sample log entries.");

        // Get and display the most recent 10 entries
        var recentEntries = logger.GetRecentEntries(10);
        Console.WriteLine($"\nMost recent {recentEntries.Count} log entries:");
        foreach (var entry in recentEntries)
        {
            Console.WriteLine($"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss}] {entry.Level} from {entry.Sender}: {entry.GetFormattedMsg()}");
        }
    }

    /// <summary>
    /// Adds a sample log entry with the specified parameters.
    /// </summary>
    /// <param name="logger">The logger to use.</param>
    /// <param name="timestamp">The timestamp for the entry.</param>
    /// <param name="counter">A counter value for the entry.</param>
    private static void AddSampleLogEntry(
        Logger<MyLogEntry> logger,
        DateTimeOffset timestamp,
        int counter)
    {

        var logLevel = counter % 3 == 0 ? LogLevel.Warning :
                     counter % 7 == 0 ? LogLevel.Error :
                     LogLevel.Information;

        var logEntry = new MyLogEntry
        {
            Timestamp = timestamp,
            Level = logLevel,
            LineIndex = counter,
            BatchId = "TestBatch",
            Sender = "ConsoleText",
            Message = "Image with illumination {illumination} has result {result}.",

            MsgParams = new Dictionary<string, object>
            {
                ["illumination"] = MsgParamsGen.GetIllumination(),
                ["result"] = MsgParamsGen.GetResult(),
            }
        };

        // Log the entry
        logger.Add(logEntry);
    }
}
