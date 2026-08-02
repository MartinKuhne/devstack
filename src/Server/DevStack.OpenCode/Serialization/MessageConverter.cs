using System.Text.Json;

using DevStack.OpenCode.Models;

namespace DevStack.OpenCode.Serialization;

/// <summary>
/// Reads and writes a <see cref="Message"/> from JSON, dispatching to
/// <see cref="UserMessage"/> or <see cref="AssistantMessage"/> based on the
/// <c>role</c> field.
/// </summary>
public sealed class MessageConverter : JsonConverter<Message>
{
    /// <inheritdoc />
    public override Message? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        var kind = root.TryGetProperty("role", out var role) ? role.GetString() ?? string.Empty : string.Empty;
        return new Message(kind, root.Clone());
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Message value, JsonSerializerOptions options)
    {
        writer.WriteRawValue(value.Raw.GetRawText());
    }
}

/// <summary>
/// Reads and writes a <see cref="MessageError"/> from JSON, preserving the
/// underlying payload as a <see cref="JsonElement"/> for downstream inspection.
/// </summary>
public sealed class MessageErrorConverter : JsonConverter<MessageError>
{
    /// <inheritdoc />
    public override MessageError? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        var kind = root.TryGetProperty("name", out var name) ? name.GetString() ?? string.Empty : string.Empty;
        return new MessageError(kind, root.Clone());
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, MessageError value, JsonSerializerOptions options)
    {
        writer.WriteRawValue(value.Raw.GetRawText());
    }
}
