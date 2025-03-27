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
    public static int Version { get; } = 2;


    private Dictionary<string, object> msgParams = null;

    private static readonly StructuredMessageFormatter structuredMessageFormatter = new();

    private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto
    };

    /// <summary>
    /// Gets or sets the unique identifier for the log entry.
    /// </summary>
    public int Id { get; set; }

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
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message parameters for structured logging.
    /// </summary>
    public IReadOnlyDictionary<string, object> MsgParams
    {
        get => msgParams;
        set => msgParams = value == null ? null : value.ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    /// <summary>
    /// Gets the formatted message with parameters substituted.
    /// </summary>
    /// <returns>The formatted message.</returns>
    public string GetFormattedMsg()
    {
        return structuredMessageFormatter.Format(Message, msgParams);
    }

    /// <summary>
    /// Serializes the message parameters to a JSON string.
    /// </summary>
    /// <returns>A JSON string representing the message parameters.</returns>
    public string SerializeMsgParams()
    {
        return JsonConvert.SerializeObject(msgParams, jsonSettings);
    }

    /// <summary>
    /// Deserializes a JSON string to populate the message parameters.
    /// </summary>
    /// <param name="json">The JSON string representing the message parameters.</param>
    public void DeserializeMsgParams(string json)
    {
        msgParams = JsonConvert.DeserializeObject<Dictionary<string, object>>(json, jsonSettings);
    }
}
