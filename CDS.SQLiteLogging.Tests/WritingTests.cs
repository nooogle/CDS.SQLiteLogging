using CDS.SQLiteLogging.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CDS.SQLiteLogging.Tests;

/// <summary>
/// Contains tests for writing log entries and verifying they can be read back.
/// </summary>
[TestClass]
public class WritingTests
{
    /// <summary>
    /// Tests that writing a specified number of log entries results in the correct count of entries being read back.
    /// </summary>
    /// <param name="numberOfEntries">The number of log entries to create.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(10)]
    [DataRow(100)]
    public void Test_WritingAndReadingEntries_ResultsInCorrectCount(int numberOfEntries)
    {
        // Arrange & Act
        new NewDatabaseTestHost().Run(
            onDatabaseCreated: (serviceProvider, dbPath) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<WritingTests>>();
                for (int i = 0; i < numberOfEntries; i++)
                {
                    logger.LogInformation("Processing item {ID}", i);
                }
            },

            onDatabaseClosed: (dbPath) =>
            {
                // Assert
                using var reader = new Reader(dbPath);
                reader.GetAllEntries().Should().HaveCount(numberOfEntries);
            });
    }

    /// <summary>
    /// Tests that writing a simple non-structured message can be read back correctly.
    /// </summary>
    [TestMethod]
    public void Test_WritingAndReadingSimpleMessage()
    {
        const string message = "This is a simple log message.";

        // Arrange & Act
        new NewDatabaseTestHost().Run(
            onDatabaseCreated: (serviceProvider, dbPath) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<WritingTests>>();
                logger.LogInformation(message);
            },

            onDatabaseClosed: (dbPath) =>
            {
                // Assert
                using var reader = new Reader(dbPath);
                var entries = reader.GetAllEntries();
                entries.Should().HaveCount(1);
                entries[0].RenderedMessage.Should().Be(message);
            });
    }

    /// <summary>
    /// Tests that writing a structured log message with parameters can be read back correctly.
    /// </summary>
    [TestMethod]
    public void Test_WritingAndReadingStructuredMessage()
    {
        const string messageTemplate = "User {Username} logged in at {LoginTime}.";
        const string username = "testuser";
        var loginTime = DateTime.Now;

        // Arrange & Act
        new NewDatabaseTestHost().Run(
            onDatabaseCreated: (serviceProvider, dbPath) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<WritingTests>>();
                logger.LogInformation(messageTemplate, username, loginTime);
            },

            onDatabaseClosed: (dbPath) =>
            {
                // Assert
                using var reader = new Reader(dbPath);
                var entries = reader.GetAllEntries();
                entries.Should().HaveCount(1);
                entries[0].MessageTemplate.Should().Be(messageTemplate);
                entries[0].Properties.Should().ContainKey("Username").WhoseValue.Should().Be(username);
                entries[0].Properties.Should().ContainKey("LoginTime").WhoseValue.Should().Be(loginTime);
            });
    }

    /// <summary>
    /// Tests that writing multiple structured log messages with different parameters can be read back correctly.
    /// </summary>
    [TestMethod]
    public void Test_WritingAndReadingMultipleStructuredMessages()
    {
        const string messageTemplate = "User {Username} performed action {Action} at {ActionTime}.";
        var logEntries = new[]
        {
            new { Username = "user1", Action = "login", ActionTime = DateTime.Now },
            new { Username = "user2", Action = "logout", ActionTime = DateTime.Now.AddMinutes(1) },
            new { Username = "user3", Action = "update", ActionTime = DateTime.Now.AddMinutes(2) }
        };

        // Arrange & Act
        new NewDatabaseTestHost().Run(
            onDatabaseCreated: (serviceProvider, dbPath) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<WritingTests>>();
                foreach (var entry in logEntries)
                {
                    logger.LogInformation(messageTemplate, entry.Username, entry.Action, entry.ActionTime);
                }
            },

            onDatabaseClosed: (dbPath) =>
            {
                // Assert
                using var reader = new Reader(dbPath);
                var entries = reader.GetAllEntries();
                entries.Should().HaveCount(logEntries.Length);

                for (int i = 0; i < logEntries.Length; i++)
                {
                    int reverseOrderIndex = logEntries.Length - 1 - i;

                    entries[reverseOrderIndex].MessageTemplate.Should().Be(messageTemplate);
                    entries[reverseOrderIndex].Properties.Should().ContainKey("Username").WhoseValue.Should().Be(logEntries[i].Username);
                    entries[reverseOrderIndex].Properties.Should().ContainKey("Action").WhoseValue.Should().Be(logEntries[i].Action);
                    entries[reverseOrderIndex].Properties.Should().ContainKey("ActionTime").WhoseValue.Should().Be(logEntries[i].ActionTime);
                }
            });
    }


    /// <summary>
    /// Tests that writing a log entry with an exception and a nested exception can be read back correctly.
    /// </summary>
    [TestMethod]
    public void Test_WritingAndReadingLogWithException()
    {
        const string message = "An error occurred.";

        // Arrange & Act
        new NewDatabaseTestHost().Run(
            onDatabaseCreated: (serviceProvider, dbPath) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<WritingTests>>();
                try
                {
                    ThrowNestedException();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, message);
                }
            },

            onDatabaseClosed: (dbPath) =>
            {
                // Assert
                using var reader = new Reader(dbPath);
                var entries = reader.GetAllEntries();
                entries.Should().HaveCount(1);
                entries[0].RenderedMessage.Should().Be(message);
                entries[0].ExceptionJson.Should().Contain("InvalidOperationException");
                entries[0].ExceptionJson.Should().Contain("ArgumentException");

                var exceptionInfo = entries[0].GetExceptionInfo();
                exceptionInfo.Should().NotBeNull();
                exceptionInfo!.Message.Should().Be("Invalid operation");
                exceptionInfo.InnerException.Should().NotBeNull();
                exceptionInfo.InnerException!.Message.Should().Be("Invalid argument");
            });
    }

    /// <summary>
    /// Helper method that throws an exception with a nested exception.
    /// </summary>
    private void ThrowNestedException()
    {
        try
        {
            ThrowInnerException();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Invalid operation", ex);
        }
    }

    /// <summary>
    /// Helper method that throws an inner exception.
    /// </summary>
    private void ThrowInnerException()
    {
        throw new ArgumentException("Invalid argument");
    }



    /// <summary>
    /// Tests that writing log entries within nested scopes can be read back correctly.
    /// </summary>
    [TestMethod]
    public void Test_WritingAndReadingLogWithNestedScopes()
    {
        const string outerScope = "OuterScope";
        const string innerScope = "InnerScope";
        const string message = "Logging within nested scopes.";

        // Arrange & Act
        new NewDatabaseTestHost().Run(
            onDatabaseCreated: (serviceProvider, dbPath) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<WritingTests>>();
                using (logger.BeginScope(outerScope))
                {
                    using (logger.BeginScope(innerScope))
                    {
                        logger.LogInformation(message);
                    }
                }
            },

            onDatabaseClosed: (dbPath) =>
            {
                // Assert
                using var reader = new Reader(dbPath);
                var entries = reader.GetAllEntries();
                entries.Should().HaveCount(1);
                entries[0].RenderedMessage.Should().Be(message);
                entries[0].ScopesJson.Should().Contain(outerScope);
                entries[0].ScopesJson.Should().Contain(innerScope);
            });
    }

    /// <summary>
    /// Tests that writing multiple log entries with different log levels can be read back correctly.
    /// </summary>
    [TestMethod]
    public void Test_WritingAndReadingLogWithDifferentLogLevels()
    {
        var logEntries = new[]
        {
            new { Level = LogLevel.Debug, Message = "Debug message" },
            new { Level = LogLevel.Information, Message = "Information message" },
            new { Level = LogLevel.Warning, Message = "Warning message" },
            new { Level = LogLevel.Error, Message = "Error message" },
            new { Level = LogLevel.Critical, Message = "Critical message" }
        };

        // Arrange & Act
        new NewDatabaseTestHost().Run(
            onDatabaseCreated: (serviceProvider, dbPath) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<WritingTests>>();
                foreach (var entry in logEntries)
                {
                    logger.Log(entry.Level, entry.Message);
                }
            },

            onDatabaseClosed: (dbPath) =>
            {
                // Assert
                using var reader = new Reader(dbPath);
                var entries = reader.GetAllEntries();
                entries.Should().HaveCount(logEntries.Length);

                for (int i = 0; i < logEntries.Length; i++)
                {
                    int reverseOrderIndex = logEntries.Length - 1 - i;
                    entries[reverseOrderIndex].Level.Should().Be(logEntries[i].Level);
                    entries[reverseOrderIndex].RenderedMessage.Should().Be(logEntries[i].Message);
                }
            });
    }

    /// <summary>
    /// Tests that writing a log entry with a custom property can be read back correctly.
    /// </summary>
    [TestMethod]
    public void Test_WritingAndReadingLogWithCustomProperty()
    {
        const string messageTemplate = "User {Username} performed an action.";
        const string username = "customuser";

        // Arrange & Act
        new NewDatabaseTestHost().Run(
            onDatabaseCreated: (serviceProvider, dbPath) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<WritingTests>>();
                logger.LogInformation(messageTemplate, username);
            },

            onDatabaseClosed: (dbPath) =>
            {
                // Assert
                using var reader = new Reader(dbPath);
                var entries = reader.GetAllEntries();
                entries.Should().HaveCount(1);
                entries[0].MessageTemplate.Should().Be(messageTemplate);
                entries[0].Properties.Should().ContainKey("Username").WhoseValue.Should().Be(username);
            });
    }

  
    /// <summary>
    /// Tests that a custom middleware can modify a log entry before it is written, using the pipeline builder and logger provider.
    /// </summary>
    [TestMethod]
    public void Test_CustomMiddleware_ModifiesLogEntry_WithPipelineBuilder()
    {
        // Arrange
        var customKey = "CustomMiddlewareKey";
        var customValue = "InjectedByMiddleware";
        var middleware = new TestMiddleware(customKey, customValue);
        var pipeline = CDS.SQLiteLogging.LogPipelineBuilder.Empty.Add(middleware).Build();

        // Use a custom logger provider to capture the log entry after middleware
        var host = new NewDatabaseTestHost();
        host.LogPipeline = pipeline;
        host.Run(
            onDatabaseCreated: (serviceProvider, dbPath) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<WritingTests>>();
                logger.LogInformation("Test message");
            },
            onDatabaseClosed: dbPath => { });

        // Assert
        middleware.WasInvoked.Should().BeTrue();
        middleware.ProcessedEntry.Should().NotBeNull();
        middleware.ProcessedEntry.Properties.Should().ContainKey(customKey).WhoseValue.Should().Be(customValue);
    }

    /// <summary>
    /// Middleware for testing: injects a property and tracks invocation.
    /// </summary>
    private class TestMiddleware : CDS.SQLiteLogging.ILogMiddleware
    {
        private readonly string key;
        private readonly string value;

        public bool WasInvoked { get; private set; }


        public LogEntry ProcessedEntry { get; set; } 


        public TestMiddleware(string key, string value)
        {
            this.key = key;
            this.value = value;
        }

        public Task InvokeAsync(LogEntry entry, Func<Task> next)
        {
            WasInvoked = true;
            entry.Properties ??= new Dictionary<string, object>();
            entry.Properties[key] = value;
            ProcessedEntry = entry;
            return next();
        }
    }
}
