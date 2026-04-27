using CDS.SQLiteLogging.MEL;
using Microsoft.Extensions.Logging;

namespace ConsoleTest.NonDIDemos;

/// <summary>
/// Simple demo that creates a logger and writes various log entries to demonstrate basic logging functionality.
/// </summary>
class LogLevelsDemo
{
    /// <summary>
    /// Runs a basic test of the SQLite Logger.
    /// </summary>
    public void Run()
    {
        // Display the test header
        Console.Clear();
        Console.WriteLine("=== Log levels demo, direct use, not using dependency injection ===\n");


        // Create path for the SQLite database file in local app data folder
        // Including version number in filename allows for schema migrations
        string dbPath = DBPathCreator.Create();

        // Initialize the logger provider - this is the core component that
        // manages writing logs to SQLite and provides logger instances
        var sqliteLoggerProvider = MELLoggerProvider.Create(dbPath);

        if (sqliteLoggerProvider == null)
        {
            Console.WriteLine("Failed to create logger provider.");
            return;
        }

        // Create the logger
        var logger = sqliteLoggerProvider.CreateLogger(nameof(LogLevelsDemo));

        // Add different levels of log entries
        // Add different levels of log entries
        logger.LogTrace("This is a trace message.");
        logger.LogDebug("This is a debug message.");
        logger.LogInformation("This is an information message.");
        logger.LogWarning("This is a warning message.");
        logger.LogError("This is an error message.");
        logger.LogCritical("This is a critical message.");

        // Log an exception
        try
        {
            CreateException();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred.");
        }

        // Wait for any pending log entries to be written to the database
        // This ensures all logs are persisted before the application exits
        FlushLogsBeforeExit(sqliteLoggerProvider);
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
        catch (Exception ex)
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

    /// <summary>
    /// Ensures all pending logs are written before exiting.
    /// </summary>
    /// <param name="loggerProvider">The SQLite logger provider to flush.</param>
    private void FlushLogsBeforeExit(MELLoggerProvider loggerProvider)
    {
        // The WaitUntilCacheIsEmpty method blocks until all cached logs are written
        // or until the timeout is reached, whichever comes first
        loggerProvider.LoggerUtilities.WaitUntilCacheIsEmpty(timeout: TimeSpan.FromSeconds(5));

        // Note: In production code, you might want to display a "saving logs" message
        // or provide feedback about the flush operation
    }
}

