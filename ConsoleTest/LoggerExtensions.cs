namespace ConsoleTest;

public class MyLogger
{
    private CDS.SQLiteLogging.Logger<MyLogEntry> logger;
    private string batchId = "No batch";
    private int lineIndex = -1;

    public MyLogger(CDS.SQLiteLogging.Logger<MyLogEntry> logger)
    {
        this.logger = logger;
    }   

    public void LogInformation(CDS.SQLiteLogging.Logger<MyLogEntry> logger, string message)
    {
        var logEntry = new MyLogEntry
        {
            Message = message,
            Level = CDS.SQLiteLogging.LogLevel.Information,
            Sender = "ConsoleTest",
            BatchId = batchId,
            LineIndex = lineIndex,
            Timestamp = DateTimeOffset.Now,
            MsgParams = null,
        };

        logger.Add(logEntry);
    }

}
