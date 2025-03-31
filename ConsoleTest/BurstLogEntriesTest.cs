using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ConsoleTest;

/// <summary>
/// Contains a test for adding a burst of log entries.
/// </summary>
/// <remarks>
/// Initialises a new instance of the <see cref="BurstLogEntriesTest"/> class.
/// </remarks>
/// <param name="logger">
/// A logger, provided by the dependency injection container.
/// </param>
class BurstLogEntriesTest(ILogger<BurstLogEntriesTest> logger)
{
    /// <summary>
    /// A logger, provided by the dependency injection container.
    /// </summary>
    private readonly ILogger<BurstLogEntriesTest> logger = logger;


    /// <summary>
    /// Runs a test of adding a burst of log entries.
    /// </summary>
    public void Run()
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

        //// Clear existing entries
        //int deletedCount = logger.DeleteAll(); TODO
        //Console.WriteLine($"Deleted {deletedCount} existing entries.");

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

            logger.LogInformation(
                "Image with illumination {illumination} has result {result}",
                [MsgParamsGen.GetIllumination(), MsgParamsGen.GetResult()]);


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
        // logger.Flush(); TODO
        flushStopwatch.Stop();

        // Report flush duration
        Console.WriteLine($"Flush completed in {flushStopwatch.Elapsed.TotalSeconds:F2} seconds.");

        // Report stats
        Console.WriteLine($"\nTest completed. Added {addedCount} entries in {stopwatch.Elapsed.TotalSeconds:F2} seconds.");
        Console.WriteLine($"Average rate: {addedCount / stopwatch.Elapsed.TotalSeconds:F2} entries/sec.");

        // TODO
        //// Let user know if any entries were discarded
        //if(logger.DiscardedEntriesCount > 0)
        //{
        //    Console.WriteLine($"*** {logger.DiscardedEntriesCount} entries were discarded due to the cache being full!");
        //}

        //// Get the size of the database file in MB
        //long dbFileSizeBytes = logger.GetDatabaseFileSize();
        //double dbFileSizeMB = dbFileSizeBytes / (1024.0 * 1024.0);
        //Console.WriteLine($"Database file size before deletion: {dbFileSizeMB:F2} MB");

        //// Clear entries at the end
        //deletedCount = logger.DeleteAll();
        //Console.WriteLine($"Deleted {deletedCount} entries at the end of the test.");
    }
}
