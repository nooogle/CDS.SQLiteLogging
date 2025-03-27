using System.Diagnostics;

namespace ConsoleTest;

/// <summary>
/// Manages application log folder creation and paths.
/// </summary>
public static class LogFolderManager
{
    /// <summary>
    /// Gets the standard log folder path for the application.
    /// </summary>
    /// <param name="testName">The name of the test, used in the folder name</param>
    /// <returns>The log folder path.</returns>
    public static string GetLogFolder(string testName)
    {
        string folderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            nameof(CDS.SQLiteLogging),
            nameof(ConsoleTest),
            testName,
            "Logs");

        // Send the folder path to debug window
        Debug.WriteLine($"Log folder path: {folderPath}");

        // Create the directory if it doesn't exist and the flag is set
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.WriteLine($"Created log folder: {folderPath}");
        }

        return folderPath;
    }
}
