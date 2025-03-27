using FluentAssertions;
using CDS.SQLiteLogging.Tests.TestSupport;
using System.Reflection;

namespace CDS.SQLiteLogging.Tests;

/// <summary>
/// Unit tests for the LogHousekeeper class.
/// </summary>
[TestClass]
public class LogHousekeeperTests
{
    private string _testFolder;
    private ConnectionManager _connectionManager;
    private string _tableName;
    private LogWriter<TestLogEntry> _logWriter;
    private LogHousekeeper<TestLogEntry> _housekeeper;

    /// <summary>
    /// Initializes the test environment.
    /// </summary>
    [TestInitialize]
    public void Initialize()
    {
        _testFolder = TestDatabaseHelper.GetTemporaryDatabaseFolder();
        _connectionManager = new ConnectionManager(_testFolder, schemaVersion: TestLogEntry.Version);

        // Create a test table
        var typeMap = TypeMapper.CreateTypeToSqliteMap(typeof(TestLogEntry).GetProperties());
        var tableCreator = new TableCreator(_connectionManager, typeMap);
        _tableName = tableCreator.CreateTableForType<TestLogEntry>();
        _logWriter = new LogWriter<TestLogEntry>(_connectionManager, _tableName);

        // Create housekeeper with 30-day retention and 1-hour cleanup interval
        _housekeeper = new LogHousekeeper<TestLogEntry>(
            _connectionManager,
            _tableName,
            TimeSpan.FromDays(30),
            TimeSpan.FromHours(1));
    }

    /// <summary>
    /// Cleans up the test environment.
    /// </summary>
    [TestCleanup]
    public void Cleanup()
    {
        _housekeeper?.Dispose();
        _connectionManager?.Dispose();
        TestDatabaseHelper.DeleteTestFolder(_testFolder);
    }

    /// <summary>
    /// Tests that the constructor initializes properties correctly.
    /// </summary>
    [TestMethod]
    public void Constructor_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var retentionPeriod = TimeSpan.FromDays(7);
        var cleanupInterval = TimeSpan.FromHours(2);

        // Act
        using var housekeeper = new LogHousekeeper<TestLogEntry>(
            _connectionManager,
            _tableName,
            retentionPeriod,
            cleanupInterval);

        // Assert
        housekeeper.RetentionPeriod.Should().Be(retentionPeriod);

        // Verify timer was created (using reflection since Timer is private)
        var timerField = typeof(LogHousekeeper<TestLogEntry>)
            .GetField("cleanupTimer", BindingFlags.NonPublic | BindingFlags.Instance);
        var timer = timerField.GetValue(housekeeper);
        timer.Should().NotBeNull();
    }

    /// <summary>
    /// Tests that the retention period is modifiable.
    /// </summary>
    [TestMethod]
    public void RetentionPeriod_ShouldBeModifiable()
    {
        // Arrange
        var initialPeriod = TimeSpan.FromDays(30);
        var newPeriod = TimeSpan.FromDays(7);

        using var housekeeper = new LogHousekeeper<TestLogEntry>(
            _connectionManager,
            _tableName,
            initialPeriod,
            TimeSpan.FromHours(1));

        // Assert initial value
        housekeeper.RetentionPeriod.Should().Be(initialPeriod);

        // Act
        housekeeper.RetentionPeriod = newPeriod;

        // Assert
        housekeeper.RetentionPeriod.Should().Be(newPeriod);
    }

    /// <summary>
    /// Tests that DeleteEntriesOlderThanAsync removes old entries.
    /// </summary>
    [TestMethod]
    public async Task DeleteEntriesOlderThanAsync_ShouldRemoveOldEntries()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var oldEntry = CreateTestLogEntry("Old Entry", now.AddDays(-10));
        var recentEntry = CreateTestLogEntry("Recent Entry", now.AddDays(-2));
        var newEntry = CreateTestLogEntry("New Entry", now);

        await _logWriter.AddBatchAsync(new[] { oldEntry, recentEntry, newEntry });

        // Act
        int deletedCount = await _housekeeper.DeleteEntriesOlderThanAsync(now.AddDays(-5).DateTime);

        // Assert
        deletedCount.Should().Be(1); // Should delete only the old entry

        var reader = new LogReader<TestLogEntry>(_connectionManager, _tableName);
        var remainingEntries = await reader.GetAllEntriesAsync();
        remainingEntries.Should().HaveCount(2);
        remainingEntries.Should().Contain(e => e.Message == "Recent Entry");
        remainingEntries.Should().Contain(e => e.Message == "New Entry");
        remainingEntries.Should().NotContain(e => e.Message == "Old Entry");
    }

    /// <summary>
    /// Tests that DeleteEntriesOlderThan removes old entries.
    /// </summary>
    [TestMethod]
    public void DeleteEntriesOlderThan_ShouldRemoveOldEntries()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var oldEntry = CreateTestLogEntry("Old Entry", now.AddDays(-10));
        var recentEntry = CreateTestLogEntry("Recent Entry", now.AddDays(-2));
        var newEntry = CreateTestLogEntry("New Entry", now);

        _logWriter.Add(oldEntry);
        _logWriter.Add(recentEntry);
        _logWriter.Add(newEntry);

        // Act
        int deletedCount = _housekeeper.DeleteEntriesOlderThan(now.AddDays(-5).DateTime);

        // Assert
        deletedCount.Should().Be(1); // Should delete only the old entry

        var reader = new LogReader<TestLogEntry>(_connectionManager, _tableName);
        var remainingEntries = reader.GetAllEntries();
        remainingEntries.Should().HaveCount(2);
        remainingEntries.Should().Contain(e => e.Message == "Recent Entry");
        remainingEntries.Should().Contain(e => e.Message == "New Entry");
        remainingEntries.Should().NotContain(e => e.Message == "Old Entry");
    }

    /// <summary>
    /// Tests that DeleteAllAsync removes all entries.
    /// </summary>
    [TestMethod]
    public async Task DeleteAllAsync_ShouldRemoveAllEntries()
    {
        // Arrange
        var entries = new List<TestLogEntry>
        {
            CreateTestLogEntry("Entry 1"),
            CreateTestLogEntry("Entry 2"),
            CreateTestLogEntry("Entry 3")
        };

        await _logWriter.AddBatchAsync(entries);

        var reader = new LogReader<TestLogEntry>(_connectionManager, _tableName);
        var initialCount = reader.GetEntryCount();
        initialCount.Should().Be(3);

        // Act
        int deletedCount = await _housekeeper.DeleteAllAsync();

        // Assert
        deletedCount.Should().Be(3);
        var remainingCount = reader.GetEntryCount();
        remainingCount.Should().Be(0);
    }

    /// <summary>
    /// Tests that DeleteAll removes all entries.
    /// </summary>
    [TestMethod]
    public void DeleteAll_ShouldRemoveAllEntries()
    {
        // Arrange
        var entries = new List<TestLogEntry>
        {
            CreateTestLogEntry("Entry 1"),
            CreateTestLogEntry("Entry 2"),
            CreateTestLogEntry("Entry 3")
        };

        foreach (var entry in entries)
        {
            _logWriter.Add(entry);
        }

        var reader = new LogReader<TestLogEntry>(_connectionManager, _tableName);
        var initialCount = reader.GetEntryCount();
        initialCount.Should().Be(3);

        // Act
        int deletedCount = _housekeeper.DeleteAll();

        // Assert
        deletedCount.Should().Be(3);
        var remainingCount = reader.GetEntryCount();
        remainingCount.Should().Be(0);
    }

    /// <summary>
    /// Tests that Dispose cleans up resources.
    /// </summary>
    [TestMethod]
    public void Dispose_ShouldCleanupResources()
    {
        // Arrange
        var housekeeper = new LogHousekeeper<TestLogEntry>(
            _connectionManager,
            _tableName,
            TimeSpan.FromDays(1),
            TimeSpan.FromHours(1));

        // Access disposed field via reflection
        var disposedField = typeof(LogHousekeeper<TestLogEntry>)
            .GetField("disposed", BindingFlags.NonPublic | BindingFlags.Instance);
        var initialDisposed = (bool)disposedField.GetValue(housekeeper);
        initialDisposed.Should().BeFalse();

        // Act
        housekeeper.Dispose();

        // Assert
        var finalDisposed = (bool)disposedField.GetValue(housekeeper);
        finalDisposed.Should().BeTrue();

        // Verify calling Dispose again doesn't throw
        Action secondDispose = () => housekeeper.Dispose();
        secondDispose.Should().NotThrow();
    }

    /// <summary>
    /// Creates a test log entry with the specified message and timestamp.
    /// </summary>
    /// <param name="message">The message for the log entry.</param>
    /// <param name="timestamp">The timestamp for the log entry.</param>
    /// <returns>A new TestLogEntry instance.</returns>
    private TestLogEntry CreateTestLogEntry(string message = "Test Message", DateTimeOffset? timestamp = null)
    {
        return new TestLogEntry
        {
            Timestamp = timestamp ?? DateTimeOffset.UtcNow,
            Level = LogLevel.Information,
            Sender = "LogHousekeeperTests",
            Message = message,
            Details = "Test Details"
        };
    }
}
