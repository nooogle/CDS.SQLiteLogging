using CDS.SQLiteLogging;
using Spectre.Console;

namespace ConsoleTest.ReaderDemos;

/// <summary>
/// Displays summary information about the SQLite log database.
/// </summary>
internal class DisplayDatabaseInfo
{
    /// <summary>
    /// Runs the demo.
    /// </summary>
    public void Run()
    {
        AnsiConsole.Write(new Rule("[bold yellow]Database Info[/]").LeftJustified());

        string dbPath = DBPathCreator.Create();

        if (!File.Exists(dbPath))
        {
            AnsiConsole.MarkupLine("[yellow]Database does not exist yet.[/]");
            AnsiConsole.MarkupLine($"[grey]Expected path: {Markup.Escape(dbPath)}[/]");
            return;
        }

        using var reader = new Reader(dbPath);

        var numEntries = reader.GetEntryCount();
        var fileSizeMB = reader.GetDatabaseFileSize() / 1024.0 / 1024.0;

        var grid = new Grid()
            .AddColumn(new GridColumn().NoWrap())
            .AddColumn();

        grid.AddRow("[bold]Path:[/]", Markup.Escape(dbPath));
        grid.AddRow("[bold]Entries:[/]", $"{numEntries:N0}");
        grid.AddRow("[bold]File size:[/]", $"{fileSizeMB:F2} MB");

        AnsiConsole.Write(new Panel(grid)
            .Header("[yellow]SQLite Database[/]")
            .Border(BoxBorder.Rounded));
    }
}
