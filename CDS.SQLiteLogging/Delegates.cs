namespace CDS.SQLiteLogging;

/// <summary>
/// Represents the method that handles a log entry received event.
/// </summary>
/// <param name="logEntry">The log entry that was received.</param>
public delegate void LogEntryReceivedEvent(LogEntry logEntry);
