using FluentAssertions;
using CDS.SQLiteLogging.Tests.TestSupport;
using Microsoft.Extensions.Logging;

namespace CDS.SQLiteLogging.Tests;

/// <summary>
/// Unit tests for the LogReader class.
/// </summary>
[TestClass]
public class LogReaderTests
{
    private string _testFolder;
    private ConnectionManager _connectionManager;
    private string _tableName;
    private LogWriter<TestLogEntry> _logWriter;
    private LogReader<TestLogEntry> _logReader;

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
        _logReader = new LogReader<TestLogEntry>(_connectionManager, _tableName);
    }

    /// <summary>
    /// Cleans up the test environment.
    /// </summary>
    [TestCleanup]
    public void Cleanup()
    {
        _connectionManager.Dispose();
        TestDatabaseHelper.DeleteTestFolder(_testFolder);
    }

    /// <summary>
    /// Tests that GetAllEntries returns an empty list when no entries exist.
    /// </summary>
    [TestMethod]
    public void GetAllEntries_ShouldReturnEmptyList_WhenNoEntriesExist()
    {
        // Act
        var entries = _logReader.GetAllEntries();

        // Assert
        entries.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that GetAllEntriesAsync returns an empty list when no entries exist.
    /// </summary>
    [TestMethod]
    public async Task GetAllEntriesAsync_ShouldReturnEmptyList_WhenNoEntriesExist()
    {
        // Act
        var entries = await _logReader.GetAllEntriesAsync();

        // Assert
        entries.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that GetAllEntries returns all entries.
    /// </summary>
    [TestMethod]
    public void GetAllEntries_ShouldReturnAllEntries()
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

        // Act
        var result = _logReader.GetAllEntries();

        // Assert
        result.Should().HaveCount(3);
        result.Select(e => e.MessageTemplate).Should().BeEquivalentTo(entries.Select(e => e.MessageTemplate));
    }

    /// <summary>
    /// Tests that GetAllEntriesAsync returns all entries.
    /// </summary>
    [TestMethod]
    public async Task GetAllEntriesAsync_ShouldReturnAllEntries()
    {
        // Arrange
        var entries = new List<TestLogEntry>
        {
            CreateTestLogEntry("Entry 1"),
            CreateTestLogEntry("Entry 2"),
            CreateTestLogEntry("Entry 3")
        };

        await _logWriter.AddBatchAsync(entries);

        // Act
        var result = await _logReader.GetAllEntriesAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Select(e => e.MessageTemplate).Should().BeEquivalentTo(entries.Select(e => e.MessageTemplate));
    }

    /// <summary>
    /// Tests that GetRecentEntries returns the most recent entries.
    /// </summary>
    [TestMethod]
    public void GetRecentEntries_ShouldReturnMostRecentEntries()
    {
        // Arrange
        var oldEntry = CreateTestLogEntry("Old Entry", new DateTimeOffset(2022, 1, 1, 12, 0, 0, TimeSpan.Zero));
        var midEntry = CreateTestLogEntry("Mid Entry", new DateTimeOffset(2022, 6, 1, 12, 0, 0, TimeSpan.Zero));
        var newEntry = CreateTestLogEntry("New Entry", new DateTimeOffset(2023, 1, 1, 12, 0, 0, TimeSpan.Zero));

        _logWriter.Add(oldEntry);
        _logWriter.Add(midEntry);
        _logWriter.Add(newEntry);

        // Act
        var result = _logReader.GetRecentEntries(2);

        // Assert
        result.Should().HaveCount(2);
        result[0].MessageTemplate.Should().Be("New Entry");
        result[1].MessageTemplate.Should().Be("Mid Entry");
    }

    /// <summary>
    /// Tests that GetRecentEntriesAsync returns the most recent entries.
    /// </summary>
    [TestMethod]
    public async Task GetRecentEntriesAsync_ShouldReturnMostRecentEntries()
    {
        // Arrange
        var entries = new List<TestLogEntry>
        {
            CreateTestLogEntry("Old Entry", new DateTimeOffset(2022, 1, 1, 12, 0, 0, TimeSpan.Zero)),
            CreateTestLogEntry("Mid Entry", new DateTimeOffset(2022, 6, 1, 12, 0, 0, TimeSpan.Zero)),
            CreateTestLogEntry("New Entry", new DateTimeOffset(2023, 1, 1, 12, 0, 0, TimeSpan.Zero))
        };

        await _logWriter.AddBatchAsync(entries);

        // Act
        var result = await _logReader.GetRecentEntriesAsync(2);

        // Assert
        result.Should().HaveCount(2);
        result[0].MessageTemplate.Should().Be("New Entry");
        result[1].MessageTemplate.Should().Be("Mid Entry");
    }

    /// <summary>
    /// Tests that GetRecentEntries limits the result to the specified maximum count.
    /// </summary>
    [TestMethod]
    public void GetRecentEntries_ShouldLimitToMaxCount()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
        {
            _logWriter.Add(CreateTestLogEntry($"Entry {i}"));
        }

        // Act
        var result = _logReader.GetRecentEntries(5);

        // Assert
        result.Should().HaveCount(5);
    }

    /// <summary>
    /// Tests that GetEntryCount returns the correct count of entries.
    /// </summary>
    [TestMethod]
    public void GetEntryCount_ShouldReturnCorrectCount()
    {
        // Arrange
        for (int i = 0; i < 3; i++)
        {
            _logWriter.Add(CreateTestLogEntry($"Entry {i}"));
        }

        // Act
        var count = _logReader.GetEntryCount();

        // Assert
        count.Should().Be(3);
    }

    /// <summary>
    /// Tests that GetAllEntries preserves all fields of the log entries correctly.
    /// </summary>
    [TestMethod]
    public void GetAllEntries_ShouldPreserveAllFieldsCorrectly()
    {
        // Arrange
        var entry = new TestLogEntry
        {
            Timestamp = new DateTimeOffset(2023, 3, 15, 14, 30, 45, TimeSpan.FromHours(2)),
            Level = LogLevel.Information,
            Sender = "TestSender",
            MessageTemplate = "Test Message",
            Details = "Detailed information"
        };

        _logWriter.Add(entry);

        // Act
        var result = _logReader.GetAllEntries().First();

        // Assert
        result.Timestamp.Should().Be(entry.Timestamp);
        result.Level.Should().Be(entry.Level);
        result.Sender.Should().Be(entry.Sender);
        result.MessageTemplate.Should().Be(entry.MessageTemplate);
        result.Details.Should().Be(entry.Details);
    }

    /// <summary>
    /// Tests that GetAllEntries handles different log types correctly.
    /// </summary>
    [TestMethod]
    public void GetAllEntries_ShouldHandleDifferentLogTypes()
    {
        // Arrange
        _logWriter.Add(CreateTestLogEntry("Info Message", level: LogLevel.Information ));
        _logWriter.Add(CreateTestLogEntry("Warning Message", level: LogLevel.Warning));
        _logWriter.Add(CreateTestLogEntry("Error Message", level: LogLevel.Error));

        // Act
        var entries = _logReader.GetAllEntries();

        // Assert
        entries.Should().HaveCount(3);
        entries.Should().Contain(e => e.Level == LogLevel.Information);
        entries.Should().Contain(e => e.Level == LogLevel.Warning);
        entries.Should().Contain(e => e.Level == LogLevel.Error);
    }

    /// <summary>
    /// Tests that GetRecentEntries returns all entries when the maximum count is larger than the total number of entries.
    /// </summary>
    [TestMethod]
    public void GetRecentEntries_ShouldReturnAllEntriesWhenMaxCountIsLargerThanTotal()
    {
        // Arrange
        for (int i = 0; i < 3; i++)
        {
            _logWriter.Add(CreateTestLogEntry($"Entry {i}"));
        }

        // Act
        var result = _logReader.GetRecentEntries(10);

        // Assert
        result.Should().HaveCount(3);
    }

    /// <summary>
    /// Creates a test log entry with the specified message, timestamp, and log type.
    /// </summary>
    /// <param name="message">The message for the log entry.</param>
    /// <param name="timestamp">The timestamp for the log entry.</param>
    /// <param name="level">The log type for the log entry.</param>
    /// <returns>A new TestLogEntry instance.</returns>
    private TestLogEntry CreateTestLogEntry(string message = "Test Message", DateTimeOffset? timestamp = null, LogLevel level = LogLevel.Information)
    {
        return new TestLogEntry
        {
            Timestamp = timestamp ?? DateTimeOffset.Now,
            Level = level,
            Sender = "LogReaderTests",
            MessageTemplate = message,
            Details = "Test Details"
        };
    }
}
