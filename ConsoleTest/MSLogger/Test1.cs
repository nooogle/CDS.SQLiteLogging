using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ConsoleTest.MSLogger;

static class Test1
{
    /// <summary>
    /// Runs a basic test of the SQLite Logger.
    /// </summary>
    public static void Run()
    {
        // Display the test header
        Console.Clear();
        Console.WriteLine("=== SQLite database provider for MS Logging (hopefully!) ===\n");

        // Create SQLite logger provider directly to control its lifetime
        var sqliteLoggerProvider = new CDS.SQLiteLogging.Microsoft.SQLiteLoggerProvider();

        // Setup dependency injection
        using var serviceProvider = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddProvider(sqliteLoggerProvider);
                builder.AddConsole();                       // stdout/terminal
                builder.AddDebug();
                builder.SetMinimumLevel(LogLevel.Debug);
            })
            .BuildServiceProvider();

        try
        {
            // Get a logger instance
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("My_Category");

            // Log some messages
            logger.LogInformation("Test started");

            // Log a simulated bread production system
            var random = new Random();
            foreach (var batch in new[] { "White1234", "WholeMeal66" })
            {
                using var loafScope = logger.BeginScope("White loaf batch {batch}", batch);

                for (int loafIndex = 0; loafIndex < 3; loafIndex++)
                {
                    using var scope = logger.BeginScope($"Loaf {loafIndex}");

                    logger.LogInformation("Mixing {flour_g} g flour and {water_g} g water", random.Next(900, 1100), random.Next(650, 750));
                    logger.LogInformation("Baking loaf for {bake_time} minutes", random.Next(25, 35));
                }
            }        
        }
        finally
        {
            // Explicitly dispose the logger provider to ensure cache flushing
            if (sqliteLoggerProvider is IDisposable disposableProvider)
            {
                disposableProvider.Dispose();
            }
        }
    }
}
