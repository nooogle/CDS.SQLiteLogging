using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CDS.SQLiteLogging;


/// <summary>
/// A Microsoft Logging framework compatible logger that logs messages to an SQLite database.
/// </summary>
public class MSSQLiteLogger : ILogger
{
    /// <summary>
    /// Version that will increment every time the database scheme is modified.
    /// This number can be used on the database file name to ensure compatibility.
    /// </summary>
    public static int DBSchemaVersion { get; } = 8;


    private readonly string categoryName;
    private readonly SQLiteWriter externalSQLiteWriter;
    private readonly IExternalScopeProvider scopeProvider;

    /// <inheritdoc/>
    public int PendingEntriesCount => externalSQLiteWriter.PendingEntriesCount;

    /// <inheritdoc/>
    public int DiscardedEntriesCount => externalSQLiteWriter.DiscardedEntriesCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="MSSQLiteLogger"/> class.
    /// </summary>
    /// <param name="categoryName">The category name for the logger.</param>
    /// <param name="externalSQLiteWriter">The SQLite logger instance. We don't own this instance and mustn't dispose it!</param>
    /// <param name="scopeProvider">The scope provider for managing logging scopes.</param>
    /// <exception cref="ArgumentNullException">Thrown if any required parameter is null.</exception>
    internal MSSQLiteLogger(string categoryName, SQLiteWriter externalSQLiteWriter, IExternalScopeProvider scopeProvider)
    {
        this.categoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
        this.externalSQLiteWriter = externalSQLiteWriter ?? throw new ArgumentNullException(nameof(externalSQLiteWriter));
        this.scopeProvider = scopeProvider ?? throw new ArgumentNullException(nameof(scopeProvider));
    }


    /// <summary>
    /// Begins a logical operation scope.
    /// </summary>
    /// <typeparam name="TState">The type of the state to associate with the scope.</typeparam>
    /// <param name="state">The identifier for the scope.</param>
    /// <returns>An <see cref="IDisposable"/> that ends the logical operation scope on dispose.</returns>
    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return scopeProvider.Push(state);
    }

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
    /// <exception cref="ArgumentNullException">Thrown if formatter is null.</exception>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (formatter == null)
        {
            throw new ArgumentNullException(nameof(formatter));
        }

        if (!IsEnabled(logLevel))
        {
            return;
        }

        // Gather scope information if present
        string? scopesJson = GetScopesJson();

        var messageTemplate = ExtractTemplate(state);
        var formattedMessage = formatter(state, exception);

        var logEntry = new LogEntry(
            timeStamp: DateTimeOffset.Now,
            category: categoryName,
            level: logLevel,
            eventId: eventId.Id,
            eventName: eventId.Name ?? string.Empty,
            messageTemplate: messageTemplate ?? formattedMessage,
            properties: ExtractStructuredParams(state),
            ex: exception,
            scopesJson: scopesJson);

        externalSQLiteWriter.Add(logEntry);
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
                if (kv.Key == "{OriginalFormat}" && kv.Value != null)
                {
                    return kv.Value.ToString();
                }
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
                .Where(kv => kv.Key != "{OriginalFormat}" && kv.Value != null)
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
            if (scope == null)
            {
                return;
            }

            switch (scope)
            {
                case IEnumerable<KeyValuePair<string, object>> kvps:
                    foreach (var kvp in kvps)
                    {
                        if (kvp.Key != "{OriginalFormat}" && kvp.Value != null && !state.ContainsKey(kvp.Key))
                        {
                            state[kvp.Key] = kvp.Value;
                        }
                    }
                    break;

                case string str:
                    state[$"scope_{unnamedScopeCount++}"] = str;
                    break;

                default:
                    var scopeString = scope.ToString();
                    if (scopeString != null)
                    {
                        state[$"scope_{unnamedScopeCount++}"] = scopeString;
                    }
                    break;
            }
        }, flattened);

        return flattened.Count == 0
            ? null
            : JsonConvert.SerializeObject(flattened);
    }

    /// <inheritdoc/>
    public int DeleteAll()
    {
        return externalSQLiteWriter.DeleteAll();
    }

    /// <inheritdoc/>
    public long GetDatabaseFileSize()
    {
        return externalSQLiteWriter.GetDatabaseFileSize();
    }
}
