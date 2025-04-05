namespace CDS.SQLiteLogViewer.Models;

public class LogEntry
{
    public int DbId { get; set; }
    public LogLevel Level { get; set; }
    public string? Category { get; set; }
    public int EventId { get; set; }
    public string? EventName { get; set; }
    public string? ExceptionJson { get; set; }
    public string MessageTemplate { get; set; } = "";
    public IReadOnlyDictionary<string, object>? Properties { get; set; }
    public string RenderedMessage { get; set; } = "";
    public string? ScopesJson { get; set; }
    public DateTimeOffset Timestamp { get; set; }

    public string? GetExceptionInfo() => ExceptionJson;
    public string GetFormattedMsg() => RenderedMessage;
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
