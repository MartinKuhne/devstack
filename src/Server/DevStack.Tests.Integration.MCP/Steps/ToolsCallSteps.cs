using AwesomeAssertions;

using DevStack.Tests.Integration.MCP.Hooks;

using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

using Reqnroll;

namespace DevStack.Tests.Integration.MCP.Steps;

/// <summary>
/// Step definitions for the <c>tools/call</c> protocol surface, including the unknown tool error contract (MCP-11).
/// </summary>
[Binding]
public sealed class ToolsCallSteps
{
    private readonly ScenarioContext _scenarioContext;
    private CallOutcome? _outcome;

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

    [Given(@"a tools/call request for the unknown tool ""(.*)""")]
    public void GivenAToolsCallRequestForTheUnknownTool(string toolName)
    {
        _scenarioContext["ToolName"] = toolName;
    }

    [When(@"^I send the tools/call request$")]
    public async Task WhenISendTheToolsCallRequest()
    {
        var toolName = _scenarioContext.GetString("ToolName") ?? "get_projects";

        try
        {
            _outcome = new CallOutcome(await Client.CallToolAsync(toolName, new Dictionary<string, object?>()), null);
        }
        catch (McpException ex)
        {
            _outcome = new CallOutcome(null, ex);
        }

        _scenarioContext["CallResult"] = _outcome;
    }

    [Then(@"the response should contain the tool result")]
    public void ThenTheResponseShouldContainTheToolResult()
    {
        _outcome.Should().NotBeNull();
        _outcome!.Error.Should().BeNull($"the tool call should not fail: {_outcome.Error?.Message}");
        _outcome.SuccessResult.IsError.Should().NotBeTrue($"the tool call should succeed: {McpToolResult.Text(_outcome.SuccessResult)}");
    }

    [Then(@"the result should contain a content array")]
    public void ThenTheResultShouldContainAContentArray()
    {
        var result = _outcome!.SuccessResult;
        result.Content.Should().NotBeEmpty("a successful tool call must return at least one content block");
        result.Content.Should().ContainSingle(c => c is TextContentBlock, "the tool result should carry text content");
    }

    [Then(@"the response should indicate a tool error")]
    public void ThenTheResponseShouldIndicateAToolError()
    {
        _outcome.Should().NotBeNull();
        _outcome!.Result.Should().BeNull("an unknown tool must be rejected with a protocol error instead of a tool result");
        _outcome.Error.Should().BeOfType<McpProtocolException>();
        _outcome.Error!.Message.Should().Contain("Unknown tool");
    }
}
