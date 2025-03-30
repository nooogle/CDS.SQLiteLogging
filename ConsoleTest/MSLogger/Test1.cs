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
                builder.SetMinimumLevel(LogLevel.Debug);
            })
            .BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = true // enables tracking for disposal               
            });

        try
        {
            // Get a logger instance
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("My_Category");

            // Log some messages
            logger.LogInformation("This is an information message.");
            logger.LogWarning("This is a warning message.");
            logger.LogError("This is an error message.");
            logger.LogCritical("Camera {cameraName} is offline", "Front Door");

            // Log using 2 levels of scope
            using (logger.BeginScope("Scope 1"))
            {
                logger.LogInformation("This is an information message.");
                logger.LogWarning("This is a warning message.");
                logger.LogError("This is an error message.");
                logger.LogCritical("Camera {cameraName} is offline", "Front Door");

                using (logger.BeginScope("Scope 2"))
                {
                    logger.LogInformation("This is an information message.");
                    logger.LogWarning("This is a warning message.");
                    logger.LogError("This is an error message.");
                    logger.LogCritical("Camera {cameraName} is offline", "Front Door");
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
