namespace ConsoleTest.DIDemos;

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
        new CDS.CLIMenus.Basic.MenuBuilder("Dependency injection demos")
            .AddItem("Middleware demo", DIMiddlewareDemo.DemoRunner.Run)
            .AddItem("Simple demo", DISimplestDemo.DemoRunner.Run)
            .AddItem("Assorted writer demos", () => new DIWriterDemos.Menu().Run())
            .Build()
            .Run();
    }
}
