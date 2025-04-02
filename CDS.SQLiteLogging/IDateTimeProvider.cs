namespace CDS.SQLiteLogging;

/// <summary>
/// Interface for providing the current date and time.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current date and time.
    /// </summary>
    DateTimeOffset Now { get; }
}
