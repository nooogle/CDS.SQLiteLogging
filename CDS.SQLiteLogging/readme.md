# CDS.SQLiteLogging

CDS.SQLiteLogging is a SQLite-backed logging library for .NET applications built on `Microsoft.Extensions.Logging`.

## Supported frameworks

- .NET 10 (`net10.0`)
- .NET 8 (`net8.0`)
- .NET Framework 4.8 (`net48`)

## Features

- SQLite-backed log storage with structured properties and scopes
- `Microsoft.Extensions.Logging` provider for DI-based apps
- Batching for efficient write throughput
- Configurable housekeeping (retention period or max entry count)
- Configurable SQLite PRAGMAs (journal mode / synchronous mode)

## Installation

```bash
dotnet add package CDS.SQLiteLogging
```

## Quick start

```csharp
using CDS.SQLiteLogging.MEL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var dbPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "CDS",
    "SQLiteLogging",
    "logs.db");

using var sqliteLoggerProvider = MELLoggerProvider.Create(dbPath);

using var serviceProvider = new ServiceCollection()
    .AddLogging(builder =>
    {
        builder.ClearProviders();
        builder.AddProvider(sqliteLoggerProvider);
        builder.SetMinimumLevel(LogLevel.Information);
    })
    .BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILoggerFactory>()
    .CreateLogger("Demo");

logger.LogInformation("Hello from CDS.SQLiteLogging");
```

## Configuration overview

### BatchingOptions

- `BatchSize`: max entries written per batch
- `MaxCacheSize`: max in-memory queued entries
- `FlushInterval`: periodic flush interval

### HouseKeepingOptions

- `Mode`: automatic or manual cleanup
- `RetentionPeriod`: age-based cleanup threshold
- `CleanupInterval`: cleanup timer interval
- `MaxEntries`: count-based retention limit

### DatabaseOptions

Defaults:

- `PRAGMA journal_mode = DELETE`
- `PRAGMA synchronous = NORMAL`

Both can be overridden with `DatabaseOptions` when creating the provider.

## Links

- Source / issues: https://github.com/nooogle/CDS.SQLiteLogging
- NuGet package: https://www.nuget.org/packages/CDS.SQLiteLogging

## License

MIT — see [LICENSE](https://github.com/nooogle/CDS.SQLiteLogging/blob/master/LICENSE.txt).

## Attributions

<a href="https://www.flaticon.com/free-icons/log-file" title="log file icons">Log file icons created by Muhammad_Usman - Flaticon</a>
