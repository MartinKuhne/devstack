using TechTalk.SpecFlow;
using DevStack.Tests.Integration.MCP.Client;
using FluentAssertions;
using System.Text.Json;

namespace DevStack.Tests.Integration.MCP.Steps;

[Binding]
public sealed class ToolsListSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly IMcpJsonRpcClient _client;
    private JsonRpcResponse? _response;

    public ToolsListSteps(ScenarioContext scenarioContext, IMcpJsonRpcClient client)
    {
        _scenarioContext = scenarioContext;
        _client = client;
    }

    [Given(@"a valid tools/list request")]
    public void GivenAValidToolsListRequest()
    {
    }

    [When(@"I send the tools/list request")]
    public async Task WhenISendTheToolsListRequest()
    {
        _response = await _client.SendRequestAsync("tools/list", default(CancellationToken));
        _scenarioContext["Response"] = _response;
    }

    [Then(@"the response should contain a list of tools")]
    public void ThenTheResponseShouldContainAListOfTools()
    {
        _response.Should().NotBeNull();
        _response!.Result.Should().NotBeNull();
    }

    [Then(@"the tools should include ""(.*)""")]
    public void ThenTheToolsShouldInclude(string toolName)
    {
        var result = _response!.Result!.ToString();
        result.Should().Contain(toolName);
    }

    [Then(@"each tool should have a name")]
    public void ThenEachToolShouldHaveAName()
    {
        var result = _response!.Result!.ToString();
        result.Should().Contain("name");
    }

    [Then(@"each tool should have a description")]
    public void ThenEachToolShouldHaveADescription()
    {
        var result = _response!.Result!.ToString();
        result.Should().Contain("description");
    }

    [Then(@"each tool should have inputSchema")]
    public void ThenEachToolShouldHaveInputSchema()
    {
        var result = _response!.Result!.ToString();
        result.Should().Contain("inputSchema");
    }
}
