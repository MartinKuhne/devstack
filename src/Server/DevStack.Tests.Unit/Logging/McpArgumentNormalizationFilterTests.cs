using System.Text.Json;

using DevStack.Mcp.Logging;

using Microsoft.Extensions.Logging.Abstractions;

using Xunit;

namespace DevStack.Tests.Unit.Logging;

public class McpArgumentNormalizationFilterTests
{
    [Fact]
    public void NormalizeArguments_ArrayArgument_CoercesToStringWithNewlines()
    {
        var arguments = new Dictionary<string, JsonElement>
        {
            ["description"] = JsonSerializer.SerializeToElement(new[] { "line1", "line2", "line3" })
        };

        McpArgumentNormalizationFilter.NormalizeArguments(arguments, "update_task", NullLogger.Instance);

        Assert.Equal(JsonValueKind.String, arguments["description"].ValueKind);
        Assert.Equal("line1\nline2\nline3", arguments["description"].GetString());
    }

    [Fact]
    public void NormalizeArguments_StringArgument_RemainsUnchanged()
    {
        var arguments = new Dictionary<string, JsonElement>
        {
            ["description"] = JsonSerializer.SerializeToElement("simple string")
        };

        McpArgumentNormalizationFilter.NormalizeArguments(arguments, "update_task", NullLogger.Instance);

        Assert.Equal(JsonValueKind.String, arguments["description"].ValueKind);
        Assert.Equal("simple string", arguments["description"].GetString());
    }

    [Fact]
    public void NormalizeArguments_NullArgument_RemainsUnchanged()
    {
        var arguments = new Dictionary<string, JsonElement>
        {
            ["description"] = JsonSerializer.SerializeToElement<string?>(null)
        };

        McpArgumentNormalizationFilter.NormalizeArguments(arguments, "update_task", NullLogger.Instance);

        Assert.Equal(JsonValueKind.Null, arguments["description"].ValueKind);
    }

    [Fact]
    public void NormalizeArguments_ArrayWithNullElements_CoercesWithEmptyStrings()
    {
        var json = @"[""item1"", null, ""item3""]";
        var arguments = new Dictionary<string, JsonElement>
        {
            ["description"] = JsonSerializer.Deserialize<JsonElement>(json)
        };

        McpArgumentNormalizationFilter.NormalizeArguments(arguments, "update_task", NullLogger.Instance);

        Assert.Equal(JsonValueKind.String, arguments["description"].ValueKind);
        Assert.Equal("item1\n\nitem3", arguments["description"].GetString());
    }

    [Fact]
    public void NormalizeArguments_ArrayWithMixedTypes_CoercesAllToStrings()
    {
        var json = @"[""text"", 42, true, null, ""more""]";
        var arguments = new Dictionary<string, JsonElement>
        {
            ["result"] = JsonSerializer.Deserialize<JsonElement>(json)
        };

        McpArgumentNormalizationFilter.NormalizeArguments(arguments, "update_task", NullLogger.Instance);

        Assert.Equal(JsonValueKind.String, arguments["result"].ValueKind);
        var result = arguments["result"].GetString();
        Assert.Contains("text", result);
        Assert.Contains("42", result);
        Assert.Contains("true", result);
        Assert.Contains("more", result);
    }

    [Fact]
    public void NormalizeArguments_EmptyArray_CoercesToEmptyString()
    {
        var arguments = new Dictionary<string, JsonElement>
        {
            ["description"] = JsonSerializer.SerializeToElement(Array.Empty<string>())
        };

        McpArgumentNormalizationFilter.NormalizeArguments(arguments, "update_task", NullLogger.Instance);

        Assert.Equal(JsonValueKind.String, arguments["description"].ValueKind);
        Assert.Equal("", arguments["description"].GetString());
    }

    [Fact]
    public void NormalizeArguments_MultipleArrayArguments_AllCoerced()
    {
        var arguments = new Dictionary<string, JsonElement>
        {
            ["description"] = JsonSerializer.SerializeToElement(new[] { "desc1", "desc2" }),
            ["result"] = JsonSerializer.SerializeToElement(new[] { "res1", "res2" }),
            ["errors"] = JsonSerializer.SerializeToElement(new[] { "err1", "err2" })
        };

        McpArgumentNormalizationFilter.NormalizeArguments(arguments, "update_task", NullLogger.Instance);

        Assert.Equal("desc1\ndesc2", arguments["description"].GetString());
        Assert.Equal("res1\nres2", arguments["result"].GetString());
        Assert.Equal("err1\nerr2", arguments["errors"].GetString());
    }

    [Fact]
    public void NormalizeArguments_NumberArgument_RemainsUnchanged()
    {
        var arguments = new Dictionary<string, JsonElement>
        {
            ["count"] = JsonSerializer.SerializeToElement(42)
        };

        McpArgumentNormalizationFilter.NormalizeArguments(arguments, "update_task", NullLogger.Instance);

        Assert.Equal(JsonValueKind.Number, arguments["count"].ValueKind);
        Assert.Equal(42, arguments["count"].GetInt32());
    }

    [Fact]
    public void NormalizeArguments_BooleanArgument_RemainsUnchanged()
    {
        var arguments = new Dictionary<string, JsonElement>
        {
            ["flag"] = JsonSerializer.SerializeToElement(true)
        };

        McpArgumentNormalizationFilter.NormalizeArguments(arguments, "update_task", NullLogger.Instance);

        Assert.Equal(JsonValueKind.True, arguments["flag"].ValueKind);
        Assert.True(arguments["flag"].GetBoolean());
    }

    [Fact]
    public void NormalizeArguments_EmptyDictionary_NoException()
    {
        var arguments = new Dictionary<string, JsonElement>();

        var exception = Record.Exception(() =>
            McpArgumentNormalizationFilter.NormalizeArguments(arguments, "update_task", NullLogger.Instance));

        Assert.Null(exception);
        Assert.Empty(arguments);
    }

    [Fact]
    public void NormalizeArguments_ArrayWithSpecialCharacters_PreservesCharacters()
    {
        var arguments = new Dictionary<string, JsonElement>
        {
            ["description"] = JsonSerializer.SerializeToElement(new[] { "line with \"quotes\"", "line with \\backslash", "line with \ttab" })
        };

        McpArgumentNormalizationFilter.NormalizeArguments(arguments, "update_task", NullLogger.Instance);

        var result = arguments["description"].GetString();
        Assert.Contains("\"quotes\"", result);
        Assert.Contains("\\backslash", result);
        Assert.Contains("\t", result);
    }

    [Fact]
    public void NormalizeArguments_ArrayWithUnicodeStrings_PreservesUnicode()
    {
        var arguments = new Dictionary<string, JsonElement>
        {
            ["description"] = JsonSerializer.SerializeToElement(new[] { "Hello 世界", "Привет мир", "🎉🎊" })
        };

        McpArgumentNormalizationFilter.NormalizeArguments(arguments, "update_task", NullLogger.Instance);

        var result = arguments["description"].GetString();
        Assert.Contains("世界", result);
        Assert.Contains("Привет", result);
        Assert.Contains("🎉", result);
    }

    [Fact]
    public void NormalizeArguments_ArrayWithNewlinesInElements_PreservesOriginalNewlines()
    {
        var arguments = new Dictionary<string, JsonElement>
        {
            ["description"] = JsonSerializer.SerializeToElement(new[] { "line1\nline2", "line3" })
        };

        McpArgumentNormalizationFilter.NormalizeArguments(arguments, "update_task", NullLogger.Instance);

        var result = arguments["description"].GetString();
        Assert.Equal("line1\nline2\nline3", result);
    }

    [Fact]
    public void NormalizeArguments_ObjectArgument_RemainsUnchanged()
    {
        var obj = new { Name = "test", Value = 42 };
        var arguments = new Dictionary<string, JsonElement>
        {
            ["data"] = JsonSerializer.SerializeToElement(obj)
        };

        McpArgumentNormalizationFilter.NormalizeArguments(arguments, "update_task", NullLogger.Instance);

        Assert.Equal(JsonValueKind.Object, arguments["data"].ValueKind);
    }
}
