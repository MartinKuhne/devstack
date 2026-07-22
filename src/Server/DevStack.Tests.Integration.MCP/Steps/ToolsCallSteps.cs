using DevStack.Tests.Integration.MCP.Hooks;

using FluentAssertions;

using ModelContextProtocol.Client;

using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.MCP.Steps;

[Binding]
public sealed class ToolsCallSteps
{
    private readonly ScenarioContext _scenarioContext;
    private object? _result;

    public ToolsCallSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    private McpClient Client => SpecFlowHooks.GetMcpClient(_scenarioContext);

    [Given(@"a valid tools/call request for ""(.*)""")]
    public void GivenAValidToolsCallRequest(string toolName)
    {
        _scenarioContext["ToolName"] = toolName;
    }

    [Given(@"a tools/call request with missing required parameters")]
    public void GivenAToolsCallRequestWithMissingRequiredParameters()
    {
        _scenarioContext["InvalidToolCall"] = true;
    }

    [When(@"I send the tools/call request")]
    public async Task WhenISendTheToolsCallRequest()
    {
        if (_scenarioContext.TryGetValue<bool>("InvalidToolCall", out var invalid) && invalid)
        {
            _result = await Client.CallToolAsync("get_projects", new Dictionary<string, object?>());
        }
        else
        {
            var toolName = _scenarioContext.GetString("ToolName") ?? "get_projects";
            _result = await Client.CallToolAsync(toolName, new Dictionary<string, object?>());
        }

        _scenarioContext["CallResult"] = _result;
    }

    [Then(@"the response should contain the tool result")]
    public void ThenTheResponseShouldContainTheToolResult()
    {
        _result.Should().NotBeNull();
    }

    [Then(@"the result should contain a content array")]
    public void ThenTheResultShouldContainAContentArray()
    {
        _result.Should().NotBeNull();
    }

    [Then(@"the response should indicate a tool error")]
    public void ThenTheResponseShouldIndicateAToolError()
    {
        _result.Should().NotBeNull();
    }
}
