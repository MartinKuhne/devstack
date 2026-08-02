using System.Text.Json;

namespace DevStack.OpenCode.Models;

/// <summary>
/// Reads and writes an <see cref="LspConfig"/> from JSON, supporting boolean
/// toggles, missing/null (use built-ins), and per-language override maps.
/// </summary>
public sealed class LspConfigConverter : JsonConverter<LspConfig>
{
    /// <inheritdoc />
    public override LspConfig? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.True:
                return LspConfig.FromBool(true);
            case JsonTokenType.False:
                return LspConfig.FromBool(false);
            case JsonTokenType.Null:
                return LspConfig.FromBool(true);
            case JsonTokenType.StartObject:
                {
                    using var doc = JsonDocument.ParseValue(ref reader);
                    var map = doc.RootElement.Deserialize<Dictionary<string, LspServerConfig>>(options)
                        ?? new Dictionary<string, LspServerConfig>(StringComparer.Ordinal);
                    return LspConfig.FromMap(map);
                }
            default:
                throw new JsonException(
                    $"OpenCode LSP must be a boolean or object; got {reader.TokenType}.");
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, LspConfig value, JsonSerializerOptions options)
    {
        switch (value.Kind)
        {
            case LspConfigKind.Bool:
                writer.WriteBooleanValue(value.Enabled ?? true);
                break;
            case LspConfigKind.Map:
                JsonSerializer.Serialize(writer, value.Map, options);
                break;
        }
    }
}
