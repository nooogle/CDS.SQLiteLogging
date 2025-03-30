using Microsoft.Extensions.Logging;

namespace CDS.SQLiteLogging.Microsoft;

public class MSSQLiteLogger : ILogger, IDisposable
{
    private readonly string categoryName;
    private readonly SQLiteLogger<LogEntry> logger;

    public MSSQLiteLogger(string categoryName, SQLiteLogger<LogEntry> logger)
    {
        this.categoryName = categoryName;
        this.logger = logger;
    }

    public void Dispose()
    {
        logger.Flush();
        logger.Dispose();
    }

    public IDisposable BeginScope<TState>(TState state) => new LoggerScope(state);

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var logEntry = new LogEntry(
            timeStamp: DateTimeOffset.Now,
            category: categoryName,
            level: logLevel,
            eventId: eventId.Id,
            eventName: eventId.Name,
            messageTemplate: ExtractTemplate(state) ?? formatter(state, exception),
            properties: ExtractStructuredParams(state),
            ex: exception);

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
}
