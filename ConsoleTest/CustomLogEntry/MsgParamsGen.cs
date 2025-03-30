namespace ConsoleTest.CustomLogEntry;

/// <summary>
/// Provides methods to generate random parameters such as person names and fruit names.
/// </summary>
public static class MsgParamsGen
{
    private static readonly string[] illumination = { "Bright field", "Dark field", "Cloudy day" };
    private static readonly string[] result = { "Pass", "Fail", "Uknown" };
    private static readonly Random random = new();

    /// <summary>
    /// Gets a random illumination from the predefined list.
    /// </summary>
    public static string GetIllumination() => illumination[random.Next(illumination.Length)];

    /// <summary>
    /// Gets a random inspection result from the predefined list.
    /// </summary>
    public static string GetResult() => result[random.Next(result.Length)];
}
