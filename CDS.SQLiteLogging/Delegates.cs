namespace CDS.SQLiteLogging;


/// <summary>
/// Delegate for handling the event when a log entry is about to be added.
/// </summary>
/// <typeparam name="TLogEntry">The type of the log entry.</typeparam>
/// <param name="logEntry">The log entry that is about to be added.</param>
/// <param name="shouldIgnore">A reference to a boolean indicating whether the log entry should be ignored.</param>
public delegate void OnAboutToAddLogEntry<TLogEntry>(TLogEntry logEntry, ref bool shouldIgnore) where TLogEntry : ILogEntry;


/// <summary>
/// Delegate for handling the event when a log entry has been added.
/// </summary>
/// <typeparam name="TLogEntry">The type of the log entry.</typeparam>
/// <param name="logEntry">The log entry that has been added.</param>
public delegate void OnAddedLogEntry<TLogEntry>(TLogEntry logEntry) where TLogEntry : ILogEntry;


