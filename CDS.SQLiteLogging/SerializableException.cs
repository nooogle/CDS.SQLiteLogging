namespace CDS.SQLiteLogging;

public class SerializableException
{
    public string Type { get; set; } = "";
    public string Message { get; set; } = "";
    public string? StackTrace { get; set; }
    public int? HResult { get; set; }
    public string? Source { get; set; }
    public string? TargetSite { get; set; }
    public Dictionary<string, object>? Data { get; set; }
    public SerializableException? InnerException { get; set; }
}
