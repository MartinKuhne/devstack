using DevStack.Api.Mcp;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Xunit;

namespace DevStack.Tests.Integration.Mcp;

public class McpMethodHandlerTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly McpMethodHandler _handler;

    public McpMethodHandlerTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<DevStackTools>();
        _serviceProvider = services.BuildServiceProvider();
        _handler = new McpMethodHandler(_serviceProvider, new MockLogger<McpMethodHandler>());
    }

    [Fact]
    public async Task HandleInitialize_Returns_Correct_ProtocolVersion()
    {
        var result = await _handler.HandleAsync("initialize", null);

        result.Should().NotBeNull();
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("protocolVersion").GetString().Should().Be("2025-03-26");
    }

    [Fact]
    public async Task HandleInitialize_Returns_Correct_ServerInfo()
    {
        var result = await _handler.HandleAsync("initialize", null);

        result.Should().NotBeNull();
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("serverInfo").GetProperty("name").GetString().Should().Be("DevStack MCP Server");
        doc.RootElement.GetProperty("serverInfo").GetProperty("version").GetString().Should().Be("1.0.0");
    }

    [Fact]
    public async Task HandleInitialize_Returns_Tools_Capability()
    {
        var result = await _handler.HandleAsync("initialize", null);

        result.Should().NotBeNull();
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("capabilities").GetProperty("tools").GetProperty("listChanged").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task HandleListTools_Returns_Array_Of_Tools()
    {
        var result = await _handler.HandleAsync("tools/list", null);

        result.Should().NotBeNull();
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("tools").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task HandleCallTool_With_Unknown_Tool_Throws_Exception()
    {
        var parameters = JsonDocument.Parse("""
            {
                "name": "unknown_tool",
                "arguments": {}
            }
            """).RootElement;

        var act = () => _handler.HandleAsync("tools/call", parameters);

        await act.Should().ThrowAsync<JsonRpcException>()
            .WithMessage("*unknown_tool*not found*");
    }

    [Fact]
    public async Task HandleCallTool_With_Missing_Parameters_Throws_Exception()
    {
        var act = () => _handler.HandleAsync("tools/call", null);

        await act.Should().ThrowAsync<JsonRpcException>()
            .WithMessage("*Missing parameters*");
    }

    [Fact]
    public async Task HandleCallTool_With_Missing_Name_Throws_Exception()
    {
        var parameters = JsonDocument.Parse("""
            {
                "arguments": {}
            }
            """).RootElement;

        var act = () => _handler.HandleAsync("tools/call", parameters);

        await act.Should().ThrowAsync<JsonRpcException>()
            .WithMessage("*Missing 'name' parameter*");
    }

    private class MockLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => default!;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }
}
