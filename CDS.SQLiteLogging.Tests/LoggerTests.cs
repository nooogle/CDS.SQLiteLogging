//using FluentAssertions;
//using Microsoft.Extensions.Logging;

//namespace CDS.SQLiteLogging.Tests;

///// <summary>
///// Unit tests for the Logger class.
///// </summary>
//[TestClass]
//public class LoggerTests
//{
//    private string _testFolder;
//    private ConnectionManager connectionManager;
//    private SQLiteLogger<TestLogEntry> logger;

//    /// <summary>
//    /// Initializes the test environment.
//    /// </summary>
//    [TestInitialize]
//    public void Initialize()
//    {
//        _testFolder = TestDatabaseHelper.GetTemporaryDatabaseFolder();
//        connectionManager = new ConnectionManager(_testFolder, schemaVersion: TestLogEntry.Version);
//        logger = new SQLiteLogger<TestLogEntry>(_testFolder, schemaVersion: TestLogEntry.Version);
//    }

//    /// <summary>
//    /// Cleans up the test environment.
//    /// </summary>
//    [TestCleanup]
//    public void Cleanup()
//    {
//        connectionManager.Dispose();
//        TestDatabaseHelper.DeleteTestFolder(_testFolder);
//    }

//    /// <summary>
//    /// Tests that the OnAboutToAddLogEntry callback is invoked correctly.
//    /// </summary>
//    [TestMethod]
//    public void Add_ShouldInvokeOnAboutToAddLogEntry()
//    {
//        // Arrange
//        var entry = CreateTestLogEntry();
//        bool callbackInvoked = false;

//        logger.OnAboutToAddLogEntry = OnAboutToAddLogEntryCallback;

//        // Act
//        logger.Add(entry);

//        // Assert
//        callbackInvoked.Should().BeTrue();

//        void OnAboutToAddLogEntryCallback(TestLogEntry logEntry, ref bool shouldIgnore)
//        {
//            callbackInvoked = true;
//            shouldIgnore = false;
//        }
//    }


//    /// <summary>
//    /// Tests that the OnAddedLogEntry callback is invoked correctly.
//    /// </summary>
//    [TestMethod]
//    public void Add_ShouldInvokeOnAddedLogEntry()
//    {
//        // Arrange
//        var entry = CreateTestLogEntry();
//        bool callbackInvoked = false;

//        logger.OnAddedLogEntry = (logEntry) =>
//        {
//            callbackInvoked = true;
//        };

//        // Act
//        logger.Add(entry);

//        // Assert
//        callbackInvoked.Should().BeTrue();
//    }

//    /// <summary>
//    /// Tests that the OnAboutToAddLogEntry callback can prevent an entry from being added.
//    /// </summary>
//    [TestMethod]
//    public void Add_ShouldRespectOnAboutToAddLogEntryIgnore()
//    {
//        // Arrange
//        var entry = CreateTestLogEntry();

//        logger.OnAboutToAddLogEntry = OnAboutToAddLogEntryCallback;

//        // Act
//        logger.Add(entry);

//        // Assert
//        int count = logger.GetAllEntries().Count;
//        count.Should().Be(0);

//        void OnAboutToAddLogEntryCallback(TestLogEntry logEntry, ref bool shouldIgnore)
//        {
//            shouldIgnore = true;
//        }
//    }

//    /// <summary>
//    /// Creates a test log entry with the specified message.
//    /// </summary>
//    /// <param name="message">The message for the log entry.</param>
//    /// <returns>A new TestLogEntry instance.</returns>
//    private TestLogEntry CreateTestLogEntry(string message = "Test Message")
//    {
//        return new TestLogEntry
//        {
//            Timestamp = DateTimeOffset.Now,
//            Level = LogLevel.Information,
//            Sender = "LoggerTests",
//            MessageTemplate = message,
//            Details = "Test Details"
//        };
//    }
//}
