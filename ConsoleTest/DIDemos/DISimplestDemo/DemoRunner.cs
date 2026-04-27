using CDS.SQLiteLogging.MEL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ConsoleTest.DISimplestDemo;

/// <summary>
/// Runs the simplest demo for the SQLite logging system using dependency injection.
/// </summary>
class DemoRunner
{
    /// <summary>
    /// Runs the demo by setting up the service provider and executing the demo logic.
    /// </summary>
    public static void Run()
    {
        // Get the path for the SQLite database file
        string dbPath = DBPathCreator.Create();

        // Create the SQLite logger provider
        var sqliteLoggerProvider = MELLoggerProvider.Create(dbPath);

        // Setup dependency injection
        using var serviceProvider = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddProvider(sqliteLoggerProvider);
                builder.SetMinimumLevel(LogLevel.Trace);
            })
            .AddTransient<DemoService>()
            .BuildServiceProvider();

        // Run the demo
        serviceProvider.GetRequiredService<DemoService>().Run();

        // Ensure all pending log entries are written before disposing the service provider
        var loggerUtilities = sqliteLoggerProvider.LoggerUtilities;
        loggerUtilities.WaitUntilCacheIsEmpty(TimeSpan.FromSeconds(5));
    }
}
