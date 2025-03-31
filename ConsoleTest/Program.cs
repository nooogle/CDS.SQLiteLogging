using CDS.SQLiteLogging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ConsoleTest;

class Program
{
    private BatchingOptions batchingOptions = new BatchingOptions();
    private HouseKeepingOptions houseKeepingOptions = new HouseKeepingOptions();
    private ServiceProvider serviceProvider;

    private void RecreateServiceProvider()
    {
        serviceProvider?.Dispose();

        string dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            nameof(CDS),
            nameof(CDS.SQLiteLogging),
            nameof(ConsoleTest),
            $"MSTest_Schema{LogEntry.Version}.db");

        var sqliteLoggerProvider = CDS.SQLiteLogging.Microsoft.SQLiteLoggerProvider.Create(
            dbPath,
            batchingOptions,
            houseKeepingOptions);

        // Setup dependency injection
        serviceProvider = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddProvider(sqliteLoggerProvider);
                builder.AddConsole();
                builder.AddDebug();
                builder.SetMinimumLevel(LogLevel.Debug);
            })
            .AddTransient<LogLevelsDemo>()
            .AddTransient<ScopeDemo.Factory>()
            .AddTransient<BurstLogEntriesTest>()
            .AddTransient<LoggerSoakTest>()
            .BuildServiceProvider();
    }


    void Run()
    {
        try
        {
            RecreateServiceProvider();

            new CDS.CLIMenus.Basic.MenuBuilder("Demos")

                .AddItem("Log levels", () => serviceProvider.GetRequiredService<LogLevelsDemo>().Run())
                .AddItem("Scope demos", () => serviceProvider.GetRequiredService<ScopeDemo.Factory>().Run())
                .AddItem("Burst log entries", () => serviceProvider.GetRequiredService<BurstLogEntriesTest>().Run())
                .AddItem("Soak test", () => serviceProvider.GetRequiredService<LoggerSoakTest>().Run())

                .AddItem("Customise batching options", CustomiseBatchingOptions)
                .AddItem("Customise housekeeping options", CustomiseHouseKeepingOptions)

                .Build()
                .Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

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


    static void Main()
    {
        new Program().Run();
    }
}
