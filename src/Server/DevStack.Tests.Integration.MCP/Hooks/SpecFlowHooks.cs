using TechTalk.SpecFlow;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using DevStack.Tests.Integration.MCP.Client;

namespace DevStack.Tests.Integration.MCP.Hooks;

[Binding]
public sealed class SpecFlowHooks
{
    private readonly ScenarioContext _scenarioContext;
    private IMcpJsonRpcClient? _mcpClient;
    private HttpClient? _httpClient;

    public SpecFlowHooks(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        var services = new ServiceCollection();
        
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:8887")
        };
        
        services.AddSingleton(_httpClient);
        services.AddSingleton<IMcpJsonRpcClient>(sp => 
            new McpJsonRpcClient(_httpClient, "http://localhost:8887/mcp"));

        var provider = services.BuildServiceProvider();
        _mcpClient = provider.GetRequiredService<IMcpJsonRpcClient>();

        _scenarioContext["McpClient"] = _mcpClient;
        _scenarioContext["HttpClient"] = _httpClient;
    }

    [AfterScenario]
    public void AfterScenario()
    {
        _httpClient?.Dispose();
    }

    public static IMcpJsonRpcClient GetMcpClient(ScenarioContext context)
    {
        return context.TryGetValue<IMcpJsonRpcClient>("McpClient", out var client) 
            ? client 
            : throw new InvalidOperationException("MCP client not initialized. Ensure BeforeScenario hook has run.");
    }
}
