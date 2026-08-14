using AwesomeAssertions;
using CDS.SQLiteLogging.Tests.Support;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CDS.SQLiteLogging.Tests;

/// <summary>
/// Contains tests for <see cref="Housekeeper.CanOpenForWrite(string)"/>.
/// </summary>
[TestClass]
public class HousekeeperCanOpenForWriteTests
{
    /// <summary>
    /// Tests that a non-existent database file is reported as not writable.
    /// </summary>
    [TestMethod]
    public void CanOpenForWrite_WhenFileDoesNotExist_ReturnsFalse()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"NonExistentLog_{Guid.NewGuid()}.db");

        var result = Housekeeper.CanOpenForWrite(dbPath);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that an existing database with no other open connection is reported as writable.
    /// </summary>
    [TestMethod]
    public void CanOpenForWrite_WhenDatabaseIsUnlocked_ReturnsTrue()
    {
        new NewDatabaseTestHost().Run(
            onDatabaseCreated: (serviceProvider, dbPath) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<HousekeeperCanOpenForWriteTests>>();
                logger.LogInformation("Entry");
            },
            onDatabaseClosed: dbPath =>
            {
                var result = Housekeeper.CanOpenForWrite(dbPath);

                result.Should().BeTrue();
            });
    }

    /// <summary>
    /// Tests that a database held open under a reserved write lock by another connection is
    /// reported as not writable, rather than throwing.
    /// </summary>
    [TestMethod]
    public void CanOpenForWrite_WhenDatabaseIsLockedByAnotherConnection_ReturnsFalse()
    {
        new NewDatabaseTestHost().Run(
            onDatabaseCreated: (serviceProvider, dbPath) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<HousekeeperCanOpenForWriteTests>>();
                logger.LogInformation("Entry");
            },
            onDatabaseClosed: dbPath =>
            {
                using var lockingConnection = new SqliteConnection($"Data Source={dbPath}");
                lockingConnection.Open();

                // BEGIN IMMEDIATE acquires the write lock straight away, holding it until rolled back.
                using (var beginCmd = new SqliteCommand("BEGIN IMMEDIATE;", lockingConnection))
                {
                    beginCmd.ExecuteNonQuery();
                }

                try
                {
                    var result = Housekeeper.CanOpenForWrite(dbPath);

                    result.Should().BeFalse();
                }
                finally
                {
                    using var rollbackCmd = new SqliteCommand("ROLLBACK;", lockingConnection);
                    rollbackCmd.ExecuteNonQuery();
                }
            });
    }
}
