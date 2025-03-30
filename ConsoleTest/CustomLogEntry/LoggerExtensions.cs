using Microsoft.Extensions.Logging;

namespace ConsoleTest.CustomLogEntry;

public class MyLogger
{
    private CDS.SQLiteLogging.SQLiteLogger<MyLogEntry> logger;
    private string batchId = "No batch";
    private int lineIndex = -1;

    public MyLogger(CDS.SQLiteLogging.SQLiteLogger<MyLogEntry> logger)
    {
        this.logger = logger;
    }   

    public void LogInformation(CDS.SQLiteLogging.SQLiteLogger<MyLogEntry> logger, string message)
    {
        var logEntry = new MyLogEntry
        {
            MessageTemplate = message,
            Level = LogLevel.Information,
            Sender = "ConsoleTest",
            BatchId = batchId,
            LineIndex = lineIndex,
            Timestamp = DateTimeOffset.Now,
            Properties = null,
        };

        logger.Add(logEntry);
    }

}
