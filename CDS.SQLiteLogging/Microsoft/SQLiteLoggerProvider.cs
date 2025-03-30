using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace CDS.SQLiteLogging.Microsoft;

public class SQLiteLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, MSSQLiteLogger> loggers = new();


    public SQLiteLoggerProvider()
    {
    }

    public ILogger CreateLogger(string categoryName)
    {
        return loggers.GetOrAdd(categoryName, name => CreateMSSQLiteLogger(name));
    }

    private MSSQLiteLogger CreateMSSQLiteLogger(string categoryName)
    {
        // TODO this needs to be a filename, and via a callback!

        string folderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            nameof(SQLiteLogging));

        var sqliteLogger = new SQLiteLogger<LogEntry>(
            folder: folderPath,
            schemaVersion: LogEntry.Version,
            batchingOptions: new BatchingOptions(), // todo allow custom options
            houseKeepingOptions: new HouseKeepingOptions()); // todo allow custom options

        var scopeProvider = new LoggerExternalScopeProvider();

        var msSQLiteLogger = new MSSQLiteLogger(
            categoryName: categoryName, 
            logger: sqliteLogger, 
            scopeProvider: scopeProvider);

        return msSQLiteLogger;
    }

    public void Dispose()
    {
        foreach (var msLogger in loggers.Values)
        {
            msLogger.Dispose();
        }

        loggers.Clear();
    }
}
