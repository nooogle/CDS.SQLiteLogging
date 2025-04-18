﻿using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace CDS.SQLiteLogging.MEL;


/// <summary>
/// Provides an implementation of <see cref="ILoggerProvider"/> that creates instances of <see cref="MELLogger"/>.
/// </summary>
public class MELLoggerProvider : ILoggerProvider
{
    private readonly Logger sharedLoggerWriter;
    private readonly LoggerExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();
    private readonly ConcurrentDictionary<string, MELLogger> loggers = new();
    private readonly IDateTimeProvider dateTimeProvider;

    /// <summary>
    /// Event that is raised when a log entry is received.
    /// </summary>
    public event LogEntryReceivedEvent? LogEntryReceived
    {
        add => sharedLoggerWriter.LogEntryReceived += value;
        remove => sharedLoggerWriter.LogEntryReceived -= value;
    }


    /// <summary>
    /// Gets the <see cref="ISQLiteWriterUtilities"/> instance for this provider.
    /// </summary>
    public ISQLiteWriterUtilities LoggerUtilities => sharedLoggerWriter;


    /// <summary>
    /// Creates a new instance of the <see cref="MELLoggerProvider"/> class.
    /// </summary>
    /// <param name="fileName">The name of the SQLite database file.</param>
    /// <param name="batchingOptions">Options for configuring batch processing.</param>
    /// <param name="houseKeepingOptions">Options for configuring housekeeping.</param>
    private MELLoggerProvider(
        string fileName, 
        BatchingOptions batchingOptions, 
        HouseKeepingOptions houseKeepingOptions,
        IDateTimeProvider dateTimeProvider)
    {
        this.dateTimeProvider = dateTimeProvider;

        sharedLoggerWriter = new Logger(
            fileName: fileName,
            batchingOptions,
            houseKeepingOptions,
            dateTimeProvider);
    }

    /// <summary>
    /// Creates a new instance of <see cref="MELLoggerProvider"/> with the specified file name.
    /// </summary>
    /// <param name="fileName">The name of the SQLite database file.</param>
    /// <returns>A new instance of <see cref="MELLoggerProvider"/>.</returns>
    public static MELLoggerProvider Create(string fileName)
    {
        return Create(fileName, new BatchingOptions(), new HouseKeepingOptions());
    }

    /// <summary>
    /// Creates a new instance of <see cref="MELLoggerProvider"/> with the specified file name and batching options.
    /// </summary>
    /// <param name="fileName">The name of the SQLite database file.</param>
    /// <param name="batchingOptions">Options for configuring batch processing.</param>
    /// <returns>A new instance of <see cref="MELLoggerProvider"/>.</returns>
    public static MELLoggerProvider Create(string fileName, BatchingOptions batchingOptions)
    {
        return Create(fileName, batchingOptions, new HouseKeepingOptions());
    }

    /// <summary>
    /// Creates a new instance of <see cref="MELLoggerProvider"/> with the specified file name, batching options, and housekeeping options.
    /// </summary>
    /// <param name="fileName">The name of the SQLite database file.</param>
    /// <param name="batchingOptions">Options for configuring batch processing.</param>
    /// <param name="houseKeepingOptions">Options for configuring housekeeping.</param>
    /// <returns>A new instance of <see cref="MELLoggerProvider"/>.</returns>
    public static MELLoggerProvider Create(string fileName, BatchingOptions batchingOptions, HouseKeepingOptions houseKeepingOptions)
    {
        return Create(
            fileName, 
            batchingOptions, 
            houseKeepingOptions,
            new DefaultDateTimeProvider());
    }


    /// <summary>
    /// Creates a new instance of <see cref="MELLoggerProvider"/> with the specified file name, batching options, and housekeeping options.
    /// </summary>
    /// <param name="fileName">The name of the SQLite database file.</param>
    /// <param name="batchingOptions">Options for configuring batch processing.</param>
    /// <param name="houseKeepingOptions">Options for configuring housekeeping.</param>
    /// <returns>A new instance of <see cref="MELLoggerProvider"/>.</returns>
    public static MELLoggerProvider Create(
        string fileName, 
        BatchingOptions batchingOptions, 
        HouseKeepingOptions houseKeepingOptions,
        IDateTimeProvider dateTimeProvider)
    {
        return new MELLoggerProvider(
            fileName, 
            batchingOptions, 
            houseKeepingOptions,
            dateTimeProvider);
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
    /// Creates a new instance of <see cref="MELLogger"/> for the specified category name.
    /// </summary>
    /// <param name="categoryName">The category name for messages produced by the logger.</param>
    /// <returns>A new instance of <see cref="MELLogger"/>.</returns>
    private MELLogger CreateMSSQLiteLogger(string categoryName)
    {
        var msSQLiteLogger = new MELLogger(
            categoryName: categoryName,
            externalSQLiteWriter: sharedLoggerWriter,
            scopeProvider: scopeProvider,
            dateTimeProvider: dateTimeProvider);

        return msSQLiteLogger;
    }

    /// <summary>
    /// Disposes the provider and all created loggers.
    /// </summary>
    public void Dispose()
    {
        loggers.Clear();
        sharedLoggerWriter?.Dispose();
    }
}
