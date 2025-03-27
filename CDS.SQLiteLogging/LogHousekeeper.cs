namespace CDS.SQLiteLogging;

/// <summary>
/// Provides automated housekeeping for log entries, periodically deleting old records.
/// </summary>
public class LogHousekeeper<TLogEntry> : IDisposable where TLogEntry : ILogEntry, new()
{
    private readonly ConnectionManager connectionManager;
    private readonly string tableName;
    private readonly Timer cleanupTimer;
    private bool disposed;
    private TimeSpan retentionPeriod;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogHousekeeper{TLogEntry}"/> class.
    /// </summary>
    /// <param name="connectionManager">The SQLite connection manager.</param>
    /// <param name="tableName">The name of the table to maintain.</param>
    /// <param name="retentionPeriod">How long to keep entries before deleting them.</param>
    /// <param name="cleanupInterval">How often to run the cleanup process.</param>
    public LogHousekeeper(
        ConnectionManager connectionManager,
        string tableName,
        TimeSpan retentionPeriod,
        TimeSpan cleanupInterval)
    {
        this.connectionManager = connectionManager;
        this.tableName = tableName;
        this.retentionPeriod = retentionPeriod;

        // Start the timer to run cleanup at the specified interval
        cleanupTimer = new Timer(
            CleanupCallback,
            null,
            TimeSpan.Zero,  // Start immediately
            cleanupInterval);
    }

    /// <summary>
    /// Gets or sets the retention period for log entries.
    /// </summary>
    public TimeSpan RetentionPeriod
    {
        get => retentionPeriod;
        set => retentionPeriod = value;
    }


    /// <summary>
    /// Callback method that performs the cleanup operation.
    /// </summary>
    /// <param name="state">State object (not used).</param>
    private async void CleanupCallback(object state)
    {
        try
        {
            await DeleteEntriesOlderThanAsync(DateTime.UtcNow - retentionPeriod)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during log cleanup: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes entries older than the specified date.
    /// </summary>
    /// <param name="cutoffDate">The cutoff date for deletion.</param>
    /// <returns>The number of entries deleted.</returns>
    public async Task<int> DeleteEntriesOlderThanAsync(DateTime cutoffDate)
    {
        int deletedCount = 0;

        await connectionManager.ExecuteInTransactionAsync(async transaction =>
        {
            string formattedDate = cutoffDate.ToString("o"); // ISO 8601 format
            string sql = $"DELETE FROM {tableName} WHERE Timestamp < @cutoffDate";

            using var cmd = new SqliteCommand(sql, connectionManager.Connection, transaction);
            cmd.Parameters.AddWithValue("@cutoffDate", formattedDate);
            deletedCount = await Task.Run(() => cmd.ExecuteNonQuery()).ConfigureAwait(false);

        }).ConfigureAwait(false);

        return deletedCount;
    }

    /// <summary>
    /// Deletes entries older than the specified date (synchronous version).
    /// </summary>
    /// <param name="cutoffDate">The cutoff date for deletion.</param>
    /// <returns>The number of entries deleted.</returns>
    public int DeleteEntriesOlderThan(DateTime cutoffDate)
    {
        return DeleteEntriesOlderThanAsync(cutoffDate).GetAwaiter().GetResult();
    }


    /// <summary>
    /// Disposes resources used by the housekeeper.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the resources used by the housekeeper.
    /// </summary>
    /// <param name="disposing">Indicates whether the method is called from Dispose.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // Stop the timer
                cleanupTimer?.Dispose();
            }
            disposed = true;
        }
    }


    /// <summary>
    /// Deletes all entries from the database.
    /// </summary>
    /// <returns>The number of entries deleted.</returns>
    public async Task<int> DeleteAllAsync()
    {
        int deletedCount = 0;

        await connectionManager.ExecuteInTransactionAsync(async transaction =>
        {
            string sql = $"DELETE FROM {tableName}";

            using var cmd = new SqliteCommand(sql, connectionManager.Connection, transaction);
            deletedCount = await Task.Run(() => cmd.ExecuteNonQuery()).ConfigureAwait(false);
        }).ConfigureAwait(false);

        return deletedCount;
    }

    /// <summary>
    /// Deletes all entries from the database (synchronous version).
    /// </summary>
    /// <returns>The number of entries deleted.</returns>
    public int DeleteAll()
    {
        return DeleteAllAsync().GetAwaiter().GetResult();
    }
}
