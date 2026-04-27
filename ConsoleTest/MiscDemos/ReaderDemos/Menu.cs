namespace ConsoleTest.ReaderDemos;

/// <summary>
/// Menu for running the demos.
/// </summary>
static class Menu
{
    /// <summary>
    /// Runs the main program logic.
    /// </summary>
    public static void Run()
    {
        new CDS.CLIMenus.Basic.MenuBuilder("Demos")
            .AddItem("Database info", () => new DisplayDatabaseInfo().Run())
            .AddItem("Get all entries", () => new GetAllEntriesDemo().Run())
            .Build()
            .Run();
    }
}
