using CDS.SQLiteLogging;

namespace ConsoleTest.ReaderDemos;

/// <summary>
/// Provides functionality to display information about the SQLite database.
/// </summary>
internal class DisplayDatabaseInfo
{
    /// <summary>
    /// Runs the process to display database information.
    /// </summary>
    public void Run()
    {
        // Open the database using SQLiteReader
        using var sqliteReader = new Reader(DBPathCreator.Create());

        // Display the number of entries in the database
        var numEntries = sqliteReader.GetEntryCount();
        Console.WriteLine($"Number of entries: {numEntries}");

        // Display the database file size in MB
        var fileSizeMB = sqliteReader.GetDatabaseFileSize() / 1024.0 / 1024.0;
        Console.WriteLine($"Database filesize: {fileSizeMB:F2} MB");
    }
}
