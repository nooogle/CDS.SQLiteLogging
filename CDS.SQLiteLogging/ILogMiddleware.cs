using System;
using System.Threading.Tasks;

namespace CDS.SQLiteLogging;

/// <summary>
/// Defines a middleware component for log processing.
/// </summary>
public interface ILogMiddleware
{
    /// <summary>
    /// Invokes the middleware logic for a log entry.
    /// </summary>
    /// <param name="entry">The log entry to process.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InvokeAsync(LogEntry entry, Func<Task> next);
}
