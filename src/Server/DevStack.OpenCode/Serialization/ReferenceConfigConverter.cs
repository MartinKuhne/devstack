using System.Text.Json;

namespace DevStack.OpenCode.Models;

/// <summary>
/// Reads and writes a <see cref="ReferenceConfig"/> from JSON, dispatching
/// to git, local, or string shorthand shapes based on the <c>repository</c>,
/// <c>path</c>, or string-vs-object shape.
/// </summary>
public sealed class ReferenceConfigConverter : JsonConverter<ReferenceConfig>
{
    /// <inheritdoc />
    public override ReferenceConfig? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return ReferenceConfig.FromString(reader.GetString()
                    ?? throw new JsonException("OpenCode reference shorthand cannot be null."));

            case JsonTokenType.StartObject:
                {
                    using var doc = JsonDocument.ParseValue(ref reader);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("repository", out _))
                    {
                        return ReferenceConfig.FromGit(root.Deserialize<ReferenceGitConfig>(options)
                            ?? throw new JsonException("Failed to deserialize reference git config."));
                    }

                    if (root.TryGetProperty("path", out _))
                    {
                        return ReferenceConfig.FromLocal(root.Deserialize<ReferenceLocalConfig>(options)
                            ?? throw new JsonException("Failed to deserialize reference local config."));
                    }

                    throw new JsonException(
                        "OpenCode reference object must contain either 'repository' or 'path'.");
                }

            case JsonTokenType.Null:
                return null;

            default:
                throw new JsonException(
                    $"OpenCode reference must be a string or object; got {reader.TokenType}.");
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, ReferenceConfig value, JsonSerializerOptions options)
    {
        switch (value.Kind)
        {
            case ReferenceKind.String:
                writer.WriteStringValue(value.Shorthand);
                break;
            case ReferenceKind.Git:
                JsonSerializer.Serialize(writer, value.Git, options);
                break;
            case ReferenceKind.Local:
                JsonSerializer.Serialize(writer, value.Local, options);
                break;
        }
    }
}
