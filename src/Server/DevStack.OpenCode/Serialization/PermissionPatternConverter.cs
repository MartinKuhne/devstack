using System.Text.Json;

using DevStack.OpenCode.Models;

namespace DevStack.OpenCode.Serialization;

/// <summary>
/// Reads and writes a <see cref="PermissionPattern"/> from JSON <c>string</c>
/// or <c>string[]</c>.
/// </summary>
public sealed class PermissionPatternConverter : JsonConverter<PermissionPattern>
{
    /// <inheritdoc />
    public override PermissionPattern Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return PermissionPattern.FromSingle(reader.GetString()
                    ?? throw new JsonException("OpenCode permission pattern cannot be null."));

            case JsonTokenType.StartArray:
                {
                    var list = JsonSerializer.Deserialize<List<string>>(ref reader, options)
                        ?? new List<string>();
                    return PermissionPattern.FromMany(list);
                }

            case JsonTokenType.Null:
                return default;

            default:
                throw new JsonException(
                    $"OpenCode permission pattern must be a string or array of strings; got {reader.TokenType}.");
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, PermissionPattern value, JsonSerializerOptions options)
    {
        if (value.IsSingle)
        {
            writer.WriteStringValue(value.Single);
        }
        else if (value.IsMany)
        {
            JsonSerializer.Serialize(writer, value.Many, options);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
