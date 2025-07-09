using System.Threading.Tasks;

namespace CDS.SQLiteLogging;

/// <summary>
/// Middleware that adds global context values to log entries.
/// </summary>
public class GlobalLogContextMiddleware : ILogMiddleware
{
    /// <inheritdoc />
    public Task InvokeAsync(LogEntry entry, Func<Task> next)
    {
        if (GlobalLogContext.Context.Count > 0)
        {
            if (entry.Properties == null)
            {
                entry.Properties = new Dictionary<string, object>();
            }
            var dict = entry.Properties as Dictionary<string, object>;
            foreach (var kvp in GlobalLogContext.Context)
            {
                if (!dict.ContainsKey(kvp.Key))
                {
                    dict[kvp.Key] = kvp.Value;
                }
            }
        }
        return next();
    }
}
