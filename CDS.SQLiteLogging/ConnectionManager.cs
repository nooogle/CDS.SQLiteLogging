using System.Data;

namespace CDS.SQLiteLogging;



/// <summary>
/// Manages the SQLite database connection.
/// </summary>
public class ConnectionManager : IDisposable
{
    private readonly string fileName;
    private readonly SqliteConnection connection;
    private bool disposed;
    private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);


    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionManager"/> class.
    /// </summary>
    /// <param name="fileName">The name of the SQLite database file.</param>
    public ConnectionManager(string fileName)
    {
        var folderPath = Path.GetDirectoryName(fileName) ?? string.Empty;
        Directory.CreateDirectory(folderPath);

        this.fileName = fileName;
        connection = CreateDbConnection(this.fileName);
        connection.Open();
    }


#if NET6_0_OR_GREATER
    private SqliteConnection CreateDbConnection(string dbPath)
    {
        var connection = new SqliteConnection($"Data Source={dbPath}");
        return connection;
    }
#else
    private SqliteConnection CreateDbConnection(string dbPath)
    {
        var connection = new SqliteConnection($"Data Source={dbPath};Version=3;");
        return connection;
    }
#endif

    /// <summary>
    /// Gets the SQLite connection.
    /// </summary>
    public SqliteConnection Connection => connection;

    /// <summary>
    /// Gets the database file path.
    /// </summary>
    public string DatabasePath => fileName;

    /// <summary>
    /// Executes a non-query SQL command.
    /// </summary>
    /// <param name="sql">The SQL command to execute.</param>
    public void ExecuteNonQuery(string sql)
    {
        using (var cmd = new SqliteCommand(sql, connection))
        {
            cmd.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Gets the size of the database file in bytes.
    /// </summary>
    public long GetDatabaseFileSize()
    {
        FileInfo dbFileInfo = new FileInfo(fileName);
        return dbFileInfo.Length;
    }

    /// <summary>
    /// Disposes resources used by the connection manager.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Executes an action within a transaction with retry logic for concurrency issues.
    /// </summary>
    /// <param name="action">The action to execute within a transaction.</param>
    /// <param name="isolationLevel">The transaction isolation level.</param>
    /// <returns>True if the operation completed successfully, false otherwise.</returns>
    public async Task<bool> ExecuteInTransactionAsync(
        Func<SqliteTransaction, Task> action,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        await semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            int retries = 0;
            while (retries < 3)
            {
                using (var transaction = connection.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        await action(transaction).ConfigureAwait(false);
                        transaction.Commit();
                        return true;
                    }


#if NET6_0_OR_GREATER
                    catch (SqliteException ex) when ((SqliteErrorCode)ex.SqliteErrorCode == SqliteErrorCode.Busy ||
                                                     (SqliteErrorCode)ex.SqliteErrorCode == SqliteErrorCode.Locked)
#else
                    catch (SqliteException ex) when (ex.ResultCode == SqliteErrorCode.Busy ||
                                                     ex.ResultCode == SqliteErrorCode.Locked)
#endif
                    {
                        transaction.Rollback();
                        retries++;
                        if (retries >= 3)
                            throw;

                        await Task.Delay(100 * retries).ConfigureAwait(false);
                    }
                }
            }
            return false;
        }
        finally
        {
            semaphore.Release();
        }
    }


    /// <summary>
    /// Executes an action within a transaction with retry logic for concurrency issues.
    /// </summary>
    /// <param name="action">The action to execute within a transaction.</param>
    /// <param name="isolationLevel">The transaction isolation level.</param>
    /// <returns>True if the operation completed successfully, false otherwise.</returns>
    public bool ExecuteInTransaction(
        Action<SqliteTransaction> action,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        semaphore.Wait();
        try
        {
            int retries = 0;
            while (retries < 3)
            {
                using (var transaction = connection.BeginTransaction(isolationLevel))
                {
                    try
                    {
                        action(transaction);
                        transaction.Commit();
                        return true;
                    }

#if NET6_0_OR_GREATER
                    catch (SqliteException ex) when ((SqliteErrorCode)ex.SqliteErrorCode == SqliteErrorCode.Busy ||
                                                     (SqliteErrorCode)ex.SqliteErrorCode == SqliteErrorCode.Locked)
#else
                catch (SqliteException ex) when (ex.ResultCode == SqliteErrorCode.Busy ||
                                                 ex.ResultCode == SqliteErrorCode.Locked)
#endif
                    {
                        transaction.Rollback();
                        retries++;
                        if (retries >= 3)
                            throw;

                        Thread.Sleep(100 * retries);
                    }
                }
            }
            return false;
        }
        finally
        {
            semaphore.Release();
        }
    }


    /// <summary>
    /// Executes an action with retry logic for concurrency issues.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    public async Task ExecuteWithRetryAsync(Func<Task> action)
    {
        await semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            int retries = 0;
            while (true)
            {
                try
                {
                    await action().ConfigureAwait(false);
                    return;
                }
#if NET6_0_OR_GREATER
                catch (SqliteException ex) when (((SqliteErrorCode)ex.SqliteErrorCode == SqliteErrorCode.Busy ||
                                                 (SqliteErrorCode)ex.SqliteErrorCode == SqliteErrorCode.Locked) &&
                                                 retries < 3)
#else
                    catch (SqliteException ex) when ((ex.ResultCode == SqliteErrorCode.Busy ||
                                                     ex.ResultCode == SqliteErrorCode.Locked) &&
                                                     retries < 3)
#endif
                {
                    retries++;
                    await Task.Delay(100 * retries).ConfigureAwait(false);
                }
            }
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <summary>
    /// Disposes the resources used by the connection manager.
    /// </summary>
    /// <param name="disposing">Indicates whether the method is called from Dispose.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing && connection != null)
            {
                connection.Close();
                connection.Dispose();
                semaphore.Dispose();
            }

            disposed = true;
        }
    }
}
