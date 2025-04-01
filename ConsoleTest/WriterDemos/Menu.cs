using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CDS.SQLiteLogging;

namespace ConsoleTest.WriterDemos;

/// <summary>
/// Menu for running the demos.
/// </summary>
class Menu
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
            .AddSingleton<ISQLiteWriterUtilities>(loggerUtilities)
            .AddTransient<LogLevelsDemo>()
            .AddTransient<ScopeDemo.Factory>()
            .AddTransient<BurstLogEntriesTest>()
            .AddTransient<LoggerSoakTest>()
            .BuildServiceProvider();
    }
    
    /// <summary>
         /// Runs the main program logic.
         /// </summary>
    public void Run()
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
            .Build()
            .Run();

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
}
