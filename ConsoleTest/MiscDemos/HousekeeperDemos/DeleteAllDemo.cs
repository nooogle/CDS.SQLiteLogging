using Spectre.Console;

namespace ConsoleTest.HousekeeperDemos;

/// <summary>
/// Deletes all log entries from the database.
/// </summary>
internal class DeleteAllDemo
{
    /// <summary>
    /// Runs the demo.
    /// </summary>
    public void Run()
    {
        AnsiConsole.Write(new Rule("[bold yellow]Delete All Log Entries[/]").LeftJustified());

        CDS.SQLiteLogging.HouseKeepingOptions options = new()
        {
            Mode = CDS.SQLiteLogging.HousekeepingMode.Manual,
        };

        using var housekeeper = new CDS.SQLiteLogging.Housekeeper(
            DBPathCreator.Create(), options, new CDS.SQLiteLogging.DefaultDateTimeProvider());

        int deleted = housekeeper.DeleteAll();

        if (deleted == 0)
        {
            AnsiConsole.MarkupLine("[grey]No entries to delete.[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[green]{deleted:N0}[/] record(s) deleted.");
        }
    }
}
