namespace CDS.SQLiteLogging;

/// <summary>
/// Default implementation of <see cref="IDateTimeProvider"/> that uses <see cref="DateTimeOffset.Now"/>.
/// </summary>
public class DefaultDateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc/>
    public DateTimeOffset Now => DateTimeOffset.Now;
}
