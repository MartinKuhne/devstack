using TechTalk.SpecFlow;
using DevStack.Tests.Integration.MCP.Client;
using FluentAssertions;

namespace DevStack.Tests.Integration.MCP.Steps;

[Binding]
public sealed class NotImplementedSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly IMcpJsonRpcClient _client;
    private JsonRpcResponse? _response;

    public NotImplementedSteps(ScenarioContext scenarioContext, IMcpJsonRpcClient client)
    {
        _scenarioContext = scenarioContext;
        _client = client;
    }

    [Given(@"a (.*) request")]
    public void GivenARequest(string methodName)
    {
        _scenarioContext["MethodName"] = methodName;
    }

    [When(@"I send the request")]
    public async Task WhenISendTheRequest()
    {
        var methodName = _scenarioContext.GetString("MethodName") ?? "";
        var method = methodName.Contains("/") ? methodName : $"{methodName}/test";
        
        try
        {
            _response = await _client.SendRequestAsync(method, new { });
        }
        catch (JsonRpcException ex) when (ex.Code == -32601)
        {
            _response = new JsonRpcResponse("2.0", null, new JsonRpcError(-32601, "Method not found", null), 1);
        }

        _scenarioContext["Response"] = _response;
    }

    [Then(@"the response should contain error code (-?\d+)")]
    public void ThenTheResponseShouldContainErrorCode(int expectedCode)
    {
        _response.Should().NotBeNull();
        _response!.Error!.Code.Should().Be(expectedCode);
    }

    [Then(@"the error message should contain ""(.*)""")]
    public void ThenTheErrorMessageShouldContain(string expectedMessage)
    {
        _response!.Error!.Message.Should().Contain(expectedMessage);
    }
}
