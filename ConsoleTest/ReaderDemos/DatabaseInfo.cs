namespace ConsoleTest.ReaderDemos;

internal class DatabaseInfo
{
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

        // Open the database
        CDS.SQLiteLogging.SQLiteReader sqliteReader = new CDS.SQLiteLogging.SQLiteReader(filename);

        // Display the number of entries
        var numEntries = sqliteReader.GetNumberOfEntries();
        Console.WriteLine($"Number of entries: {numEntries}");

        // display the database filesize in MB
        var fileSizeMB = sqliteReader.GetDatabaseFileSize() / 1024.0 / 1024.0;
        Console.WriteLine($"Database filesize: {fileSizeMB:F2} MB");
    }
}
