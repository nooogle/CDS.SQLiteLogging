using System.Collections.Concurrent;

namespace CDS.SQLiteLogging;

/// <summary>
/// Provides a static, thread-safe global context for log properties.
/// </summary>
public static class GlobalLogContext
{
    private static readonly ConcurrentDictionary<string, object> context = new();

    /// <summary>
    /// Gets the global context dictionary.
    /// </summary>
    public static ConcurrentDictionary<string, object> Context => context;

    /// <summary>
    /// Sets a value in the global context.
    /// </summary>
    public static void Set(string key, object value) => context[key] = value;

    /// <summary>
    /// Removes a value from the global context.
    /// </summary>
    public static bool Remove(string key) => context.TryRemove(key, out _);

    /// <summary>
    /// Clears all values from the global context.
    /// </summary>
    public static void Clear() => context.Clear();
}
