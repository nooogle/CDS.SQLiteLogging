using CDS.SQLiteLogging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ConsoleTest;

/// <summary>
/// Main program class.
/// </summary>
class Program
{
    /// <summary>
    /// Configuration options for batch processing of log entries.
    /// </summary>
    private BatchingOptions batchingOptions = new BatchingOptions();

    /// <summary>
    /// Configuration options for housekeeping of log entries.
    /// </summary>
    private HouseKeepingOptions houseKeepingOptions = new HouseKeepingOptions();

    /// <summary>
    /// Service provider for dependency injection.
    /// </summary>
    private ServiceProvider? serviceProvider;

    /// <summary>
    /// Recreates the service provider with updated configurations.
    /// </summary>
    private void RecreateServiceProvider()
    {
        serviceProvider?.Dispose();

        string dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            nameof(CDS),
            nameof(CDS.SQLiteLogging),
            nameof(ConsoleTest),
            $"Log_V{MSSQLiteLogger.DBSchemaVersion}.db");

        var sqliteLoggerProvider = CDS.SQLiteLogging.SQLiteLoggerProvider.Create(
            dbPath,
            batchingOptions,
            houseKeepingOptions);

        var loggerUtilities = sqliteLoggerProvider.LoggerUtilities;

        // Setup dependency injection
        serviceProvider = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddProvider(sqliteLoggerProvider);
                //builder.AddConsole();
                //builder.AddDebug();
                builder.SetMinimumLevel(LogLevel.Debug);
            })
            .AddSingleton<ISQLiteLoggerUtilities>(loggerUtilities)
            .AddTransient<LogLevelsDemo>()
            .AddTransient<ScopeDemo.Factory>()
            .AddTransient<BurstLogEntriesTest>()
            .AddTransient<LoggerSoakTest>()
            .BuildServiceProvider();
    }

    /// <summary>
    /// Runs the main program logic.
    /// </summary>
    void Run()
    {
        try
        {
            RecreateServiceProvider();
            if (serviceProvider == null)
            {
                Console.WriteLine("Failed to create service provider.");
                return;
            }

            new CDS.CLIMenus.Basic.MenuBuilder("Demos")
                .AddItem("Log levels", () => serviceProvider.GetRequiredService<LogLevelsDemo>().Run())
                .AddItem("Scope demos", () => serviceProvider.GetRequiredService<ScopeDemo.Factory>().Run())
                .AddItem("Burst log entries", () => serviceProvider.GetRequiredService<BurstLogEntriesTest>().Run())
                .AddItem("Soak test", () => serviceProvider.GetRequiredService<LoggerSoakTest>().Run())
                .AddItem("Customise batching options", CustomiseBatchingOptions)
                .AddItem("Customise housekeeping options", CustomiseHouseKeepingOptions)
                .AddItem("Delete all entries", DeleteAllEntries)
                .AddItem("Display database metrics", DisplayDatabaseMetrics)
                .SetOnItemComplete(DisplayDatabaseMetrics)
                .Build()
                .Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        serviceProvider?.Dispose();
    }

    /// <summary>
    /// Customizes the batching options and recreates the service provider.
    /// </summary>
    private void CustomiseBatchingOptions()
    {
        batchingOptions = CustomOptionsEditor.GetBatchingOptions(batchingOptions);
        RecreateServiceProvider();
    }

    /// <summary>
    /// Customizes the housekeeping options and recreates the service provider.
    /// </summary>
    private void CustomiseHouseKeepingOptions()
    {
        houseKeepingOptions = CustomOptionsEditor.GetHouseKeepingOptions(houseKeepingOptions);
        RecreateServiceProvider();
    }

    /// <summary>
    /// Deletes all log entries from the database.
    /// </summary>
    private void DeleteAllEntries()
    {
        var loggerUtilities = serviceProvider!.GetRequiredService<ISQLiteLoggerUtilities>();
        var numberOfDeletedEntries = loggerUtilities.DeleteAll();
        Console.WriteLine($"Deleted {numberOfDeletedEntries} entries.");
    }

    /// <summary>
    /// Displays the database metrics.
    /// </summary>
    private void DisplayDatabaseMetrics()
    {
        var loggerUtilities = serviceProvider!.GetRequiredService<ISQLiteLoggerUtilities>();

        Console.WriteLine("");
        Console.WriteLine("=== End of demo database Metrics ===");
        Console.WriteLine("");
        Console.WriteLine($"DB file size: {loggerUtilities.GetDatabaseFileSize() / (1024.0 * 1024.0):0.00} MB");
        Console.WriteLine($"Pending entries: {loggerUtilities.PendingEntriesCount}");
        Console.WriteLine($"Discarded entries: {loggerUtilities.DiscardedEntriesCount}");
    }

    /// <summary>
    /// Entry point of the program.
    /// </summary>
    static void Main()
    {
        new Program().Run();
    }
}
