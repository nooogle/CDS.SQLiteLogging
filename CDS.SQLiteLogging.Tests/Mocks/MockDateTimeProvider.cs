namespace CDS.SQLiteLogging.Tests.Mocks;


/// <summary>
/// Mock implementation of <see cref="IDateTimeProvider"/> for testing.
/// </summary>
public class MockDateTimeProvider : IDateTimeProvider
{
    /// <inheritdoc/>
    public DateTimeOffset Now { get; set; } = DateTimeOffset.Now;
}
