using AwesomeAssertions;
using CDS.SQLiteLogging.Tests.Support;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CDS.SQLiteLogging.Tests;

/// <summary>
/// Contains tests verifying that <see cref="GlobalLogContext"/> values are captured
/// at the moment of each log call, not when the background thread dequeues the entry.
/// </summary>
[TestClass]
public class GlobalLogContextTests
{
    private const string SessionKey = "Session";

    /// <summary>
    /// Verifies that each log entry captures the <see cref="GlobalLogContext"/> value
    /// that was active at the time of the call, even if the context is changed before
    /// the background processing thread flushes the batch.
    /// </summary>
    [TestMethod]
    [TestCategory("GlobalLogContext")]
    public void LogEntry_ShouldCaptureGlobalContextAtCallTime_NotAtFlushTime()
    {
        var testHost = new NewDatabaseTestHost
        {
            LogPipeline = new LogPipeline([new GlobalLogContextMiddleware()]),
        };

        testHost.Run(
            onDatabaseCreated: (serviceProvider, dbPath) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<GlobalLogContextTests>>();

                // Arrange: set the first context value and log an entry.
                GlobalLogContext.Set(SessionKey, "first");
                logger.LogInformation("Entry one");

                // Act: change the context before the background thread has a chance to
                // flush the first entry — the fix ensures "first" was already baked in.
                GlobalLogContext.Set(SessionKey, "second");
                logger.LogInformation("Entry two");
            },

            onDatabaseClosed: dbPath =>
            {
                GlobalLogContext.Clear();

                // Assert
                using var reader = new Reader(dbPath);
                var entries = reader.GetAllEntries()
                    .OrderBy(e => e.Timestamp)
                    .ToList();

                entries.Should().HaveCount(2);

                // Entry one must carry the context that was active when it was logged.
                var firstProps = entries[0].Properties as IDictionary<string, object>;
                firstProps.Should().ContainKey(SessionKey)
                    .WhoseValue.Should().Be("first");

                // Entry two must carry the updated context value.
                var secondProps = entries[1].Properties as IDictionary<string, object>;
                secondProps.Should().ContainKey(SessionKey)
                    .WhoseValue.Should().Be("second");
            });
    }
}
