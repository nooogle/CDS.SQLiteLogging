using Newtonsoft.Json;
using System.Collections;

namespace CDS.SQLiteLogging;

public static class ExceptionSerializer
{
    private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore
    };


    private static SerializableException Flatten(Exception ex)
    {
        return new SerializableException
        {
            Type = ex.GetType().FullName ?? "Unknown",
            Message = ex.Message,
            StackTrace = ex.StackTrace,
            HResult = ex.HResult,
            Source = ex.Source,
            TargetSite = ex.TargetSite?.ToString(),
            
            Data = 
                ex
                .Data
                .Cast<DictionaryEntry>()
                .ToDictionary(d => d.Key.ToString()!, d => d.Value!),
            
            InnerException = ex.InnerException != null ? Flatten(ex.InnerException) : null
        };
    }

    public static string ToJson(Exception? ex)
    {
        if (ex == null) { return ""; }

        var dto = Flatten(ex);

        return JsonConvert.SerializeObject(dto, jsonSettings);
    }

    public static SerializableException? FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) { return null; }

        return JsonConvert.DeserializeObject<SerializableException>(json!);
    }
}
