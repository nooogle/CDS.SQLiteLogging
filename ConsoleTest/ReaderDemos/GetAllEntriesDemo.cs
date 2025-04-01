namespace ConsoleTest.ReaderDemos;

internal class GetAllEntriesDemo
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

        CDS.SQLiteLogging.SQLiteReader sqliteReader = new CDS.SQLiteLogging.SQLiteReader(filename);

        // Display the number of entries
        var numEntries = sqliteReader.GetNumberOfEntries();
        Console.WriteLine($"Number of entries: {numEntries}");

        // Display all entries
        sqliteReader.GetAllEntries().ForEach(e => Console.WriteLine(e));
    }
}
