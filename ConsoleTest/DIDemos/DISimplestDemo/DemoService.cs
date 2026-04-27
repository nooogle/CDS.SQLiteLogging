using Microsoft.Extensions.Logging;

namespace ConsoleTest.DISimplestDemo;

/// <summary>
/// Provides demo services for processing and logging.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DemoService"/> class.
/// </remarks>
/// <param name="logger">The logger instance.</param>
class DemoService(ILogger<DemoService> logger)
{
    /// <summary>
    /// Runs the demo service.
    /// </summary>
    public void Run()
    {
        using var scope = logger.BeginScope("DemoService.Run");
        logger.LogDebug("Here we go!");
        DoSomeProcessing();
    }

    /// <summary>
    /// Processes some items and logs the progress.
    /// </summary>
    private void DoSomeProcessing()
    {
        using var scope = logger.BeginScope("DemoService.DoSomeProcessing");

        for (var i = 0; i < 4; i++)
        {
            logger.LogInformation("Processing item {ItemNumber}", i);
        }
    }
}
