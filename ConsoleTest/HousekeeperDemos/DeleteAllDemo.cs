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
        // create the housekeeping options
        CDS.SQLiteLogging.HouseKeepingOptions options = new()
        {
            Mode = CDS.SQLiteLogging.HousekeepingMode.Manual,
        };

        // create the housekeeper
        using var connectionManager = new CDS.SQLiteLogging.ConnectionManager(DBPathCreator.Create());
        using var housekeeper = new CDS.SQLiteLogging.Housekeeper(connectionManager, options, new CDS.SQLiteLogging.DefaultDateTimeProvider());

        // delete all log entries
        int numberOfRecordsDeleted = housekeeper.DeleteAll();
        Console.WriteLine($"{numberOfRecordsDeleted} records deleted.");
    }
}
