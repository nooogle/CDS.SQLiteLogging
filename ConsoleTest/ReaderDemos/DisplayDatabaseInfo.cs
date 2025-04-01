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
        // Get the filename of the database from the user
        Console.WriteLine("Enter the filename of the database:");
        string filename = Console.ReadLine()?.Trim('\"') ?? string.Empty;

        // Check if the file exists
        if (!File.Exists(filename))
        {
            Console.WriteLine("File not found.");
            return;
        }

        // Open the database using SQLiteReader
        using var sqliteReader = new CDS.SQLiteLogging.SQLiteReader(filename);

        // Display the number of entries in the database
        var numEntries = sqliteReader.GetNumberOfEntries();
        Console.WriteLine($"Number of entries: {numEntries}");

        // Display the database file size in MB
        var fileSizeMB = sqliteReader.GetDatabaseFileSize() / 1024.0 / 1024.0;
        Console.WriteLine($"Database filesize: {fileSizeMB:F2} MB");
    }
}
