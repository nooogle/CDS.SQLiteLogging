namespace CDS.SQLiteLogging.Internal;

/// <summary>
/// Defines the SQLite database schema for log entries.
/// This class provides a single source of truth for all table and column names used throughout the logging system.
/// </summary>
internal static class DatabaseSchema
{
    /// <summary>
    /// Contains table name constants.
    /// </summary>
    public static class Tables
    {
        /// <summary>
        /// The name of the log entries table.
        /// </summary>
        public const string LogEntry = nameof(LogEntry);
    }

    /// <summary>
    /// Contains column name constants for the LogEntry table.
    /// </summary>
    public static class Columns
    {
        /// <summary>
        /// The database-assigned unique identifier column (PRIMARY KEY AUTOINCREMENT).
        /// </summary>
        public const string DbId = nameof(DbId);

        /// <summary>
        /// The log category column (e.g., namespace.classname).
        /// </summary>
        public const string Category = nameof(Category);

        /// <summary>
        /// The event ID column (numeric identifier for the log event).
        /// </summary>
        public const string EventId = nameof(EventId);

        /// <summary>
        /// The event name column (string identifier for the log event).
        /// </summary>
        public const string EventName = nameof(EventName);

        /// <summary>
        /// The timestamp column (ISO 8601 format string).
        /// </summary>
        public const string Timestamp = nameof(Timestamp);

        /// <summary>
        /// The log level column (0=Trace, 1=Debug, 2=Information, 3=Warning, 4=Error, 5=Critical).
        /// </summary>
        public const string Level = nameof(Level);

        /// <summary>
        /// The managed thread ID column.
        /// </summary>
        public const string ManagedThreadId = nameof(ManagedThreadId);

        /// <summary>
        /// The message template column (structured logging template with placeholders).
        /// </summary>
        public const string MessageTemplate = nameof(MessageTemplate);

        /// <summary>
        /// The properties column (JSON-serialized structured logging parameters).
        /// </summary>
        public const string Properties = nameof(Properties);

        /// <summary>
        /// The rendered message column (final formatted message with parameters substituted).
        /// </summary>
        public const string RenderedMessage = nameof(RenderedMessage);

        /// <summary>
        /// The exception JSON column (serialized exception information).
        /// </summary>
        public const string ExceptionJson = nameof(ExceptionJson);

        /// <summary>
        /// The scopes JSON column (serialized scope information).
        /// </summary>
        public const string ScopesJson = nameof(ScopesJson);
    }

    /// <summary>
    /// Gets all column names in the order they appear in the table.
    /// </summary>
    /// <returns>An array of all column names.</returns>
    public static string[] GetAllColumns()
    {
        return
        [
            Columns.DbId,
            Columns.Category,
            Columns.EventId,
            Columns.EventName,
            Columns.Timestamp,
            Columns.Level,
            Columns.ManagedThreadId,
            Columns.MessageTemplate,
            Columns.Properties,
            Columns.RenderedMessage,
            Columns.ExceptionJson,
            Columns.ScopesJson
        ];
    }

    /// <summary>
    /// Gets all insertable column names (excludes auto-increment DbId).
    /// </summary>
    /// <returns>An array of insertable column names.</returns>
    public static string[] GetInsertableColumns()
    {
        return
        [
            Columns.Category,
            Columns.EventId,
            Columns.EventName,
            Columns.Timestamp,
            Columns.Level,
            Columns.ManagedThreadId,
            Columns.MessageTemplate,
            Columns.Properties,
            Columns.RenderedMessage,
            Columns.ExceptionJson,
            Columns.ScopesJson
        ];
    }

    /// <summary>
    /// Gets the column definitions for CREATE TABLE statement.
    /// </summary>
    /// <returns>A list of column definitions.</returns>
    public static List<string> GetColumnDefinitions()
    {
        return
        [
            $"{Columns.DbId} INTEGER PRIMARY KEY AUTOINCREMENT",
            $"{Columns.Category} TEXT",
            $"{Columns.EventId} INTEGER",
            $"{Columns.EventName} TEXT",
            $"{Columns.Timestamp} TEXT",
            $"{Columns.Level} INTEGER",
            $"{Columns.ManagedThreadId} INTEGER",
            $"{Columns.MessageTemplate} TEXT",
            $"{Columns.Properties} TEXT",
            $"{Columns.RenderedMessage} TEXT",
            $"{Columns.ExceptionJson} TEXT",
            $"{Columns.ScopesJson} TEXT"
        ];
    }
}
