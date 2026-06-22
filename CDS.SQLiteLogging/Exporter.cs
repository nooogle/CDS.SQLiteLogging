using CDS.SQLiteLogging.Internal;

namespace CDS.SQLiteLogging;

/// <summary>
/// Provides functionality to export log entries from one SQLite database to another.
/// </summary>
public static class Exporter
{
    /// <summary>
    /// Exports log entries by ID from a source database to a destination database.
    /// </summary>
    /// <param name="dbFileNameSource">The source database file path.</param>
    /// <param name="dbFileNameDestination">The destination database file path.</param>
    /// <param name="idsToExport">The array of log entry IDs to export.</param>
    /// <param name="cancellationToken">Cancellation token checked between batches.</param>
    /// <exception cref="ArgumentException">Thrown when file paths or IDs are invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when none of the requested IDs exist in the source database.</exception>
    public static void Export(
        string dbFileNameSource,
        string dbFileNameDestination,
        long[] idsToExport,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dbFileNameSource))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(dbFileNameSource));
        }

        if (string.IsNullOrWhiteSpace(dbFileNameDestination))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(dbFileNameDestination));
        }

        if (idsToExport == null || idsToExport.Length == 0)
        {
            throw new ArgumentException("Value cannot be null or empty.", nameof(idsToExport));
        }

        using var sourceConnectionManager = new ConnectionManager(dbFileNameSource);
        using var destinationConnectionManager = new ConnectionManager(dbFileNameDestination);

        int exportedRowCount = DirectDBExporter.Export(
            sourceConnectionManager,
            destinationConnectionManager,
            idsToExport,
            cancellationToken);

        if (exportedRowCount == 0)
        {
            throw new InvalidOperationException("No log entries were exported. Ensure that the supplied IDs are persisted DbId values from the source database.");
        }
    }

    /// <summary>
    /// Exports log entries by ID from a source database to a destination database.
    /// </summary>
    /// <param name="dbFileNameSource">The source database file path.</param>
    /// <param name="dbFileNameDestination">The destination database file path.</param>
    /// <param name="idsToExport">The array of log entry IDs to export.</param>
    /// <param name="cancellationToken">Cancellation token checked between batches.</param>
    /// <returns>A completed task.</returns>
    [Obsolete("Use Export() instead. SQLite has no native async I/O; this wrapper provides no concurrency benefit.")]
    public static Task ExportAsync(
        string dbFileNameSource,
        string dbFileNameDestination,
        long[] idsToExport,
        CancellationToken cancellationToken = default)
    {
        Export(dbFileNameSource, dbFileNameDestination, idsToExport, cancellationToken);
        return Task.CompletedTask;
    }
}
