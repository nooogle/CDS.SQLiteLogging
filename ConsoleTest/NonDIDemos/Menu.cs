namespace ConsoleTest.NonDIDemos;

/// <summary>
/// Menu for non-DI logging demos.
/// </summary>
static class Menu
{
    /// <summary>
    /// Runs the non-DI demos sub-menu.
    /// </summary>
    public static void Run()
    {
        SpectreMenu.Run("Non-DI Demos (direct creation — no dependency injection)",
            ("Simplest demo", () => new SimplestWriterDemo().Run()),
            ("Log levels", () => new LogLevelsDemo().Run()));
    }
}
