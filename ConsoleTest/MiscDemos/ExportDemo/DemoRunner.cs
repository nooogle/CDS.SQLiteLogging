using CDS.SQLiteLogging;
using Spectre.Console;

namespace ConsoleTest.ExportDemo;

/// <summary>
/// Demonstrates exporting log entries from one SQLite database to another.
/// Reads all entries from the live database and exports every other one to a new file.
/// </summary>
internal static class DemoRunner
{
    /// <summary>
    /// Runs the export demo.
    /// </summary>
    public static void Run()
    {
        AnsiConsole.Write(new Rule("[bold yellow]SQLite Log Export Demo[/]").LeftJustified());

        string dbPath = DBPathCreator.Create();

        if (!File.Exists(dbPath))
        {
            AnsiConsole.MarkupLine($"[red]Source database not found:[/] [grey]{Markup.Escape(dbPath)}[/]");
            AnsiConsole.MarkupLine("[grey]Run a logging demo first to generate log entries.[/]");
            return;
        }

        string exportPath = Path.Combine(
            Path.GetDirectoryName(dbPath) ?? string.Empty,
            "ExportedLog.db");

        var infoGrid = new Grid().AddColumn(new GridColumn().NoWrap()).AddColumn();
        infoGrid.AddRow("[bold]Source:[/]", Markup.Escape(dbPath));
        infoGrid.AddRow("[bold]Destination:[/]", Markup.Escape(exportPath));
        AnsiConsole.Write(new Panel(infoGrid).Border(BoxBorder.Rounded));

        if (File.Exists(exportPath)) { File.Delete(exportPath); }

        try
        {
            // Read source entries
            long[] everyOtherDbId = [];
            int totalSourceCount = 0;

            AnsiConsole.Status().Start("Reading source database...", _ =>
            {
                using var reader = new Reader(dbPath);
                var allEntries = reader.GetAllEntries();
                totalSourceCount = allEntries.Count;
                everyOtherDbId = allEntries
                    .Where((_, index) => index % 2 == 0)
                    .Select(e => e.DbId)
                    .ToArray();
            });

            if (totalSourceCount == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No log entries found in the source database.[/]");
                AnsiConsole.MarkupLine("[grey]Run a logging demo first to generate log entries.[/]");
                return;
            }

            AnsiConsole.MarkupLine($"Found [bold]{totalSourceCount:N0}[/] entries — exporting every other one ([bold]{everyOtherDbId.Length:N0}[/] total).");

            // Export
            var sw = System.Diagnostics.Stopwatch.StartNew();
            AnsiConsole.Status().Start("Exporting...", _ =>
            {
                Exporter.Export(
                    dbFileNameSource: dbPath,
                    dbFileNameDestination: exportPath,
                    idsToExport: everyOtherDbId);
            });
            sw.Stop();

            // Verify
            int exportedCount;
            IReadOnlyList<LogEntry> sample = [];

            using (var exportedReader = new Reader(exportPath))
            {
                exportedCount = exportedReader.GetEntryCount();
                sample = exportedReader.GetRecentEntries(5);
            }

            bool success = exportedCount == everyOtherDbId.Length;

            // Summary panel
            var summaryGrid = new Grid().AddColumn(new GridColumn().NoWrap()).AddColumn();
            summaryGrid.AddRow("[bold]Expected entries:[/]", $"{everyOtherDbId.Length:N0}");
            summaryGrid.AddRow("[bold]Actual entries:[/]", $"{exportedCount:N0}");
            summaryGrid.AddRow("[bold]Duration:[/]", $"{sw.Elapsed.TotalSeconds:F2} s");
            summaryGrid.AddRow(
                "[bold]Status:[/]",
                success ? "[green]SUCCESS ✓[/]" : "[red]FAILED ✗[/]");

            AnsiConsole.Write(new Panel(summaryGrid)
                .Header("[yellow]Export Summary[/]")
                .Border(BoxBorder.Rounded));

            // Sample table
            if (sample.Count > 0)
            {
                var table = new Table()
                    .Title("Sample Exported Entries (most recent first)")
                    .AddColumn("[bold]Time[/]")
                    .AddColumn("[bold]Level[/]")
                    .AddColumn("[bold]Message[/]")
                    .Border(TableBorder.Rounded);

                foreach (var entry in sample)
                {
                    table.AddRow(
                        entry.Timestamp.ToLocalTime().ToString("HH:mm:ss.fff"),
                        LevelMarkup(entry.Level),
                        Markup.Escape(entry.RenderedMessage));
                }

                AnsiConsole.Write(table);
            }
        }
        catch (InvalidOperationException ex)
        {
            AnsiConsole.MarkupLine($"[yellow]Warning:[/] {Markup.Escape(ex.Message)}");
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }
    }

    private static string LevelMarkup(Microsoft.Extensions.Logging.LogLevel level) => level switch
    {
        Microsoft.Extensions.Logging.LogLevel.Trace => "[grey]Trace[/]",
        Microsoft.Extensions.Logging.LogLevel.Debug => "[grey]Debug[/]",
        Microsoft.Extensions.Logging.LogLevel.Information => "[green]Info[/]",
        Microsoft.Extensions.Logging.LogLevel.Warning => "[yellow]Warn[/]",
        Microsoft.Extensions.Logging.LogLevel.Error => "[red bold]Error[/]",
        Microsoft.Extensions.Logging.LogLevel.Critical => "[red bold on white]CRIT[/]",
        _ => Markup.Escape(level.ToString())
    };
}
