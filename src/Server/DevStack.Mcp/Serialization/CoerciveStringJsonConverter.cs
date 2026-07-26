using System.Text.Json;
using System.Text.Json.Serialization;

namespace DevStack.Mcp.Serialization;

/// <summary>
/// A JSON converter that defensively handles array-to-string coercion.
/// When a JSON array is encountered where a string is expected, the converter
/// joins the array elements with newlines instead of throwing an exception.
/// </summary>
public class CoerciveStringJsonConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Null => null,
            JsonTokenType.StartArray => ReadArray(ref reader),
            _ => throw new JsonException($"Unexpected token type {reader.TokenType} when expecting string")
        };
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }

    private static string ReadArray(ref Utf8JsonReader reader)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var elements = new List<string>();

        foreach (var element in doc.RootElement.EnumerateArray())
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                elements.Add(element.GetString() ?? string.Empty);
            }
            else
            {
                elements.Add(element.GetRawText());
            }
        }

        return string.Join("\n", elements);
    }
}
