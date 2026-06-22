using AwesomeAssertions;
using CDS.SQLiteLogging.Tests.Support;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CDS.SQLiteLogging.Tests;

/// <summary>
/// Tests for <see cref="Reader.GetEntriesByMessageParam"/> — matching semantics,
/// null/empty handling, JSON key/path edge cases, and the obsolete async wrapper.
/// </summary>
[TestClass]
[TestCategory("Reader")]
public class ReaderMessageParamTests
{
    /// <summary>
    /// An entry whose integer parameter matches should be returned.
    /// </summary>
    [TestMethod]
    public void GetEntriesByMessageParam_WithMatchingIntParam_ReturnsEntry()
    {
        new NewDatabaseTestHost().Run(
            onDatabaseCreated: (serviceProvider, dbPath) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<ReaderMessageParamTests>>();
                logger.LogInformation("User {UserId} logged in", 42);
                logger.LogInformation("User {UserId} logged in", 99);
            },

            onDatabaseClosed: (dbPath) =>
            {
                using var reader = new Reader(dbPath);

                var results = reader.GetEntriesByMessageParam("UserId", 42);

                results.Should().HaveCount(1);
                results[0].Properties.Should().ContainKey("UserId");
                results[0].Properties!["UserId"].Should().Be(42L);
            });
    }

    /// <summary>
    /// An entry whose string parameter matches should be returned.
    /// </summary>
    [TestMethod]
    public void GetEntriesByMessageParam_WithMatchingStringParam_ReturnsEntry()
    {
        new NewDatabaseTestHost().Run(
            onDatabaseCreated: (serviceProvider, dbPath) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<ReaderMessageParamTests>>();
                logger.LogInformation("Hello {Name}", "Alice");
                logger.LogInformation("Hello {Name}", "Bob");
            },

            onDatabaseClosed: (dbPath) =>
            {
                using var reader = new Reader(dbPath);

                var results = reader.GetEntriesByMessageParam("Name", "Alice");

                results.Should().HaveCount(1);
                results[0].RenderedMessage.Should().Be("Hello Alice");
            });
    }

    /// <summary>
    /// When no entry has the requested value, an empty list is returned.
    /// </summary>
    [TestMethod]
    public void GetEntriesByMessageParam_WithNoMatchingValue_ReturnsEmpty()
    {
        new NewDatabaseTestHost().Run(
            onDatabaseCreated: (serviceProvider, dbPath) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<ReaderMessageParamTests>>();
                logger.LogInformation("Order {OrderId} placed", 100);
                logger.LogInformation("Order {OrderId} placed", 200);
            },

            onDatabaseClosed: (dbPath) =>
            {
                using var reader = new Reader(dbPath);

                var results = reader.GetEntriesByMessageParam("OrderId", 999);

                results.Should().BeEmpty();
            });
    }

    /// <summary>
    /// When the key does not appear in any entry's properties, an empty list is returned.
    /// </summary>
    [TestMethod]
    public void GetEntriesByMessageParam_WithNonExistentKey_ReturnsEmpty()
    {
        new NewDatabaseTestHost().Run(
            onDatabaseCreated: (serviceProvider, dbPath) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<ReaderMessageParamTests>>();
                logger.LogInformation("Ping {Host}", "server1");
            },

            onDatabaseClosed: (dbPath) =>
            {
                using var reader = new Reader(dbPath);

                var results = reader.GetEntriesByMessageParam("NoSuchKey", "anything");

                results.Should().BeEmpty();
            });
    }

    /// <summary>
    /// Only the entries whose specific parameter matches are returned; others are excluded.
    /// </summary>
    [TestMethod]
    public void GetEntriesByMessageParam_WithMixedEntries_ReturnsOnlyMatching()
    {
        new NewDatabaseTestHost().Run(
            onDatabaseCreated: (serviceProvider, dbPath) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<ReaderMessageParamTests>>();
                logger.LogInformation("Job {JobId} started", 1);
                logger.LogInformation("Job {JobId} started", 2);
                logger.LogInformation("Job {JobId} started", 3);
                logger.LogInformation("No job id here");
            },

            onDatabaseClosed: (dbPath) =>
            {
                using var reader = new Reader(dbPath);

                var results = reader.GetEntriesByMessageParam("JobId", 2);

                results.Should().HaveCount(1);
                results[0].RenderedMessage.Should().Be("Job 2 started");
            });
    }

    /// <summary>
    /// When the database contains no entries at all, an empty list is returned.
    /// </summary>
    [TestMethod]
    public void GetEntriesByMessageParam_WithEmptyDatabase_ReturnsEmpty()
    {
        new NewDatabaseTestHost().Run(
            onDatabaseCreated: (serviceProvider, dbPath) => { },

            onDatabaseClosed: (dbPath) =>
            {
                using var reader = new Reader(dbPath);

                var results = reader.GetEntriesByMessageParam("AnyKey", "AnyValue");

                results.Should().BeEmpty();
            });
    }

    /// <summary>
    /// A key that contains a dot is treated as a literal property name and finds no match,
    /// since structured-log properties are stored flat (no nested JSON objects).
    /// </summary>
    [TestMethod]
    public void GetEntriesByMessageParam_WithDottedKey_ReturnsEmpty()
    {
        new NewDatabaseTestHost().Run(
            onDatabaseCreated: (serviceProvider, dbPath) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<ReaderMessageParamTests>>();
                logger.LogInformation("Value {Level}", "high");
            },

            onDatabaseClosed: (dbPath) =>
            {
                using var reader = new Reader(dbPath);

                // "Level.sub" is a nested JSON path; properties are stored flat so this should not match.
                var results = reader.GetEntriesByMessageParam("Level.sub", "high");

                results.Should().BeEmpty();
            });
    }

    /// <summary>
    /// The obsolete async wrapper delegates to the sync method and returns an identical result.
    /// </summary>
    [TestMethod]
    public void GetEntriesByMessageParamAsync_ObsoleteWrapper_ReturnsSameResultAsSync()
    {
        new NewDatabaseTestHost().Run(
            onDatabaseCreated: (serviceProvider, dbPath) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<ReaderMessageParamTests>>();
                logger.LogInformation("Sensor {SensorId} reading", 7);
                logger.LogInformation("Sensor {SensorId} reading", 8);
            },

            onDatabaseClosed: (dbPath) =>
            {
                using var reader = new Reader(dbPath);

                var syncResults = reader.GetEntriesByMessageParam("SensorId", 7);

#pragma warning disable CS0618
                var asyncResults = reader.GetEntriesByMessageParamAsync("SensorId", 7).GetAwaiter().GetResult();
#pragma warning restore CS0618

                asyncResults.Should().HaveCount(syncResults.Count);
                asyncResults.Select(e => e.DbId).Should().BeEquivalentTo(syncResults.Select(e => e.DbId));
            });
    }
}
