using Newtonsoft.Json;
using System.Collections;

namespace CDS.SQLiteLogViewer.Models;

/// <summary>
/// Provides methods for serializing and deserializing exceptions.
/// </summary>
static class ExceptionSerializer
{
    private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore
    };

    /// <summary>
    /// Flattens the specified exception into a serializable format.
    /// </summary>
    /// <param name="ex">The exception to flatten.</param>
    /// <returns>A <see cref="SerializableException"/> representing the flattened exception.</returns>
    private static SerializableException Flatten(Exception ex) =>
        new SerializableException
        {
            Type = ex.GetType().FullName ?? "Unknown",
            Message = ex.Message,
            StackTrace = ex.StackTrace,
            HResult = ex.HResult,
            Source = ex.Source,
            TargetSite = ex.TargetSite?.ToString(),
            Data = ex.Data.Cast<DictionaryEntry>().ToDictionary(d => d.Key.ToString()!, d => d.Value!),
            InnerException = ex.InnerException != null ? Flatten(ex.InnerException) : null
        };

    /// <summary>
    /// Serializes the specified exception to a JSON string.
    /// </summary>
    /// <param name="ex">The exception to serialize.</param>
    /// <returns>A JSON string representing the exception.</returns>
    public static string ToJson(Exception? ex)
    {
        if (ex == null) return string.Empty;

        var dto = Flatten(ex);
        return JsonConvert.SerializeObject(dto, jsonSettings);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="SerializableException"/>.
    /// </summary>
    /// <param name="json">The JSON string representing the exception.</param>
    /// <returns>A <see cref="SerializableException"/> object.</returns>
    public static SerializableException? FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;

        return JsonConvert.DeserializeObject<SerializableException>(json!);
    }
}
