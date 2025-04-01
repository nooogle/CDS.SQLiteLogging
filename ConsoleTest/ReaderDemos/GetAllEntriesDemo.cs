using CDS.SQLiteLogging;
using Newtonsoft.Json;

namespace ConsoleTest.ReaderDemos;

/// <summary>
/// Demonstrates how to retrieve and display all log entries from an SQLite database.
/// </summary>
internal class GetAllEntriesDemo
{
    /// <summary>
    /// Runs the process to retrieve and display all log entries.
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
        using var sqliteReader = new SQLiteReader(filename);

        // Display the number of entries in the database
        var numEntries = sqliteReader.GetNumberOfEntries();
        Console.WriteLine($"Number of entries: {numEntries}");

        // Retrieve and display all log entries
        var allEntries = sqliteReader.GetAllEntries();
        allEntries.ForEach(DisplayLogEntry);
    }

    /// <summary>
    /// Displays a log entry in a formatted manner.
    /// </summary>
    /// <param name="entry">The log entry to display.</param>
    private void DisplayLogEntry(LogEntry entry)
    {
        // Deserialize the scopes JSON to a dictionary
        var scopes = entry.DeserialiseScopesJson();

        // Convert the scopes dictionary to a single line string
        var scopesAsSingleLineString = string.Join(", ", scopes.Select(kv => $"{kv.Key} = {kv.Value}"));

        // Display the log entry with its scopes
        Console.WriteLine($"{entry} (scope: {scopesAsSingleLineString})");

        // Display exception info
        if(!string.IsNullOrEmpty(entry.ExceptionJson))
        {
            var exception = entry.GetExceptionInfo();
            Console.WriteLine($"Exception: {exception}");
            Console.WriteLine();
        }
    }
}
