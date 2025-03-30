using CDS.SQLiteLogging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ConsoleTest.CustomLogEntry;


// TODO this should use the new rendering message etc.


/// <summary>
/// Represents a log entry with additional properties for batch processing and sender information.
/// </summary>
public sealed class MyLogEntry : ILogEntry
{
    /// <summary>
    /// The message parameters for structured logging.
    /// </summary>
    private Dictionary<string, object> msgParams = null;


    /// <summary>
    /// A formatter for structured log messages.
    /// </summary>
    private static readonly StructuredMessageFormatter structuredMessageFormatter = new();


    /// <summary>
    /// JSON settings for serializing and deserializing message parameters.
    /// </summary>
    private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto
    };


    /// <summary>
    /// Increment this value every time this class is modified.
    /// </summary>
    public static int Version { get; } = 4;

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
    /// Gets or sets the batch identifier for the log entry.
    /// </summary>
    public string BatchId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the line index in the batch.
    /// </summary>
    public int LineIndex { get; set; }

    /// <summary>
    /// Gets or sets the sender of the log entry.
    /// </summary>
    public string Sender { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message of the log entry.
    /// </summary>
    public string MessageTemplate { get; set; }

    /// <summary>
    /// Gets or sets the message parameters for structured logging.
    /// </summary>
    public IReadOnlyDictionary<string, object> Properties
    {
        get => msgParams;
        set => msgParams = value == null ? null : value.ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    /// <inheritdoc/>
    public string ExceptionJson { get; set; }

    /// <inheritdoc/>
    public void SetException(Exception ex) => ExceptionJson = ExceptionSerializer.ToJson(ex);


    /// <inheritdoc/>
    public SerializableException? GetExceptionInfo() => ExceptionSerializer.FromJson(ExceptionJson);


    /// <summary>
    /// Gets the formatted message with parameters substituted.
    /// </summary>
    /// <returns>The formatted message.</returns>
    public string GetFormattedMsg() => structuredMessageFormatter.Format(MessageTemplate, Properties);

    /// <summary>
    /// Serializes the message parameters to a JSON string.
    /// </summary>
    /// <returns>A JSON string representing the message parameters.</returns>
    public string SerializeMsgParams() => JsonConvert.SerializeObject(Properties, jsonSettings);

    /// <summary>
    /// Deserializes a JSON string to populate the message parameters.
    /// </summary>
    /// <param name="json">The JSON string representing the message parameters.</param>
    public void DeserializeMsgParams(string json) => Properties = JsonConvert.DeserializeObject<Dictionary<string, object>>(json, jsonSettings);
}
