# Async Simplification Plan

## Background

A deep dive in June 2026 confirmed that the async/await layer in CDS.SQLiteLogging is almost entirely
synthetic:

- **SQLite has no native async I/O.** `Microsoft.Data.Sqlite` provides `ExecuteNonQueryAsync()` etc.
  for ADO.NET interface consistency only — they call the synchronous C SQLite library internally.
  `System.Data.SQLite` (the .NET 4.8 package) does not expose async methods at all.
- **The write path is already correctly offloaded.** `BatchLogCache` uses a dedicated background
  thread (`ProcessEntriesLoop`). Async/await on top of that gives no concurrency benefit — it only
  adds state machine allocations and `ConfigureAwait(false)` noise.
- **Read/export methods were synthetically async.** Every `Reader.*Async` and `Exporter.ExportAsync`
  resolved to a synchronous `cmd.ExecuteReader()` / `cmd.ExecuteNonQuery()` call behind an `async`
  boundary, returning `Task.CompletedTask` afterwards.

Two components are genuinely async and must remain so:

- **`LogPipeline.ExecuteAsync` / `ILogMiddleware.InvokeAsync`** — middleware can legitimately await
  real I/O (HTTP, gRPC, etc.). This is the designed extension point.
- **`BatchLogCache.WaitUntilCacheIsEmptyAsync`** — polls with `await Task.Delay(10)` rather than
  `Thread.Sleep(10)`. In an async context (e.g. ASP.NET graceful shutdown) this yields the thread
  between polls. The sync twin `WaitUntilCacheIsEmpty` blocks; they are meaningfully different.

---

## Phase 1 — Completed in v2.x: Synthetic async → sync primary + `[Obsolete]` wrappers

**Goal:** Make synchronous methods the canonical API. Retain async method signatures as `[Obsolete]`
wrappers so callers get a compile-time migration warning without a runtime break.

### Public API changes

| Type | Before | After |
|------|--------|-------|
| `ISQLiteWriterUtilities` | `Task<int> DeleteAllAsync()` — primary | `[Obsolete]`, calls `DeleteAll()` |
| `Reader` | `GetAllEntriesAsync()` primary; `GetAllEntries()` `[Obsolete]` | Flipped: sync primary, async `[Obsolete]` |
| `Reader` | `GetRecentEntriesAsync()` primary; `GetRecentEntries()` `[Obsolete]` | Flipped |
| `Reader` | `GetEntriesByMessageParamAsync()` only, no sync twin | Added sync `GetEntriesByMessageParam()`; async is `[Obsolete]` |
| `Reader` | `SelectAsync()` primary; `Select()` `[Obsolete]` | Flipped |
| `Reader` | `QueryAsync<T>()` primary; `Query<T>()` `[Obsolete]` | Flipped; sync gains optional `CancellationToken` param |
| `Housekeeper` | `DeleteByIdsAsync()` only, no sync twin | Added sync `DeleteByIds()`; async is `[Obsolete]` |
| `Exporter` | `ExportAsync()` primary; `Export()` calls it | Flipped; `Export()` gains optional `CancellationToken` |

### Internal-only changes (no backward-compat wrappers needed)

- **`ConnectionManager`**: removed `ExecuteInTransactionAsync` and `ExecuteWithRetryAsync`; added
  sync `ExecuteWithRetry(Action)`.
- **`LogWriter`**: removed `AddAsync` (was already `[Obsolete]`) and `AddBatchAsync`; `AddBatch` is
  the sole write method.
- **`DirectDBExporter`**: converted `ExportAsync` / `ExportBatchAsync` → sync `Export` /
  `ExportBatch`; `CancellationToken` is checked at batch boundaries.
- **`Logger` (internal)**: fixed naming bug — `DeleteByIds()` returned `Task` without the `Async`
  suffix; flipped to sync `DeleteByIds()` + `[Obsolete] Task DeleteByIdsAsync()` wrapper.

### Backward compatibility

No runtime break for callers. The only source-level change is that previously non-obsolete async
methods now emit `CS0618` warnings. Implementors of `ISQLiteWriterUtilities` gain a new
`DeleteByIds(long[] ids)` member (no known external implementations).

---

## Phase 2 — Future: LogPipeline sync support (design discussion)

**Question:** Should `LogPipeline` / `ILogMiddleware` support a synchronous contract alongside (or
instead of) the current async one?

### Current situation

```csharp
public interface ILogMiddleware
{
    Task InvokeAsync(LogEntry entry, Func<Task> next);
}
```

The pipeline is invoked synchronously from the log-call thread inside `BatchLogCache.ApplyPipeline`
via `.GetAwaiter().GetResult()`. This means any middleware that genuinely awaits (e.g. an HTTP
call without `ConfigureAwait(false)`) will deadlock if a synchronisation context is present. The
current built-in middleware (`GlobalLogContextMiddleware`) avoids this because it returns
`Task.CompletedTask` immediately.

### Options

**A — Keep async-only (status quo)**

- Pro: no API change; simple.
- Con: the interface implies that middleware can freely `await`, but in practice doing so from a
  synchronisation-context thread will deadlock. This is a latent footgun for consumers writing
  custom middleware.
- Note: custom middleware that uses `ConfigureAwait(false)` throughout is safe today.

**B — Add a parallel sync `ILogMiddleware` contract**

```csharp
public interface ISyncLogMiddleware
{
    void Invoke(LogEntry entry, Action next);
}
```

`LogPipelineBuilder` would accept either interface, and the internal pipeline would call
`Invoke(...)` directly for sync middleware, sidestepping `.GetAwaiter().GetResult()` entirely.

- Pro: honest about what the calling context allows; removes the `ConfigureAwait(false)` requirement.
- Con: two interfaces; the pipeline builder and executor need to handle mixed registrations.

**C — Move pipeline invocation to the background thread**

Currently the pipeline runs on the *caller's* thread so that ambient context (e.g.
`GlobalLogContext`) is captured at call time. Moving execution to the background thread would break
this without extra work.

Possible mitigation: capture `GlobalLogContext` values before enqueue and restore them on the
background thread. This is already how the pipeline interacts with `GlobalLogContextMiddleware`
— the middleware reads from the same static dictionary that was populated on the caller's thread.
Whether moving the invocation site is safe depends on whether consumers' custom middleware relies on
any other ambient context (e.g. `AsyncLocal<T>`, `HttpContext`).

**D — Remove the pipeline; replace with a callback event**

`LogEntryReceived` already exists as an event callback. A simpler "pre-write transform" hook could
replace the pipeline without async complexity.

- Pro: very clear semantics; no deadlock risk.
- Con: loses middleware chaining (ability to short-circuit or compose transforms).

**Recommendation (to validate before implementing):** Option B. Add `ISyncLogMiddleware` with a
sync `Invoke(LogEntry, Action)` contract and a matching overload on `LogPipelineBuilder`. Keep the
existing async contract for back-compat. The internal pipeline calls `Invoke` directly for sync
middleware registrations and `.GetAwaiter().GetResult()` only for legacy async ones.

---

## Phase 3 — Future release: Remove `[Obsolete]` methods

Once callers have had adequate time to migrate (suggested: one minor version after Phase 1):

- Remove all `[Obsolete]` async wrapper methods added in Phase 1.
- Remove `Task<int> DeleteAllAsync()` from `ISQLiteWriterUtilities`.
- Remove `Task DeleteByIdsAsync(long[] ids)` from `ISQLiteWriterUtilities` and `Housekeeper`.
- Consider whether `WaitUntilCacheIsEmptyAsync` should be kept (it is genuinely async) or also
  removed if the call sites have all migrated to the sync version.
- Update `CHANGELOG.md`, bump minor version.
