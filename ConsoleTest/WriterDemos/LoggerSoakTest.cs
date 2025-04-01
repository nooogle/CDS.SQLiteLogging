using CDS.SQLiteLogging;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ConsoleTest.WriterDemos;

/// <summary>
/// Provides soak testing capabilities for the SqliteLogger.
/// </summary>
class LoggerSoakTest
{
    private readonly ILogger logger;
    private readonly ISQLiteWriterUtilities loggerUtilities;
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    private readonly List<long> addTimesMs = new List<long>();
    private readonly Stopwatch stopwatchTotal = new Stopwatch();
    private readonly object lockObject = new object();
    private int totalEntriesAdded;
    private int entriesPerSecond;
    private long minAddTimeMs = long.MaxValue;
    private long maxAddTimeMs = 0;
    private long totalAddTimeMs = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggerSoakTest"/> class.
    /// </summary>
    /// <param name="folder">The folder where the logs will be stored.</param>
    /// <param name="entriesPerSecond">The number of log entries to add per second.</param>
    public LoggerSoakTest(ILogger<LoggerSoakTest> logger, ISQLiteWriterUtilities loggerUtilities)
    {
        this.logger = logger;
        this.loggerUtilities = loggerUtilities;
    }


    /// <summary>
    /// Runs the soak test until canceled.
    /// </summary>
    public void Run()
    {
        // Display header
        Console.Clear();
        Console.WriteLine("==== SQLite Logger Soak Test ====");
        Console.WriteLine();
        Console.Write("Enter entries per second (default 10): ");
        string rateInput = Console.ReadLine() ?? string.Empty;
        entriesPerSecond = string.IsNullOrWhiteSpace(rateInput) ? 10 : int.Parse(rateInput);


        Console.WriteLine("Starting soak test. Press any key to stop...");
        Console.WriteLine($"Adding {entriesPerSecond} log entries per second");
        Console.WriteLine("Housekeeping configured to run every 2 minutes");
        Console.WriteLine("Retention period set to 1 minute");
        Console.WriteLine();

        // Start the test in a background thread
        Task.Run(() => RunTestAsync(cancellationTokenSource.Token));

        // Start the metrics reporter in another thread
        Task.Run(() => ReportMetricsAsync(cancellationTokenSource.Token));

        // Wait for a key press to stop the test
        Console.ReadKey(true);
        cancellationTokenSource.Cancel();

        // Give time for graceful shutdown
        Thread.Sleep(1000);

        // Display final statistics
        DisplayFinalStatistics();
    }

    /// <summary>
    /// Runs the actual test, adding log entries at the specified rate.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation.</param>
    private async Task RunTestAsync(CancellationToken cancellationToken)
    {
        stopwatchTotal.Start();
        int counter = 0;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Add entries for this second
                for (int i = 0; i < entriesPerSecond && !cancellationToken.IsCancellationRequested; i++)
                {
                    counter++;
                    await AddLogEntryWithTimingAsync(counter);
                }

                // Wait until the next second if we finished early
                await Task.Delay(1000, cancellationToken).ContinueWith(t => { });
            }
        }
        catch (TaskCanceledException)
        {
            // Expected during cancellation
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during test: {ex.Message}");
        }
        finally
        {
            stopwatchTotal.Stop();

            // Ensure pending entries are flushed
            try
            {
                await loggerUtilities.WaitUntilCacheIsEmptyAsync(timeout: TimeSpan.FromSeconds(10));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error flushing logs: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Adds a log entry and measures the time taken.
    /// </summary>
    /// <param name="counter">The sequence number for this entry.</param>
    private Task AddLogEntryWithTimingAsync(int counter)
    {
        var stopwatch = Stopwatch.StartNew();

        var logLevel = counter % 3 == 0 ? LogLevel.Warning :
                      counter % 7 == 0 ? LogLevel.Error :
                      LogLevel.Information;

        logger.Log(
            logLevel,
            message: "Image with illumination {illumination} has result {result}.",
            args: [MsgParamsGen.GetIllumination(), MsgParamsGen.GetResult()]);


        stopwatch.Stop();
        long elapsedMs = stopwatch.ElapsedMilliseconds;

        lock (lockObject)
        {
            totalEntriesAdded++;
            totalAddTimeMs += elapsedMs;
            minAddTimeMs = Math.Min(minAddTimeMs, elapsedMs);
            maxAddTimeMs = Math.Max(maxAddTimeMs, elapsedMs);
            addTimesMs.Add(elapsedMs);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Reports metrics periodically during the test.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation.</param>
    private async Task ReportMetricsAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Report every 5 seconds
                await Task.Delay(5000, cancellationToken);
                DisplayCurrentStatistics();
            }
        }
        catch (TaskCanceledException)
        {
            // Expected during cancellation
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reporting metrics: {ex.Message}");
        }
    }

    /// <summary>
    /// Displays current statistics for the test.
    /// </summary>
    private void DisplayCurrentStatistics()
    {
        long pendingEntries;
        long discardedEntries;
        int entriesAdded;
        double avgTime;
        long min, max;
        double p50, p90, p99;

        lock (lockObject)
        {
            entriesAdded = totalEntriesAdded;
            pendingEntries = loggerUtilities.PendingEntriesCount;
            discardedEntries = loggerUtilities.DiscardedEntriesCount;
            avgTime = totalEntriesAdded > 0 ? (double)totalAddTimeMs / totalEntriesAdded : 0;
            min = minAddTimeMs == long.MaxValue ? 0 : minAddTimeMs;
            max = maxAddTimeMs;

            // Calculate percentiles
            if (addTimesMs.Count > 0)
            {
                var sortedTimes = new List<long>(addTimesMs);
                sortedTimes.Sort();
                p50 = sortedTimes[(int)(sortedTimes.Count * 0.5)];
                p90 = sortedTimes[(int)(sortedTimes.Count * 0.9)];
                p99 = sortedTimes[(int)(sortedTimes.Count * 0.99)];
            }
            else
            {
                p50 = p90 = p99 = 0;
            }
        }

        var elapsedTime = stopwatchTotal.Elapsed;
        var entriesPerSecond = elapsedTime.TotalSeconds > 0 ? entriesAdded / elapsedTime.TotalSeconds : 0;

        Console.Clear();
        Console.WriteLine("==== SQLite Logger Soak Test ====");
        Console.WriteLine($"Running for: {elapsedTime:hh\\:mm\\:ss}");
        Console.WriteLine($"Entries added: {entriesAdded:N0} ({entriesPerSecond:N2}/sec)");
        Console.WriteLine($"Entries pending in cache: {pendingEntries:N0}, discarded entry count: {discardedEntries:N0}");
        Console.WriteLine();
        Console.WriteLine("Log Entry Add Performance:");
        Console.WriteLine($"  Min: {min} ms");
        Console.WriteLine($"  Max: {max} ms");
        Console.WriteLine($"  Avg: {avgTime:N2} ms");
        Console.WriteLine($"  P50: {p50} ms");
        Console.WriteLine($"  P90: {p90} ms");
        Console.WriteLine($"  P99: {p99} ms");
        Console.WriteLine();
        Console.WriteLine("Press any key to stop the test...");
    }

    /// <summary>
    /// Displays final statistics after the test completes.
    /// </summary>
    private void DisplayFinalStatistics()
    {
        Console.Clear();
        Console.WriteLine("==== SQLite Logger Soak Test Results ====");

        var elapsedTime = stopwatchTotal.Elapsed;
        int entriesAdded;
        long discardedEntries;
        double avgTime;
        long min, max;
        double p50 = 0, p90 = 0, p99 = 0; // Initialize with default values

        lock (lockObject)
        {
            entriesAdded = totalEntriesAdded;
            discardedEntries = loggerUtilities.DiscardedEntriesCount;
            avgTime = totalEntriesAdded > 0 ? (double)totalAddTimeMs / totalEntriesAdded : 0;
            min = minAddTimeMs == long.MaxValue ? 0 : minAddTimeMs;
            max = maxAddTimeMs;

            // Calculate percentiles only if we have data
            if (addTimesMs.Count > 0)
            {
                var sortedTimes = new List<long>(addTimesMs);
                sortedTimes.Sort();
                p50 = sortedTimes[(int)(sortedTimes.Count * 0.5)];
                p90 = sortedTimes[(int)(sortedTimes.Count * 0.9)];
                p99 = sortedTimes[(int)(sortedTimes.Count * 0.99)];

                // Calculate standard deviation
                double mean = sortedTimes.Average();
                double sumOfSquares = sortedTimes.Sum(time => Math.Pow(time - mean, 2));
                double stdDev = Math.Sqrt(sumOfSquares / sortedTimes.Count);
                Console.WriteLine($"Standard Deviation: {stdDev:N2} ms");
            }
        }

        var entriesPerSecond = elapsedTime.TotalSeconds > 0 ? entriesAdded / elapsedTime.TotalSeconds : 0;

        Console.WriteLine($"Test duration: {elapsedTime:hh\\:mm\\:ss}");
        Console.WriteLine($"Total entries added: {entriesAdded:N0}");
        Console.WriteLine($"Total entries discarded: {discardedEntries:N0}");
        Console.WriteLine($"Average throughput: {entriesPerSecond:N2} entries/second");
        Console.WriteLine();
        Console.WriteLine("Log Entry Add Performance:");
        Console.WriteLine($"  Minimum time: {min} ms");
        Console.WriteLine($"  Maximum time: {max} ms");
        Console.WriteLine($"  Average time: {avgTime:N2} ms");
        Console.WriteLine($"  Median (P50): {p50} ms");
        Console.WriteLine($"  90th percentile: {p90} ms");
        Console.WriteLine($"  99th percentile: {p99} ms");
        Console.WriteLine();
    }
}
