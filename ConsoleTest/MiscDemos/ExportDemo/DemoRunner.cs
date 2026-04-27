using CDS.SQLiteLogging;
using Microsoft.Extensions.Logging;

namespace ConsoleTest.ExportDemo;

/// <summary>
/// Demonstrates exporting log entries from one SQLite database to another.
/// This demo reads log entries from an existing database and exports every other entry to a new database file.
/// </summary>
internal static class DemoRunner
{
    /// <summary>
    /// Runs the export demo asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the source database file does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no log entries are found in the source database.</exception>
    public static async Task RunAsync()
    {
        try
        {
            Console.Clear();
            Console.WriteLine("=== SQLite Log Export Demo ===\n");

            // Step 1: Get source database path
            string dbPath = DBPathCreator.Create();
            Console.WriteLine($"Source database: {dbPath}");

            // Validate source database exists
            if (!File.Exists(dbPath))
            {
                throw new FileNotFoundException($"Source database file not found: {dbPath}");
            }

            // Step 2: Create export destination path
            string exportPath = Path.Combine(
                Path.GetDirectoryName(dbPath) ?? string.Empty,
                "ExportedLog.db");
            Console.WriteLine($"Destination database: {exportPath}\n");

            // Delete existing export file if it exists
            if (File.Exists(exportPath))
            {
                Console.WriteLine("Deleting existing export database...");
                File.Delete(exportPath);
            }

            // Step 3: Read all entries from source database
            Console.WriteLine("Reading log entries from source database...");
            using var reader = new Reader(dbPath);
            var allEntries = reader.GetAllEntries();

            if (allEntries.Count == 0)
            {
                throw new InvalidOperationException("No log entries found in the source database.");
            }

            Console.WriteLine($"Found {allEntries.Count:N0} log entries in source database.");

            // Step 4: Select every other entry to export (demonstrating filtering)
            long[] everyOtherDbId = allEntries
                .Where((entry, index) => index % 2 == 0)
                .Select(entry => entry.DbId)
                .ToArray();

            Console.WriteLine($"Exporting {everyOtherDbId.Length:N0} entries (every other entry)...\n");

            // Step 5: Perform the export
            var startTime = DateTime.Now;
            await Exporter.ExportAsync(
                dbFileNameSource: dbPath,
                dbFileNameDestination: exportPath,
                idsToExport: everyOtherDbId).ConfigureAwait(false);

            var duration = DateTime.Now - startTime;

            // Step 6: Verify export success
            Console.WriteLine($"Export completed in {duration.TotalSeconds:F2} seconds.\n");
            
            using var exportedReader = new Reader(exportPath);
            int exportedCount = exportedReader.GetEntryCount();
            
            Console.WriteLine($"Verification:");
            Console.WriteLine($"  - Expected entries: {everyOtherDbId.Length:N0}");
            Console.WriteLine($"  - Actual entries:   {exportedCount:N0}");
            Console.WriteLine($"  - Status: {(exportedCount == everyOtherDbId.Length ? "SUCCESS ✓" : "FAILED ✗")}");

            // Step 7: Display sample entries from export
            Console.WriteLine("\nSample of exported entries:");
            var sampleEntries = exportedReader.GetRecentEntries(5);
            foreach (var entry in sampleEntries)
            {
                Console.WriteLine($"  [{entry.Level}] {entry.Timestamp:yyyy-MM-dd HH:mm:ss} - {entry.RenderedMessage}");
            }

            Console.WriteLine($"\nExport file location: {exportPath}");
        }
        catch (FileNotFoundException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nError: {ex.Message}");
            Console.WriteLine("Please ensure the source database exists before running this demo.");
            Console.ResetColor();
            throw;
        }
        catch (InvalidOperationException ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nWarning: {ex.Message}");
            Console.WriteLine("Please run a logging demo first to generate log entries.");
            Console.ResetColor();
            throw;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nUnexpected error during export: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Console.ResetColor();
            throw;
        }
    }

    /// <summary>
    /// Runs the export demo synchronously (wrapper for backward compatibility).
    /// </summary>
    public static void Run()
    {
        RunAsync().GetAwaiter().GetResult();
    }
}
