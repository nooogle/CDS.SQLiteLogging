s# CDS.SQLiteLogging

[![CI](https://github.com/nooogle/CDS.SQLiteLogging/actions/workflows/ci.yml/badge.svg)](https://github.com/nooogle/CDS.SQLiteLogging/actions/workflows/ci.yml)
[![NuGet Version](https://img.shields.io/nuget/v/CDS.SQLiteLogging)](https://www.nuget.org/packages/CDS.SQLiteLogging)
[![NuGet Downloads](https://img.shields.io/nuget/dt/CDS.SQLiteLogging)](https://www.nuget.org/packages/CDS.SQLiteLogging)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.txt)
[![.NET](https://img.shields.io/badge/.NET-8%20%7C%2010-512BD4)](https://dotnet.microsoft.com)


CDS.SQLiteLogging is a logging library for .NET applications that uses SQLite for storing 
log entries. It supports batch processing, housekeeping, and integrates with 
the Microsoft.Extensions.Logging framework.

## TODO

This is a first draft of the readme. It needs to be updated with:
* Existing features still to document:
  * UI live log viewer support
  * DB browser tool and SQLite links
  * Any rational for why we are doing this and what the benefits are!
  * Unit tests and the little tick box thing that shows everything is passing!
* Features to add/demo
  * Standalone log reader demo for offline log review


## Features

- **Batch Processing**: Efficiently writes log entries in batches to improve performance.
- **Housekeeping**: Automatically or manually cleans up old log entries based on configurable retention policies.
- **Flexible Configuration**: Easily configurable options for batching, housekeeping, and logging levels.
- **Integration with Microsoft.Extensions.Logging**: Seamlessly integrates with the .NET logging framework.


## Installation

You can install the CDS.SQLiteLogging library via NuGet:

`dotnet add package CDS.SQLiteLogging`


## Usage

### Basic Setup for Microsoft Logging and Dependency Injection

To set up the library in a .NET application, follow these steps:

1. **Configure the Logger Provider**:


```csharp
using CDS.SQLiteLogging;
using CDS.SQLiteLogging.MEL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Get the path for the SQLite database file
string dbPath = GetDatabasePath();

// Create the SQLite logger provider
var sqliteLoggerProvider = MELLoggerProvider.Create(dbPath);

// Setup dependency injection
using var serviceProvider = new ServiceCollection()
    .AddLogging(builder =>
    {
        builder.ClearProviders();
        builder.AddProvider(sqliteLoggerProvider);
        builder.SetMinimumLevel(LogLevel.Trace);
    })
    .AddTransient<DemoService>() // a demo service for this readme!
    .BuildServiceProvider();
```

Example database path builder:

```csharp
private static string GetDatabasePath()
{
    return Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        nameof(CDS),
        nameof(CDS.SQLiteLogging),
        nameof(ConsoleTest),
        $"Log_V{MELLogger.DBSchemaVersion}.db");
}
```


2. **Using the Logger**:

```csharp
using Microsoft.Extensions.Logging;

namespace ConsoleTest.DISimplestDemo;

/// <summary>
/// Provides demo services for processing and logging.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DemoService"/> class.
/// </remarks>
/// <param name="logger">The logger instance.</param>
class DemoService(ILogger<DemoService> logger)
{
    /// <summary>
    /// Runs the demo service.
    /// </summary>
    public void Run()
    {
        using var scope = logger.BeginScope("DemoService.Run");
        logger.LogDebug("Here we go!");
        DoSomeProcessing();
    }

    /// <summary>
    /// Processes some items and logs the progress.
    /// </summary>
    private void DoSomeProcessing()
    {
        using var scope = logger.BeginScope("DemoService.DoSomeProcessing");

        for (var i = 0; i < 4; i++)
        {
            logger.LogInformation("Processing item {ItemNumber}", i);
        }
    }
}
```


### Configuration Options

#### Batching Options

- **BatchSize**: The maximum number of entries to write in a single batch.
- **MaxCacheSize**: The maximum number of entries to cache. Any items logged when the cache is full will be discarded.
- **FlushInterval**: The interval in milliseconds between cache flushes.

#### Housekeeping Options

- **Mode**: The housekeeping mode (Automatic or Manual).
- **RetentionPeriod**: The retention period for log entries.
- **CleanupInterval**: The interval between cleanup operations.

#### Database Options

By default, the library uses SQLite's native `PRAGMA journal_mode = DELETE` and `PRAGMA synchronous = NORMAL`.
Both can be overridden by supplying `DatabaseOptions` when creating the provider.

```csharp
using CDS.SQLiteLogging;
using CDS.SQLiteLogging.MEL;

// WAL mode with NORMAL synchronous (good general-purpose choice)
var walProvider = MELLoggerProvider.Create(
    fileName: dbPath,
    databaseOptions: new DatabaseOptions
    {
        JournalMode = SqliteJournalMode.Wal,
        SynchronousMode = SqliteSynchronousMode.Normal,
    });

// In-memory journal with synchronous OFF (maximum write speed, no durability)
var fastProvider = MELLoggerProvider.Create(
    fileName: dbPath,
    databaseOptions: new DatabaseOptions
    {
        JournalMode = SqliteJournalMode.Memory,
        SynchronousMode = SqliteSynchronousMode.Off,
    });
```

Supported `SqliteJournalMode` values: `Delete` (default), `Truncate`, `Persist`, `Memory`, `Wal`, `Off`.

Supported `SqliteSynchronousMode` values: `Off`, `Normal` (default), `Full`, `Extra`.



## API Reference

### Public Classes and Methods

- **SQLiteWriter**: Provides writing capabilities for SQLite logging with caching, batching, and housekeeping.
- **SQLiteReader**: Provides read-only access to SQLite log entries.
- **BatchingOptions**: Configuration options for batch processing of log entries.
- **DatabaseOptions**: Configuration options for SQLite connection behavior.
- **HouseKeepingOptions**: Configuration options for housekeeping of log entries.
- **SqliteSynchronousMode**: Supported values for `PRAGMA synchronous`.

## Contributing

We welcome contributions to the CDS.SQLiteLogging library. Please feel free to submit issues and pull requests on GitHub.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.txt) file for details.

## Contact

For support or questions, please contact us via GitHub.


## Attributions

<a href="https://www.flaticon.com/free-icons/log-file" title="log file icons">Log file icons created by Muhammad_Usman - Flaticon</a>

