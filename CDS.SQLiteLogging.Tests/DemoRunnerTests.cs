using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CDS.SQLiteLogging.Tests;

[TestClass]
public class DemoRunnerTests
{
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(10)]
    [DataRow(100)]
    public void Test_CreatingEntries_CanBeReadBack(int numberOfEntries)
    {
        NewDatabaseTestHost.Run(
            onDatabaseCreated: (serviceProvider, dbPath) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<DemoRunnerTests>>();
                for (int i = 0; i < numberOfEntries; i++)
                {
                    logger.LogInformation("Processing item {ID}", i);
                }
            },

            onDatabaseClosed: (dbPath) =>
            {
                using var reader = new SQLiteReader(dbPath);
                reader.GetAllEntries().Should().HaveCount(numberOfEntries);
            });
    }
}
