using CDS.SQLiteLogging.MEL;
using Microsoft.Extensions.Logging;

namespace ConsoleTest.NonDIDemos;

/// <summary>
/// Simple demo that creates a logger and writes various log entries to demonstrate basic logging functionality.
/// </summary>
class SimplestWriterDemo
{
    /// <summary>
    /// Runs a basic test of the SQLite Logger. This does not trap exception to keep the 
    /// code simple and focused on demonstrating logging functionality. 
    /// In production code, you should add appropriate error handling.
    /// </summary>
    public void Run()
    {
        // Create the logger
        string dbPath = DBPathCreator.Create();
        var sqliteLoggerProvider = MELLoggerProvider.Create(dbPath);
        var logger = sqliteLoggerProvider.CreateLogger(categoryName: nameof(SimplestWriterDemo));

        // Tell the user what we're doing
        Console.Clear();
        Console.WriteLine("=== Simple direct-demo (not using dependency injection) ===");
        Console.WriteLine("This demo creates a logger and writes an information message to the log.");
        Console.WriteLine($"The log is written to the database file: {dbPath}");


        // Log something!
        logger.LogInformation("This is an information message.");

        // Cleanup
        sqliteLoggerProvider.LoggerUtilities.WaitUntilCacheIsEmpty(timeout: TimeSpan.FromSeconds(5));
    }
}
