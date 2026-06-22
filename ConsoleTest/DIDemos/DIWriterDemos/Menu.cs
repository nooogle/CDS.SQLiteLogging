using CDS.SQLiteLogging;
using CDS.SQLiteLogging.MEL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ConsoleTest.DIWriterDemos;

/// <summary>
/// Menu for the assorted writer demos. Owns the DI service provider and
/// rebuilds it when batching or housekeeping options change.
/// </summary>
class Menu
{
    private BatchingOptions batchingOptions = new BatchingOptions();
    private HouseKeepingOptions houseKeepingOptions = new HouseKeepingOptions();
    private ServiceProvider? serviceProvider;

    private void RecreateServiceProvider()
    {
        serviceProvider?.Dispose();

        var sqliteLoggerProvider = MELLoggerProvider.Create(
            DBPathCreator.Create(),
            databaseOptions: new DatabaseOptions { JournalMode = SqliteJournalMode.Memory, SynchronousMode = SqliteSynchronousMode.Off },
            batchingOptions,
            houseKeepingOptions);

        var loggerUtilities = sqliteLoggerProvider.LoggerUtilities;

        serviceProvider = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddProvider(sqliteLoggerProvider);
                builder.SetMinimumLevel(LogLevel.Trace);
            })
            .AddSingleton(loggerUtilities)
            .AddTransient<LogLevelsDemo>()
            .AddTransient<ScopeDemo.Factory>()
            .AddTransient<BurstLogEntriesTest>()
            .AddTransient<LoggerSoakTest>()
            .BuildServiceProvider();
    }

    /// <summary>
    /// Runs the writer demos sub-menu.
    /// </summary>
    public void Run()
    {
        RecreateServiceProvider();

        SpectreMenu.Run("Assorted Writer Demos",
            ("Log levels", () => serviceProvider!.GetRequiredService<LogLevelsDemo>().Run()),
            ("Scope demos", () => serviceProvider!.GetRequiredService<ScopeDemo.Factory>().Run()),
            ("Burst log entries", () => serviceProvider!.GetRequiredService<BurstLogEntriesTest>().Run()),
            ("Soak test", () => serviceProvider!.GetRequiredService<LoggerSoakTest>().Run()),
            ("Customise batching options", CustomiseBatchingOptions),
            ("Customise housekeeping options", CustomiseHouseKeepingOptions));

        serviceProvider!.GetRequiredService<ISQLiteWriterUtilities>()
            .WaitUntilCacheIsEmpty(TimeSpan.FromSeconds(5));

        serviceProvider?.Dispose();
    }

    private void CustomiseBatchingOptions()
    {
        batchingOptions = CustomOptionsEditor.GetBatchingOptions(batchingOptions);
        RecreateServiceProvider();
    }

    private void CustomiseHouseKeepingOptions()
    {
        houseKeepingOptions = CustomOptionsEditor.GetHouseKeepingOptions(houseKeepingOptions);
        RecreateServiceProvider();
    }
}
