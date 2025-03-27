namespace ConsoleTest;

sealed class Program
{
    static void Main()
    {
        new CDS.CLIMenus.Basic.MenuBuilder("Tests")
            
            .AddItem(
            "Built-in LogEntry Demo",
            "Demonstrates the built-in log entry class. This doesn't require any custom log entry class. Extensions to the " +
            $"main {nameof(CDS.SQLiteLogging.Logger<CDS.SQLiteLogging.LogEntry>)} class allow for simple AddXXX method calls to be used, without needing to " +
            $"first create LogEntry instances.",
            BuiltInLogEntryDemo.Run)
            
            .AddItem("Basic SQLite Logger Test", AdHocTests.RunBasicTest)
            
            .AddItem("Soak Test", LoggerSoakTest.RunFromConsole)
            
            .AddItem("Burst Log Entries Test", BurstLogEntriesTest.Run)
            
            .Build()
            .Run();
    }
}
