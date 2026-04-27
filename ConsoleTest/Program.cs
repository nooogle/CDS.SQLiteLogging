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
                .AddItem("Non-DI demos", NonDIDemos.Menu.Run)
                .AddItem("DI demos", DIDemos.Menu.Run)
                .AddItem("Misc demo", MiscDemos.Menu.Run)
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
