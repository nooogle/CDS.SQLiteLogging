namespace CDS.SQLiteLogging;

/// <summary>
/// Represents a basic log entry with essential properties.
/// </summary>
public interface ILogEntry
{
    /// <summary>
    /// Gets the unique identifier for the log entry.
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Gets the timestamp when the log entry was created.
    /// </summary>
    DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// The log level.
    /// </summary>
    LogLevel Level { get; set; }

    /// <summary>
    /// Gets the message of the log entry. Use the MsgParams if this is a structured message.
    /// </summary>
    string Message { get; set; }

    /// <summary>
    /// Gets or sets the message parameters for structured logging.
    /// </summary>
    IReadOnlyDictionary<string, object> MsgParams { get; set; }

    /// <summary>
    /// Serializes the MsgParams dictionary to a JSON string.
    /// </summary>
    /// <returns>A JSON string representing the message parameters.</returns>
    string SerializeMsgParams();

    /// <summary>
    /// Deserializes a JSON string to populate the MsgParams dictionary.
    /// </summary>
    /// <param name="json">The JSON string representing the message parameters.</param>
    void DeserializeMsgParams(string json);
}
