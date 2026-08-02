using System.Text.Json;

using DevStack.OpenCode.Models;

namespace DevStack.OpenCode.Serialization;

/// <summary>Reads and writes a <see cref="PartInput"/> from JSON, preserving the discriminator.</summary>
public sealed class PartInputConverter : JsonConverter<PartInput>
{
    /// <inheritdoc />
    public override PartInput? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        var type = root.TryGetProperty("type", out var t) ? t.GetString() ?? string.Empty : string.Empty;
        return new PartInput(type, root.Clone());
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, PartInput value, JsonSerializerOptions options)
    {
        writer.WriteRawValue(value.Raw.GetRawText());
    }
}
