using FluentAssertions;
using SqliteLogger.Tests.TestSupport;

namespace SqliteLogger.Tests;

/// <summary>
/// Unit tests for the ConnectionManager class.
/// </summary>
[TestClass]
public class ConnectionManagerTests
{
    private string _testFolder;

    /// <summary>
    /// Initializes the test environment.
    /// </summary>
    [TestInitialize]
    public void Initialize()
    {
        _testFolder = TestDatabaseHelper.GetTemporaryDatabaseFolder();
    }

    /// <summary>
    /// Cleans up the test environment.
    /// </summary>
    [TestCleanup]
    public void Cleanup()
    {
        TestDatabaseHelper.DeleteTestFolder(_testFolder);
    }

    /// <summary>
    /// Tests that the constructor creates the database folder if it does not exist.
    /// </summary>
    [TestMethod]
    public void Constructor_ShouldCreateDatabaseFolderIfNotExists()
    {
        // Arrange
        var nonExistentFolder = Path.Combine(_testFolder, "SubFolder");
        Directory.Exists(nonExistentFolder).Should().BeFalse();

        // Act
        using var connectionManager = new ConnectionManager(nonExistentFolder, schemaVersion: 1);

        // Assert
        Directory.Exists(nonExistentFolder).Should().BeTrue();
    }

    /// <summary>
    /// Tests that the connection is created and accessible.
    /// </summary>
    [TestMethod]
    public void Connection_ShouldBeCreatedAndAccessible()
    {
        // Arrange & Act
        using var connectionManager = new ConnectionManager(_testFolder, schemaVersion: 1);

        // Assert
        connectionManager.Connection.Should().NotBeNull();
        connectionManager.Connection.State.Should().Be(System.Data.ConnectionState.Open);
    }

    /// <summary>
    /// Tests that ExecuteInTransactionAsync executes the provided action.
    /// </summary>
    [TestMethod]
    public async Task ExecuteInTransactionAsync_ShouldExecuteAction()
    {
        // Arrange
        using var connectionManager = new ConnectionManager(_testFolder, schemaVersion: 1);
        bool actionExecuted = false;

        // Act
        await connectionManager.ExecuteInTransactionAsync(transaction =>
        {
            actionExecuted = true;
            transaction.Should().NotBeNull();
            return Task.CompletedTask;
        });

        // Assert
        actionExecuted.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Dispose closes the connection.
    /// </summary>
    [TestMethod]
    public void Dispose_ShouldCloseConnection()
    {
        // Arrange
        var connectionManager = new ConnectionManager(_testFolder, schemaVersion: 1);

        // Act
        connectionManager.Dispose();

        // Assert
        connectionManager.Connection.State.Should().Be(System.Data.ConnectionState.Closed);
    }
}
