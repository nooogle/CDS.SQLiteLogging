# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Test Commands

```bash
# Build the main library
dotnet build CDS.SQLiteLogging/CDS.SQLiteLogging.csproj --configuration Release

# Run all tests
dotnet test CDS.SQLiteLogging.Tests/CDS.SQLiteLogging.Tests.csproj

# Run a single test by name
dotnet test CDS.SQLiteLogging.Tests/CDS.SQLiteLogging.Tests.csproj --filter "FullyQualifiedName~TestMethodName"

# Run the interactive console demo
dotnet run --project ConsoleTest/ConsoleTest.csproj

# Run benchmarks
dotnet run --project Benchmarking/Benchmarking.csproj -c Release
```

## Architecture Overview

CDS.SQLiteLogging is a .NET library providing SQLite-backed log storage that integrates with Microsoft.Extensions.Logging (MEL). It is published as a NuGet package.

**Projects:**
- `CDS.SQLiteLogging` — main library (targets .NET 10, .NET 8, .NET Framework 4.8)
- `CDS.SQLiteLogging.Views` — Windows Forms log viewer controls (Windows-only targets)
- `CDS.SQLiteLogging.Tests` — MSTest unit tests
- `ConsoleTest` — interactive demo/test app
- `WinFormsTest` — Windows Forms test app
- `Benchmarking` — BenchmarkDotNet performance tests

**Solution file:** `CDS.SQLiteLogging.slnx` (newer slnx format)

## Key Abstractions

**Entry point:** `MELLoggerProvider.Create()` — factory method (with multiple overloads) that wires up and returns an `ILoggerProvider` for registration in a DI container.

**Internal processing pipeline:**

1. `MELLogger` (implements `ILogger`) collects log calls and delegates to `Logger`
2. `BatchLogCache` — single-threaded background queue (ConcurrentQueue + dedicated thread) that batches entries by count threshold, time interval, or on shutdown
3. `LogPipeline` — async middleware chain (similar to ASP.NET Core middleware) invoked on the background thread; built via `LogPipelineBuilder`
4. `LogWriter` — executes parameterized INSERT statements into SQLite
5. `Housekeeper` — timer-driven cleanup; supports time-based (`RetentionPeriod`) and count-based (`MaxEntries`) modes
6. `ConnectionManager` — owns the SQLite connection and configures PRAGMAs (journal mode, synchronous mode)

**Cross-framework SQLite:** Uses conditional compilation (`#if`) with `using` aliases so `System.Data.SQLite` is used on .NET Framework 4.8 and `Microsoft.Data.Sqlite` on .NET 8+.

**Data model:**
- `LogEntry` holds structured properties (dictionary), scopes (serialized as JSON), and exception data (via `ExceptionSerializer`)
- `LiveId` is the transient in-memory identifier; `DbId` is the AUTOINCREMENT PRIMARY KEY assigned on insert
- `DatabaseSchema` is the single source of truth for all table/column names

**Configuration objects:** `BatchingOptions`, `HouseKeepingOptions`, `DatabaseOptions` (SQLite PRAGMAs)

**Ambient context:** `GlobalLogContext` is a static, thread-safe dictionary for attaching properties to all log entries without explicit parameter passing. `GlobalLogContextMiddleware` reads it inside the pipeline.

## Coding Conventions

From `CONTRIBUTING.md`:
- Allman-style braces (opening brace on its own line)
- File-scoped namespaces (`namespace Foo;`)
- Nullable reference types enabled (`<Nullable>enable</Nullable>`)
- XML doc comments on public members
- MSTest with Arrange-Act-Assert structure and descriptive test names

## Versioning & Release

Uses [MinVer](https://github.com/adamralph/minver) — version is derived automatically from git tags (e.g., `git tag v2.5.0`). Tagging triggers the GitHub Actions release workflow to build, test, pack, and publish to NuGet.
