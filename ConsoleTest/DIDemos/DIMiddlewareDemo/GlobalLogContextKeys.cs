namespace ConsoleTest.DIMiddlewareDemo;

/// <summary>
/// Keys for the global log context.
/// </summary>
public static class GlobalLogContextKeys
{
    /// <summary>
    /// The key for the application ID in the global log context.
    /// </summary>
    public const string AppId = "AppId";


    /// <summary>
    /// The key for the user ID in the global log context.
    /// </summary>
    public const string BatchNumber = "BatchNumber";
}
