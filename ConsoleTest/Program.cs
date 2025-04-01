namespace ConsoleTest;

/// <summary>
/// Main program class.
/// </summary>
class Program
{


    /// <summary>
    /// Runs the main program logic.
    /// </summary>
    void Run()
    {
        try
        {

            new CDS.CLIMenus.Basic.MenuBuilder("Demos")
                .AddItem("Simplest demo (dependency injection)", DISimplestDemo.DemoRunner.Run)
                .AddItem("Writer demos (dependency injection)", new DIWriterDemos.Menu().Run)
                .AddItem("Writer demos (non DI)", new NonDIWriterDemos.Menu().Run)
                .AddItem("Reader demos", ReaderDemos.Menu.Run)
                .AddItem("Housekeeping demos", HousekeeperDemos.Menu.Run)
                .Build()
                .Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }


    /// <summary>
    /// Entry point of the program.
    /// </summary>
    static void Main()
    {
        new Program().Run();
    }
}
