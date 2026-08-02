using System.Text.Json;

namespace DevStack.OpenCode.Models;

/// <summary>
/// Reads and writes an <see cref="AutoUpdateConfig"/> from JSON <c>true</c>,
/// <c>false</c>, or the string <c>"notify"</c>.
/// </summary>
public sealed class AutoUpdateConfigConverter : JsonConverter<AutoUpdateConfig>
{
    /// <inheritdoc />
    public override AutoUpdateConfig Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.True:
                return AutoUpdateConfig.Enabled();

            case JsonTokenType.False:
            case JsonTokenType.Null:
                return AutoUpdateConfig.Disabled();

            case JsonTokenType.String:
                {
                    var s = reader.GetString();
                    return s switch
                    {
                        "notify" => AutoUpdateConfig.Notify(),
                        "true" => AutoUpdateConfig.Enabled(),
                        "false" => AutoUpdateConfig.Disabled(),
                        null => AutoUpdateConfig.Disabled(),
                        _ => throw new JsonException(
                            $"OpenCode autoupdate must be a boolean or the string 'notify'; got '{s}'."),
                    };
                }

            default:
                throw new JsonException(
                    $"OpenCode autoupdate must be a boolean or the string 'notify'; got {reader.TokenType}.");
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, AutoUpdateConfig value, JsonSerializerOptions options)
    {
        switch (value.Mode)
        {
            case AutoUpdateMode.Enabled:
                writer.WriteBooleanValue(true);
                break;
            case AutoUpdateMode.Notify:
                writer.WriteStringValue("notify");
                break;
            default:
                writer.WriteBooleanValue(false);
                break;
        }
    }
}
