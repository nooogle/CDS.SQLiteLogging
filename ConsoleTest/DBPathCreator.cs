using CDS.SQLiteLogging.MEL;

namespace ConsoleTest;

/// <summary>
/// Utility class for creating the database path.
/// </summary>
static class DBPathCreator
{
    /// <summary>
    /// Creates the database file path with version information.
    /// </summary>
    /// <returns>The full path to the SQLite database file.</returns>
    public static string Create()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            nameof(CDS),
            nameof(CDS.SQLiteLogging),
            nameof(ConsoleTest),
            $"Log_V{MELLogger.DBSchemaVersion}.db");
    }
}
