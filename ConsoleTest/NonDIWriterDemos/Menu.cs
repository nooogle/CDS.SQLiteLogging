using CDS.SQLiteLogging;

namespace ConsoleTest.NonDIWriterDemos;

/// <summary>
/// Menu for running the SQLite logging demos without dependency injection.
/// </summary>
/// <remarks>
/// This demonstrates how to use the SQLite logging system without dependency injection.
/// Once the <see cref="MSSQLiteLoggerProvider"/> is created it can be used to create loggers,
/// each logger being for a specific category.
/// </remarks>
class Menu
{
    /// <summary>
    /// Runs the main program logic by setting up a logger and presenting demo options.
    /// </summary>
    public void Run()
    {
        // Create path for the SQLite database file in local app data folder
        // Including version number in filename allows for schema migrations
        string dbPath = CreateDatabasePath();

        // Initialize the logger provider - this is the core component that
        // manages writing logs to SQLite and provides logger instances
        var sqliteLoggerProvider = MSSQLiteLoggerProvider.Create(dbPath);

        if (sqliteLoggerProvider == null)
        {
            Console.WriteLine("Failed to create logger provider.");
            return;
        }

        // Build a simple CLI menu with demo options
        // Each demo will use the same logger provider but can create its own loggers
        DisplayDemoMenu(sqliteLoggerProvider);

        // Wait for any pending log entries to be written to the database
        // This ensures all logs are persisted before the application exits
        FlushLogsBeforeExit(sqliteLoggerProvider);
    }

    /// <summary>
    /// Creates the database file path with version information.
    /// </summary>
    /// <returns>The full path to the SQLite database file.</returns>
    private string CreateDatabasePath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            nameof(CDS),
            nameof(CDS.SQLiteLogging),
            nameof(ConsoleTest),
            $"Log_V{MSSQLiteLogger.DBSchemaVersion}.db");
    }

    /// <summary>
    /// Builds and displays the demo selection menu.
    /// </summary>
    /// <param name="loggerProvider">The SQLite logger provider to use in demos.</param>
    private void DisplayDemoMenu(MSSQLiteLoggerProvider loggerProvider)
    {
        new CDS.CLIMenus.Basic.MenuBuilder("SQLite Logging Demos")
            .AddItem("Log levels", () => new LogLevelsDemo(loggerProvider).Run())
            // Additional demo options can be added here
            .Build()
            .Run();
    }

    /// <summary>
    /// Ensures all pending logs are written before exiting.
    /// </summary>
    /// <param name="loggerProvider">The SQLite logger provider to flush.</param>
    private void FlushLogsBeforeExit(MSSQLiteLoggerProvider loggerProvider)
    {
        // The WaitUntilCacheIsEmpty method blocks until all cached logs are written
        // or until the timeout is reached, whichever comes first
        loggerProvider.LoggerUtilities.WaitUntilCacheIsEmpty(timeout: TimeSpan.FromSeconds(5));

        // Note: In production code, you might want to display a "saving logs" message
        // or provide feedback about the flush operation
    }
}
