namespace ConsoleTest.HousekeeperDemos;


/// <summary>
/// Deletes all log entries from the database.
/// </summary>
internal class DeleteAllDemo
{
    /// <summary>
    /// Runs the demo.
    /// </summary>
    public void Run()
    {
        // get filename of the database
        Console.WriteLine("Enter the filename of the database:");
        string filename = Console.ReadLine()?.Trim('\"') ?? string.Empty;
        if(!File.Exists(filename))
        {
            Console.WriteLine("File not found.");
            return;
        }


        // create the housekeeping options
        CDS.SQLiteLogging.HouseKeepingOptions options = new()
        {
            Mode = CDS.SQLiteLogging.HousekeepingMode.Manual,
        };

        // create the housekeeper
        using CDS.SQLiteLogging.ConnectionManager connectionManager = new(filename);
        using CDS.SQLiteLogging.Housekeeper housekeeper = new(connectionManager, options);


        // delete all log entries
        int numberOfRecordsDeleted = housekeeper.DeleteAll();
        Console.WriteLine($"{numberOfRecordsDeleted} records deleted.");
    }
}
