//using Microsoft.Extensions.Logging;

//namespace CDS.SQLiteLogging;

//public static class CoreLoggerExtensions
//{
//    public static void AddInformation(this SQLiteLogger<LogEntry> logger, string message) => logger.Add(LogLevel.Information, message);

//    public static void AddInformation(this SQLiteLogger<LogEntry> logger, string message, params KeyValuePair<string, object>[] msgParams) => 
//        logger.Add(
//            level: LogLevel.Information, 
//            message: message, 
//            exception: null,
//            msgParams);

//    public static void AddException(
//        this SQLiteLogger<LogEntry> logger,
//        Exception exception,
//        string message)
//    {
//        logger.Add(
//            level: LogLevel.Error,
//            message: message,
//            exception: exception,
//            msgParams: null);
//    }

//    public static void AddException(
//        this SQLiteLogger<LogEntry> logger,
//        Exception exception,
//        string message,
//        params KeyValuePair<string, object>[] msgParams)
//    {
//        logger.Add(
//            level: LogLevel.Information,
//            message: message,
//            exception: exception,
//            msgParams);
//    }

//    public static void Add(this SQLiteLogger<LogEntry> logger, LogLevel level, string message) => 
//        logger.Add(
//            level: level, 
//            message: message, 
//            exception: null,
//            msgParams: null);


//    public static void Add(
//        this SQLiteLogger<LogEntry> logger, 
//        LogLevel level, 
//        string message, 
//        Exception exception, 
//        params KeyValuePair<string, object>[] msgParams)
//    {
//        var logEntry = new LogEntry
//        {
//            Timestamp = DateTimeOffset.Now,
//            Level = level,
//            MessageTemplate = message,
//            Properties = msgParams == null ? null : msgParams.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
//        };

//        logEntry.SetException(exception);

//        logger.Add(logEntry);
//    }
//}
