using Spectre.Console;

namespace ConsoleTest;

/// <summary>
/// Main program class.
/// </summary>
class Program
{
    void Run()
    {
        AnsiConsole.Write(
            new FigletText("CDS SQLiteLogging")
                .LeftJustified()
                .Color(Color.Yellow));

        AnsiConsole.MarkupLine("[grey]Interactive demos for the CDS.SQLiteLogging library[/]");

        try
        {
            SpectreMenu.RunRoot("Main Menu",
                ("Non-DI demos", NonDIDemos.Menu.Run),
                ("DI demos", DIDemos.Menu.Run),
                ("Misc demos", MiscDemos.Menu.Run));
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }
    }

    static void Main()
    {
        new Program().Run();
    }
}
