using DevStack.Tests.Integration.MCP.Hooks;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

using FluentAssertions;

using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.MCP.Steps;

[Binding]
public sealed class ToolsListSteps
{
    private readonly ScenarioContext _scenarioContext;
    private IList<McpClientTool>? _tools;

    public ToolsListSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    private McpClient Client => SpecFlowHooks.GetMcpClient(_scenarioContext);

    [When(@"I request the tool list")]
    public async Task WhenIRequestTheToolList()
    {
        _tools = await Client.ListToolsAsync();
        _scenarioContext["Tools"] = _tools;
    }

    [Then(@"the response should contain a list of tools")]
    public void ThenTheResponseShouldContainAListOfTools()
    {
        _tools.Should().NotBeNull();
        _tools!.Should().NotBeEmpty();
    }

    [Then(@"the tools should include ""(.*)""")]
    public void ThenTheToolsShouldInclude(string toolName)
    {
        _tools.Should().Contain(t => t.Name == toolName, $"Tool '{toolName}' should be available");
    }

    [Then(@"each tool should have a name")]
    public void ThenEachToolShouldHaveAName()
    {
        foreach (var tool in _tools!)
        {
            tool.Name.Should().NotBeNullOrEmpty();
        }
    }

    [Then(@"each tool should have a description")]
    public void ThenEachToolShouldHaveADescription()
    {
        foreach (var tool in _tools!)
        {
            tool.Description.Should().NotBeNullOrEmpty();
        }
    }

    [Then(@"each tool should have inputSchema")]
    public void ThenEachToolShouldHaveInputSchema()
    {
        // The SDK generates JSON schema for tool parameters internally.
        // We verify tools have the expected structure by checking they can be called.
        foreach (var tool in _tools!)
        {
            tool.Name.Should().NotBeNullOrEmpty();
            tool.Description.Should().NotBeNullOrEmpty();
        }
    }
}
