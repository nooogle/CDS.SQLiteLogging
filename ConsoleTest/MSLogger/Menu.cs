namespace ConsoleTest.MSLogger;

sealed class Menu
{
    public static void Run()
    {
        new CDS.CLIMenus.Basic.MenuBuilder("Default log entry")
                       
            .AddItem("Test", Test1.Run)
            
            .Build()
            .Run();
    }
}
