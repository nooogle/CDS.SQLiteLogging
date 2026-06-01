using AwesomeAssertions;
using CDS.SQLiteLogging.Tests.Support;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CDS.SQLiteLogging.Tests;

/// <summary>
/// Tests for <see cref="Reader.QueryAsync{T}"/> and its synchronous wrapper.
/// </summary>
[TestClass]
public class ReaderQueryTests
{
    /// <summary>
    /// Tests that QueryAsync returns the correct count when mapping a scalar COUNT(*) result.
    /// </summary>
    [TestMethod]
    public void QueryAsync_WithCountStar_ReturnsCorrectCount()
    {
        const int entryCount = 5;

        new NewDatabaseTestHost().Run(
            onDatabaseCreated: (serviceProvider, dbPath) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<ReaderQueryTests>>();
                for (int i = 0; i < entryCount; i++)
                {
                    logger.LogInformation("Entry {Index}", i);
                }
            },

            onDatabaseClosed: (dbPath) =>
            {
                // Arrange
                using var reader = new Reader(dbPath);
                string sql = $"SELECT COUNT(*) FROM {Reader.TableName}";

                // Act
                var results = reader.QueryAsync(sql, r => r.GetInt64(0)).GetAwaiter().GetResult();

                // Assert
                results.Should().HaveCount(1);
                results[0].Should().Be(entryCount);
            });
    }

    /// <summary>
    /// Tests that QueryAsync returns an empty list when the table has no rows matching the query.
    /// </summary>
    [TestMethod]
    public void QueryAsync_WithNoMatchingRows_ReturnsEmptyList()
    {
        new NewDatabaseTestHost().Run(
            onDatabaseCreated: (serviceProvider, dbPath) => { },

            onDatabaseClosed: (dbPath) =>
            {
                using var reader = new Reader(dbPath);
                string sql = $"SELECT RenderedMessage FROM {Reader.TableName} WHERE 1=0";

                var results = reader.QueryAsync(sql, r => r.GetString(0)).GetAwaiter().GetResult();

                results.Should().BeEmpty();
            });
    }

    /// <summary>
    /// Tests that QueryAsync maps multiple rows correctly using a custom projection.
    /// </summary>
    [TestMethod]
    public void QueryAsync_WithMultipleRows_MapsEachRowViaDelegate()
    {
        const int entryCount = 3;

        new NewDatabaseTestHost().Run(
            onDatabaseCreated: (serviceProvider, dbPath) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<ReaderQueryTests>>();
                for (int i = 0; i < entryCount; i++)
                {
                    logger.LogInformation("Entry {Index}", i);
                }
            },

            onDatabaseClosed: (dbPath) =>
            {
                using var reader = new Reader(dbPath);
                string sql = $"SELECT RenderedMessage FROM {Reader.TableName}";

                var results = reader.QueryAsync(sql, r => r.GetString(0)).GetAwaiter().GetResult();

                results.Should().HaveCount(entryCount);
                results.Should().AllSatisfy(msg => msg.Should().StartWith("Entry "));
            });
    }

    /// <summary>
    /// Tests that the synchronous Query wrapper returns the same result as QueryAsync.
    /// </summary>
    [TestMethod]
    public void Query_SyncWrapper_ReturnsSameResultAsAsync()
    {
        const int entryCount = 4;

        new NewDatabaseTestHost().Run(
            onDatabaseCreated: (serviceProvider, dbPath) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<ReaderQueryTests>>();
                for (int i = 0; i < entryCount; i++)
                {
                    logger.LogInformation("Item {Index}", i);
                }
            },

            onDatabaseClosed: (dbPath) =>
            {
                using var reader = new Reader(dbPath);
                string sql = $"SELECT COUNT(*) FROM {Reader.TableName}";

#pragma warning disable CS0618
                var results = reader.Query(sql, r => r.GetInt64(0));
#pragma warning restore CS0618

                results.Should().HaveCount(1);
                results[0].Should().Be(entryCount);
            });
    }

    /// <summary>
    /// Tests that QueryAsync respects cancellation and throws OperationCanceledException.
    /// </summary>
    [TestMethod]
    public void QueryAsync_WithCancelledToken_ThrowsOperationCanceledException()
    {
        new NewDatabaseTestHost().Run(
            onDatabaseCreated: (serviceProvider, dbPath) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<ReaderQueryTests>>();
                logger.LogInformation("A log entry");
            },

            onDatabaseClosed: (dbPath) =>
            {
                using var reader = new Reader(dbPath);
                using var cts = new CancellationTokenSource();
                cts.Cancel();

                string sql = $"SELECT * FROM {Reader.TableName}";

                var act = () => reader.QueryAsync(sql, r => r.GetString(0), cts.Token).GetAwaiter().GetResult();

                act.Should().Throw<OperationCanceledException>();
            });
    }
}
