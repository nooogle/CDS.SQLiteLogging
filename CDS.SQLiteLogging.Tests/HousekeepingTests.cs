using CDS.SQLiteLogging.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CDS.SQLiteLogging.Tests;

/// <summary>
/// Contains tests for writing log entries and verifying they can be read back.
/// </summary>
[TestClass]
public class HousekeepingTests
{
    /// <summary>
    /// Tests that manual housekeeping correctly deletes old log entries, leaving only the expected entries.
    /// </summary>
    [TestMethod]
    public void ManualHousekeeping_ShouldDeleteOldEntries_LeavingExpectedEntries()
    {
        var databaseTestHost = new NewDatabaseTestHost();

        // Mock the DateTimeProvider to control the current time in tests
        var mockDateTimeProvider = new Mocks.MockDateTimeProvider();
        databaseTestHost.DateTimeProvider = mockDateTimeProvider;

        // Set housekeeping options to manual mode with a retention period of 1 day
        databaseTestHost.HouseKeepingOptions.Mode = HousekeepingMode.Manual;
        databaseTestHost.HouseKeepingOptions.RetentionPeriod = TimeSpan.FromDays(1);

        var logEntryStartTime = new DateTimeOffset(
            year: 2030,
            month: 12,
            day: 20,
            hour: 14,
            minute: 0,
            second: 0,
            offset: TimeSpan.Zero);

        // Arrange & Act
        databaseTestHost.Run(
            onDatabaseCreated: (serviceProvider, dbPath) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<WritingTests>>();

                // Write one log entry every hour from the start time for a period of 24 hours (inclusive, so 25 log entries in total)
                for (int i = 0; i < 25; i++)
                {
                    mockDateTimeProvider.Now = logEntryStartTime.AddHours(i);
                    logger.LogInformation($"Log entry {i}");
                }
            },

            onDatabaseClosed: (dbPath) =>
            {
                // Move the clock forward by 2 days to simulate the passage of time
                mockDateTimeProvider.Now = logEntryStartTime.AddDays(2);

                // Perform manual housekeeping to delete old log entries
                using var connectionManager = new ConnectionManager(dbPath);
                using var manualHousekeeper = new Housekeeper(connectionManager, databaseTestHost.HouseKeepingOptions, mockDateTimeProvider);
                manualHousekeeper.ExecuteHousekeeping();

                // Get the log entries from the database
                using var reader = new Reader(connectionManager);
                var entries = reader.GetAllEntries();

                // Assert that only 1 entry is left in the database, and it should be 1 day after the start time
                entries.Should().HaveCount(1);
                entries[0].Timestamp.Should().Be(logEntryStartTime.AddHours(24));
            });
    }
}
