using System.Text.Json;

namespace DevStack.OpenCode.Models;

/// <summary>
/// Reads and writes a <see cref="TimeoutValue"/> from JSON <c>number</c> or
/// literal <c>false</c>, matching the OpenCode schema union.
/// </summary>
public sealed class TimeoutValueConverter : JsonConverter<TimeoutValue>
{
    /// <inheritdoc />
    public override TimeoutValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.False:
                return TimeoutValue.Disable();

            case JsonTokenType.Number:
                {
                    var ms = reader.GetInt32();
                    if (ms <= 0)
                    {
                        throw new JsonException(
                            $"OpenCode timeout must be a positive integer or false; got {ms}.");
                    }

                    return TimeoutValue.FromMilliseconds(ms);
                }

            case JsonTokenType.Null:
                return default;

            default:
                throw new JsonException(
                    $"OpenCode timeout must be a positive integer or false; got {reader.TokenType}.");
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TimeoutValue value, JsonSerializerOptions options)
    {
        if (value.Disabled)
        {
            writer.WriteBooleanValue(false);
        }
        else
        {
            writer.WriteNumberValue(value.Milliseconds);
        }
    }
}
