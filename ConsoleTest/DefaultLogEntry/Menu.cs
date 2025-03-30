namespace ConsoleTest.DefaultLogEntry;

sealed class Menu
{
    public static void Run()
    {
        new CDS.CLIMenus.Basic.MenuBuilder("Default log entry")
                       
            .AddItem("Test", BuiltInLogEntryDemo.Run)
            
            .Build()
            .Run();
    }
}
