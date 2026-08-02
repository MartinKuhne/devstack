using System.Text.Json;

namespace DevStack.OpenCode.Models;

/// <summary>
/// Reads and writes an <see cref="McpOAuthOrDisabled"/> from JSON <c>object</c>
/// or literal <c>false</c>.
/// </summary>
public sealed class McpOAuthOrDisabledConverter : JsonConverter<McpOAuthOrDisabled>
{
    /// <inheritdoc />
    public override McpOAuthOrDisabled Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.False:
            case JsonTokenType.Null:
                return McpOAuthOrDisabled.Disable();

            case JsonTokenType.StartObject:
                {
                    var config = JsonSerializer.Deserialize<McpOAuthConfig>(ref reader, options)
                        ?? throw new JsonException("Failed to deserialize MCP OAuth configuration.");
                    return McpOAuthOrDisabled.FromConfig(config);
                }

            default:
                throw new JsonException(
                    $"OpenCode MCP oauth must be an object or false; got {reader.TokenType}.");
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, McpOAuthOrDisabled value, JsonSerializerOptions options)
    {
        if (value.Disabled || value.Config is null)
        {
            writer.WriteBooleanValue(false);
        }
        else
        {
            JsonSerializer.Serialize(writer, value.Config, options);
        }
    }
}
