using Microsoft.Extensions.Logging;

namespace WinFormsTest.DomainSpecificLiveLogViewer;

/// <summary>
/// Represents a factory that makes bread.
/// </summary>
internal class BreadFactory
{
    private readonly ILogger<BreadFactory> logger;
    private readonly Random random;

    /// <summary>
    /// Initializes a new instance of the <see cref="BreadFactory"/> class.
    /// </summary>
    /// <param name="logger">The logger to use for logging messages.</param>
    public BreadFactory(ILogger<BreadFactory> logger)
    {
        this.logger = logger;
        random = new Random();
    }

    /// <summary>
    /// Simulates making bread based on the provided orders.
    /// </summary>
    /// <param name="orders">An array of bread orders.</param>
    public async Task MakeBread(BreadOrder[] orders)
    {
        foreach (var order in orders)
        {
            await ProcessOrderAsync(order);
        }
    }

    /// <summary>
    /// Processes a single bread order.
    /// </summary>
    /// <param name="order">The bread order to process.</param>
    private async Task ProcessOrderAsync(BreadOrder order)
    {
        // Create a scope for the batch number
        using var batchScope = logger.BeginScope("BatchNumber: {BatchNumber}", order.BatchNumber);

        logger.LogInformation("Starting batch with {FlourType} flour and {NumberOfLoafs} loafs", order.FlourType, order.NumberOfLoafs);

        for (int loafIndex = 1; loafIndex <= order.NumberOfLoafs; loafIndex++)
        {
            await MakeOneLoafAsync(loafIndex);
        }

        logger.LogInformation("Batch {BatchNumber} completed", order.BatchNumber);
    }

    /// <summary>
    /// Simulates making a single loaf of bread.
    /// </summary>
    /// <param name="loafIndex">The index of the loaf being made.</param>
    private async Task MakeOneLoafAsync(int loafIndex)
    {
        // Create a scope for the loaf number
        using var loafScope = logger.BeginScope("LoafNumber: {LoafNumber}", loafIndex);

        logger.LogInformation("Mixing ingredients for loaf");
        await Task.Delay(500); // Simulate mixing time

        logger.LogInformation("Baking loaf");
        await Task.Delay(1000); // Simulate baking time

        if (random.Next(0, 10) < 2) // 20% chance to burn the loaf
        {
            logger.LogCritical("Loaf is burnt!");
        }
        else
        {
            logger.LogInformation("Cooling loaf");
            await Task.Delay(300); // Simulate cooling time

            logger.LogInformation("Loaf is ready");
        }
    }
}
