using System.Text.Json;

namespace DevStack.OpenCode.Models;

/// <summary>
/// Reads and writes a <see cref="PermissionActionRule"/> from JSON, supporting
/// the OpenCode shape of either a flat action string or a sub-tool map.
/// </summary>
public sealed class PermissionActionRuleConverter : JsonConverter<PermissionActionRule>
{
    /// <inheritdoc />
    public override PermissionActionRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                {
                    var actionStr = reader.GetString();
                    return PermissionActionRule.FromAction(PermissionConfigConverter.ParseAction(actionStr));
                }

            case JsonTokenType.StartObject:
                {
                    using var doc = JsonDocument.ParseValue(ref reader);
                    var map = doc.RootElement.Deserialize<Dictionary<string, PermissionAction>>(options)
                        ?? new Dictionary<string, PermissionAction>(StringComparer.Ordinal);
                    return PermissionActionRule.FromMap(map);
                }

            case JsonTokenType.Null:
                return null;

            default:
                throw new JsonException(
                    $"OpenCode permission rule must be a string or object; got {reader.TokenType}.");
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, PermissionActionRule value, JsonSerializerOptions options)
    {
        switch (value.Kind)
        {
            case PermissionActionRuleKind.Action:
                PermissionConfigConverter.WriteActionString(writer, value.Action!.Value);
                break;

            case PermissionActionRuleKind.SubToolMap:
                {
                    writer.WriteStartObject();
                    foreach (var kvp in value.SubToolMap!)
                    {
                        writer.WritePropertyName(kvp.Key);
                        PermissionConfigConverter.WriteActionString(writer, kvp.Value);
                    }

                    writer.WriteEndObject();
                }

                break;
        }
    }
}
