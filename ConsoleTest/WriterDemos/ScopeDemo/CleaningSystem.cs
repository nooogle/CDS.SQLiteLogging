using Microsoft.Extensions.Logging;

namespace ConsoleTest.WriterDemos.ScopeDemo;

/// <summary>
/// Represents a cleaning system for the factory.
/// </summary>
class CleaningSystem
{
    private readonly ILogger<CleaningSystem> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CleaningSystem"/> class.
    /// </summary>
    /// <param name="logger">The logger to use for logging operations.</param>
    public CleaningSystem(ILogger<CleaningSystem> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Simulates the cleaning process.
    /// </summary>
    public void Clean()
    {
        using var cleaningScope = logger.BeginScope("Cleaning");

        logger.LogInformation("Washing the pots");
        logger.LogWarning("Some pots are still dirty!");
    }
}
