# Benchmarking Plan

## Background

`Benchmarking/StringBuildingBenchmarks.cs` is currently the only benchmark in the project — a narrow
micro-benchmark of string-building code, unrelated to the actual SQLite write/read/housekeeping
pipeline. BenchmarkDotNet is already wired up and building correctly (`Benchmarking/Benchmarking.csproj`),
so the infrastructure is ready; what's missing is benchmarks of the log system itself.

This gap became visible while fixing a concurrency bug in WALIS's log viewer (2026-08-14): `Reader`
was forcing `PRAGMA journal_mode = DELETE` on every open, which threw `SQLITE_BUSY` whenever a live
WAL writer already had the database open (see `CHANGELOG.md` → `[Unreleased]` → Fixed). The fix
(read-only `Reader`, `Housekeeper.CanOpenForWrite`, a `DatabaseOptions` overload so callers can match
a live writer's journal mode) was reasoned out from SQLite locking semantics and confirmed with a
targeted regression test, not from measured throughput/latency data. We don't actually know, for
example, the cost of `Wal` vs. the library's `Delete` default, or how housekeeping's `VACUUM` scales
with table size. This plan is about closing that gap.

## Goals

Answer these questions with real numbers rather than intuition:

1. **Write throughput & latency** under different `BatchingOptions` (`BatchSize`, `FlushInterval`,
   `MaxCacheSize`).
2. **Journal mode comparison** — `Delete` (current library default) vs. `Wal` vs. `Memory` — for both
   raw write throughput and, specifically, concurrent-read-while-writing behavior (the exact scenario
   that caused the bug above).
3. **Housekeeping cost** — `DeleteOldEntries` / `VACUUM` duration as a function of table size.
4. **Read/query performance** — `Reader.Select` / `GetRecentEntries` at realistic row counts (10k /
   100k / 1M rows), with and without a concurrent writer active.
5. **Connection-open cost** — including the new read-only path added to `Reader`.

## Proposed Approach

Keep this in the existing `Benchmarking` project rather than a new one — BenchmarkDotNet is already
set up there. Use `[GlobalSetup]` / `[GlobalCleanup]` to create a temp SQLite file per benchmark class
run (`Path.GetTempPath()`), and `[Params]` to sweep journal mode, batch size, and row count as
BenchmarkDotNet parameters rather than hand-rolled loops:

```csharp
[MemoryDiagnoser]
public class WriteThroughputBenchmarks
{
    private string _dbPath = null!;
    private CDS.SQLiteLogging.MEL.MELLoggerProvider _provider = null!;
    private ILogger _logger = null!;

    [Params(SqliteJournalMode.Delete, SqliteJournalMode.Wal, SqliteJournalMode.Memory)]
    public SqliteJournalMode JournalMode { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"bench_{Guid.NewGuid()}.db");
        _provider = MELLoggerProvider.Create(_dbPath, new DatabaseOptions { JournalMode = JournalMode });
        _logger = _provider.CreateLogger(nameof(WriteThroughputBenchmarks));
    }

    [Benchmark]
    public void WriteOneThousandEntries()
    {
        for (var i = 0; i < 1000; i++)
        {
            _logger.LogInformation("Benchmark entry {Index}", i);
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _provider.Dispose();
        SqliteConnection.ClearAllPools();
        File.Delete(_dbPath);
    }
}
```

New benchmark classes alongside `StringBuildingBenchmarks.cs`, e.g. `WriteThroughputBenchmarks.cs`,
`JournalModeConcurrencyBenchmarks.cs`, `ReaderQueryBenchmarks.cs`, `HousekeepingBenchmarks.cs`.

## Realistic Load Reference

WALIS's own disk-write stress test (`Docs/development/DiskStressTest.md` in the WALIS repo) is a
useful model for both *how* to benchmark and *what target numbers matter*:

- **Methodology worth borrowing**: discard a warm-up phase from the results; report latency as
  median/p95/p99/max rather than a single mean; define an explicit stall threshold
  (`max(100ms, median × 3)`); report per-second throughput buckets to catch bursty behaviour that an
  averaged figure would hide.
- **Target load to benchmark against**: WALIS's production cycle is 1.2s, generating on the order of
  one log burst per cycle (Information-level entries per inspection, occasional Debug/Warning) — a
  much lower rate than the 8×5MiB image-write burst per cycle that `DiskStressTest.md` validates. Log
  benchmarks should therefore be parameterized around realistic sustained entry rates (tens of
  entries/sec), with a separate worst-case pass for Debug-level logging temporarily enabled during a
  field investigation, rather than a single synthetic max-throughput number.

## Open Questions (need field data, not just benchmarks)

Mirroring the "Online Study Required" pattern in WALIS's `Docs/architecture/Housekeeping.md`:

- What log verbosity/entry rate does a real WALIS line actually produce over a full shift? This
  determines what benchmark parameters are actually representative.
- Does the drive hosting `WALIS_Log_v*.db` matter as much as it does for image writes? SQLite's WAL
  checkpoint/fsync behaviour may be more sensitive to slow or networked storage than the sequential
  image writes `DiskStressTest.md` measures — this would need its own drive-qualification pass if so.

## Recommendation

Scope this as separate follow-up work, not bundled with the Reader/Housekeeper concurrency fix already
shipped. Suggested order:

1. Journal-mode comparison (`Delete` vs `Wal` vs `Memory`) under concurrent read+write — this directly
   informs whether the library's `Delete` default is still the right choice.
2. Write throughput vs. `BatchingOptions`.
3. Housekeeping cost (`DeleteOldEntries` / `VACUUM`) vs. table size.
4. Read/query performance at realistic row counts, with and without a concurrent writer.

No public API changes are anticipated, so this work doesn't need to block or coordinate with any
release tag.
