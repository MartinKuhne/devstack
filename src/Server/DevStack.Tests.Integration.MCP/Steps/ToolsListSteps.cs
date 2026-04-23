using TechTalk.SpecFlow;
using DevStack.Tests.Integration.MCP.Client;
using DevStack.Tests.Integration.MCP.Hooks;
using FluentAssertions;
using System.Text.Json;

namespace DevStack.Tests.Integration.MCP.Steps;

[Binding]
public sealed class ToolsListSteps
{
    private readonly ScenarioContext _scenarioContext;
    private JsonRpcResponse? _response;

    public ToolsListSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    private IMcpJsonRpcClient Client => SpecFlowHooks.GetMcpClient(_scenarioContext);

    [Given(@"a valid tools/list request")]
    public void GivenAValidToolsListRequest()
    {
    }

    [When(@"I send the tools/list request")]
    public async Task WhenISendTheToolsListRequest()
    {
        _response = await Client.SendRequestAsync("tools/list", default(CancellationToken));
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
        var result = GetResultJson();
        result.Should().Contain(toolName);
    }

    [Then(@"each tool should have a name")]
    public void ThenEachToolShouldHaveAName()
    {
        var result = GetResultJson();
        result.Should().Contain("name");
    }

    [Then(@"each tool should have a description")]
    public void ThenEachToolShouldHaveADescription()
    {
        var result = GetResultJson();
        result.Should().Contain("description");
    }

    [Then(@"each tool should have inputSchema")]
    public void ThenEachToolShouldHaveInputSchema()
    {
        var result = GetResultJson();
        result.Should().Contain("inputSchema");
    }

    private string GetResultJson()
    {
        _response!.Result.Should().NotBeNull();
        var resultJson = JsonSerializer.Serialize(_response.Result);
        var doc = JsonDocument.Parse(resultJson);
        var root = doc.RootElement;

        JsonElement sourceProp = root;

        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("content", out var contentProp) && contentProp.ValueKind == JsonValueKind.Array && contentProp.GetArrayLength() > 0)
        {
            var firstBlock = contentProp[0];
            if (firstBlock.TryGetProperty("text", out var textProp) && textProp.ValueKind == JsonValueKind.String)
            {
                resultJson = textProp.GetString()!;
                doc = JsonDocument.Parse(resultJson);
                sourceProp = doc.RootElement;
            }
        }

        if (sourceProp.ValueKind == JsonValueKind.Object && sourceProp.TryGetProperty("tools", out var toolsProp))
        {
            return toolsProp.GetRawText();
        }

        return resultJson;
    }
}
