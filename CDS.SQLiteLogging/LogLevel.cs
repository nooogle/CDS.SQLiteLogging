namespace CDS.SQLiteLogging;

/// <summary>
/// Defines severity levels for log entries.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Very detailed, diagnostic messages. Used for deep troubleshooting.
    /// </summary>
    Trace = 0,

    /// <summary>
    /// Short-term useful debugging info during development.
    /// </summary>
    Debug = 1,

    /// <summary>
    /// Normal runtime events (e.g., "User logged in"). Typically enabled in prod.
    /// </summary>
    Information = 2,

    /// <summary>
    /// Unexpected events that don't cause failure (e.g., retry attempts).
    /// </summary>
    Warning = 3,

    /// <summary>
    /// Recoverable failures (e.g., failed save to DB).
    /// </summary>
    Error = 4,

    /// <summary>
    /// Fatal issues causing system shutdown or major malfunction.
    /// </summary>
    Critical = 5,

    /// <summary>
    /// Special value to disable logging (used for filtering).
    /// </summary>
    None = 6
}
