using CDS.SQLiteLogging;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.Diagnostics;

namespace ConsoleTest.DIWriterDemos;

/// <summary>
/// Runs a long-running soak test that adds log entries at a fixed rate and
/// displays live performance statistics.
/// </summary>
class LoggerSoakTest
{
    private readonly ILogger logger;
    private readonly ISQLiteWriterUtilities loggerUtilities;
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    private readonly List<long> addTimesMs = [];
    private readonly Stopwatch stopwatchTotal = new Stopwatch();
    private readonly object lockObject = new object();
    private int totalEntriesAdded;
    private int entriesPerSecond;
    private long minAddTimeMs = long.MaxValue;
    private long maxAddTimeMs;
    private long totalAddTimeMs;
    private int lastHousekeepingDeletedCount;
    private long lastDbFileSizeBytes;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggerSoakTest"/> class.
    /// </summary>
    public LoggerSoakTest(ILogger<LoggerSoakTest> logger, ISQLiteWriterUtilities loggerUtilities)
    {
        this.logger = logger;
        this.loggerUtilities = loggerUtilities;
    }

    /// <summary>
    /// Runs the soak test until the user presses a key.
    /// </summary>
    public void Run()
    {
        AnsiConsole.Write(new Rule("[bold yellow]SQLite Logger Soak Test[/]").LeftJustified());

        entriesPerSecond = AnsiConsole.Prompt(
            new TextPrompt<int>("Entries per second:")
                .DefaultValue(10)
                .Validate(v => v > 0
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Must be > 0[/]")));

        AnsiConsole.MarkupLine("[grey]Housekeeping runs every 30 s  |  Press any key to stop[/]\n");

        Task.Run(() => RunTestAsync(cancellationTokenSource.Token));

        // Live display — runs on the main thread, polls every second for key press
        var statsTable = BuildStatsTable();
        var hkTimer = Stopwatch.StartNew();

        AnsiConsole.Live(statsTable)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Start(ctx =>
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    if (Console.KeyAvailable)
                    {
                        Console.ReadKey(true);
                        cancellationTokenSource.Cancel();
                        break;
                    }

                    if (hkTimer.Elapsed.TotalSeconds >= 30)
                    {
                        lastHousekeepingDeletedCount = loggerUtilities.ExecuteHousekeeping();
                        lastDbFileSizeBytes = loggerUtilities.GetDatabaseFileSize();
                        hkTimer.Restart();
                    }

                    RefreshStatsTable(statsTable);
                    ctx.Refresh();
                    Thread.Sleep(1000);
                }
            });

        // Grace period for the background test task to finish flushing
        Thread.Sleep(1200);

        DisplayFinalStatistics();
    }

    private Table BuildStatsTable()
    {
        return new Table()
            .Title("[bold yellow]Live Statistics[/]")
            .AddColumn("[bold]Metric[/]")
            .AddColumn(new TableColumn("[bold]Value[/]").RightAligned())
            .Border(TableBorder.Rounded);
    }

    private void RefreshStatsTable(Table table)
    {
        int entriesAdded;
        long pendingEntries, discardedEntries, min, max;
        double avgTime;
        (double p50, double p90, double p99) percentiles;

        lock (lockObject)
        {
            entriesAdded = totalEntriesAdded;
            pendingEntries = loggerUtilities.PendingEntriesCount;
            discardedEntries = loggerUtilities.DiscardedEntriesCount;
            avgTime = totalEntriesAdded > 0 ? (double)totalAddTimeMs / totalEntriesAdded : 0;
            min = minAddTimeMs == long.MaxValue ? 0 : minAddTimeMs;
            max = maxAddTimeMs;
            percentiles = CalculatePercentiles();
        }

        var elapsed = stopwatchTotal.Elapsed;
        var throughput = elapsed.TotalSeconds > 0 ? entriesAdded / elapsed.TotalSeconds : 0;

        table.Rows.Clear();
        table.AddRow("[bold]Running time[/]", elapsed.ToString(@"hh\:mm\:ss"));
        table.AddRow("[bold]Entries added[/]", $"{entriesAdded:N0} ({throughput:N1}/sec)");
        table.AddRow("[bold]Pending in cache[/]", $"{pendingEntries:N0}");
        table.AddRow("[bold]Discarded[/]", discardedEntries > 0 ? $"[yellow]{discardedEntries:N0}[/]" : "0");
        table.AddRow("[bold]DB file size[/]", $"{lastDbFileSizeBytes / 1024.0 / 1024.0:N2} MB");
        table.AddRow("[bold]Last HK deleted[/]", $"{lastHousekeepingDeletedCount:N0}");
        table.AddRow(string.Empty, string.Empty);
        table.AddRow("[bold]Min add time[/]", $"{min} ms");
        table.AddRow("[bold]Max add time[/]", $"{max} ms");
        table.AddRow("[bold]Avg add time[/]", $"{avgTime:N2} ms");
        table.AddRow("[bold]P50[/]", $"{percentiles.p50:N0} ms");
        table.AddRow("[bold]P90[/]", $"{percentiles.p90:N0} ms");
        table.AddRow("[bold]P99[/]", $"{percentiles.p99:N0} ms");
    }

    private void DisplayFinalStatistics()
    {
        AnsiConsole.Write(new Rule("[bold yellow]Final Results[/]").LeftJustified());

        var elapsed = stopwatchTotal.Elapsed;
        int entriesAdded;
        long discardedEntries, min, max;
        double avgTime, stdDev = 0;
        (double p50, double p90, double p99) percentiles;

        lock (lockObject)
        {
            entriesAdded = totalEntriesAdded;
            discardedEntries = loggerUtilities.DiscardedEntriesCount;
            avgTime = totalEntriesAdded > 0 ? (double)totalAddTimeMs / totalEntriesAdded : 0;
            min = minAddTimeMs == long.MaxValue ? 0 : minAddTimeMs;
            max = maxAddTimeMs;
            percentiles = CalculatePercentiles();

            if (addTimesMs.Count > 0)
            {
                double mean = addTimesMs.Average();
                stdDev = Math.Sqrt(addTimesMs.Sum(t => Math.Pow(t - mean, 2)) / addTimesMs.Count);
            }
        }

        var throughput = elapsed.TotalSeconds > 0 ? entriesAdded / elapsed.TotalSeconds : 0;

        var table = new Table()
            .AddColumn("[bold]Metric[/]")
            .AddColumn(new TableColumn("[bold]Value[/]").RightAligned())
            .Border(TableBorder.Rounded);

        table.AddRow("Test duration", elapsed.ToString(@"hh\:mm\:ss"));
        table.AddRow("Total entries added", $"{entriesAdded:N0}");
        table.AddRow("Discarded entries", discardedEntries > 0 ? $"[yellow]{discardedEntries:N0}[/]" : "[green]0[/]");
        table.AddRow("Average throughput", $"{throughput:N2} entries/sec");
        table.AddRow(string.Empty, string.Empty);
        table.AddRow("Min add time", $"{min} ms");
        table.AddRow("Max add time", $"{max} ms");
        table.AddRow("Avg add time", $"{avgTime:N2} ms");
        table.AddRow("Std deviation", $"{stdDev:N2} ms");
        table.AddRow("P50 (median)", $"{percentiles.p50:N0} ms");
        table.AddRow("P90", $"{percentiles.p90:N0} ms");
        table.AddRow("P99", $"{percentiles.p99:N0} ms");

        AnsiConsole.Write(table);
    }

    private (double p50, double p90, double p99) CalculatePercentiles()
    {
        if (addTimesMs.Count == 0) { return (0, 0, 0); }

        var sorted = new List<long>(addTimesMs);
        sorted.Sort();
        return (
            sorted[(int)(sorted.Count * 0.50)],
            sorted[(int)(sorted.Count * 0.90)],
            sorted[(int)(sorted.Count * 0.99)]);
    }

    private async Task RunTestAsync(CancellationToken cancellationToken)
    {
        int counter = 0;

        try
        {
            await WarmUpAsync(cancellationToken);

            stopwatchTotal.Start();

            while (!cancellationToken.IsCancellationRequested)
            {
                for (int i = 0; i < entriesPerSecond && !cancellationToken.IsCancellationRequested; i++)
                {
                    counter++;
                    AddLogEntryWithTiming(counter);
                }

                await Task.Delay(1000, cancellationToken).ContinueWith(_ => { });
            }
        }
        catch (TaskCanceledException) { }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error during test:[/] {Markup.Escape(ex.Message)}");
        }
        finally
        {
            stopwatchTotal.Stop();

            try
            {
                await loggerUtilities.WaitUntilCacheIsEmptyAsync(timeout: TimeSpan.FromSeconds(10));
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error flushing logs:[/] {Markup.Escape(ex.Message)}");
            }
        }
    }

    private async Task WarmUpAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        logger.LogInformation("Warm-up log entry for soak test.");
        await loggerUtilities.WaitUntilCacheIsEmptyAsync(timeout: TimeSpan.FromSeconds(10));
    }

    private void AddLogEntryWithTiming(int counter)
    {
        var sw = Stopwatch.StartNew();

        var level = counter % 3 == 0 ? LogLevel.Warning :
                    counter % 7 == 0 ? LogLevel.Error :
                    LogLevel.Information;

        logger.Log(level,
            message: "Image with illumination {illumination} has result {result}.",
            args: [MsgParamsGen.GetIllumination(), MsgParamsGen.GetResult()]);

        sw.Stop();

        lock (lockObject)
        {
            totalEntriesAdded++;
            totalAddTimeMs += sw.ElapsedMilliseconds;
            minAddTimeMs = Math.Min(minAddTimeMs, sw.ElapsedMilliseconds);
            maxAddTimeMs = Math.Max(maxAddTimeMs, sw.ElapsedMilliseconds);
            addTimesMs.Add(sw.ElapsedMilliseconds);
        }
    }
}
