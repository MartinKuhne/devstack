using System.Text.Json;

using DevStack.OpenCode.Models;

namespace DevStack.OpenCode.Serialization;

/// <summary>Reads and writes a <see cref="Part"/> from JSON, dispatching on the <c>type</c> field.</summary>
public sealed class PartConverter : JsonConverter<Part>
{
    /// <inheritdoc />
    public override Part? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        var kind = root.TryGetProperty("type", out var type) ? type.GetString() ?? string.Empty : string.Empty;
        return new Part(kind, root.Clone());
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Part value, JsonSerializerOptions options)
    {
        writer.WriteRawValue(value.Raw.GetRawText());
    }
}

/// <summary>Reads and writes a <see cref="ToolState"/> from JSON, preserving the discriminator.</summary>
public sealed class ToolStateConverter : JsonConverter<ToolState>
{
    /// <inheritdoc />
    public override ToolState? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        var status = root.TryGetProperty("status", out var s) ? s.GetString() ?? string.Empty : string.Empty;
        return new ToolState(status, root.Clone());
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, ToolState value, JsonSerializerOptions options)
    {
        writer.WriteRawValue(value.Raw.GetRawText());
    }
}

/// <summary>Reads and writes a <see cref="FilePartSource"/> from JSON, preserving the discriminator.</summary>
public sealed class FilePartSourceConverter : JsonConverter<FilePartSource>
{
    /// <inheritdoc />
    public override FilePartSource? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        var kind = root.TryGetProperty("type", out var t) ? t.GetString() ?? string.Empty : string.Empty;
        return new FilePartSource(kind, root.Clone());
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, FilePartSource value, JsonSerializerOptions options)
    {
        writer.WriteRawValue(value.Raw.GetRawText());
    }
}
