using CDS.SQLiteLogging.Internal;

namespace CDS.SQLiteLogging;

/// <summary>
/// Provides housekeeping for log entries, periodically deleting old records.
/// </summary>
public class Housekeeper : IDisposable
{
    private readonly ConnectionManager connectionManager;
    private Timer? cleanupTimer;
    private bool disposed;
    private readonly HouseKeepingOptions options;
    private int cleanupInProgress;
    private IDateTimeProvider dateTimeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="Housekeeper"/> class.
    /// </summary>
    /// <param name="connectionManager">The SQLite connection manager.</param>
    /// <param name="options">The housekeeping configuration options.</param>
    public Housekeeper(
        ConnectionManager connectionManager,
        HouseKeepingOptions options,
        IDateTimeProvider dateTimeProvider)
    {
        this.dateTimeProvider = dateTimeProvider;
        this.connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        this.options = options ?? throw new ArgumentNullException(nameof(options));

        // Start the timer only if in automatic mode
        if (options.Mode == HousekeepingMode.Automatic)
        {
            cleanupTimer = new Timer(
                CleanupCallback,
                null,
                TimeSpan.Zero,  // Start immediately
                options.CleanupInterval);
        }
    }

    /// <summary>
    /// Gets or sets the retention period for log entries.
    /// </summary>
    public TimeSpan RetentionPeriod
    {
        get => options.RetentionPeriod;
        set => options.RetentionPeriod = value;
    }

    /// <summary>
    /// Gets the current housekeeping mode.
    /// </summary>
    public HousekeepingMode Mode => options.Mode;

    /// <summary>
    /// Executes a housekeeping cycle that removes old entries to limit the database size.
    /// Can be called manually regardless of the housekeeping mode.
    /// </summary>
    /// <returns>The number of entries deleted.</returns>
    public int ExecuteHousekeeping()
    {
        try
        {
            return DeleteOldEntries();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during manual log cleanup: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Callback method that performs the cleanup operation.
    /// </summary>
    /// <param name="state">State object (not used).</param>
    private void CleanupCallback(object? state)
    {
        // If a cleanup is already in progress, skip this invocation
        if (Interlocked.CompareExchange(ref cleanupInProgress, 1, 0) != 0)
        {
            System.Diagnostics.Debug.WriteLine("Skipping log cleanup because previous operation is still in progress");
            return;
        }

        try
        {
            DeleteOldEntries();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during log cleanup: {ex.Message}");
        }
        finally
        {
            // Reset the flag indicating cleanup is done
            Interlocked.Exchange(ref cleanupInProgress, 0);
        }
    }

    /// <summary>
    /// Deletes entries older than the specified date or until the database size is within the limit.
    /// </summary>
    /// <returns>The number of entries deleted.</returns>
    public int DeleteOldEntries()
    {
        var cutoffDate = dateTimeProvider.Now - options.RetentionPeriod;
        int totalDeletedCount = 0;

        connectionManager.ExecuteInTransaction(transaction =>
        {
            // First, always delete records older than cutoff date
            string formattedDate = cutoffDate.ToString("o"); // ISO 8601 format
            string sql = $"DELETE FROM {TableCreator.TableName} WHERE Timestamp < @cutoffDate";

            using (var cmd = new SqliteCommand(sql, connectionManager.Connection, transaction))
            {
                cmd.Parameters.AddWithValue("@cutoffDate", formattedDate);
                totalDeletedCount += cmd.ExecuteNonQuery();
            }
        });

        // If we deleted any records, vacuum the database to reclaim space
        if (totalDeletedCount > 0)
        {
            using var vacuumCmd = new SqliteCommand("VACUUM", connectionManager.Connection);
            vacuumCmd.ExecuteNonQuery();
        }

        return totalDeletedCount;
    }

    /// <summary>
    /// Deletes all entries from the database.
    /// </summary>
    /// <returns>The number of entries deleted.</returns>
    public int DeleteAll()
    {
        int deletedCount = 0;

        connectionManager.ExecuteInTransaction(transaction =>
        {
            string sql = $"DELETE FROM {TableCreator.TableName}";

            using var cmd = new SqliteCommand(sql, connectionManager.Connection, transaction);
            deletedCount = cmd.ExecuteNonQuery();
        });

        return deletedCount;
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
                cleanupTimer = null;
            }
            disposed = true;
        }
    }
}

