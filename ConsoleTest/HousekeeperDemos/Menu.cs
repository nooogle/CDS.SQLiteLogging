namespace ConsoleTest.HousekeeperDemos;

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
        new CDS.CLIMenus.Basic.MenuBuilder("Housekeeping")
            .AddItem("Delete all", () => new DeleteAllDemo().Run())
            .Build()
            .Run();
    }
}
