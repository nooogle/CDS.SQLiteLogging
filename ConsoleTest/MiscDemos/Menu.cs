namespace ConsoleTest.MiscDemos;

/// <summary>
/// Menu for running SQLite logging demos without dependency injection.
/// </summary>
static class Menu
{
    /// <summary>
    /// Runs the main program logic by setting up a logger and presenting demo options.
    /// </summary>
    public static void Run()
    {
        new CDS.CLIMenus.Basic.MenuBuilder("Miscellaneous demos")
            .AddItem("Export demo", ExportDemo.DemoRunner.Run)
            .AddItem("Housekeeping demos", HousekeeperDemos.Menu.Run)
            .AddItem("Reader demos", ReaderDemos.Menu.Run)
            .Build()
            .Run();
    }
}
