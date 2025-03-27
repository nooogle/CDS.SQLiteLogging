using System.Diagnostics;

namespace WinFormsTest;

/// <summary>
/// Manages application log folder creation and paths.
/// </summary>
public static class LogFolderManager
{
    /// <summary>
    /// Gets the standard log folder path for the application.
    /// </summary>
    /// <param name="createIfNotExists">Whether to create the folder if it doesn't exist.</param>
    /// <returns>The log folder path.</returns>
    public static string GetLogFolder(bool createIfNotExists = true)
    {
        string folderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            nameof(CDS.SQLiteLogging),
            nameof(WinFormsTest),
            "Logs");

        // Send the folder path to debug window
        Debug.WriteLine($"Log folder path: {folderPath}");

        // Create the directory if it doesn't exist and the flag is set
        if (createIfNotExists && !Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.WriteLine($"Created log folder: {folderPath}");
        }

        return folderPath;
    }
}
