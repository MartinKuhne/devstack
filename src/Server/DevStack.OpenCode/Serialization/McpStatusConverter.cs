using System.Text.Json;

using DevStack.OpenCode.Models;

namespace DevStack.OpenCode.Serialization;

/// <summary>Reads and writes an <see cref="McpStatus"/> from JSON, preserving the discriminator.</summary>
public sealed class McpStatusConverter : JsonConverter<McpStatus>
{
    /// <inheritdoc />
    public override McpStatus? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        var status = root.TryGetProperty("status", out var s) ? s.GetString() ?? string.Empty : string.Empty;
        return new McpStatus(status, root.Clone());
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, McpStatus value, JsonSerializerOptions options)
    {
        writer.WriteRawValue(value.Raw.GetRawText());
    }
}
