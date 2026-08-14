# ConnectionManager Dispose Safety — Design Discussion

## Background

Flagged in code review (2026-08-14): `ConnectionManager.Dispose(bool)` calls `semaphore.Wait()`
with no timeout before closing the connection:

```csharp
protected virtual void Dispose(bool disposing)
{
    if (!disposed)
    {
        if (disposing && connection != null)
        {
            semaphore.Wait();
            try { connection.Close(); connection.Dispose(); }
            finally { semaphore.Dispose(); }
        }
        disposed = true;
    }
}
```

`ExecuteNonQueryGuarded`, `ExecuteInTransaction`, and `ExecuteWithRetry` all take the same
semaphore and none of them check `disposed` before or after acquiring it.

**Normal contention is fine** — if one thread holds the semaphore running a query while another
thread calls `Dispose()`, the disposing thread just blocks until the query finishes, which is the
intended serialization. The actual risk is a same-thread reentrant call: if code running *inside* an
`ExecuteInTransaction`/`ExecuteWithRetry` action (or a custom `ILogMiddleware` invoked from that
call stack) ends up calling `Dispose()` on the owning `Housekeeper`/`ConnectionManager` before the
outer call returns, `SemaphoreSlim.Wait()` blocks forever on the same thread that already holds the
one available slot — nothing can ever release it. There's no known call path that does this in the
current codebase today, but nothing prevents a future caller (or consumer's custom middleware) from
introducing one, and if it happens the failure mode is a silent, undiagnosable hang rather than an
exception.

## Options

**A — Leave as-is**
- Pro: no change, no risk of altering behavior under normal (non-reentrant) contention.
- Con: if reentrancy is ever introduced, the failure is a silent hang with no exception, log, or
  stack trace pointing at the cause.

**B — Timed `Wait` with a clear failure mode**

Replace `semaphore.Wait()` in `Dispose(bool)` (and optionally the other guarded methods) with
`semaphore.Wait(timeout)`, throwing a descriptive `InvalidOperationException` (or logging and
proceeding best-effort) if the timeout elapses.
- Pro: converts a silent hang into a fast, diagnosable failure.
- Con: needs a timeout value that's long enough not to trip under legitimate slow operations (e.g.
  `VACUUM` on a large table) but short enough to be useful — this is a judgment call, not something
  derivable from the code. Also raises the question of whether `Dispose()` should ever throw (typical
  .NET guidance says no) versus logging and disposing best-effort anyway.

**C — Guard the guarded methods with the `disposed` flag**

Add an early check (`if (disposed) throw new ObjectDisposedException(...)`) to
`ExecuteNonQueryGuarded`, `ExecuteInTransaction`, and `ExecuteWithRetry`, and set `disposed = true`
before acquiring the semaphore in `Dispose(bool)` rather than after.
- Pro: shrinks the window in which a new operation can start after disposal has begun; makes misuse
  (calling a guarded method after `Dispose()`) fail clearly instead of silently.
- Con: does not, by itself, prevent the reentrant same-thread deadlock described above — an
  operation already in flight on the same thread has already passed the check.

**D — Reentrancy detection**

Track the managing thread ID while the semaphore is held; if `Dispose()` is entered from that same
thread while it's held, fail fast with a clear exception instead of calling `Wait()` at all.
- Pro: directly targets the actual failure mode.
- Con: most invasive option; adds state and complexity for a scenario with no known trigger today.

## Recommendation (to validate before implementing)

B + C together: add a bounded timeout to the `Dispose(bool)` wait so a hang becomes a diagnosable
error instead of an infinite block, and add `disposed` checks to the guarded methods so misuse after
disposal is explicit. Skip D unless a real reentrant call path is identified — it's solving a
theoretical problem with real added complexity.

Open question for whoever picks this up: should the timeout expiry in `Dispose()` throw, or log and
dispose best-effort? Throwing from `Dispose()` is unconventional and could mask the original bug
that caused the reentrancy; logging and proceeding matches the existing `Debug.WriteLine`-and-continue
pattern used elsewhere in `Housekeeper`, but risks disposing a connection that's mid-operation on
another thread.
