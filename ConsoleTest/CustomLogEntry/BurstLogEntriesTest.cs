using CDS.SQLiteLogging;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ConsoleTest.CustomLogEntry;

/// <summary>
/// Contains a test for adding a burst of log entries.
/// </summary>
static class BurstLogEntriesTest
{
    public static void Run()
    {
        Console.Clear();
        Console.WriteLine("=== Burst Log Entries Test ===\n");

        // Prompt user for the number of log entries
        Console.Write("Enter the number of log entries to add: ");
        if (!int.TryParse(Console.ReadLine(), out int numberOfEntries) || numberOfEntries <= 0)
        {
            Console.WriteLine("Invalid number of entries.");
            return;
        }

        // Ask user for the batch size
        Console.Write("Enter the batch size: ");
        if (!int.TryParse(Console.ReadLine(), out int batchSize) || batchSize <= 0)
        {
            Console.WriteLine("Invalid batch size.");
            return;
        }

        // Ask user for the flush interval in seconds
        Console.Write("Enter the flush interval in seconds: ");
        if (!int.TryParse(Console.ReadLine(), out int flushIntervalSeconds) || flushIntervalSeconds <= 0)
        {
            Console.WriteLine("Invalid flush interval.");
            return;
        }

        // Setup batching options
        BatchingOptions batchingOptions = new()
        {
            MaxCacheSize = batchSize,
            FlushInterval = TimeSpan.FromSeconds(flushIntervalSeconds)
        };

        // Get the standard log folder
        string folder = LogFolderManager.GetLogFolder(nameof(BurstLogEntriesTest));
        Console.WriteLine($"Using log folder: {folder}");

        // Create a new instance of the SQLite Logger class
        using var logger = new SQLiteLogger<MyLogEntry>(
            Path.Combine(folder, $"{nameof(BurstLogEntriesTest)}_Schema{MyLogEntry.Version}.db"),
            batchingOptions,
            new HouseKeepingOptions());

        // Clear existing entries
        int deletedCount = logger.DeleteAll();
        Console.WriteLine($"Deleted {deletedCount} existing entries.");

        // Start timing the process
        var stopwatch = Stopwatch.StartNew();
        int addedCount = 0;
        bool quit = false;

        Console.WriteLine("Press 'q' to quit.");

        for (int i = 1; i <= numberOfEntries && !quit; i++)
        {
            if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q)
            {
                quit = true;
                break;
            }

            // Add log entry
            var logEntry = new MyLogEntry
            {
                Timestamp = DateTimeOffset.Now,
                Level = LogLevel.Information,
                LineIndex = i,
                BatchId = "BurstTestBatch",
                Sender = "BurstLogEntriesTest",
                MessageTemplate = "Image with illumination {illumination} has result {result}.",

                Properties = new Dictionary<string, object>
                {
                    ["illumination"] = MsgParamsGen.GetIllumination(),
                    ["result"] = MsgParamsGen.GetResult(),
                }
            };
            logger.Add(logEntry);
            addedCount++;

            // Provide feedback every 1000 entries
            if (i % 1000 == 0)
            {
                double rate = addedCount / stopwatch.Elapsed.TotalSeconds;
                double eta = (numberOfEntries - addedCount) / rate;
                Console.WriteLine($"Added {addedCount}/{numberOfEntries} entries. Rate: {rate:F2} entries/sec. ETA: {eta:F2} sec.");
            }
        }

        // Stop timing
        stopwatch.Stop();

        // Notify user that flush is starting
        Console.WriteLine("Starting flush...");

        // Measure flush duration
        var flushStopwatch = Stopwatch.StartNew();
        logger.Flush();
        flushStopwatch.Stop();

        // Report flush duration
        Console.WriteLine($"Flush completed in {flushStopwatch.Elapsed.TotalSeconds:F2} seconds.");

        // Report stats
        Console.WriteLine($"\nTest completed. Added {addedCount} entries in {stopwatch.Elapsed.TotalSeconds:F2} seconds.");
        Console.WriteLine($"Average rate: {addedCount / stopwatch.Elapsed.TotalSeconds:F2} entries/sec.");

        // Let user know if any entries were discarded
        if(logger.DiscardedEntriesCount > 0)
        {
            Console.WriteLine($"*** {logger.DiscardedEntriesCount} entries were discarded due to the cache being full!");
        }

        // Get the size of the database file in MB
        long dbFileSizeBytes = logger.GetDatabaseFileSize();
        double dbFileSizeMB = dbFileSizeBytes / (1024.0 * 1024.0);
        Console.WriteLine($"Database file size before deletion: {dbFileSizeMB:F2} MB");

        // Clear entries at the end
        deletedCount = logger.DeleteAll();
        Console.WriteLine($"Deleted {deletedCount} entries at the end of the test.");
    }
}
