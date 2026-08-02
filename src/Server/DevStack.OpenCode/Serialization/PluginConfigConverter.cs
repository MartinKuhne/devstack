using System.Text.Json;

namespace DevStack.OpenCode.Models;

/// <summary>
/// Reads and writes a <see cref="PluginConfig"/> from JSON. Accepts either a
/// bare string (plugin name) or a 2-element array <c>[name, options]</c>.
/// </summary>
public sealed class PluginConfigConverter : JsonConverter<PluginConfig>
{
    /// <inheritdoc />
    public override PluginConfig Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                {
                    var name = reader.GetString()
                        ?? throw new JsonException("OpenCode plugin name cannot be null.");
                    return PluginConfig.FromName(name);
                }

            case JsonTokenType.StartArray:
                {
                    if (!reader.Read())
                    {
                        throw new JsonException("Unexpected end of OpenCode plugin array.");
                    }

                    if (reader.TokenType != JsonTokenType.String)
                    {
                        throw new JsonException(
                            $"OpenCode plugin tuple first element must be a string; got {reader.TokenType}.");
                    }

                    var name = reader.GetString()
                        ?? throw new JsonException("OpenCode plugin name cannot be null.");

                    if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
                    {
                        throw new JsonException(
                            "OpenCode plugin tuple second element must be an options object.");
                    }

                    using var doc = JsonDocument.ParseValue(ref reader);
                    var opts = doc.RootElement.Deserialize<Dictionary<string, JsonElement>>(options)
                        ?? new Dictionary<string, JsonElement>(StringComparer.Ordinal);

                    if (!reader.Read() || reader.TokenType != JsonTokenType.EndArray)
                    {
                        throw new JsonException("OpenCode plugin tuple must have exactly two elements.");
                    }

                    return PluginConfig.FromTuple(name, opts);
                }

            case JsonTokenType.Null:
                return default;

            default:
                throw new JsonException(
                    $"OpenCode plugin must be a string or [name, options] array; got {reader.TokenType}.");
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, PluginConfig value, JsonSerializerOptions options)
    {
        if (value.Options is null)
        {
            writer.WriteStringValue(value.Name);
        }
        else
        {
            writer.WriteStartArray();
            writer.WriteStringValue(value.Name);
            JsonSerializer.Serialize(writer, value.Options, options);
            writer.WriteEndArray();
        }
    }
}
