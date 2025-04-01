namespace CDS.SQLiteLogging.Tests;


/// <summary>
/// Helper class for managing test databases.
/// </summary>
public static class TestDatabaseHelper
{
    /// <summary>
    /// Creates a temporary folder for database testing.
    /// </summary>
    /// <returns>The path to the temporary test folder.</returns>
    public static string GetTemporaryDatabaseFileName()
    {
        string dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            nameof(CDS),
            nameof(SQLiteLogging),
            nameof(Tests),
            $"Log_V{MSSQLiteLogger.DBSchemaVersion}.db");

        var dbFolder = Path.GetDirectoryName(dbPath);
        Directory.CreateDirectory(dbFolder);

        return dbPath;
    }

    /// <summary>
    /// Deletes a test database folder and all its contents.
    /// </summary>
    /// <param name="folderPath">The folder path to delete.</param>
    public static void DeleteTestFolder(string dbPath)
    {
        try
        {
            string folderPath = Path.GetDirectoryName(dbPath);

            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }

            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, recursive: true);
            }
        }
        catch (IOException)
        {
            // If deletion fails (e.g., due to a file being locked), 
            // we'll just let the OS clean it up later
        }
    }
}
