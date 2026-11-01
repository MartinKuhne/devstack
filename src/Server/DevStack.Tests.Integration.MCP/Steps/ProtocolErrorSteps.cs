using AwesomeAssertions;

using DevStack.Tests.Integration.MCP.Hooks;

using ModelContextProtocol;
using ModelContextProtocol.Client;

using Reqnroll;

namespace DevStack.Tests.Integration.MCP.Steps;

/// <summary>
/// Step definitions for malformed <c>tools/call</c> requests (MCP-04).
/// </summary>
/// <remarks>
/// Unknown primitive names are protocol-level failures and are answered with a JSON-RPC error,
/// which the client re-throws. Argument binding failures (missing argument, wrong type, unknown
/// enum literal) happen inside the tool invocation and are therefore answered with a result whose
/// <c>IsError</c> flag is set and whose text is the generic invocation error.
/// </remarks>
[Binding]
public sealed class ProtocolErrorSteps
{
    private readonly ScenarioContext _scenarioContext;
    private CallOutcome? _outcome;
    private string _tool = string.Empty;

    public ProtocolErrorSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    private McpClient Client => SpecFlowHooks.GetMcpClient(_scenarioContext);

    private CallOutcome Outcome => _outcome
        ?? throw new InvalidOperationException("No tool call has been made by this scenario.");

    /// <summary>Calls a tool and captures either the result or the protocol error thrown by the server.</summary>
    private async Task<CallOutcome> CallAsync(string tool, Dictionary<string, object?> arguments)
    {
        try
        {
            return new CallOutcome(await Client.CallToolAsync(tool, arguments), null);
        }
        catch (McpException ex)
        {
            return new CallOutcome(null, ex);
        }
    }

    /// <summary>Records a call so that the following Then step can assert on its outcome.</summary>
    private async Task RecordCallAsync(string tool, Dictionary<string, object?> arguments)
    {
        _tool = tool;
        _outcome = await CallAsync(tool, arguments);
    }

    [When(@"I call the unknown tool ""(.*)""")]
    public async Task WhenICallTheUnknownTool(string toolName)
    {
        await RecordCallAsync(toolName, new Dictionary<string, object?>());
    }

    [When(@"I call get_project with no arguments")]
    public async Task WhenICallGetProjectWithNoArguments()
    {
        await RecordCallAsync("get_project", new Dictionary<string, object?>());
    }

    [When(@"I call get_project with the id ""(.*)""")]
    public async Task WhenICallGetProjectWithTheId(string id)
    {
        await RecordCallAsync("get_project", new Dictionary<string, object?> { ["id"] = id });
    }

    [When(@"I call get_task with the id ""(.*)""")]
    public async Task WhenICallGetTaskWithTheId(string id)
    {
        await RecordCallAsync("get_task", new Dictionary<string, object?> { ["id"] = id });
    }

    [When(@"I call update_deliverable_status with the target status ""(.*)""")]
    public async Task WhenICallUpdateDeliverableStatusWithTheTargetStatus(string targetStatus)
    {
        await RecordCallAsync("update_deliverable_status", new Dictionary<string, object?>
        {
            ["id"] = Guid.NewGuid().ToString(),
            ["targetStatus"] = targetStatus,
            ["actor"] = "protocol-test",
        });
    }

    [When(@"I call get_next_deliverable with the status ""(.*)""")]
    public async Task WhenICallGetNextDeliverableWithTheStatus(string status)
    {
        await RecordCallAsync("get_next_deliverable", new Dictionary<string, object?>
        {
            ["projectId"] = Guid.NewGuid().ToString(),
            ["status"] = status,
        });
    }

    [When(@"I call update_task_status with the target status ""(.*)""")]
    public async Task WhenICallUpdateTaskStatusWithTheTargetStatus(string targetStatus)
    {
        await RecordCallAsync("update_task_status", new Dictionary<string, object?>
        {
            ["id"] = Guid.NewGuid().ToString(),
            ["targetStatus"] = targetStatus,
            ["actor"] = "protocol-test",
        });
    }

    [Then(@"the server rejects the call as a protocol error containing ""(.*)""")]
    public void ThenTheServerRejectsTheCallAsAProtocolErrorContaining(string fragment)
    {
        Outcome.Result.Should().BeNull($"{_tool} must be rejected with a JSON-RPC error instead of a tool result");
        Outcome.Error.Should().BeOfType<McpProtocolException>();
        Outcome.Error!.Message.Should().Contain(fragment);
    }

    [Then(@"the server rejects the call as a tool execution error")]
    public void ThenTheServerRejectsTheCallAsAToolExecutionError()
    {
        Outcome.Error.Should().BeNull($"{_tool} must be answered with a tool result, not a JSON-RPC error");
        Outcome.SuccessResult.IsError.Should().BeTrue($"{_tool} should report a tool error: {McpToolResult.Text(Outcome.SuccessResult)}");
        McpToolResult.Text(Outcome.SuccessResult).Should().Be($"An error occurred invoking '{_tool}'.");
    }
}
