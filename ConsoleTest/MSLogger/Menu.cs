namespace ConsoleTest.MSLogger;

sealed class Menu
{
    public static void Run()
    {
        new CDS.CLIMenus.Basic.MenuBuilder("Default log entry")
                       
            .AddItem("Bread factory simulation", () => new BreadFactorySimulator.Factory().Run())
            
            .Build()
            .Run();
    }
}
