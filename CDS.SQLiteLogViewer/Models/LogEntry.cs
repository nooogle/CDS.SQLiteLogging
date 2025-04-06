namespace CDS.SQLiteLogViewer.Models;

public class LogEntry
{
    public long DbId { get; set; }
    public LogLevel Level { get; set; }
    public string? Category { get; set; }
    public int EventId { get; set; }
    public string? EventName { get; set; }
    public SerializableException? SerializedException { get; set; } 
    public string ExceptionMessage => SerializedException == null ? string.Empty : SerializedException.Message;
    public string MessageTemplate { get; set; } = "";
    public IReadOnlyDictionary<string, object>? Properties { get; set; }
    public string RenderedMessage { get; set; } = "";
    public string? ScopesJson { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

public enum LogLevel
{
    Trace,
    Debug,
    Information,
    Warning,
    Error,
    Critical,
    None
}
