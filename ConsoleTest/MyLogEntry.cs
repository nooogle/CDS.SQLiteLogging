using CDS.SQLiteLogging;

namespace ConsoleTest;

/// <summary>
/// Represents a log entry with additional properties for batch processing and sender information.
/// </summary>
public sealed class MyLogEntry : ILogEntry
{
    private readonly LogEntry logEntry = new();

    /// <summary>
    /// Increment this value every time this class is modified.
    /// </summary>
    public static int Version { get; } = 2;

    /// <summary>
    /// Gets or sets the unique identifier for the log entry.
    /// </summary>
    public int Id
    {
        get => logEntry.Id;
        set => logEntry.Id = value;
    }

    /// <summary>
    /// Gets or sets the timestamp when the log entry was created.
    /// </summary>
    public DateTimeOffset Timestamp
    {
        get => logEntry.Timestamp;
        set => logEntry.Timestamp = value;
    }

    /// <summary>
    /// Gets or sets the log level.
    /// </summary>
    public LogLevel Level
    {
        get => logEntry.Level;
        set => logEntry.Level = value;
    }

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
    public string Message
    {
        get => logEntry.Message;
        set => logEntry.Message = value;
    }

    /// <summary>
    /// Gets or sets the message parameters for structured logging.
    /// </summary>
    public IReadOnlyDictionary<string, object> MsgParams
    {
        get => logEntry.MsgParams;
        set => logEntry.MsgParams = value.ToDictionary(kv => kv.Key, kv => kv.Value);  
    }

    /// <summary>
    /// Gets the formatted message with parameters substituted.
    /// </summary>
    /// <returns>The formatted message.</returns>
    public string GetFormattedMsg() => logEntry.GetFormattedMsg();

    /// <summary>
    /// Serializes the message parameters to a JSON string.
    /// </summary>
    /// <returns>A JSON string representing the message parameters.</returns>
    public string SerializeMsgParams() => logEntry.SerializeMsgParams();

    /// <summary>
    /// Deserializes a JSON string to populate the message parameters.
    /// </summary>
    /// <param name="json">The JSON string representing the message parameters.</param>
    public void DeserializeMsgParams(string json) => logEntry.DeserializeMsgParams(json);
}
