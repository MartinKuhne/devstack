using System.Text.Json;
using System.Text.RegularExpressions;

using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace DevStack.Tests.Integration.MCP.Steps;

/// <summary>
/// Helpers for reading the responses produced by the DevStack MCP tools.
/// </summary>
/// <remarks>
/// Successful tool results are markdown documents of the form
/// <c>## &lt;Title&gt;\n\n```json\n{...}\n```</c>, optionally followed by a usage hint.
/// Soft tool errors are returned as a plain <c>{"error":"..."}</c> JSON document.
/// </remarks>
internal static class McpToolResult
{
    private static readonly Regex JsonBlock = new(@"```json\s*\n(.*?)\n```", RegexOptions.Singleline | RegexOptions.Compiled);

    /// <summary>Returns the first non-empty text block of a tool result.</summary>
    public static string Text(CallToolResult result)
    {
        foreach (var content in result.Content)
        {
            if (content is TextContentBlock textBlock && !string.IsNullOrEmpty(textBlock.Text))
            {
                return textBlock.Text;
            }
        }

        throw new InvalidOperationException($"Tool result contains no text content. IsError={result.IsError}");
    }

    /// <summary>Returns the JSON payload of a tool result, unwrapping the markdown code fence when present.</summary>
    public static string Json(CallToolResult result)
    {
        var text = Text(result);
        var match = JsonBlock.Match(text);
        return match.Success ? match.Groups[1].Value.Trim() : text.Trim();
    }

    /// <summary>Parses the JSON payload of a tool result.</summary>
    public static JsonElement Data(CallToolResult result) => JsonDocument.Parse(Json(result)).RootElement;

    /// <summary>Reads a string property from a JSON object, ignoring the property name casing.</summary>
    /// <remarks>
    /// The MCP tools serialize plain C# records, so the payload uses PascalCase property names
    /// (<c>Id</c>, <c>Title</c>, ...) while the soft error payload uses <c>error</c>.
    /// </remarks>
    public static string? StringProperty(JsonElement element, string name)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (property.NameEquals(name) || property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return property.Value.ValueKind == JsonValueKind.Null ? null : property.Value.GetString();
            }
        }

        return null;
    }

    /// <summary>Reads a required string property, failing the caller when it is absent.</summary>
    public static string RequiredStringProperty(JsonElement element, string name)
    {
        var value = StringProperty(element, name);
        if (value is null)
        {
            throw new InvalidOperationException($"Property '{name}' is missing from tool payload: {element}");
        }

        return value;
    }
}
