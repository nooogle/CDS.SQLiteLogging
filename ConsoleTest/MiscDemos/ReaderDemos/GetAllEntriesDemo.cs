using CDS.SQLiteLogging;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace ConsoleTest.ReaderDemos;

/// <summary>
/// Retrieves and displays all log entries from the database in a formatted table.
/// </summary>
internal class GetAllEntriesDemo
{
    /// <summary>
    /// Runs the demo.
    /// </summary>
    public void Run()
    {
        AnsiConsole.Write(new Rule("[bold yellow]All Log Entries[/]").LeftJustified());

        using var sqliteReader = new Reader(DBPathCreator.Create());

        var count = sqliteReader.GetEntryCount();
        AnsiConsole.MarkupLine($"Total entries in database: [bold]{count:N0}[/]\n");

        var allEntries = sqliteReader.GetAllEntries();

        if (allEntries.IsEmpty)
        {
            AnsiConsole.MarkupLine("[grey]No entries found. Run a logging demo first.[/]");
            return;
        }

        var table = new Table()
            .AddColumn("[bold]Time[/]")
            .AddColumn("[bold]Level[/]")
            .AddColumn("[bold]Message[/]")
            .AddColumn("[bold]Scopes[/]")
            .Border(TableBorder.Rounded);

        foreach (var entry in allEntries)
        {
            var scopes = entry.DeserialiseScopesJson();
            var scopeStr = scopes.Count > 0
                ? string.Join(", ", scopes.Select(kv => $"{kv.Key}={kv.Value}"))
                : string.Empty;

            table.AddRow(
                entry.Timestamp.ToLocalTime().ToString("HH:mm:ss.fff"),
                LevelMarkup(entry.Level),
                Markup.Escape(entry.RenderedMessage),
                Markup.Escape(scopeStr));

            if (!string.IsNullOrEmpty(entry.ExceptionJson))
            {
                var ex = entry.GetExceptionInfo();
                table.AddRow(
                    string.Empty,
                    "[red]^ exception[/]",
                    Markup.Escape(ex?.Message ?? string.Empty),
                    string.Empty);
            }
        }

        AnsiConsole.Write(table);
    }

    private static string LevelMarkup(LogLevel level) => level switch
    {
        LogLevel.Trace => "[grey]Trace[/]",
        LogLevel.Debug => "[grey]Debug[/]",
        LogLevel.Information => "[green]Info[/]",
        LogLevel.Warning => "[yellow]Warn[/]",
        LogLevel.Error => "[red bold]Error[/]",
        LogLevel.Critical => "[red bold on white]CRIT[/]",
        _ => Markup.Escape(level.ToString())
    };
}
