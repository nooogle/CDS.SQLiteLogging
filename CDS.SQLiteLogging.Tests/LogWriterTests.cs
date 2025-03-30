using FluentAssertions;
using CDS.SQLiteLogging.Tests.TestSupport;
using Microsoft.Extensions.Logging;

namespace CDS.SQLiteLogging.Tests;

/// <summary>
/// Unit tests for the LogWriter class.
/// </summary>
[TestClass]
public class LogWriterTests
{
    private string _testFolder;
    private ConnectionManager _connectionManager;
    private string _tableName;
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
    /// Tests that a log entry is inserted correctly.
    /// </summary>
    [TestMethod]
    public void Add_ShouldInsertLogEntry()
    {
        // Arrange
        var logWriter = new LogWriter<TestLogEntry>(_connectionManager, _tableName);
        var entry = CreateTestLogEntry();

        // Act
        logWriter.Add(entry);

        // Assert
        int count = _logReader.GetEntryCount();
        count.Should().Be(1);
    }

    /// <summary>
    /// Tests that a log entry is inserted correctly using the asynchronous method.
    /// </summary>
    [TestMethod]
    public async Task AddAsync_ShouldInsertLogEntry()
    {
        // Arrange
        var logWriter = new LogWriter<TestLogEntry>(_connectionManager, _tableName);
        var entry = CreateTestLogEntry();

        // Act
        await logWriter.AddAsync(entry);

        // Assert
        int count = _logReader.GetEntryCount();
        count.Should().Be(1);
    }

    /// <summary>
    /// Tests that multiple log entries are inserted correctly using the asynchronous batch method.
    /// </summary>
    [TestMethod]
    public async Task AddBatchAsync_ShouldInsertMultipleEntries()
    {
        // Arrange
        var logWriter = new LogWriter<TestLogEntry>(_connectionManager, _tableName);
        var entries = new List<TestLogEntry>
        {
            CreateTestLogEntry("Entry 1"),
            CreateTestLogEntry("Entry 2"),
            CreateTestLogEntry("Entry 3")
        };

        // Act
        await logWriter.AddBatchAsync(entries);

        // Assert
        int count = _logReader.GetEntryCount();
        count.Should().Be(3);
    }

    /// <summary>
    /// Tests that a log entry with a DateTimeOffset is handled correctly.
    /// </summary>
    [TestMethod]
    public void Add_ShouldHandleDateTimeOffset()
    {
        // Arrange
        var logWriter = new LogWriter<TestLogEntry>(_connectionManager, _tableName);
        var entry = new TestLogEntry
        {
            Timestamp = new DateTimeOffset(2023, 1, 1, 12, 0, 0, TimeSpan.FromHours(2)),
            MessageTemplate = "Testing DateTimeOffset"
        };

        // Act
        logWriter.Add(entry);

        // Assert
        var storedEntry = _logReader.GetAllEntries().FirstOrDefault();
        storedEntry.Should().NotBeNull();
        storedEntry.Timestamp.Should().Be(entry.Timestamp);
    }

    /// <summary>
    /// Tests that adding a null log entry throws an ArgumentNullException.
    /// </summary>
    [TestMethod]
    public void Add_ShouldHandleNullEntry()
    {
        // Arrange
        var logWriter = new LogWriter<TestLogEntry>(_connectionManager, _tableName);

        // Act
        Action act = () => logWriter.Add(null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that adding a null log entry asynchronously throws an ArgumentNullException.
    /// </summary>
    [TestMethod]
    public async Task AddAsync_ShouldHandleNullEntry()
    {
        // Arrange
        var logWriter = new LogWriter<TestLogEntry>(_connectionManager, _tableName);

        // Act
        Func<Task> act = async () => await logWriter.AddAsync(null);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that adding an empty list of log entries asynchronously does not insert any entries.
    /// </summary>
    [TestMethod]
    public async Task AddBatchAsync_ShouldHandleEmptyList()
    {
        // Arrange
        var logWriter = new LogWriter<TestLogEntry>(_connectionManager, _tableName);
        var entries = new List<TestLogEntry>();

        // Act
        await logWriter.AddBatchAsync(entries);

        // Assert
        int count = _logReader.GetEntryCount();
        count.Should().Be(0);
    }

    /// <summary>
    /// Tests that adding a list of log entries with a null entry asynchronously throws an ArgumentNullException.
    /// </summary>
    [TestMethod]
    public async Task AddBatchAsync_ShouldHandleNullEntryInList()
    {
        // Arrange
        var logWriter = new LogWriter<TestLogEntry>(_connectionManager, _tableName);
        var entries = new List<TestLogEntry>
        {
            CreateTestLogEntry("Entry 1"),
            null,
            CreateTestLogEntry("Entry 3")
        };

        // Act
        Func<Task> act = async () => await logWriter.AddBatchAsync(entries);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that all fields of a log entry are inserted correctly.
    /// </summary>
    [TestMethod]
    public void Add_ShouldInsertAllFieldsCorrectly()
    {
        // Arrange
        var logWriter = new LogWriter<TestLogEntry>(_connectionManager, _tableName);
        var entry = CreateTestLogEntry();

        // Act
        logWriter.Add(entry);

        // Assert
        var storedEntry = _logReader.GetAllEntries().FirstOrDefault();
        storedEntry.Should().NotBeNull();
        storedEntry.Timestamp.Should().Be(entry.Timestamp);
        storedEntry.Level.Should().Be(entry.Level);
        storedEntry.Sender.Should().Be(entry.Sender);
        storedEntry.MessageTemplate.Should().Be(entry.MessageTemplate);
        storedEntry.Details.Should().Be(entry.Details);
    }

    /// <summary>
    /// Tests that a burst of log entries are added correctly.
    /// </summary>
    [TestMethod]
    public void BurstLogEntriesTest_ShouldAddEntriesCorrectly()
    {
        // Arrange
        var logWriter = new LogWriter<TestLogEntry>(_connectionManager, _tableName);
        int numberOfEntries = 1000; // Example number of entries

        // Act
        for (int i = 0; i < numberOfEntries; i++)
        {
            var entry = new TestLogEntry
            {
                Timestamp = DateTimeOffset.Now,
                Level = LogLevel.Information,
                Sender = "BurstLogEntriesTest",
                MessageTemplate = $"Log entry {i}",
                Details = "Test Details"
            };
            logWriter.Add(entry);
        }

        // Assert
        int count = _logReader.GetEntryCount();
        count.Should().Be(numberOfEntries);
    }

    /// <summary>
    /// Creates a test log entry with the specified message.
    /// </summary>
    /// <param name="message">The message for the log entry.</param>
    /// <returns>A new TestLogEntry instance.</returns>
    private TestLogEntry CreateTestLogEntry(string message = "Test Message")
    {
        return new TestLogEntry
        {
            Timestamp = DateTimeOffset.Now,
            Level = LogLevel.Information,
            Sender = "LogWriterTests",
            MessageTemplate = message,
            Details = "Test Details"
        };
    }
}
