using System.Globalization;
using System.Text;

namespace CDS.SQLiteLogging;

/// <summary>
/// A lightweight, high-performance formatter for structured log messages.
/// </summary>
class StructuredMessageFormatter
{
    // The text to substitute when a parameter is missing or an error occurs during formatting.
    private const string MissingParameterSubstitution = "MissingMsgParam";

    // A cache to store parsed templates for fast re-use.
    private static readonly Dictionary<string, List<TemplateSegment>> TemplateCache = new Dictionary<string, List<TemplateSegment>>();

    /// <summary>
    /// Formats the provided message template by substituting placeholders with values from the parameters.
    /// Supports advanced formatting (e.g. {Key:format}) and escaping of braces (e.g. '{{' and '}}').
    /// </summary>
    /// <param name="template">The message template containing placeholders.</param>
    /// <param name="parameters">Optional: the list of key-value pairs for substitution. The tempate is returned unaltered if this is null.</param>
    /// <returns>The fully formatted message.</returns>
    /// <exception cref="ArgumentNullException">Thrown if template is null.</exception>
    public string Format(string template, IEnumerable<KeyValuePair<string, object>>? parameters)
    {
        if (template == null)
        {
            throw new ArgumentNullException(nameof(template));
        }

        if (parameters == null)
        {
            return template;
        }

        // Build a dictionary from the provided key-value pairs for fast lookup.
        var paramDict = parameters.ToDictionary(kv => kv.Key, kv => kv.Value);

        // Retrieve the parsed template from the cache if available.
        List<TemplateSegment>? segments;
        if (!TemplateCache.TryGetValue(template, out segments))
        {
            segments = ParseTemplate(template);
            TemplateCache[template] = segments;
        }

        // Build the formatted string by appending each segment.
        var sb = new StringBuilder();
        foreach (var segment in segments)
        {
            segment.Append(sb, paramDict);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Parses the message template into a list of segments (literals and placeholders).
    /// </summary>
    private static List<TemplateSegment> ParseTemplate(string template)
    {
        var segments = new List<TemplateSegment>();
        int pos = 0;
        int len = template.Length;
        var literalBuilder = new StringBuilder();

        while (pos < len)
        {
            char c = template[pos];
            if (c == '{')
            {
                // Check for escaped open brace "{{"
                if (pos + 1 < len && template[pos + 1] == '{')
                {
                    literalBuilder.Append('{');
                    pos += 2;
                }
                else
                {
                    // Flush any accumulated literal text.
                    if (literalBuilder.Length > 0)
                    {
                        segments.Add(new LiteralSegment(literalBuilder.ToString()));
                        literalBuilder.Clear();
                    }
                    int start = pos + 1;
                    int end = template.IndexOf('}', start);
                    if (end == -1)
                    {
                        // No matching closing brace found; treat the rest as literal.
                        literalBuilder.Append(template.Substring(pos));
                        break;
                    }
                    // Extract the placeholder content between braces.
                    string placeholderContent = template.Substring(start, end - start);

                    // Split by colon to separate the key and the optional format specifier.
                    string key;
                    string? format = null;
                    int colonIndex = placeholderContent.IndexOf(':');
                    if (colonIndex >= 0)
                    {
                        key = placeholderContent.Substring(0, colonIndex).Trim();
                        format = placeholderContent.Substring(colonIndex + 1).Trim();
                    }
                    else
                    {
                        key = placeholderContent.Trim();
                    }

                    segments.Add(new PlaceholderSegment(key, format, MissingParameterSubstitution));
                    pos = end + 1;
                }
            }
            else if (c == '}')
            {
                // Check for escaped closing brace "}}"
                if (pos + 1 < len && template[pos + 1] == '}')
                {
                    literalBuilder.Append('}');
                    pos += 2;
                }
                else
                {
                    // Unescaped '}' – treat it as a literal character.
                    literalBuilder.Append(c);
                    pos++;
                }
            }
            else
            {
                literalBuilder.Append(c);
                pos++;
            }
        }

        // Flush any remaining literal text.
        if (literalBuilder.Length > 0)
        {
            segments.Add(new LiteralSegment(literalBuilder.ToString()));
        }
        return segments;
    }

    /// <summary>
    /// Abstract base class for segments of the parsed template.
    /// </summary>
    private abstract class TemplateSegment
    {
        public abstract void Append(StringBuilder sb, IDictionary<string, object> parameters);
    }

    /// <summary>
    /// Represents a literal text segment in the template.
    /// </summary>
    private class LiteralSegment : TemplateSegment
    {
        private readonly string _text;
        public LiteralSegment(string text)
        {
            _text = text;
        }
        public override void Append(StringBuilder sb, IDictionary<string, object> parameters)
        {
            sb.Append(_text);
        }
    }

    /// <summary>
    /// Represents a placeholder segment (e.g. {Key} or {Key:format}) in the template.
    /// </summary>
    private class PlaceholderSegment : TemplateSegment
    {
        private readonly string _key;
        private readonly string? _format;
        private readonly string _missingSubstitution;
        public PlaceholderSegment(string key, string? format, string missingSubstitution)
        {
            _key = key;
            _format = format;
            _missingSubstitution = missingSubstitution;
        }
        public override void Append(StringBuilder sb, IDictionary<string, object> parameters)
        {
            // Look up the parameter by key.
            if (!parameters.TryGetValue(_key, out object? value))
            {
                sb.Append(_missingSubstitution);
                return;
            }
            if (value == null)
            {
                sb.Append("null");
                return;
            }
            try
            {
                // If a format is specified and the value supports formatting, use it.
                if (!string.IsNullOrEmpty(_format) && value is IFormattable formattable)
                {
                    sb.Append(formattable.ToString(_format, CultureInfo.InvariantCulture));
                }
                else
                {
                    sb.Append(value.ToString());
                }
            }
            catch
            {
                // In case of any formatting error, inject the missing substitution.
                sb.Append(_missingSubstitution);
            }
        }
    }
}
