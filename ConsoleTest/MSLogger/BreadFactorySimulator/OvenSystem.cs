using Microsoft.Extensions.Logging;

namespace ConsoleTest.MSLogger.BreadFactorySimulator;

/// <summary>
/// Represents an oven system that bakes bread.
/// </summary>
class OvenSystem
{
    private readonly ILogger<OvenSystem> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OvenSystem"/> class.
    /// </summary>
    /// <param name="logger">The logger to use for logging operations.</param>
    public OvenSystem(ILogger<OvenSystem> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Simulates the process of making bread.
    /// </summary>
    public void MakeBread()
    {
        var random = new Random();
        foreach (var batch in new[] { "White1234", "WholeMeal66" })
        {
            using var loafScope = logger.BeginScope("White loaf batch {batch}", batch);

            for (int loafIndex = 0; loafIndex < 3; loafIndex++)
            {
                CookOneLoafOfBread(random, loafIndex);
            }
        }
    }

    /// <summary>
    /// Simulates the process of cooking one loaf of bread.
    /// </summary>
    /// <param name="random">The random number generator to use for generating random values.</param>
    /// <param name="loafIndex">The index of the loaf being cooked.</param>
    private void CookOneLoafOfBread(Random random, int loafIndex)
    {
        using var scope = logger.BeginScope($"Loaf {loafIndex + 1}");
        logger.LogInformation("Mixing {flour_g} g flour and {water_g} g water", random.Next(900, 1100), random.Next(650, 750));
        logger.LogInformation("Baking loaf for {bake_time} minutes", random.Next(25, 35));
    }
}
