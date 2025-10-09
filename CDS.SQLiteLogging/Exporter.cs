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
    /// <param name="cancellationToken">Cancellation token for long-running operations.</param>
    /// <returns>A task representing the asynchronous export operation.</returns>
    /// <exception cref="ArgumentException">Thrown when file paths or IDs are invalid.</exception>
    public static async Task ExportAsync(
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

        if (idsToExport == null)
        {
            throw new ArgumentException("Value cannot be null or empty.", nameof(idsToExport));
        }

        using var sourceConnectionManager = new ConnectionManager(dbFileNameSource);
        using var destinationConnectionManager = new ConnectionManager(dbFileNameDestination);

        await DirectDBExporter.ExportAsync(
            sourceConnectionManager,
            destinationConnectionManager,
            idsToExport,
            cancellationToken).ConfigureAwait(false);
    }
}
