namespace ConsoleTest.DIDemos;

/// <summary>
/// Menu for dependency-injection logging demos.
/// </summary>
static class Menu
{
    /// <summary>
    /// Runs the DI demos sub-menu.
    /// </summary>
    public static void Run()
    {
        SpectreMenu.Run("Dependency Injection Demos",
            ("Middleware demo", DIMiddlewareDemo.DemoRunner.Run),
            ("Simplest DI demo", DISimplestDemo.DemoRunner.Run),
            ("Assorted writer demos", () => new DIWriterDemos.Menu().Run()));
    }
}
