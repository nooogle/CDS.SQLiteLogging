using AwesomeAssertions;
using CDS.SQLiteLogging.Tests.Support;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CDS.SQLiteLogging.Tests;

/// <summary>
/// Tests that <see cref="Reader"/> can open a database that another connection already has open,
/// as happens when a viewer app inspects a log database while the producing app is still running.
/// </summary>
[TestClass]
public class ReaderConcurrencyTests
{
    /// <summary>
    /// Reproduces opening a log viewer against a database while the writing process's connection
    /// (configured for WAL mode, matching production usage) is still live. Before the read-only
    /// connection fix, <see cref="Reader"/> forced a journal-mode PRAGMA on open, which requires
    /// exclusive access and fails while the writer connection is attached.
    /// </summary>
    [TestMethod]
    public void Reader_OpensSuccessfully_WhileWalWriterConnectionIsStillOpen()
    {
        var host = new NewDatabaseTestHost
        {
            DatabaseOptions = new DatabaseOptions { JournalMode = SqliteJournalMode.Wal },
        };

        host.Run(
            onDatabaseCreated: (serviceProvider, dbPath) =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<ReaderConcurrencyTests>>();
                logger.LogInformation("Entry");

                var loggerUtilities = serviceProvider.GetRequiredService<ISQLiteWriterUtilities>();
                loggerUtilities.WaitUntilCacheIsEmpty(TimeSpan.FromSeconds(5));

                // Act: open a Reader while the WAL writer's connection is still open (not yet disposed).
                var act = () =>
                {
                    using var reader = new Reader(dbPath);
                    reader.GetEntryCount().Should().Be(1);
                };

                act.Should().NotThrow();
            },

            onDatabaseClosed: (dbPath) => { });
    }
}
