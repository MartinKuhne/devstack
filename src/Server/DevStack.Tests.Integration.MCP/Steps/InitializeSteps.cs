using TechTalk.SpecFlow;
using DevStack.Tests.Integration.MCP.Client;
using DevStack.Tests.Integration.MCP.Hooks;
using FluentAssertions;
using System.Net.Http;
using System.Text;
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

        try
        {
            _response = await Client.SendRequestAsync("initialize", request);
        }
        catch (JsonRpcException ex)
        {
            _response = new JsonRpcResponse("2.0", null, new JsonRpcError(ex.Code, ex.Message, ex.Data), null);
        }
        _scenarioContext["Response"] = _response;
    }

    [Then(@"the response should contain protocol version ""(.*)""")]
    public void ThenTheResponseShouldContainProtocolVersion(string expectedVersion)
    {
        _response.Should().NotBeNull();
        var result = GetResultJson();
        result.Should().Contain(expectedVersion);
    }

    [Then(@"the response should contain server name ""(.*)""")]
    public void ThenTheResponseShouldContainServerName(string expectedName)
    {
        _response.Should().NotBeNull();
        var result = GetResultJson();
        result.Should().Contain(expectedName);
    }

    [Then(@"the response should contain tools capability")]
    public void ThenTheResponseShouldContainToolsCapability()
    {
        _response.Should().NotBeNull();
        var result = GetResultJson();
        result.Should().Contain("tools");
    }

    private string GetResultJson()
    {
        _response!.Result.Should().NotBeNull();
        var resultJson = JsonSerializer.Serialize(_response.Result);
        var doc = JsonDocument.Parse(resultJson);
        var root = doc.RootElement;

        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("content", out var contentProp) && contentProp.ValueKind == JsonValueKind.Array && contentProp.GetArrayLength() > 0)
        {
            var firstBlock = contentProp[0];
            if (firstBlock.TryGetProperty("text", out var textProp) && textProp.ValueKind == JsonValueKind.String)
            {
                return textProp.GetString()!;
            }
        }

        return resultJson;
    }
}
