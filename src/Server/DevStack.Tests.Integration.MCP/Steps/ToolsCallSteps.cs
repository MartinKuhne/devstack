using System.Text.Json;

using DevStack.Tests.Integration.MCP.Client;
using DevStack.Tests.Integration.MCP.Hooks;

using FluentAssertions;

using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.MCP.Steps;

[Binding]
public sealed class ToolsCallSteps
{
    private readonly ScenarioContext _scenarioContext;
    private JsonRpcResponse? _response;

    public ToolsCallSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    private IMcpJsonRpcClient Client => SpecFlowHooks.GetMcpClient(_scenarioContext);

    [Given(@"a valid tools/call request for ""(.*)""")]
    public void GivenAValidToolsCallRequest(string toolName)
    {
        _scenarioContext["ToolName"] = toolName;
    }

    [Given(@"a tools/call request with missing required parameters")]
    public void GivenAToolsCallRequestWithMissingRequiredParameters()
    {
        _scenarioContext["InvalidParams"] = true;
    }

    [When(@"I send the tools/call request")]
    public async Task WhenISendTheToolsCallRequest()
    {
        try
        {
            if (_scenarioContext.TryGetValue<bool>("InvalidParams", out var invalid) && invalid)
            {
                var invalidRequest = new { name = "", arguments = new { } };
                _response = await Client.SendRequestAsync("tools/call", invalidRequest);
            }
            else
            {
                var toolName = _scenarioContext.GetString("ToolName") ?? "get_projects";
                var request = new { name = toolName, arguments = new { } };
                _response = await Client.SendRequestAsync("tools/call", request);
            }
        }
        catch (JsonRpcException ex)
        {
            _response = new JsonRpcResponse("2.0", null, new JsonRpcError(ex.Code, ex.Message, ex.Data), null);
        }

        _scenarioContext["Response"] = _response;
    }

    [Then(@"the response should contain the tool result")]
    public void ThenTheResponseShouldContainTheToolResult()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
        _response!.Result.Should().NotBeNull();
    }

    [Then(@"the response should contain an error with code (-?\d+)")]
    public void ThenTheResponseShouldContainAnErrorWithCode(int expectedCode)
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().NotBeNull();
        _response!.Error!.Code.Should().Be(expectedCode);
    }

    [Then(@"the error code should be (-?\d+)")]
    public void ThenTheErrorCodeShouldBe(int expectedCode)
    {
        _response!.Error!.Code.Should().Be(expectedCode);
    }

    [Then(@"the response should have an error field")]
    public void ThenTheResponseShouldHaveAnErrorField()
    {
        _response!.Error.Should().NotBeNull();
    }

    [Then(@"the response should not have a result field")]
    public void ThenTheResponseShouldNotHaveAResultField()
    {
        _response!.Result.Should().BeNull();
    }

    [Then(@"the result should contain a content array")]
    public void ThenTheResultShouldContainAContentArray()
    {
        _response!.Result.Should().NotBeNull();
        var resultJson = JsonSerializer.Serialize(_response.Result);
        resultJson.Should().Contain("content");
    }
}
