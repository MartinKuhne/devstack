using System.Text.Json;

namespace DevStack.OpenCode.Models;

/// <summary>
/// Reads and writes a <see cref="PermissionConfig"/> from JSON, supporting the
/// three OpenCode shapes: a flat action string, a tool-to-action map, or a
/// fully structured rule object.
/// </summary>
public sealed class PermissionConfigConverter : JsonConverter<PermissionConfig>
{
    /// <inheritdoc />
    public override PermissionConfig? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                {
                    var actionStr = reader.GetString();
                    var action = ParseAction(actionStr);
                    return PermissionConfig.FromAction(action);
                }

            case JsonTokenType.StartObject:
                {
                    using var doc = JsonDocument.ParseValue(ref reader);
                    var root = doc.RootElement;

                    // Disambiguation heuristic: the schema's permission union allows two
                    // object shapes — a flat tool→action map, and a fully structured rule
                    // object whose per-tool values are themselves anyOf [action, sub-tool-map].
                    // A value of JsonValueKind.Object signals "this is a structured rule";
                    // a value of String signals "this is a flat map entry". When the
                    // object mixes both kinds, we treat it as a rule object (the more
                    // specific schema).
                    if (HasNestedObjectValue(root))
                    {
                        var rules = root.Deserialize<PermissionRuleConfig>(options)
                            ?? throw new JsonException("Failed to deserialize permission rules object.");
                        return PermissionConfig.FromRules(rules);
                    }

                    var map = root.Deserialize<Dictionary<string, PermissionAction>>(options)
                        ?? new Dictionary<string, PermissionAction>(StringComparer.Ordinal);
                    return PermissionConfig.FromMap(map);
                }

            case JsonTokenType.Null:
                return null;

            default:
                throw new JsonException(
                    $"OpenCode permission must be a string, object, or null; got {reader.TokenType}.");
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, PermissionConfig value, JsonSerializerOptions options)
    {
        switch (value.Kind)
        {
            case PermissionKind.Action:
                WriteActionString(writer, value.Action!.Value);
                break;

            case PermissionKind.Map:
                {
                    writer.WriteStartObject();
                    foreach (var kvp in value.Map!)
                    {
                        writer.WritePropertyName(kvp.Key);
                        WriteActionString(writer, kvp.Value);
                    }

                    writer.WriteEndObject();
                }

                break;

            case PermissionKind.Rules:
                JsonSerializer.Serialize(writer, value.Rules, options);
                break;
        }
    }

    private static bool HasNestedObjectValue(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        foreach (var prop in element.EnumerateObject())
        {
            if (prop.Value.ValueKind == JsonValueKind.Object)
            {
                return true;
            }
        }

        return false;
    }

    internal static PermissionAction ParseAction(string? actionStr)
    {
        return actionStr switch
        {
            "ask" => PermissionAction.Ask,
            "allow" => PermissionAction.Allow,
            "deny" => PermissionAction.Deny,
            _ => throw new JsonException(
                $"OpenCode permission action must be 'ask', 'allow', or 'deny'; got '{actionStr}'."),
        };
    }

    internal static void WriteActionString(Utf8JsonWriter writer, PermissionAction action)
    {
        var s = action switch
        {
            PermissionAction.Ask => "ask",
            PermissionAction.Allow => "allow",
            PermissionAction.Deny => "deny",
            _ => throw new JsonException($"Unknown permission action: {action}"),
        };
        writer.WriteStringValue(s);
    }
}
