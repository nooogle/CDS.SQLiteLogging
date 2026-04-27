namespace ConsoleTest.NonDIDemos;

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
        new CDS.CLIMenus.Basic.MenuBuilder("Non-DI demos (direct creation - not using dependency injection)")
            .AddItem("Simplest demo", () => new SimplestWriterDemo().Run())
            .AddItem("Log levels", () => new LogLevelsDemo().Run())
            .Build()
            .Run();
    }
}
