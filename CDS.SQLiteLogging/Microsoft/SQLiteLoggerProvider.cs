using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace CDS.SQLiteLogging.Microsoft;

/// <summary>
/// Provides an implementation of <see cref="ILoggerProvider"/> that creates instances of <see cref="MSSQLiteLogger"/>.
/// </summary>
public class SQLiteLoggerProvider : ILoggerProvider
{
    private readonly SQLiteLogger<LogEntry> sharedLogger;
    private readonly LoggerExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();
    private readonly ConcurrentDictionary<string, MSSQLiteLogger> loggers = new();

    /// <summary>
    /// Creates a new instance of the <see cref="SQLiteLoggerProvider"/> class.
    /// </summary>
    /// <param name="fileName">The name of the SQLite database file.</param>
    /// <param name="batchingOptions">Options for configuring batch processing.</param>
    /// <param name="houseKeepingOptions">Options for configuring housekeeping.</param>
    private SQLiteLoggerProvider(string fileName, BatchingOptions batchingOptions, HouseKeepingOptions houseKeepingOptions)
    {
        sharedLogger = new SQLiteLogger<LogEntry>(
            fileName: fileName,
            batchingOptions,
            houseKeepingOptions);
    }

    /// <summary>
    /// Creates a new instance of <see cref="SQLiteLoggerProvider"/> with the specified file name.
    /// </summary>
    /// <param name="fileName">The name of the SQLite database file.</param>
    /// <returns>A new instance of <see cref="SQLiteLoggerProvider"/>.</returns>
    public static SQLiteLoggerProvider Create(string fileName)
    {
        return new SQLiteLoggerProvider(fileName, new BatchingOptions(), new HouseKeepingOptions());
    }

    /// <summary>
    /// Creates a new instance of <see cref="SQLiteLoggerProvider"/> with the specified file name and batching options.
    /// </summary>
    /// <param name="fileName">The name of the SQLite database file.</param>
    /// <param name="batchingOptions">Options for configuring batch processing.</param>
    /// <returns>A new instance of <see cref="SQLiteLoggerProvider"/>.</returns>
    public static SQLiteLoggerProvider Create(string fileName, BatchingOptions batchingOptions)
    {
        return new SQLiteLoggerProvider(fileName, batchingOptions, new HouseKeepingOptions());
    }

    /// <summary>
    /// Creates a new instance of <see cref="SQLiteLoggerProvider"/> with the specified file name, batching options, and housekeeping options.
    /// </summary>
    /// <param name="fileName">The name of the SQLite database file.</param>
    /// <param name="batchingOptions">Options for configuring batch processing.</param>
    /// <param name="houseKeepingOptions">Options for configuring housekeeping.</param>
    /// <returns>A new instance of <see cref="SQLiteLoggerProvider"/>.</returns>
    public static SQLiteLoggerProvider Create(string fileName, BatchingOptions batchingOptions, HouseKeepingOptions houseKeepingOptions)
    {
        return new SQLiteLoggerProvider(fileName, batchingOptions, houseKeepingOptions);
    }

    /// <summary>
    /// Creates a new <see cref="ILogger"/> instance for the specified category name.
    /// </summary>
    /// <param name="categoryName">The category name for messages produced by the logger.</param>
    /// <returns>A new <see cref="ILogger"/> instance.</returns>
    public ILogger CreateLogger(string categoryName)
    {
        return loggers.GetOrAdd(categoryName, name => CreateMSSQLiteLogger(name));
    }

    /// <summary>
    /// Creates a new instance of <see cref="MSSQLiteLogger"/> for the specified category name.
    /// </summary>
    /// <param name="categoryName">The category name for messages produced by the logger.</param>
    /// <returns>A new instance of <see cref="MSSQLiteLogger"/>.</returns>
    private MSSQLiteLogger CreateMSSQLiteLogger(string categoryName)
    {
        var msSQLiteLogger = new MSSQLiteLogger(
            categoryName: categoryName,
            logger: sharedLogger,
            scopeProvider: scopeProvider);

        return msSQLiteLogger;
    }

    /// <summary>
    /// Disposes the provider and all created loggers.
    /// </summary>
    public void Dispose()
    {
        foreach (var msLogger in loggers.Values)
        {
            msLogger.Dispose();
        }

        loggers.Clear();
    }
}
