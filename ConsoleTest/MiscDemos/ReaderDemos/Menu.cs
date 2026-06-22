namespace ConsoleTest.ReaderDemos;

/// <summary>
/// Menu for reader demos.
/// </summary>
static class Menu
{
    /// <summary>
    /// Runs the reader demos sub-menu.
    /// </summary>
    public static void Run()
    {
        SpectreMenu.Run("Reader Demos",
            ("Database info", () => new DisplayDatabaseInfo().Run()),
            ("Get all entries", () => new GetAllEntriesDemo().Run()));
    }
}
