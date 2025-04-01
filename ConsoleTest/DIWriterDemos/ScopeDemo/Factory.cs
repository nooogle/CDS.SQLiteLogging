using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ConsoleTest.DIWriterDemos.ScopeDemo;

/// <summary>
/// Represents a factory that simulates bread production.
/// </summary>
class Factory
{
    private readonly ILogger<Factory> logger;
    private readonly IServiceProvider serviceProvider;


    public Factory(ILogger<Factory> logger, IServiceProvider serviceProvider)
    {
        this.logger = logger;
        this.serviceProvider = serviceProvider;
    }


    /// <summary>
    /// Runs the factory simulation.
    /// </summary>
    public void Run()
    {
        // Log some messages
        logger.LogInformation("Factory simulation starting");

        // Create an oven system
        var ovenLogger = serviceProvider.GetRequiredService<ILogger<OvenSystem>>();
        var ovenSystem = new OvenSystem(ovenLogger);
        ovenSystem.MakeBread();

        // Create and run a cleaning system
        var cleaningLogger = serviceProvider.GetRequiredService<ILogger<CleaningSystem>>();
        var cleaningSystem = new CleaningSystem(cleaningLogger);
        cleaningSystem.Clean();
    }
}
