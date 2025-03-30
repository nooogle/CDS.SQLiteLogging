using CDS.SQLiteLogging;
using ConsoleTest.CustomLogEntry;

namespace ConsoleTest.DefaultLogEntry;

/// <summary>
/// Contains ad-hoc tests for the SQLite Logger.
/// </summary>
static class BuiltInLogEntryDemo
{
    /// <summary>
    /// Runs a basic test of the SQLite Logger.
    /// </summary>
    public static void Run()
    {
        // Display the test header
        Console.Clear();
        Console.WriteLine("=== Built-in logger demo ===\n");

        // Get the standard log folder
        string folder = LogFolderManager.GetLogFolder(testName: nameof(BuiltInLogEntryDemo));
        Console.WriteLine($"Using log folder: {folder}");

        // Create a new instance of the SQLite Logger class
        using var logger = new SQLiteLogger<LogEntry>(
            folder,
            schemaVersion: LogEntry.Version,
            new BatchingOptions(),
            new HouseKeepingOptions());

        // Clear existing entries (optional)
        int deletedCount = logger.DeleteAll();
        Console.WriteLine($"Deleted {deletedCount} existing entries.");
        Console.WriteLine("\nAdding log entries...");

        // Add the simplest log entry possible!
        logger.AddInformation("This is an information message.");

        // Add structured log entries
        logger.AddInformation("This is a structured message. Person {Name} has age {Age}.", new("Name", "Alice"), new("Age", 25));
        logger.AddInformation("This is a structured message. Person {Name} has age {Age}.", new("Name", "Jon"), new("Age", 21));

        // Log an exception
        try
        {
            CreateException();
        }
        catch (Exception ex) when (logger != null)
        {
            logger.AddException(ex, "An exception occurred.");
        }


        // Get and display all entries
        var allEntries = logger.GetAllEntries();
        Console.WriteLine($"\nDisplaying all entries read back from the database using the formatted message:");
        foreach (var entry in allEntries)
        {
            Console.WriteLine($"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss}] [{entry.Level}] [{entry.GetFormattedMsg()}]");
        }

        // Find only entries for person "Alice"
        var aliceEntries = logger.GetEntriesByMessageParam("Name", "Alice");
        Console.WriteLine($"\nDisplaying entries for 'Alice' using the formatted message:");
        foreach (var entry in aliceEntries)
        {
            Console.WriteLine($"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss}] [{entry.Level}] [{entry.GetFormattedMsg()}]");
        }
    }


    /// <summary>
    /// Creates an exception with an inner exception.
    /// </summary>
    private static Exception? CreateException()
    {
        try
        {
            CreateInnerException();
        }
        catch(Exception ex)
        {
            throw new InvalidOperationException("This is a demo exception!", ex);
        }

        return null;
    }


    /// <summary>
    /// Creates an inner exception.
    /// </summary>
    private static void CreateInnerException()
    {
        throw new NotImplementedException("This is another demo exception");
    }
}
