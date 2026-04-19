using TechTalk.SpecFlow;
using DevStack.Tests.Integration.MCP.Client;
using DevStack.Tests.Integration.MCP.Hooks;
using FluentAssertions;
using System.Text.Json;

namespace DevStack.Tests.Integration.MCP.Steps;

[Binding]
public sealed class InitializeSteps
{
    private readonly ScenarioContext _scenarioContext;
    private JsonRpcResponse? _response;

    public InitializeSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    private IMcpJsonRpcClient Client => SpecFlowHooks.GetMcpClient(_scenarioContext);

    [Given(@"a valid initialize request with protocol version ""(.*)""")]
    public void GivenAValidInitializeRequest(string protocolVersion)
    {
        _scenarioContext["ProtocolVersion"] = protocolVersion;
    }

    [When(@"I send the initialize request")]
    public async Task WhenISendTheInitializeRequest()
    {
        var protocolVersion = _scenarioContext.GetString("ProtocolVersion") ?? "2024-11-05";
        var request = new
        {
            protocolVersion = protocolVersion,
            capabilities = new { }
        };

        _response = await Client.SendRequestAsync("initialize", request);
        _scenarioContext["Response"] = _response;
    }

    [Then(@"the response should contain protocol version ""(.*)""")]
    public void ThenTheResponseShouldContainProtocolVersion(string expectedVersion)
    {
        _response.Should().NotBeNull();
        var result = _response!.Result!.ToString();
        result.Should().Contain(expectedVersion);
    }

    [Then(@"the response should contain server name ""(.*)""")]
    public void ThenTheResponseShouldContainServerName(string expectedName)
    {
        _response.Should().NotBeNull();
        var result = _response!.Result!.ToString();
        result.Should().Contain(expectedName);
    }

    [Then(@"the response should contain tools capability")]
    public void ThenTheResponseShouldContainToolsCapability()
    {
        _response.Should().NotBeNull();
        var result = _response!.Result!.ToString();
        result.Should().Contain("tools");
    }
}
