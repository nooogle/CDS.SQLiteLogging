namespace SqliteLogger.Tests.TestSupport;


/// <summary>
/// Helper class for managing test databases.
/// </summary>
public static class TestDatabaseHelper
{
    /// <summary>
    /// Creates a temporary folder for database testing.
    /// </summary>
    /// <returns>The path to the temporary test folder.</returns>
    public static string GetTemporaryDatabaseFolder()
    {
        string tempFolder = Path.Combine(
            Path.GetTempPath(),
            "SqliteLoggerTests",
            $"Test_{Guid.NewGuid():N}");

        Directory.CreateDirectory(tempFolder);
        System.Diagnostics.Debug.WriteLine($"Created test folder for DB: {tempFolder}");
        return tempFolder;
    }

    /// <summary>
    /// Deletes a test database folder and all its contents.
    /// </summary>
    /// <param name="folderPath">The folder path to delete.</param>
    public static void DeleteTestFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            try
            {
                Directory.Delete(folderPath, recursive: true);
            }
            catch (IOException)
            {
                // If deletion fails (e.g., due to a file being locked), 
                // we'll just let the OS clean it up later
            }
        }
    }
}
