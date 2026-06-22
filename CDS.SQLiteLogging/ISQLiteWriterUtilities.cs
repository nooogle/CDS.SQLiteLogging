
namespace CDS.SQLiteLogging;


/// <summary>
/// Provides utility methods for managing an SQLite log database.
/// </summary>
public interface ISQLiteWriterUtilities
{
    /// <summary>
    /// Waits until the cache is empty, ensuring all log entries have been written to the
    /// database. Typically called before shutting down the application.
    /// </summary>
    /// <param name="timeout">The maximum time to wait for the cache to empty.</param>
    void WaitUntilCacheIsEmpty(TimeSpan timeout);

    /// <summary>
    /// Waits asynchronously until the cache is empty. Unlike the synchronous overload, this
    /// polls with <c>await Task.Delay</c> rather than blocking the calling thread.
    /// </summary>
    /// <param name="timeout">The maximum time to wait for the cache to empty.</param>
    Task WaitUntilCacheIsEmptyAsync(TimeSpan timeout);

    /// <summary>
    /// Deletes all log entries from the database.
    /// </summary>
    /// <returns>Number of entries deleted.</returns>
    int DeleteAll();

    /// <summary>
    /// Deletes all log entries from the database.
    /// </summary>
    /// <returns>A task containing the number of entries deleted.</returns>
    [Obsolete("Use DeleteAll() instead. SQLite has no native async I/O; this wrapper provides no concurrency benefit.")]
    Task<int> DeleteAllAsync();

    /// <summary>
    /// Deletes specific log entries from the database by their database IDs.
    /// </summary>
    /// <param name="ids">The database IDs of the entries to delete.</param>
    void DeleteByIds(long[] ids);

    /// <summary>
    /// Deletes specific log entries from the database by their database IDs.
    /// </summary>
    /// <param name="ids">The database IDs of the entries to delete.</param>
    /// <returns>A completed task.</returns>
    [Obsolete("Use DeleteByIds() instead. SQLite has no native async I/O; this wrapper provides no concurrency benefit.")]
    Task DeleteByIdsAsync(long[] ids);

    /// <summary>
    /// Gets the size of the database file.
    /// </summary>
    /// <returns>Size of the database file in bytes.</returns>
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

    /// <summary>
    /// Executes a housekeeping cycle immediately, deleting entries that exceed the
    /// retention period or the maximum entry count.
    /// </summary>
    /// <returns>The total number of entries deleted.</returns>
    int ExecuteHousekeeping();
}
