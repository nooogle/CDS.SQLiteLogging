using Bogus;
using Microsoft.Extensions.Logging;

namespace ConsoleTest.DIMiddlewareDemo;

/// <summary>
/// Provides demo services for processing and logging, demonstrating the use of 
/// global log context and middleware pipeline.
/// </summary>
class DemoService(ILogger<DemoService> logger)
{
    private Random random = new();
    private Faker faker = new();

    /// <summary>
    /// Runs the demo service, simulating the processing of multiple batches and products.
    /// </summary>
    public void Run()
    {
        logger.LogDebug("Here we go!");

        for (int batchCounter = 0; batchCounter < 2; batchCounter++)
        {
            // Generate a unique batch number for each batch
            var batchNumber = $"BATCH-{faker.Random.Number(1000, 9999)}-{faker.Random.AlphaNumeric(2).ToUpper()}animation-loop";

            ProcessBatch(batchNumber: batchNumber);
        }
    }

    /// <summary>
    /// Processes a single batch, setting the batch number in the global log context and processing products within the batch.
    /// </summary>
    /// <param name="batchNumber">The unique batch number for this batch.</param>
    private void ProcessBatch(string batchNumber)
    {
        // Set the batch number in the global log context so it is included in all log entries for this batch
        CDS.SQLiteLogging.GlobalLogContext.Set(
            key: GlobalLogContextKeys.BatchNumber,
            value: batchNumber);

        for (int productCounter = 0; productCounter < 2; productCounter++)
        {
            // Process each product asynchronously and wait for completion
            ProcessProduct(productIndex: productCounter + 1).Wait();
        }

        // Remove the batch number from the global context after processing the batch
        CDS.SQLiteLogging.GlobalLogContext.Remove(GlobalLogContextKeys.BatchNumber);
    }

    /// <summary>
    /// Processes a single product by running two tasks in parallel, each simulating work for the product.
    /// </summary>
    /// <param name="productIndex">The index of the product being processed.</param>
    private async Task ProcessProduct(int productIndex)
    {
        logger.LogInformation("Processing product {ProductIndex}", productIndex);

        // Start two tasks in parallel for the product
        var task1 = PerformTaskDemo(productIndex: productIndex, task: 1);
        var task2 = PerformTaskDemo(productIndex: productIndex, task: 2);

        await Task.WhenAll(task1, task2);
    }

    /// <summary>
    /// Simulates a task for a product, logging each step and introducing a random delay to mimic work.
    /// </summary>
    /// <param name="productIndex">The index of the product.</param>
    /// <param name="task">The task number for this product.</param>
    private async Task PerformTaskDemo(int productIndex, int task)
    {
        // Use a logging scope to group log entries for this task and product
        using var scope = logger.BeginScope("Task {Task} for product {ProductIndex}", task, productIndex);

        for (int step = 1; step <= 3; step++)
        {
            logger.LogInformation("Task {Task} for product {ProductIndex} - Step {Step}", task, productIndex, step);

            // Simulate some work with a random delay
            await Task.Delay(random.Next(5, 100));
        }
    }
}
