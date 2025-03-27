namespace SqliteLogger.Tests.TestSupport;

/// <summary>
/// A simple log entry implementation for testing purposes.
/// </summary>
public class TestLogEntry : ILogEntry
{
    private readonly LogEntry logEntry = new();

    /// <summary>
    /// Increment this value every time this class is modified.
    /// </summary>
    public static int Version { get; } = 2;

    public int Id
    {
        get => logEntry.Id;
        set => logEntry.Id = value;
    }

    public DateTimeOffset Timestamp
    {
        get => logEntry.Timestamp;
        set => logEntry.Timestamp = value;
    }

    public LogLevel Level
    {
        get => logEntry.Level;
        set => logEntry.Level = value;
    }


    /// <summary>
    /// Gets or sets the sender/source of the log entry.
    /// </summary>
    public string Sender { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional details for the log entry.
    /// </summary>
    public string Details { get; set; } = string.Empty;

    public string Message
    {
        get => logEntry.Message;
        set => logEntry.Message = value;
    }

    public IReadOnlyDictionary<string, object> MsgParams
    {
        get => logEntry.MsgParams;
        set => logEntry.MsgParams = value.ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    public string GetFormattedMsg()
    {
        return logEntry.GetFormattedMsg();
    }

    public string SerializeMsgParams()
    {
        return logEntry.SerializeMsgParams();
    }

    public void DeserializeMsgParams(string json)
    {
        logEntry.DeserializeMsgParams(json);
    }
}
