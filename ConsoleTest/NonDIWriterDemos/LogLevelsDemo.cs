using Microsoft.Extensions.Logging;

namespace ConsoleTest.NonDIWriterDemos;

/// <summary>
/// Contains ad-hoc tests for the SQLite Logger.
/// </summary>
/// <remarks>
/// Initialises a new instance of the <see cref="LogLevelsDemo"/> class.
/// </remarks>
class LogLevelsDemo(CDS.SQLiteLogging.MEL.MELLoggerProvider sqliteLoggerProvider)
{
    /// <summary>
    /// A logger
    /// </summary>
    private readonly ILogger logger = sqliteLoggerProvider.CreateLogger(categoryName: $"{nameof(NonDIWriterDemos)}.{nameof(LogLevelsDemo)}");

    /// <summary>
    /// Runs a basic test of the SQLite Logger.
    /// </summary>
    public void Run()
    {
        // Display the test header
        Console.Clear();
        Console.WriteLine("=== Built-in logger demo ===\n");

        // Add different levels of log entries
        // Add different levels of log entries
        logger.LogTrace("This is a trace message.");
        logger.LogDebug("This is a debug message.");
        logger.LogInformation("This is an information message.");
        logger.LogWarning("This is a warning message.");
        logger.LogError("This is an error message.");
        logger.LogCritical("This is a critical message.");

        // Log an exception
        try
        {
            CreateException();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred.");
        }
    }

    /// <summary>
    /// Creates an exception with an inner exception.
    /// </summary>
    private static Exception? CreateException()
    {
        try
        {
            CreateInnerException();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("This is a demo exception!", ex);
        }

        return null;
    }

    /// <summary>
    /// Creates an inner exception.
    /// </summary>
    private static void CreateInnerException()
    {
        throw new NotImplementedException("This is another demo exception");
    }
}
