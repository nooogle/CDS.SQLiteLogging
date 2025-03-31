using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CDS.SQLiteLogging.Microsoft;

//// TODO other methods and async varients
//public interface IMSSQLiteLogger : ILogger
//{
//    void Flush();


//    /// <summary>
//    /// Deletes all log entries from the database.
//    /// </summary>
//    /// <returns>
//    /// Number of entries deleted.
//    /// </returns>
//    int DeleteAll();


//    /// <summary>
//    /// Gets the size of the database file.
//    /// </summary>
//    /// <returns>
//    /// Size of the database file in bytes.
//    /// </returns>
//    long GetDatabaseFileSize();

//    int PendingEntriesCount { get; } 

//    int DiscardedEntriesCount { get; }
//}


/// <summary>
/// A logger implementation that logs messages to an SQLite database.
/// </summary>
public class MSSQLiteLogger : ILogger, IDisposable
{
    private readonly string categoryName;
    private readonly SQLiteLogger logger;
    private readonly IExternalScopeProvider scopeProvider;


    /// <inheritdoc/>
    public int PendingEntriesCount => logger.PendingEntriesCount;

    /// <inheritdoc/>
    public int DiscardedEntriesCount => logger.DiscardedEntriesCount;


    /// <summary>
    /// Initializes a new instance of the <see cref="MSSQLiteLogger"/> class.
    /// </summary>
    /// <param name="categoryName">The category name for the logger.</param>
    /// <param name="logger">The SQLite logger instance.</param>
    /// <param name="scopeProvider">The scope provider for managing logging scopes.</param>
    public MSSQLiteLogger(string categoryName, SQLiteLogger logger, IExternalScopeProvider scopeProvider)
    {
        this.categoryName = categoryName;
        this.logger = logger;
        this.scopeProvider = scopeProvider;
    }

    /// <summary>
    /// Disposes the logger and flushes any pending log entries.
    /// </summary>
    public void Dispose()
    {
        logger.Flush();
        logger.Dispose();
    }

    /// <summary>
    /// Begins a logical operation scope.
    /// </summary>
    /// <typeparam name="TState">The type of the state to associate with the scope.</typeparam>
    /// <param name="state">The identifier for the scope.</param>
    /// <returns>An <see cref="IDisposable"/> that ends the logical operation scope on dispose.</returns>
    public IDisposable BeginScope<TState>(TState state) => scopeProvider.Push(state);

    /// <summary>
    /// Checks if the given log level is enabled.
    /// </summary>
    /// <param name="logLevel">The log level to check.</param>
    /// <returns><c>true</c> if the log level is enabled; otherwise, <c>false</c>.</returns>
    public bool IsEnabled(LogLevel logLevel) => true;

    /// <summary>
    /// Writes a log entry.
    /// </summary>
    /// <typeparam name="TState">The type of the state object.</typeparam>
    /// <param name="logLevel">The log level.</param>
    /// <param name="eventId">The event ID.</param>
    /// <param name="state">The state object.</param>
    /// <param name="exception">The exception to log, or <c>null</c> if none.</param>
    /// <param name="formatter">The function to create a log message from the state and exception.</param>
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

    /// <summary>
    /// Extracts the original message template from the state if available.
    /// </summary>
    /// <typeparam name="TState">The type of the state object.</typeparam>
    /// <param name="state">The state object.</param>
    /// <returns>The original message template if found; otherwise, <c>null</c>.</returns>
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

    /// <summary>
    /// Extracts structured parameters from the state if available.
    /// </summary>
    /// <typeparam name="TState">The type of the state object.</typeparam>
    /// <param name="state">The state object.</param>
    /// <returns>A dictionary of structured parameters if found; otherwise, <c>null</c>.</returns>
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

    /// <summary>
    /// Gathers scope information and serializes it to JSON.
    /// </summary>
    /// <returns>A JSON string representing the scope information, or <c>null</c> if no scopes are present.</returns>
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

    /// <inheritdoc/>
    public void Flush()
    {
        logger.Flush();
    }

    /// <inheritdoc/>
    public int DeleteAll()
    {
        int deleteCount = logger.DeleteAll();
        return deleteCount;
    }


    /// <inheritdoc/>
    public long GetDatabaseFileSize()
    {
        return logger.GetDatabaseFileSize();
    }
}
