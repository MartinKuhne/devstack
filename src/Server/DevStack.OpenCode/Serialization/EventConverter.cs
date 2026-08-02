using System.Text.Json;

using DevStack.OpenCode.Models;

namespace DevStack.OpenCode.Serialization;

/// <summary>Reads and writes an <see cref="SdkEvent"/> from JSON, preserving the discriminator and payload.</summary>
public sealed class EventConverter : JsonConverter<SdkEvent>
{
    /// <inheritdoc />
    public override SdkEvent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        var type = root.TryGetProperty("type", out var t) ? t.GetString() ?? string.Empty : string.Empty;
        return new SdkEvent(type, root.Clone());
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, SdkEvent value, JsonSerializerOptions options)
    {
        writer.WriteRawValue(value.Raw.GetRawText());
    }
}
