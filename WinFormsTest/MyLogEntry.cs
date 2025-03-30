using CDS.SQLiteLogging;
using Microsoft.Extensions.Logging;

namespace WinFormsTest;

sealed class MyLogEntry : ILogEntry
{
    private readonly LogEntry logEntry = new();

    /// <summary>
    /// Increment this value every time this class is modified.
    /// </summary>
    public static int Version { get; } = 2;

    public int DbId
    {
        get => logEntry.DbId;
        set => logEntry.DbId = value;
    }

    public EventId EventId
    {
        get => logEntry.EventId;
        set => logEntry.EventId = value;
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

    public string BatchNumber { get; set; } = string.Empty;

    public int LineIndex { get; set; }

    public string Sender { get; set; } = string.Empty;


    public string MessageTemplate
    {
        get => logEntry.MessageTemplate;
        set => logEntry.MessageTemplate = value;
    }

    public IReadOnlyDictionary<string, object> Properties
    {
        get => logEntry.Properties;
        set => logEntry.Properties = value.ToDictionary(kv => kv.Key, kv => kv.Value);
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
