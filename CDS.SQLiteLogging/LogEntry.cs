using CDS.SQLiteLogging.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CDS.SQLiteLogging;

/// <summary>
/// Represents a log entry.
/// </summary>
public class LogEntry
{
    private static int nextLiveId = 1; // Static variable to keep track of the next LiveId


    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    public override string ToString() => $"[{Timestamp.ToLocalTime():yyyy-MM-dd HH:mm:ss.fff}] {RenderedMessage}";


    /// <summary>
    /// Stores the message parameters for structured logging.
    /// </summary>
    private Dictionary<string, object>? properties = null;

    /// <summary>
    /// Formatter for structured log messages.
    /// </summary>
    private static readonly StructuredMessageFormatter structuredMessageFormatter = new();

    /// <summary>
    /// JSON serializer settings.
    /// </summary>
    private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto
    };

    /// <summary>
    /// Gets or sets the unique identifier for the log entry.
    /// </summary>
    public long DbId { get; set; }

    /// <summary>
    /// Returns the Live Identifier - this is assigned each time a log entry object
    /// is created. It is used to identify the log entry in a live environent only,
    /// since log entries sent to the database only have the DbId assigned
    /// via a batch insert thread. When reading back log entries from the database,
    /// use the DbId property. When displaying log entires in a live environment,
    /// use the LiveId property.
    /// </summary>
    public int LiveId { get; set; } 

    /// <summary>
    /// Gets or sets the category of the log entry.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets the event ID of the log entry.
    /// </summary>
    public int EventId { get; set; }

    /// <summary>
    /// Gets or sets the event name of the log entry.
    /// </summary>
    public string? EventName { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the log entry was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the log level.
    /// </summary>
    public LogLevel Level { get; set; }

    /// <summary>
    /// Gets or sets the managed thread identifier for the log entry.
    /// </summary>
    public int ManagedThreadId { get; set; }

    /// <summary>
    /// Gets or sets the message of the log entry. If used in the context of structured logging, this should be a template.
    /// For example, "User {Username} logged in.". Use the MsgParams for the actual values.
    /// </summary>
    public string MessageTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message parameters for structured logging.
    /// </summary>
    public Dictionary<string, object>? Properties
    {
        get => properties;
        set => properties = value == null ? null : new Dictionary<string, object>(value);
    }

    /// <summary>
    /// Gets or sets the rendered message. This is the formatted message with parameters substituted.
    /// </summary>
    public string RenderedMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JSON representation of the exception.
    /// </summary>
    public string? ExceptionJson { get; set; }

    /// <summary>
    /// Gets or sets the JSON representation of the scopes.
    /// </summary>
    public string? ScopesJson { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogEntry"/> class.
    /// </summary>
    public LogEntry()
    {
        Timestamp = DateTimeOffset.Now;
        ManagedThreadId = Environment.CurrentManagedThreadId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogEntry"/> class with specified parameters.
    /// </summary>
    /// <param name="timeStamp">The timestamp of the log entry.</param>
    /// <param name="category">The category of the log entry.</param>
    /// <param name="level">The log level.</param>
    /// <param name="eventId">The event ID.</param>
    /// <param name="eventName">The event name.</param>
    /// <param name="messageTemplate">The message template.</param>
    /// <param name="properties">The message parameters.</param>
    /// <param name="ex">The exception.</param>
    /// <param name="scopesJson">The JSON representation of the scopes.</param>
    public LogEntry(
        DateTimeOffset timeStamp,
        string category,
        LogLevel level,
        int eventId,
        string eventName,
        string messageTemplate,
        Dictionary<string, object>? properties,
        Exception? ex,
        string? scopesJson)
    {
        // Assign a unique LiveId for this log entry
        LiveId = nextLiveId++;

        ManagedThreadId = Environment.CurrentManagedThreadId;

        Timestamp = timeStamp;
        Category = category;
        Level = level;
        EventId = eventId;
        EventName = eventName;
        MessageTemplate = messageTemplate;
        Properties = properties is Dictionary<string, object> d ? d : (properties != null ? new Dictionary<string, object>(properties) : null);
        ExceptionJson = ExceptionSerializer.ToJson(ex);
        ScopesJson = scopesJson;

        RenderedMessage = GetFormattedMsg();
    }

    /// <summary>
    /// Sets the exception for the log entry.
    /// </summary>
    /// <param name="ex">The exception to set.</param>
    public void SetException(Exception ex) => ExceptionJson = ExceptionSerializer.ToJson(ex);

    /// <summary>
    /// Gets the exception information from the JSON representation.
    /// </summary>
    /// <returns>A <see cref="SerializableException"/> object.</returns>
    public SerializableException? GetExceptionInfo() => ExceptionSerializer.FromJson(ExceptionJson);

    /// <summary>
    /// Gets the formatted message with parameters substituted.
    /// </summary>
    /// <returns>The formatted message.</returns>
    public string GetFormattedMsg() => structuredMessageFormatter.Format(MessageTemplate, properties);

    /// <summary>
    /// Serializes the message parameters to a JSON string.
    /// </summary>
    /// <returns>A JSON string representing the message parameters.</returns>
    public string SerializeMsgParams() => JsonConvert.SerializeObject(properties, jsonSettings);

    /// <summary>
    /// Deserializes a JSON string to populate the message parameters.
    /// </summary>
    /// <param name="json">The JSON string representing the message parameters.</param>
    public void DeserializeMsgParams(string json) => properties = JsonConvert.DeserializeObject<Dictionary<string, object>>(json, jsonSettings);


    /// <summary>
    /// Deserialises the scopes JSON.
    /// </summary>
    /// <returns>
    /// A dictionary of scopes.
    /// </returns>
    public Dictionary<string, string> DeserialiseScopesJson()
    {
        Dictionary<string, string>? result = null;

        if (!string.IsNullOrEmpty(ScopesJson))
        {
            result = JsonConvert.DeserializeObject<Dictionary<string, string>>(ScopesJson!);
        }

        result ??= [];

        return result;
    }
}
