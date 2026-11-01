using AwesomeAssertions;

using DevStack.Tests.Integration.MCP.Hooks;

using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

using Reqnroll;

namespace DevStack.Tests.Integration.MCP.Steps;

/// <summary>
/// Step definitions for the MCP environment started with <c>FeatureManagement:AgentTaskTools = false</c> (MCP-08).
/// </summary>
/// <remarks>
/// Every step text is prefixed with "flag-off" so that it cannot be confused with the steps of the
/// assembly-level MCP environment.
/// </remarks>
[Binding]
public sealed class FeatureFlagOffSteps
{
    /// <summary>The tools contributed by <c>ProjectTools</c> and <c>DeliverableTools</c>, which are always registered.</summary>
    private static readonly string[] ProjectAndDeliverableTools =
    [
        "get_projects",
        "get_project",
        "create_project",
        "get_deliverable",
        "get_next_deliverable",
        "create_deliverable",
        "update_deliverable",
        "update_deliverable_status",
    ];

    private readonly ScenarioContext _scenarioContext;
    private McpClient? _client;
    private IList<McpClientTool>? _tools;
    private CallOutcome? _outcome;
    private string _tool = string.Empty;

    public FeatureFlagOffSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    private McpClient Client => _client
        ?? throw new InvalidOperationException("The flag-off MCP client has not been connected.");

    private CallOutcome Outcome => _outcome
        ?? throw new InvalidOperationException("No flag-off tool call has been made by this scenario.");

    /// <summary>Connects a client to the isolated flag-off MCP host.</summary>
    [Given(@"the flag-off MCP client is connected")]
    public async Task GivenTheFlagOffMcpClientIsConnected()
    {
        var transport = new HttpClientTransport(new HttpClientTransportOptions
        {
            Endpoint = new Uri($"{FeatureFlagOffHooks.Current.AppUrl}/mcp"),
            TransportMode = HttpTransportMode.StreamableHttp,
        });

        _client = await McpClient.CreateAsync(transport);
        _scenarioContext["FlagOffMcpClient"] = _client;
    }

    /// <summary>Releases the flag-off client; the environment itself is disposed with the feature.</summary>
    [AfterScenario]
    public void AfterScenario()
    {
        (_client as IDisposable)?.Dispose();
        _client = null;
    }

    /// <summary>Calls a tool on the flag-off host and captures either the result or the protocol error.</summary>
    private async Task RecordCallAsync(string tool, Dictionary<string, object?> arguments)
    {
        _tool = tool;
        try
        {
            _outcome = new CallOutcome(await Client.CallToolAsync(tool, arguments), null);
        }
        catch (McpException ex)
        {
            _outcome = new CallOutcome(null, ex);
        }
    }

    [When(@"I request the flag-off tool list")]
    public async Task WhenIRequestTheFlagOffToolList()
    {
        _tools = await Client.ListToolsAsync();
    }

    [Then(@"the flag-off tools are exactly the project and deliverable tools")]
    public void ThenTheFlagOffToolsAreExactlyTheProjectAndDeliverableTools()
    {
        _tools.Should().NotBeNull("the flag-off tool list must have been requested first");
        _tools!.Select(t => t.Name).Should().BeEquivalentTo(ProjectAndDeliverableTools);
    }

    [Then(@"the flag-off tools do not include ""(.*)""")]
    public void ThenTheFlagOffToolsDoNotInclude(string toolName)
    {
        _tools.Should().NotBeNull("the flag-off tool list must have been requested first");
        _tools!.Select(t => t.Name).Should().NotContain(toolName);
    }

    [When(@"I call the flag-off create_task tool")]
    public async Task WhenICallTheFlagOffCreateTaskTool()
    {
        await RecordCallAsync("create_task", new Dictionary<string, object?>
        {
            ["deliverableId"] = Guid.NewGuid().ToString(),
            ["title"] = "[DeleteAfterTest] flag-off task",
        });
    }

    [When(@"I call the flag-off get_projects tool")]
    public async Task WhenICallTheFlagOffGetProjectsTool()
    {
        await RecordCallAsync("get_projects", new Dictionary<string, object?>());
    }

    [Then(@"the flag-off call fails with the protocol error ""(.*)""")]
    public void ThenTheFlagOffCallFailsWithTheProtocolError(string expectedMessage)
    {
        Outcome.Result.Should().BeNull($"{_tool} must be rejected with a JSON-RPC error instead of a tool result");
        Outcome.Error.Should().BeOfType<McpProtocolException>();
        Outcome.Error!.Message.Should().Contain(expectedMessage);
    }

    [Then(@"the flag-off call succeeds")]
    public void ThenTheFlagOffCallSucceeds()
    {
        Outcome.Error.Should().BeNull($"{_tool} should not fail: {Outcome.Error?.Message}");
        Outcome.SuccessResult.IsError.Should().NotBeTrue($"{_tool} should not report a tool error: {McpToolResult.Text(Outcome.SuccessResult)}");
    }
}
