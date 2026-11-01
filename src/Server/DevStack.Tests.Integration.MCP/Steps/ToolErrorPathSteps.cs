using System.Text.Json;

using AwesomeAssertions;

using DevStack.Tests.Integration.MCP.Hooks;

using ModelContextProtocol;
using ModelContextProtocol.Client;

using Reqnroll;

namespace DevStack.Tests.Integration.MCP.Steps;

/// <summary>
/// Step definitions for the tool error contracts (MCP-02 hard errors, MCP-03 soft errors).
/// </summary>
/// <remarks>
/// The MCP SDK exposes three distinct failure shapes to the client:
/// <list type="bullet">
/// <item><description>A tool that throws <see cref="McpProtocolException"/> produces a JSON-RPC error, so
/// <c>CallToolAsync</c> throws an <see cref="McpProtocolException"/> whose message is
/// <c>Request failed (remote): &lt;original message&gt;</c>.</description></item>
/// <item><description>A tool that throws any other exception produces a result with
/// <c>IsError = true</c> and the generic text <c>An error occurred invoking '&lt;tool&gt;'.</c>.</description></item>
/// <item><description>A tool that returns <c>ToolResponse.Error</c> produces a normal result
/// (<c>IsError</c> is not set) whose payload is <c>{"error":"..."}</c>.</description></item>
/// </list>
/// </remarks>
[Binding]
public sealed class ToolErrorPathSteps
{
    private const string EmptyProjectKey = "EmptyCoverageProjectId";
    private const string EmptyDeliverableKey = "EmptyCoverageDeliverableId";

    private readonly ScenarioContext _scenarioContext;
    private readonly string _suffix = Guid.NewGuid().ToString("N")[..8];
    private CallOutcome? _outcome;
    private string _tool = string.Empty;

    public ToolErrorPathSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    private McpClient Client => SpecFlowHooks.GetMcpClient(_scenarioContext);

    private CallOutcome Outcome => _outcome
        ?? throw new InvalidOperationException("No tool call has been made by this scenario.");

    /// <summary>Turns the JSON argument literal from the scenario outline into tool arguments.</summary>
    private static Dictionary<string, object?> ParseArguments(string json)
    {
        var unknown = Guid.NewGuid().ToString();
        var normalized = json.Replace("<unknown>", unknown);

        var arguments = new Dictionary<string, object?>();
        using var document = JsonDocument.Parse(normalized);
        foreach (var property in document.RootElement.EnumerateObject())
        {
            arguments[property.Name] = property.Value.ValueKind == JsonValueKind.Null ? null : property.Value.GetString();
        }

        return arguments;
    }

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

    [When(@"I call the tool ""(.*)"" with arguments (.*)")]
    public async Task WhenICallTheToolWithArguments(string tool, string argumentsJson)
    {
        _tool = tool;
        _outcome = await CallAsync(tool, ParseArguments(argumentsJson));
        RegisterCreatedRowIfAny();
    }

    /// <summary>
    /// Registers the id of a row that a successful <c>create_*</c> call produced, so that the scenario
    /// cleanup hook (MCP-10) removes it as well.
    /// </summary>
    private void RegisterCreatedRowIfAny()
    {
        var result = _outcome?.Result;
        if (!_tool.StartsWith("create_", StringComparison.Ordinal) || result is null || result.IsError is true)
        {
            return;
        }

        CreatedRowRegistry.RegisterFromPayload(_scenarioContext, McpToolResult.Json(result));
    }

    [When(@"I call get_next_deliverable for the empty coverage project with status ""(.*)""")]
    public async Task WhenICallGetNextDeliverableForTheEmptyCoverageProject(string status)
    {
        _tool = "get_next_deliverable";
        _outcome = await CallAsync("get_next_deliverable", new Dictionary<string, object?>
        {
            ["projectId"] = _scenarioContext.GetString(EmptyProjectKey),
            ["status"] = status,
        });
    }

    [When(@"I call get_next_task for the empty coverage project")]
    public async Task WhenICallGetNextTaskForTheEmptyCoverageProject()
    {
        _tool = "get_next_task";
        _outcome = await CallAsync("get_next_task", new Dictionary<string, object?>
        {
            ["projectId"] = _scenarioContext.GetString(EmptyProjectKey),
        });
    }

    [Then(@"the call fails with the protocol error ""(.*)""")]
    public void ThenTheCallFailsWithTheProtocolError(string expectedMessage)
    {
        Outcome.Result.Should().BeNull($"{_tool} must be rejected with a JSON-RPC error instead of a tool result");
        Outcome.Error.Should().BeOfType<McpProtocolException>();
        Outcome.Error!.Message.Should().Contain(expectedMessage);
        Outcome.Error!.Message.Should().NotContain("at DevStack.", "protocol errors must not leak stack traces");
    }

    [Then(@"the call reports a tool execution error")]
    public void ThenTheCallReportsAToolExecutionError()
    {
        Outcome.Error.Should().BeNull($"{_tool} must be answered with a tool result, not a JSON-RPC error");
        Outcome.SuccessResult.IsError.Should().BeTrue($"{_tool} should report a tool error: {McpToolResult.Text(Outcome.SuccessResult)}");
        McpToolResult.Text(Outcome.SuccessResult).Should().Be($"An error occurred invoking '{_tool}'.");
    }

    [Then(@"the call returns the soft error ""(.*)""")]
    public void ThenTheCallReturnsTheSoftError(string expectedMessage)
    {
        Outcome.Error.Should().BeNull($"{_tool} must be answered with a tool result, not a JSON-RPC error");
        Outcome.SuccessResult.IsError.Should().NotBeTrue("soft errors are returned as normal tool results");
        var payload = McpToolResult.Data(Outcome.SuccessResult);
        McpToolResult.RequiredStringProperty(payload, "error").Should().Be(expectedMessage);
    }

    [Given(@"an empty coverage project")]
    public async Task GivenAnEmptyCoverageProject()
    {
        var outcome = await CallAsync("create_project", new Dictionary<string, object?>
        {
            ["name"] = $"[DeleteAfterTest] MCP Empty {_suffix}",
            ["repository"] = $"https://github.com/deleteaftertest/empty-{_suffix}",
        });

        outcome.Error.Should().BeNull($"create_project should succeed: {outcome.Error?.Message}");
        outcome.SuccessResult.IsError.Should().NotBeTrue("create_project should not report a tool error");
        _scenarioContext[EmptyProjectKey] = McpToolResult.RequiredStringProperty(McpToolResult.Data(outcome.SuccessResult), "Id");
        CreatedRowRegistry.Register(_scenarioContext, _scenarioContext.GetString(EmptyProjectKey));
    }

    [Given(@"a deliverable in the empty coverage project")]
    public async Task GivenADeliverableInTheEmptyCoverageProject()
    {
        var outcome = await CallAsync("create_deliverable", new Dictionary<string, object?>
        {
            ["projectId"] = _scenarioContext.GetString(EmptyProjectKey),
            ["title"] = $"[DeleteAfterTest] Deliverable {_suffix}",
        });

        outcome.Error.Should().BeNull($"create_deliverable should succeed: {outcome.Error?.Message}");
        outcome.SuccessResult.IsError.Should().NotBeTrue("create_deliverable should not report a tool error");
        _scenarioContext[EmptyDeliverableKey] = McpToolResult.RequiredStringProperty(McpToolResult.Data(outcome.SuccessResult), "Id");
        CreatedRowRegistry.Register(_scenarioContext, _scenarioContext.GetString(EmptyDeliverableKey));
    }
}
