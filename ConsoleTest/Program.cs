namespace ConsoleTest;

sealed class Menu
{
    static void Main()
    {
        new CDS.CLIMenus.Basic.MenuBuilder("Tests")

            .AddItem("Custom log entry demos", CustomLogEntry.Menu.Run)

            .AddItem("Default log entry demos", DefaultLogEntry.Menu.Run)

            .AddItem("MS Logger demos", MSLogger.Menu.Run)

            .Build()
            .Run();
    }
}
