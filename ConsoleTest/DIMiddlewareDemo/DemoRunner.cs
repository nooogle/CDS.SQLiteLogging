using CDS.SQLiteLogging.MEL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ConsoleTest.DIMiddlewareDemo;

/// <summary>
/// Demonstrates how to use the middleware pipeline and global log context
/// in a dependency injection scenario with the SQLite logging system.
/// </summary>
/// <remarks>
/// This class sets up a service provider with a SQLite logger and configures
/// the middleware pipeline to include global context values (such as application ID).
/// It writes log entries and then reads them back from the database, displaying
/// the global context values for each entry.
/// </remarks>
class DemoRunner
{
    /// <summary>
    /// Runs the demo by writing log entries with global context and reading them back.
    /// </summary>
    public static void Run()
    {
        // Create a database path for the SQLite log file
        string dbPath = DBPathCreator.Create();

        // Write log entries using the configured middleware pipeline and global context
        WriteLogEntries(dbPath);

        // Read back and display log entries from the database
        ReadBackLogEntries(dbPath);
    }

    /// <summary>
    /// Writes log entries to the SQLite database using a service provider.
    /// Configures the middleware pipeline to include the global log context.
    /// </summary>
    /// <param name="dbPath">The path to the SQLite database file.</param>
    /// <returns>The created <see cref="ServiceProvider"/> instance.</returns>
    private static ServiceProvider WriteLogEntries(string dbPath)
    {
        // Create the SQLite logging pipeline with global context middleware
        var logPipeline =
            CDS.SQLiteLogging.LogPipelineBuilder.Empty
            .Add(new CDS.SQLiteLogging.GlobalLogContextMiddleware())
            .Build();

        // Set a global context value for an application-wide identifier
        CDS.SQLiteLogging.GlobalLogContext.Set(
            key: GlobalLogContextKeys.AppId,
            value: $"{DateTime.Now.Ticks}");

        // Create the SQLite logger provider with the middleware pipeline
        var sqliteLoggerProvider = MELLoggerProvider.Create(
            fileName: dbPath,
            logPipeline: logPipeline);

        // Create a service provider with the SQLite logger and a demo service
        var serviceProvider = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddProvider(sqliteLoggerProvider);
                builder.SetMinimumLevel(LogLevel.Trace);
            })
            .AddTransient<DemoService>()
            .BuildServiceProvider();

        // Run the demo service, which writes log entries
        serviceProvider.GetRequiredService<DemoService>().Run();

        // Ensure all pending log entries are written before disposing the service provider
        var loggerUtilities = sqliteLoggerProvider.LoggerUtilities;
        loggerUtilities.WaitUntilCacheIsEmpty(TimeSpan.FromSeconds(5));

        return serviceProvider;
    }

    /// <summary>
    /// Reads back log entries from the SQLite database and displays them in the console,
    /// including global context values such as AppId and BatchNumber.
    /// </summary>
    /// <param name="dbPath">The path to the SQLite database file.</param>
    private static void ReadBackLogEntries(string dbPath)
    {
        using var sqliteReader = new CDS.SQLiteLogging.Reader(dbPath);

        // Display the number of entries in the database
        var numEntries = sqliteReader.GetEntryCount();
        Console.WriteLine($"Number of entries: {numEntries}");

        // Retrieve and display all log entries, showing global context values
        var allEntries = sqliteReader.GetAllEntries();
        allEntries.ForEach(entry =>
        {
            Console.WriteLine(
                $"AppId [{GetApplicationId(entry)}] " +
                $"batch [{GetBatchNumber(entry)}] : " +
                $"{entry.RenderedMessage}");
        });
    }

    /// <summary>
    /// Retrieves the application identifier from a log entry's properties.
    /// </summary>
    /// <param name="logEntry">The log entry to interrogate.</param>
    /// <returns>The application identifier if found, otherwise an empty string.</returns>
    private static string GetApplicationId(CDS.SQLiteLogging.LogEntry logEntry) =>
        GetProperty(logEntry, GlobalLogContextKeys.AppId);

    /// <summary>
    /// Retrieves the batch number from a log entry's properties.
    /// </summary>
    /// <param name="logEntry">The log entry to interrogate.</param>
    /// <returns>The batch number if found, otherwise an empty string.</returns>
    private static string GetBatchNumber(CDS.SQLiteLogging.LogEntry logEntry) =>
        GetProperty(logEntry, GlobalLogContextKeys.BatchNumber);

    /// <summary>
    /// Gets a specific property value from a log entry's properties.
    /// </summary>
    /// <param name="logEntry">The log entry to interrogate.</param>
    /// <param name="key">Key of a property to find.</param>
    /// <returns>
    /// The value of the property if found, otherwise an empty string.
    /// </returns>
    private static string GetProperty(CDS.SQLiteLogging.LogEntry logEntry, string key)
    {
        // Get the property value from the properties of the log entry
        if (logEntry.Properties != null && logEntry.Properties.TryGetValue(key, out var value))
        {
            return value?.ToString() ?? string.Empty;
        }
        return string.Empty;
    }
}
