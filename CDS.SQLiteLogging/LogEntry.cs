using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CDS.SQLiteLogging;

/// <summary>
/// Represents a basic log entry with essential properties and methods.
/// </summary>
public class LogEntry : ILogEntry
{
    /// <summary>
    /// Increment this value every time this class is modified.
    /// </summary>
    public static int Version { get; } = 8;


    private Dictionary<string, object> properties = null;

    private static readonly StructuredMessageFormatter structuredMessageFormatter = new();

    private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto
    };

    /// <summary>
    /// Gets or sets the unique identifier for the log entry.
    /// </summary>
    public int DbId { get; set; }


    /// <inheritdoc/>
    public string Category { get; set; }


    /// <inheritdoc/>
    public int EventId { get; set; }


    /// <inheritdoc/>
    public string EventName { get; set; }


    /// <summary>
    /// Gets or sets the timestamp when the log entry was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the log level.
    /// </summary>
    public LogLevel Level { get; set; }

    /// <summary>
    /// Gets or sets the message of the log entry. If used in the context of structured logging, this should be a template.
    /// For example, "User {Username} logged in.". Use the MsgParams for the actual values.
    /// </summary>
    public string MessageTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message parameters for structured logging.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Properties
    {
        get => properties;
        set => properties = value == null ? null : value.ToDictionary(kv => kv.Key, kv => kv.Value);
    }


    /// <summary>
    /// Gets or sets the rendered message. This is the formatted message with parameters substituted.
    /// </summary>
    public string RenderedMessage { get; set; } = string.Empty;


    /// <inheritdoc/>
    public string ExceptionJson { get; set; }


    public string? ScopesJson { get; set; }


    public LogEntry()
    {
        Timestamp = DateTimeOffset.Now;
    }


    public LogEntry(
        DateTimeOffset timeStamp,
        string category,
        LogLevel level,
        int eventId,
        string eventName,
        string messageTemplate,
        IReadOnlyDictionary<string, object>? properties,
        Exception? ex,
        string? scopesJson)
    {
        Timestamp = timeStamp;
        Category = category;
        Level = level;
        EventId = eventId;
        EventName = eventName;
        Timestamp = DateTimeOffset.Now;
        MessageTemplate = messageTemplate;
        Properties = properties;
        ExceptionJson = ExceptionSerializer.ToJson(ex);
        ScopesJson = scopesJson;

        RenderedMessage = GetFormattedMsg();
    }


    /// <inheritdoc/>
    public void SetException(Exception ex) => ExceptionJson = ExceptionSerializer.ToJson(ex);


    /// <inheritdoc/>
    public SerializableException? GetExceptionInfo() => ExceptionSerializer.FromJson(ExceptionJson);


    /// <summary>
    /// Gets the formatted message with parameters substituted.
    /// </summary>
    /// <returns>The formatted message.</returns>
    public string GetFormattedMsg()
    {
        return structuredMessageFormatter.Format(MessageTemplate, properties);
    }

    /// <summary>
    /// Serializes the message parameters to a JSON string.
    /// </summary>
    /// <returns>A JSON string representing the message parameters.</returns>
    public string SerializeMsgParams()
    {
        return JsonConvert.SerializeObject(properties, jsonSettings);
    }

    /// <summary>
    /// Deserializes a JSON string to populate the message parameters.
    /// </summary>
    /// <param name="json">The JSON string representing the message parameters.</param>
    public void DeserializeMsgParams(string json)
    {
        properties = JsonConvert.DeserializeObject<Dictionary<string, object>>(json, jsonSettings);
    }
}
