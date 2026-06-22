using ConsoleTest.HousekeeperDemos;

namespace ConsoleTest.MiscDemos.HousekeeperDemos;

/// <summary>
/// Menu for housekeeping demos.
/// </summary>
static class Menu
{
    /// <summary>
    /// Runs the housekeeping demos sub-menu.
    /// </summary>
    public static void Run()
    {
        SpectreMenu.Run("Housekeeping Demos",
            ("Delete all entries", () => new DeleteAllDemo().Run()));
    }
}
