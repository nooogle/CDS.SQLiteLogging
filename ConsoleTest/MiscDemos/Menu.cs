namespace ConsoleTest.MiscDemos;

/// <summary>
/// Menu for miscellaneous demos.
/// </summary>
static class Menu
{
    /// <summary>
    /// Runs the miscellaneous demos sub-menu.
    /// </summary>
    public static void Run()
    {
        SpectreMenu.Run("Miscellaneous Demos",
            ("Export demo", ExportDemo.DemoRunner.Run),
            ("Housekeeping demos", HousekeeperDemos.Menu.Run),
            ("Reader demos", ReaderDemos.Menu.Run));
    }
}
