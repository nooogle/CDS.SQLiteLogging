using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ConsoleTest.MSLogger.BreadFactorySimulator;

/// <summary>
/// Represents a factory that simulates bread production.
/// </summary>
class Factory
{
    /// <summary>
    /// Runs the factory simulation.
    /// </summary>
    public void Run()
    {
        try
        {
            string dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                nameof(CDS),
                nameof(CDS.SQLiteLogging),
                nameof(ConsoleTest),
                $"MSTest_Schema{CDS.SQLiteLogging.LogEntry.Version}.db");

            using var sqliteLoggerProvider = new CDS.SQLiteLogging.Microsoft.SQLiteLoggerProvider(dbPath);

            // Setup dependency injection
            using var serviceProvider = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.AddProvider(sqliteLoggerProvider);
                    builder.AddConsole();
                    builder.AddDebug();
                    builder.SetMinimumLevel(LogLevel.Debug);
                })
                .BuildServiceProvider();

            // Create a logger for this class
            var factoryLogger = serviceProvider.GetRequiredService<ILogger<Factory>>();

            // Log some messages
            factoryLogger.LogInformation("Factory simulation starting");

            // Create an oven system
            var ovenLogger = serviceProvider.GetRequiredService<ILogger<OvenSystem>>();
            var ovenSystem = new OvenSystem(ovenLogger);
            ovenSystem.MakeBread();

            // Create and run a cleaning system
            var cleaningLogger = serviceProvider.GetRequiredService<ILogger<CleaningSystem>>();
            var cleaningSystem = new CleaningSystem(cleaningLogger);
            cleaningSystem.Clean();
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"An error occurred: {ex.Message}");
            throw;
        }
    }
}
