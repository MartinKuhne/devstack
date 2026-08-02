using System.Text.Json;

namespace DevStack.OpenCode.Models;

/// <summary>
/// Reads and writes a <see cref="FormatterConfig"/> from JSON, supporting
/// boolean toggles, missing/null (use built-ins), and per-formatter maps.
/// </summary>
public sealed class FormatterConfigConverter : JsonConverter<FormatterConfig>
{
    /// <inheritdoc />
    public override FormatterConfig? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.True:
                return FormatterConfig.FromBool(true);
            case JsonTokenType.False:
                return FormatterConfig.FromBool(false);
            case JsonTokenType.Null:
                return FormatterConfig.FromBool(true);
            case JsonTokenType.StartObject:
                {
                    using var doc = JsonDocument.ParseValue(ref reader);
                    var map = doc.RootElement.Deserialize<Dictionary<string, FormatterOverride>>(options)
                        ?? new Dictionary<string, FormatterOverride>(StringComparer.Ordinal);
                    return FormatterConfig.FromMap(map);
                }
            default:
                throw new JsonException(
                    $"OpenCode formatter must be a boolean or object; got {reader.TokenType}.");
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, FormatterConfig value, JsonSerializerOptions options)
    {
        switch (value.Kind)
        {
            case FormatterConfigKind.Bool:
                writer.WriteBooleanValue(value.Enabled ?? true);
                break;
            case FormatterConfigKind.Map:
                JsonSerializer.Serialize(writer, value.Map, options);
                break;
        }
    }
}
