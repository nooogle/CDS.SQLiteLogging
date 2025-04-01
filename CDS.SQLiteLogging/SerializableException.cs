using System.Text;

namespace CDS.SQLiteLogging;

/// <summary>
/// Represents a serializable exception.
/// </summary>
public class SerializableException
{
    /// <summary>
    /// Gets or sets the type of the exception.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message of the exception.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the stack trace of the exception.
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Gets or sets the HRESULT of the exception.
    /// </summary>
    public int? HResult { get; set; }

    /// <summary>
    /// Gets or sets the source of the exception.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Gets or sets the target site of the exception.
    /// </summary>
    public string? TargetSite { get; set; }

    /// <summary>
    /// Gets or sets the data associated with the exception.
    /// </summary>
    public Dictionary<string, object>? Data { get; set; }

    /// <summary>
    /// Gets or sets the inner exception.
    /// </summary>
    public SerializableException? InnerException { get; set; }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Exception Type: {Type}");
        sb.AppendLine($"Message: {Message}");
        if (HResult.HasValue)
        {
            sb.AppendLine($"HResult: {HResult.Value}");
        }
        if (!string.IsNullOrEmpty(Source))
        {
            sb.AppendLine($"Source: {Source}");
        }
        if (!string.IsNullOrEmpty(TargetSite))
        {
            sb.AppendLine($"Target Site: {TargetSite}");
        }
        if (!string.IsNullOrEmpty(StackTrace))
        {
            sb.AppendLine("Stack Trace:");
            sb.AppendLine(StackTrace);
        }
        if (InnerException != null)
        {
            sb.AppendLine("Inner Exception:");
            sb.AppendLine(InnerException.ToString());
        }
        return sb.ToString();
    }
}
