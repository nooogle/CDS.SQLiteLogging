namespace WinFormsAppSeriLog;

/// <summary>
/// The main entry point for the application.
/// </summary>
internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
#if NET48
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new FormMain());
#else
        ApplicationConfiguration.Initialize();
        Application.Run(new FormMain());
#endif
    }
}
