using TechTalk.SpecFlow;
using DevStack.Tests.Integration.MCP.Client;
using DevStack.Tests.Integration.MCP.Hooks;
using FluentAssertions;

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
        if (_scenarioContext.TryGetValue<bool>("InvalidParams", out var invalid) && invalid)
        {
            var invalidRequest = new { name = "", arguments = new { } };
            _response = await Client.SendRequestAsync("tools/call", invalidRequest);
        }
        else
        {
            var toolName = _scenarioContext.GetString("ToolName") ?? "devstack_getProjects";
            var request = new { name = toolName, arguments = new { } };
            _response = await Client.SendRequestAsync("tools/call", request);
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
}
