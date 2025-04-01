
namespace CDS.SQLiteLogging;


/// <summary>
/// Provides utility methods for managing an SQLite log database.
/// </summary>
public interface ISQLiteLoggerUtilities
{
    /// <summary>
    /// Waits until the cache is empty. This is useful when you want 
    /// to ensure that all log entries have been written to the database,
    /// usually before shutting down the application.
    /// </summary>
    /// <param name="timeout">The maximum time to wait for the cache to empty.</param>
    void WaitUntilCacheIsEmpty(TimeSpan timeout);


    /// <summary>
    /// Waits until the cache is empty asynchronously. This is useful when you want
    /// to ensure that all log entries have been written to the database,
    /// usually before shutting down the application.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    /// <param name="timeout">The maximum time to wait for the cache to empty.</param>
    Task WaitUntilCacheIsEmptyAsync(TimeSpan timeout);


    /// <summary>
    /// Flushes any pending log entries to the database.
    /// </summary>
    void Flush();


    /// <summary>
    /// Flushes any pending log entries to the database asynchronously.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    Task FlushAsync();


    /// <summary>
    /// Deletes all log entries from the database.
    /// </summary>
    /// <returns>
    /// Number of entries deleted.
    /// </returns>
    int DeleteAll();


    /// <summary>
    /// Deletes all log entries from the database asynchronously.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The completed task result is the number of entries deleted.
    /// </returns>
    Task<int> DeleteAllAsync();


    /// <summary>
    /// Gets the size of the database file.
    /// </summary>
    /// <returns>
    /// Size of the database file in bytes.
    /// </returns>
    long GetDatabaseFileSize();


    /// <summary>
    /// The number of log entries that are pending to be written to the database.
    /// </summary>
    int PendingEntriesCount { get; }


    /// <summary>
    /// The number of log entries that have been discarded because the cache was full.
    /// </summary>
    int DiscardedEntriesCount { get; }


    /// <summary>
    /// Resets the count of discarded log entries.
    /// </summary>
    void ResetDiscardedEntriesCount();
}
