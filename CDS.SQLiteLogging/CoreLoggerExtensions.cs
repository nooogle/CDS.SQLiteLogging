namespace CDS.SQLiteLogging;

public static class CoreLoggerExtensions
{
    public static void AddInformation(this Logger<LogEntry> logger, string message) => logger.Add(LogLevel.Information, message);

    public static void AddInformation(this Logger<LogEntry> logger, string message, params KeyValuePair<string, object>[] msgParams) => logger.Add(LogLevel.Information, message, msgParams);


    public static void Add(this Logger<LogEntry> logger, LogLevel level, string message) => logger.Add(level, message, null);


    public static void Add(this Logger<LogEntry> logger, LogLevel level, string message, params KeyValuePair<string, object>[] msgParams)
    {
        var logEntry = new LogEntry
        {
            Timestamp = DateTimeOffset.Now,
            Level = level,
            Message = message,
            MsgParams = msgParams == null ? null : msgParams.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
        };

        logger.Add(logEntry);
    }
}
