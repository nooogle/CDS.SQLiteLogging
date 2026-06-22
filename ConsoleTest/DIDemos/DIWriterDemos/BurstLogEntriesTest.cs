using CDS.SQLiteLogging;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.Diagnostics;

namespace ConsoleTest.DIWriterDemos;

/// <summary>
/// Writes a configurable burst of log entries and reports throughput.
/// </summary>
class BurstLogEntriesTest(ILogger<BurstLogEntriesTest> logger, ISQLiteWriterUtilities loggerUtilities)
{
    private readonly ILogger<BurstLogEntriesTest> logger = logger;
    private readonly ISQLiteWriterUtilities loggerUtilities = loggerUtilities;

    /// <summary>
    /// Runs the burst test.
    /// </summary>
    public void Run()
    {
        AnsiConsole.Write(new Rule("[bold yellow]Burst Log Entries Test[/]").LeftJustified());

        var numberOfEntries = AnsiConsole.Prompt(
            new TextPrompt<int>("Number of log entries to add:")
                .DefaultValue(10_000)
                .Validate(v => v > 0
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Must be > 0[/]")));

        var stopwatch = Stopwatch.StartNew();
        int addedCount = 0;

        AnsiConsole.Progress()
            .AutoClear(false)
            .HideCompleted(false)
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new RemainingTimeColumn(),
                new SpinnerColumn())
            .Start(ctx =>
            {
                var task = ctx.AddTask("[green]Writing log entries[/]", maxValue: numberOfEntries);

                for (int i = 0; i < numberOfEntries; i++)
                {
                    logger.LogInformation(
                        "Image with illumination {illumination} has result {result}",
                        MsgParamsGen.GetIllumination(), MsgParamsGen.GetResult());

                    addedCount++;
                    task.Increment(1);
                }
            });

        stopwatch.Stop();

        AnsiConsole.MarkupLine("[grey]Flushing cached entries to database...[/]");
        var flushSw = Stopwatch.StartNew();
        loggerUtilities.WaitUntilCacheIsEmpty(timeout: TimeSpan.FromSeconds(10));
        flushSw.Stop();

        var resultsTable = new Table()
            .AddColumn("Metric")
            .AddColumn(new TableColumn("Value").RightAligned())
            .Border(TableBorder.Rounded);

        resultsTable.AddRow("Entries added", $"{addedCount:N0}");
        resultsTable.AddRow("Write time", $"{stopwatch.Elapsed.TotalSeconds:F2} s");
        resultsTable.AddRow("Flush time", $"{flushSw.Elapsed.TotalSeconds:F2} s");
        resultsTable.AddRow("Average rate", $"{addedCount / stopwatch.Elapsed.TotalSeconds:N0} entries/sec");

        AnsiConsole.Write(new Panel(resultsTable)
            .Header("[yellow]Results[/]")
            .Border(BoxBorder.Rounded));
    }
}
