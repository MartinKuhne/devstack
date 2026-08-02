using System.Text.Json;

namespace DevStack.OpenCode.Models;

/// <summary>
/// Reads and writes an <see cref="McpServerConfig"/> from JSON, dispatching
/// to local, remote, or simple toggle shapes based on the <c>type</c> field
/// or the presence of <c>command</c> / <c>url</c> discriminators.
/// </summary>
public sealed class McpServerConfigConverter : JsonConverter<McpServerConfig>
{
    /// <inheritdoc />
    public override McpServerConfig? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException(
                $"OpenCode MCP server entry must be an object; got {reader.TokenType}.");
        }

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("OpenCode MCP server entry must be an object.");
        }

        // Discriminator precedence: explicit "type", then "command"/"url", then a fallback
        // to the minimal { "enabled": bool } shape.
        if (root.TryGetProperty("type", out var typeElem))
        {
            var typeStr = typeElem.GetString();
            return typeStr switch
            {
                "local" => McpServerConfig.FromLocal(root.Deserialize<McpLocalConfig>(options)
                    ?? throw new JsonException("Failed to deserialize MCP local config.")),
                "remote" => McpServerConfig.FromRemote(root.Deserialize<McpRemoteConfig>(options)
                    ?? throw new JsonException("Failed to deserialize MCP remote config.")),
                _ => throw new JsonException(
                    $"OpenCode MCP server 'type' must be 'local' or 'remote'; got '{typeStr}'."),
            };
        }

        if (root.TryGetProperty("command", out _))
        {
            return McpServerConfig.FromLocal(root.Deserialize<McpLocalConfig>(options)
                ?? throw new JsonException("Failed to deserialize MCP local config."));
        }

        if (root.TryGetProperty("url", out _))
        {
            return McpServerConfig.FromRemote(root.Deserialize<McpRemoteConfig>(options)
                ?? throw new JsonException("Failed to deserialize MCP remote config."));
        }

        return McpServerConfig.FromToggle(root.Deserialize<McpEnableToggle>(options)
            ?? new McpEnableToggle { Enabled = true });
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, McpServerConfig value, JsonSerializerOptions options)
    {
        switch (value.Kind)
        {
            case McpServerKind.Local:
                JsonSerializer.Serialize(writer, value.Local, options);
                break;
            case McpServerKind.Remote:
                JsonSerializer.Serialize(writer, value.Remote, options);
                break;
            case McpServerKind.Toggle:
                JsonSerializer.Serialize(writer, value.Toggle, options);
                break;
        }
    }
}
