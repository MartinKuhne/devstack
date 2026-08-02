using System.Text.Json;

using DevStack.OpenCode.Models;

namespace DevStack.OpenCode.Serialization;

/// <summary>
/// Reads and writes a <see cref="BashPermissionUnion"/> from JSON, supporting
/// the OpenCode shape of either a flat string action or a string map.
/// </summary>
public sealed class BashPermissionUnionConverter : JsonConverter<BashPermissionUnion>
{
    /// <inheritdoc />
    public override BashPermissionUnion? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return BashPermissionUnion.FromAction(reader.GetString()
                    ?? throw new JsonException("OpenCode bash permission cannot be null."));

            case JsonTokenType.StartObject:
                {
                    var map = JsonSerializer.Deserialize<Dictionary<string, string>>(ref reader, options)
                        ?? new Dictionary<string, string>(StringComparer.Ordinal);
                    return BashPermissionUnion.FromMap(map);
                }

            case JsonTokenType.Null:
                return null;

            default:
                throw new JsonException(
                    $"OpenCode bash permission must be a string or object; got {reader.TokenType}.");
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, BashPermissionUnion value, JsonSerializerOptions options)
    {
        if (value.IsMap)
        {
            JsonSerializer.Serialize(writer, value.Payload, options);
        }
        else
        {
            writer.WriteStringValue((string)value.Payload);
        }
    }
}
