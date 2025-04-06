using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CDS.SQLiteLogging.Tests.Support;

delegate void OnDatabaseCreatedDelegate(IServiceProvider serviceProvider, string dbPath);
delegate void OnDatabaseClosedDelegate(string dbPath);

class NewDatabaseTestHost
{
    public BatchingOptions BatchingOptions { get; set; } = new BatchingOptions();
    public HouseKeepingOptions HouseKeepingOptions { get; set; } = new HouseKeepingOptions();
    public IDateTimeProvider DateTimeProvider { get; set; } = new DefaultDateTimeProvider();


    public void Run(
        OnDatabaseCreatedDelegate onDatabaseCreated,
        OnDatabaseClosedDelegate onDatabaseClosed)
    {
        // Arrange
        string dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            nameof(CDS),
            nameof(SQLiteLogging),
            nameof(Tests),
            $"TestLog_V{MEL.MELLogger.DBSchemaVersion}.db");

        // Delete the database if it exists
        if (File.Exists(dbPath))
        {
            File.Delete(dbPath);
        }

        // Create the SQLite logger provider
        var sqliteLoggerProvider = MEL.MELLoggerProvider.Create(
            fileName: dbPath,
            batchingOptions: BatchingOptions,
            houseKeepingOptions: HouseKeepingOptions,
            dateTimeProvider: DateTimeProvider);

        // Get the logger utilities - we want to make these available to the demo classes
        var loggerUtilities = sqliteLoggerProvider.LoggerUtilities;

        // Setup dependency injection
        var serviceProvider = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddProvider(sqliteLoggerProvider);
                builder.SetMinimumLevel(LogLevel.Trace);
            })
            .AddSingleton(loggerUtilities)
            .BuildServiceProvider();

        // Test callback
        onDatabaseCreated(serviceProvider, dbPath);

        // Cleanup
        loggerUtilities.WaitUntilCacheIsEmpty(TimeSpan.FromSeconds(5));

        sqliteLoggerProvider.Dispose();
        serviceProvider.Dispose();

        // Test callback
        onDatabaseClosed(dbPath);

        // Clear the connection pool
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

        // Delete the database
        if (File.Exists(dbPath))
        {
            File.Delete(dbPath);
        }
    }
}
