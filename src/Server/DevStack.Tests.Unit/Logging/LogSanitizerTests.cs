using DevStack.Mcp.Logging;
using Xunit;

namespace DevStack.Tests.Unit.Logging;

public class LogSanitizerTests
{
    [Fact]
    public void Sanitize_NullOrEmpty_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, LogSanitizer.Sanitize(null));
        Assert.Equal(string.Empty, LogSanitizer.Sanitize(string.Empty));
    }

    [Fact]
    public void Sanitize_LineBreaks_RemovesNewlinesAndCarriageReturns()
    {
        var input = "GET /mcp\r\nHTTP/1.1 200 OK\r\nHeader: injected";
        var expected = "GET /mcpHTTP/1.1 200 OKHeader: injected";

        var actual = LogSanitizer.Sanitize(input);

        Assert.Equal(expected, actual);
        Assert.DoesNotContain("\r", actual);
        Assert.DoesNotContain("\n", actual);
    }

    [Fact]
    public void Sanitize_ValidInput_PreservesNormalString()
    {
        var input = "POST /mcp";
        var expected = "POST /mcp";

        var actual = LogSanitizer.Sanitize(input);

        Assert.Equal(expected, actual);
    }
}
