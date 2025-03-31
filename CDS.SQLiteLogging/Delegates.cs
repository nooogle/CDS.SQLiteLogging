namespace CDS.SQLiteLogging;

/// <summary>
/// Delegate for handling the event when a log entry is about to be added.
/// </summary>
/// <param name="logEntry">The log entry that is about to be added.</param>
/// <param name="shouldIgnore">A reference to a boolean indicating whether the log entry should be ignored.</param>
public delegate void OnAboutToAddLogEntry(LogEntry logEntry, ref bool shouldIgnore);

/// <summary>
/// Delegate for handling the event when a log entry has been added.
/// </summary>
/// <param name="logEntry">The log entry that has been added.</param>
public delegate void OnAddedLogEntry(LogEntry logEntry);
