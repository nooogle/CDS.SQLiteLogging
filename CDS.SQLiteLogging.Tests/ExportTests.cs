using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CDS.SQLiteLogging.Tests;

/// <summary>
/// Contains tests for the export functionality that copies log entries from one database to another.
/// </summary>
[TestClass]
[TestCategory("Export")]
public class ExportTests
{
    /// <summary>
    /// Tests that exporting all entries from one database to another results in correct count and data.
    /// </summary>
    [TestMethod]
    public void ExportAsync_WithAllEntries_ShouldCopyAllData()
    {
        // Arrange
        const int entryCount = 50;
        string sourceDbPath = GetTempDbPath();
        string destinationDbPath = GetTempDbPath();

        try
        {
            // Create source database with log entries
            CreateDatabaseWithEntries(sourceDbPath, entryCount);

            // Get all IDs
            long[] allIds;
            using (var sourceReader = new Reader(sourceDbPath))
            {
                allIds = sourceReader.GetAllEntries()
                    .Select(e => e.DbId)
                    .ToArray();
            }

            // Act
            Exporter.Export(sourceDbPath, destinationDbPath, allIds);

            // Assert
            using var destReader = new Reader(destinationDbPath);
            var exportedEntries = destReader.GetAllEntries();
            
            exportedEntries.Should().HaveCount(entryCount);
            exportedEntries.Select(e => e.RenderedMessage).Should().OnlyContain(m => m.StartsWith("Log entry "));
        }
        finally
        {
            CleanupDatabase(sourceDbPath);
            CleanupDatabase(destinationDbPath);
        }
    }

    /// <summary>
    /// Tests that exporting selected entries only copies those specific entries.
    /// </summary>
    [TestMethod]
    public void ExportAsync_WithSelectedEntries_ShouldCopyOnlySelectedData()
    {
        // Arrange
        const int totalEntries = 100;
        const int entriesToExport = 10;
        string sourceDbPath = GetTempDbPath();
        string destinationDbPath = GetTempDbPath();

        try
        {
            // Create source database
            CreateDatabaseWithEntries(sourceDbPath, totalEntries);

            // Select specific IDs and their corresponding messages to export
            long[] idsToExport;
            string[] expectedMessages;
            using (var sourceReader = new Reader(sourceDbPath))
            {
                var selectedEntries = sourceReader.GetAllEntries()
                    .Take(entriesToExport)
                    .ToArray();
                
                idsToExport = selectedEntries.Select(e => e.DbId).ToArray();
                expectedMessages = selectedEntries.Select(e => e.RenderedMessage).ToArray();
            }

            // Act
            Exporter.Export(sourceDbPath, destinationDbPath, idsToExport);

            // Assert
            using var destReader = new Reader(destinationDbPath);
            var exportedEntries = destReader.GetAllEntries();
            
            // Verify count
            exportedEntries.Should().HaveCount(entriesToExport);
            
            // Verify the actual content (messages) match, since DbIds are auto-generated in the destination
            var exportedMessages = exportedEntries.Select(e => e.RenderedMessage).OrderBy(m => m).ToArray();
            var sortedExpectedMessages = expectedMessages.OrderBy(m => m).ToArray();
            exportedMessages.Should().BeEquivalentTo(sortedExpectedMessages);
        }
        finally
        {
            CleanupDatabase(sourceDbPath);
            CleanupDatabase(destinationDbPath);
        }
    }

    /// <summary>
    /// Tests that exporting entries preserves all data including structured logging properties.
    /// </summary>
    [TestMethod]
    public void ExportAsync_ShouldPreserveStructuredLoggingData()
    {
        // Arrange
        string sourceDbPath = GetTempDbPath();
        string destinationDbPath = GetTempDbPath();
        const string username = "testuser";
        const string action = "login";

        try
        {
            // Create source database with structured logging using direct approach
            var sqliteLoggerProvider = MEL.MELLoggerProvider.Create(
                fileName: sourceDbPath,
                batchingOptions: new BatchingOptions(),
                houseKeepingOptions: new HouseKeepingOptions { Mode = HousekeepingMode.Manual },
                dateTimeProvider: new DefaultDateTimeProvider());

            var loggerUtilities = sqliteLoggerProvider.LoggerUtilities;
            var serviceProvider = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.AddProvider(sqliteLoggerProvider);
                    builder.SetMinimumLevel(LogLevel.Trace);
                })
                .AddSingleton(loggerUtilities)
                .BuildServiceProvider();

            var logger = serviceProvider.GetRequiredService<ILogger<ExportTests>>();
            logger.LogInformation("User {Username} performed {Action}", username, action);

            loggerUtilities.WaitUntilCacheIsEmpty(TimeSpan.FromSeconds(5));
            sqliteLoggerProvider.Dispose();
            serviceProvider.Dispose();

            // Get the ID
            long[] idsToExport;
            using (var sourceReader = new Reader(sourceDbPath))
            {
                idsToExport = sourceReader.GetAllEntries()
                    .Select(e => e.DbId)
                    .ToArray();
            }

            // Act
            Exporter.Export(sourceDbPath, destinationDbPath, idsToExport);

            // Assert
            using var destReader = new Reader(destinationDbPath);
            var exportedEntries = destReader.GetAllEntries();
            
            exportedEntries.Should().HaveCount(1);
            exportedEntries[0].Properties.Should().ContainKey("Username").WhoseValue.Should().Be(username);
            exportedEntries[0].Properties.Should().ContainKey("Action").WhoseValue.Should().Be(action);
        }
        finally
        {
            CleanupDatabase(sourceDbPath);
            CleanupDatabase(destinationDbPath);
        }
    }

    /// <summary>
    /// Tests that exporting entries preserves exception data.
    /// </summary>
    [TestMethod]
    public void ExportAsync_ShouldPreserveExceptionData()
    {
        // Arrange
        string sourceDbPath = GetTempDbPath();
        string destinationDbPath = GetTempDbPath();
        const string errorMessage = "Test error occurred";

        try
        {
            // Create source database with exception using direct approach
            var sqliteLoggerProvider = MEL.MELLoggerProvider.Create(
                fileName: sourceDbPath,
                batchingOptions: new BatchingOptions(),
                houseKeepingOptions: new HouseKeepingOptions { Mode = HousekeepingMode.Manual },
                dateTimeProvider: new DefaultDateTimeProvider());

            var loggerUtilities = sqliteLoggerProvider.LoggerUtilities;
            var serviceProvider = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.AddProvider(sqliteLoggerProvider);
                    builder.SetMinimumLevel(LogLevel.Trace);
                })
                .AddSingleton(loggerUtilities)
                .BuildServiceProvider();

            var logger = serviceProvider.GetRequiredService<ILogger<ExportTests>>();
            try
            {
                throw new InvalidOperationException("Test exception", new ArgumentException("Inner exception"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, errorMessage);
            }

            loggerUtilities.WaitUntilCacheIsEmpty(TimeSpan.FromSeconds(5));
            sqliteLoggerProvider.Dispose();
            serviceProvider.Dispose();

            // Get the ID
            long[] idsToExport;
            using (var sourceReader = new Reader(sourceDbPath))
            {
                idsToExport = sourceReader.GetAllEntries()
                    .Select(e => e.DbId)
                    .ToArray();
            }

            // Act
            Exporter.Export(sourceDbPath, destinationDbPath, idsToExport);

            // Assert
            using var destReader = new Reader(destinationDbPath);
            var exportedEntries = destReader.GetAllEntries();
            
            exportedEntries.Should().HaveCount(1);
            exportedEntries[0].RenderedMessage.Should().Be(errorMessage);
            exportedEntries[0].ExceptionJson.Should().Contain("InvalidOperationException");
            exportedEntries[0].ExceptionJson.Should().Contain("ArgumentException");
            
            var exceptionInfo = exportedEntries[0].GetExceptionInfo();
            exceptionInfo.Should().NotBeNull();
            exceptionInfo!.InnerException.Should().NotBeNull();
        }
        finally
        {
            CleanupDatabase(sourceDbPath);
            CleanupDatabase(destinationDbPath);
        }
    }

    /// <summary>
    /// Tests that exporting entries preserves scope data.
    /// </summary>
    [TestMethod]
    public void ExportAsync_ShouldPreserveScopeData()
    {
        // Arrange
        string sourceDbPath = GetTempDbPath();
        string destinationDbPath = GetTempDbPath();
        const string scopeName = "TestScope";

        try
        {
            // Create source database with scopes using direct approach
            var sqliteLoggerProvider = MEL.MELLoggerProvider.Create(
                fileName: sourceDbPath,
                batchingOptions: new BatchingOptions(),
                houseKeepingOptions: new HouseKeepingOptions { Mode = HousekeepingMode.Manual },
                dateTimeProvider: new DefaultDateTimeProvider());

            var loggerUtilities = sqliteLoggerProvider.LoggerUtilities;
            var serviceProvider = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.AddProvider(sqliteLoggerProvider);
                    builder.SetMinimumLevel(LogLevel.Trace);
                })
                .AddSingleton(loggerUtilities)
                .BuildServiceProvider();

            var logger = serviceProvider.GetRequiredService<ILogger<ExportTests>>();
            using (logger.BeginScope(scopeName))
            {
                logger.LogInformation("Message in scope");
            }

            loggerUtilities.WaitUntilCacheIsEmpty(TimeSpan.FromSeconds(5));
            sqliteLoggerProvider.Dispose();
            serviceProvider.Dispose();

            // Get the ID
            long[] idsToExport;
            using (var sourceReader = new Reader(sourceDbPath))
            {
                idsToExport = sourceReader.GetAllEntries()
                    .Select(e => e.DbId)
                    .ToArray();
            }

            // Act
            Exporter.Export(sourceDbPath, destinationDbPath, idsToExport);

            // Assert
            using var destReader = new Reader(destinationDbPath);
            var exportedEntries = destReader.GetAllEntries();
            
            exportedEntries.Should().HaveCount(1);
            exportedEntries[0].ScopesJson.Should().Contain(scopeName);
        }
        finally
        {
            CleanupDatabase(sourceDbPath);
            CleanupDatabase(destinationDbPath);
        }
    }

    /// <summary>
    /// Tests that exporting entries preserves different log levels.
    /// </summary>
    [TestMethod]
    public void ExportAsync_ShouldPreserveLogLevels()
    {
        // Arrange
        string sourceDbPath = GetTempDbPath();
        string destinationDbPath = GetTempDbPath();
        var logLevels = new[] { LogLevel.Debug, LogLevel.Information, LogLevel.Warning, LogLevel.Error, LogLevel.Critical };

        try
        {
            // Create source database with different log levels using direct approach
            var sqliteLoggerProvider = MEL.MELLoggerProvider.Create(
                fileName: sourceDbPath,
                batchingOptions: new BatchingOptions(),
                houseKeepingOptions: new HouseKeepingOptions { Mode = HousekeepingMode.Manual },
                dateTimeProvider: new DefaultDateTimeProvider());

            var loggerUtilities = sqliteLoggerProvider.LoggerUtilities;
            var serviceProvider = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.AddProvider(sqliteLoggerProvider);
                    builder.SetMinimumLevel(LogLevel.Trace);
                })
                .AddSingleton(loggerUtilities)
                .BuildServiceProvider();

            var logger = serviceProvider.GetRequiredService<ILogger<ExportTests>>();
            foreach (var level in logLevels)
            {
                logger.Log(level, $"{level} message");
            }

            loggerUtilities.WaitUntilCacheIsEmpty(TimeSpan.FromSeconds(5));
            sqliteLoggerProvider.Dispose();
            serviceProvider.Dispose();

            // Get all IDs
            long[] idsToExport;
            using (var sourceReader = new Reader(sourceDbPath))
            {
                idsToExport = sourceReader.GetAllEntries()
                    .Select(e => e.DbId)
                    .OrderBy(id => id) // Ensure consistent ordering
                    .ToArray();
            }

            // Act
            Exporter.Export(sourceDbPath, destinationDbPath, idsToExport);

            // Assert
            using var destReader = new Reader(destinationDbPath);
            var exportedEntries = destReader.GetAllEntries();
            
            exportedEntries.Should().HaveCount(logLevels.Length);
            exportedEntries.Select(e => e.Level).Should().BeEquivalentTo(logLevels);
        }
        finally
        {
            CleanupDatabase(sourceDbPath);
            CleanupDatabase(destinationDbPath);
        }
    }

    /// <summary>
    /// Tests that exporting with large number of IDs (>999) works correctly due to batching.
    /// </summary>
    [TestMethod]
    public void ExportAsync_WithLargeNumberOfIds_ShouldHandleBatching()
    {
        // Arrange
        const int entryCount = 2000; // More than SQLite's 999 parameter limit
        string sourceDbPath = GetTempDbPath();
        string destinationDbPath = GetTempDbPath();

        try
        {
            // Create source database with many entries
            CreateDatabaseWithEntries(sourceDbPath, entryCount);

            // Get all IDs and messages
            long[] allIds;
            string[] expectedMessages;
            using (var sourceReader = new Reader(sourceDbPath))
            {
                var allEntries = sourceReader.GetAllEntries();
                allIds = allEntries.Select(e => e.DbId).ToArray();
                expectedMessages = allEntries.Select(e => e.RenderedMessage).ToArray();
            }

            // Act
            Exporter.Export(sourceDbPath, destinationDbPath, allIds);

            // Assert
            using var destReader = new Reader(destinationDbPath);
            var exportedEntries = destReader.GetAllEntries();
            
            // Verify count
            exportedEntries.Should().HaveCount(entryCount);
            
            // Verify the actual content (messages) match, since DbIds are auto-generated in the destination
            var exportedMessages = exportedEntries.Select(e => e.RenderedMessage).OrderBy(m => m).ToArray();
            var sortedExpectedMessages = expectedMessages.OrderBy(m => m).ToArray();
            exportedMessages.Should().BeEquivalentTo(sortedExpectedMessages);
        }
        finally
        {
            CleanupDatabase(sourceDbPath);
            CleanupDatabase(destinationDbPath);
        }
    }

    /// <summary>
    /// Tests that exporting throws ArgumentException when source database path is null or empty.
    /// </summary>
    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    public void ExportAsync_WithInvalidSourcePath_ShouldThrowArgumentException(string invalidPath)
    {
        // Arrange
        string destinationDbPath = GetTempDbPath();
        var idsToExport = new long[] { 1, 2, 3 };

        // Act & Assert
        Action act = () => Exporter.Export(invalidPath!, destinationDbPath, idsToExport);
        act.Should().Throw<ArgumentException>()
            .WithParameterName("dbFileNameSource");

        CleanupDatabase(destinationDbPath);
    }

    /// <summary>
    /// Tests that exporting throws ArgumentException when destination database path is null or empty.
    /// </summary>
    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    public void ExportAsync_WithInvalidDestinationPath_ShouldThrowArgumentException(string invalidPath)
    {
        // Arrange
        string sourceDbPath = GetTempDbPath();
        CreateDatabaseWithEntries(sourceDbPath, 5);
        var idsToExport = new long[] { 1, 2, 3 };

        try
        {
            // Act & Assert
            Action act = () => Exporter.Export(sourceDbPath, invalidPath!, idsToExport);
            act.Should().Throw<ArgumentException>()
                .WithParameterName("dbFileNameDestination");
        }
        finally
        {
            CleanupDatabase(sourceDbPath);
        }
    }

    /// <summary>
    /// Tests that exporting throws ArgumentException when IDs array is null or empty.
    /// </summary>
    [TestMethod]
    public void ExportAsync_WithNullOrEmptyIds_ShouldThrowArgumentException()
    {
        // Arrange
        string sourceDbPath = GetTempDbPath();
        string destinationDbPath = GetTempDbPath();
        CreateDatabaseWithEntries(sourceDbPath, 5);

        try
        {
            // Act & Assert - null IDs
            Action actNull = () => Exporter.Export(sourceDbPath, destinationDbPath, null!);
            actNull.Should().Throw<ArgumentException>()
                .WithParameterName("idsToExport");

            // Act & Assert - empty IDs
            Action actEmpty = () => Exporter.Export(sourceDbPath, destinationDbPath, Array.Empty<long>());
            actEmpty.Should().Throw<ArgumentException>()
                .WithParameterName("idsToExport");
        }
        finally
        {
            CleanupDatabase(sourceDbPath);
            CleanupDatabase(destinationDbPath);
        }
    }

    /// <summary>
    /// Tests that exporting non-existent IDs doesn't cause errors and results in empty destination.
    /// </summary>
    [TestMethod]
    public void ExportAsync_WithNonExistentIds_ShouldNotThrowAndCreateEmptyDestination()
    {
        // Arrange
        string sourceDbPath = GetTempDbPath();
        string destinationDbPath = GetTempDbPath();
        CreateDatabaseWithEntries(sourceDbPath, 5);
        
        // Use IDs that don't exist
        var nonExistentIds = new long[] { 999, 1000, 1001 };

        try
        {
            // Act
            Exporter.Export(sourceDbPath, destinationDbPath, nonExistentIds);

            // Assert
            using var destReader = new Reader(destinationDbPath);
            var exportedEntries = destReader.GetAllEntries();
            exportedEntries.Should().BeEmpty();
        }
        finally
        {
            CleanupDatabase(sourceDbPath);
            CleanupDatabase(destinationDbPath);
        }
    }

    /// <summary>
    /// Tests that exporting every other entry works correctly.
    /// </summary>
    [TestMethod]
    public void ExportAsync_WithEveryOtherEntry_ShouldCopyCorrectSubset()
    {
        // Arrange
        const int totalEntries = 20;
        string sourceDbPath = GetTempDbPath();
        string destinationDbPath = GetTempDbPath();

        try
        {
            CreateDatabaseWithEntries(sourceDbPath, totalEntries);

            // Get every other entry's ID and message (GetAllEntries returns in descending timestamp order)
            long[] everyOtherId;
            string[] expectedMessages;
            using (var sourceReader = new Reader(sourceDbPath))
            {
                var selectedEntries = sourceReader.GetAllEntries()
                    .Where((_, index) => index % 2 == 0)
                    .ToArray();
                
                everyOtherId = selectedEntries.Select(e => e.DbId).ToArray();
                expectedMessages = selectedEntries.Select(e => e.RenderedMessage).ToArray();
            }

            // Act
            Exporter.Export(sourceDbPath, destinationDbPath, everyOtherId);

            // Assert
            using var destReader = new Reader(destinationDbPath);
            var exportedEntries = destReader.GetAllEntries();
            
            // Verify count
            exportedEntries.Should().HaveCount(totalEntries / 2);
            
            // Verify the actual content (messages) match, since DbIds are auto-generated in the destination
            var exportedMessages = exportedEntries.Select(e => e.RenderedMessage).OrderBy(m => m).ToArray();
            var sortedExpectedMessages = expectedMessages.OrderBy(m => m).ToArray();
            exportedMessages.Should().BeEquivalentTo(sortedExpectedMessages);
        }
        finally
        {
            CleanupDatabase(sourceDbPath);
            CleanupDatabase(destinationDbPath);
        }
    }

    /// <summary>
    /// Tests async export method works correctly.
    /// </summary>
    [TestMethod]
    public async Task ExportAsync_AsyncMethod_ShouldWork()
    {
        // Arrange
        const int entryCount = 25;
        string sourceDbPath = GetTempDbPath();
        string destinationDbPath = GetTempDbPath();

        try
        {
            CreateDatabaseWithEntries(sourceDbPath, entryCount);

            long[] allIds;
            using (var sourceReader = new Reader(sourceDbPath))
            {
                allIds = sourceReader.GetAllEntries()
                    .Select(e => e.DbId)
                    .ToArray();
            }

            // Act
            await Exporter.ExportAsync(sourceDbPath, destinationDbPath, allIds);

            // Assert
            using var destReader = new Reader(destinationDbPath);
            var exportedEntries = destReader.GetAllEntries();
            exportedEntries.Should().HaveCount(entryCount);
        }
        finally
        {
            CleanupDatabase(sourceDbPath);
            CleanupDatabase(destinationDbPath);
        }
    }

    /// <summary>
    /// Tests that export can be cancelled via CancellationToken.
    /// </summary>
    [TestMethod]
    public async Task ExportAsync_WithCancellation_ShouldThrowOperationCanceledException()
    {
        // Arrange
        const int entryCount = 1000;
        string sourceDbPath = GetTempDbPath();
        string destinationDbPath = GetTempDbPath();

        try
        {
            CreateDatabaseWithEntries(sourceDbPath, entryCount);

            long[] allIds;
            using (var sourceReader = new Reader(sourceDbPath))
            {
                allIds = sourceReader.GetAllEntries()
                    .Select(e => e.DbId)
                    .ToArray();
            }

            using var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel immediately

            // Act & Assert
            Func<Task> act = async () => await Exporter.ExportAsync(sourceDbPath, destinationDbPath, allIds, cts.Token);
            await act.Should().ThrowAsync<OperationCanceledException>();
        }
        finally
        {
            CleanupDatabase(sourceDbPath);
            CleanupDatabase(destinationDbPath);
        }
    }

    #region Helper Methods

    /// <summary>
    /// Gets a temporary database path.
    /// </summary>
    private static string GetTempDbPath()
    {
        return Path.Combine(
            Path.GetTempPath(),
            $"TestExport_{Guid.NewGuid()}_V{MEL.MELLogger.DBSchemaVersion}.db");
    }

    /// <summary>
    /// Creates a database with specified number of log entries.
    /// </summary>
    private static void CreateDatabaseWithEntries(string dbPath, int entryCount)
    {
        // Create the SQLite logger provider
        var sqliteLoggerProvider = MEL.MELLoggerProvider.Create(
            fileName: dbPath,
            batchingOptions: new BatchingOptions() { MaxCacheSize = entryCount * 2 },
            houseKeepingOptions: new HouseKeepingOptions { Mode = HousekeepingMode.Manual },
            dateTimeProvider: new DefaultDateTimeProvider());

        var loggerUtilities = sqliteLoggerProvider.LoggerUtilities;

        var serviceProvider = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddProvider(sqliteLoggerProvider);
                builder.SetMinimumLevel(LogLevel.Trace);
            })
            .AddSingleton(loggerUtilities)
            .BuildServiceProvider();

        var logger = serviceProvider.GetRequiredService<ILogger<ExportTests>>();
        
        for (int i = 0; i < entryCount; i++)
        {
            logger.LogInformation($"Log entry {i}");
        }

        // Ensure all entries are written
        loggerUtilities.WaitUntilCacheIsEmpty(TimeSpan.FromSeconds(10));
        
        // Dispose in correct order
        sqliteLoggerProvider.Dispose();
        serviceProvider.Dispose();
        
        // Clear connection pool to release file handles
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        
        // Verify entries were actually written
        using var verifyReader = new Reader(dbPath);
        int actualCount = verifyReader.GetEntryCount();
        if (actualCount != entryCount)
        {
            throw new InvalidOperationException($"Expected {entryCount} entries but found {actualCount} in database");
        }
    }

    /// <summary>
    /// Cleans up a database file.
    /// </summary>
    private static void CleanupDatabase(string dbPath)
    {
        if (string.IsNullOrEmpty(dbPath))
            return;

        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        
        if (File.Exists(dbPath))
        {
            try
            {
                File.Delete(dbPath);
            }
            catch
            {
                // Ignore cleanup errors in tests
            }
        }
    }

    #endregion
}
