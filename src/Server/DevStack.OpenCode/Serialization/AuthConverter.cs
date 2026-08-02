using System.Text.Json;

using DevStack.OpenCode.Models;

namespace DevStack.OpenCode.Serialization;

/// <summary>Reads and writes an <see cref="Auth"/> from JSON, preserving the discriminator.</summary>
public sealed class AuthConverter : JsonConverter<Auth>
{
    /// <inheritdoc />
    public override Auth? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        var kind = root.TryGetProperty("type", out var t) ? t.GetString() ?? string.Empty : string.Empty;
        return new Auth(kind, root.Clone());
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Auth value, JsonSerializerOptions options)
    {
        writer.WriteRawValue(value.Raw.GetRawText());
    }
}
