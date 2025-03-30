using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CDS.SQLiteLogging.Microsoft;

public class MSSQLiteLogger : ILogger, IDisposable
{
    private readonly string categoryName;
    private readonly SQLiteLogger<LogEntry> logger;
    private readonly IExternalScopeProvider scopeProvider = new LoggerExternalScopeProvider();

    public MSSQLiteLogger(string categoryName, SQLiteLogger<LogEntry> logger, IExternalScopeProvider scopeProvider)
    {
        this.categoryName = categoryName;
        this.logger = logger;
        this.scopeProvider = scopeProvider;
    }

    public void Dispose()
    {
        logger.Flush();
        logger.Dispose();
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return scopeProvider.Push(state);
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        // Gather scope information if present
        string scopesJson = GetScopesJson();

        var logEntry = new LogEntry(
            timeStamp: DateTimeOffset.Now,
            category: categoryName,
            level: logLevel,
            eventId: eventId.Id,
            eventName: eventId.Name,
            messageTemplate: ExtractTemplate(state) ?? formatter(state, exception),
            properties: ExtractStructuredParams(state),
            ex: exception,
            scopesJson: scopesJson);

        logger.Add(logEntry);
    }

    private string? ExtractTemplate<TState>(TState state)
    {
        if (state is IEnumerable<KeyValuePair<string, object>> kvps)
        {
            foreach (var kv in kvps)
            {
                if (kv.Key == "{OriginalFormat}")
                    return kv.Value?.ToString();
            }
        }
        return null;
    }

    private IReadOnlyDictionary<string, object>? ExtractStructuredParams<TState>(TState state)
    {
        if (state is IEnumerable<KeyValuePair<string, object>> kvps)
        {
            var dict = kvps
                .Where(kv => kv.Key != "{OriginalFormat}")
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            return dict.Count > 0 ? dict : null;
        }

        return null;
    }


    private string? GetScopesJson()
    {
        var flattened = new Dictionary<string, object>();
        int unnamedScopeCount = 0;

        scopeProvider.ForEachScope((scope, state) =>
        {
            switch (scope)
            {
                case IEnumerable<KeyValuePair<string, object>> kvps:
                    foreach (var kvp in kvps)
                    {
                        if (kvp.Key != "{OriginalFormat}" && !state.ContainsKey(kvp.Key))
                        {
                            state[kvp.Key] = kvp.Value;
                        }
                    }
                    break;

                case string str:
                    state[$"scope_{unnamedScopeCount++}"] = str;
                    break;

                default:
                    state[$"scope_{unnamedScopeCount++}"] = scope?.ToString() ?? "(null)";
                    break;
            }
        }, flattened);

        return flattened.Count == 0
            ? null
            : JsonConvert.SerializeObject(flattened);
    }

}
