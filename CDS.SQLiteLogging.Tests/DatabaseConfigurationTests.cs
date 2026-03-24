using AwesomeAssertions;
using CDS.SQLiteLogging.Tests.Support;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CDS.SQLiteLogging.Tests;

/// <summary>
/// Contains tests for database-level SQLite configuration defaults.
/// </summary>
[TestClass]
public class DatabaseConfigurationTests
{
    private const int SynchronousOff = 0;
    private const int SynchronousNormal = 1;

    /// <summary>
    /// Tests that newly created logging databases default to WAL mode with NORMAL synchronous behavior.
    /// </summary>
    [TestMethod]
    public void NewDatabase_ShouldDefaultToWalJournalMode_AndNormalSynchronous()
    {
        // Arrange & Act
        new NewDatabaseTestHost().Run(
            onDatabaseCreated: (serviceProvider, dbPath) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<DatabaseConfigurationTests>>();
                logger.LogInformation("Create database");
            },
            onDatabaseClosed: dbPath =>
            {
                AssertDatabaseSettings(dbPath, SynchronousNormal);
            });
    }

    /// <summary>
    /// Tests that the client can configure the SQLite synchronous mode to OFF.
    /// </summary>
    [TestMethod]
    public void NewDatabase_ShouldAllowConfiguringOffSynchronousMode()
    {
        var testHost = new NewDatabaseTestHost
        {
            DatabaseOptions = new DatabaseOptions
            {
                SynchronousMode = SqliteSynchronousMode.Off,
            },
        };

        testHost.Run(
            onDatabaseCreated: (serviceProvider, dbPath) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<DatabaseConfigurationTests>>();
                logger.LogInformation("Create database with OFF synchronous mode");
            },
            onDatabaseClosed: dbPath =>
            {
                AssertDatabaseSettings(dbPath, SynchronousOff);
            });
    }

    /// <summary>
    /// Asserts the configured database PRAGMA settings for a database file.
    /// </summary>
    /// <param name="dbPath">The SQLite database path.</param>
    /// <param name="expectedSynchronous">The expected SQLite synchronous numeric value.</param>
    private static void AssertDatabaseSettings(string dbPath, int expectedSynchronous)
    {
        using var connection = new SqliteConnection($"Data Source={dbPath}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA journal_mode;";
        var journalMode = Convert.ToString(command.ExecuteScalar());

        command.CommandText = "PRAGMA synchronous;";
        var synchronous = Convert.ToInt32(command.ExecuteScalar());

        journalMode.Should().Be("wal");
        synchronous.Should().Be(expectedSynchronous);
    }
}
