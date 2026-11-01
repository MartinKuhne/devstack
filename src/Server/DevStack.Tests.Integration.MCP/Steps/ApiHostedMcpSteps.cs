using System.Net;

using DevStack.Tests.Integration.MCP.Hooks;

using FluentAssertions;

using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.MCP.Steps;

/// <summary>
/// Step definitions for the MCP endpoint that <c>DevStack.Api</c> hosts on <c>/mcp</c> (MCP-09).
/// </summary>
/// <remarks>
/// The API image registers the same tool classes as the standalone MCP host but a shorter request-filter
/// pipeline: it adds <c>McpToolLoggingFilter</c> only and therefore omits <c>McpArgumentNormalizationFilter</c>.
/// The scenarios below pin what that difference means in practice instead of assuming the hosts are identical.
/// Every step text is prefixed with "API-hosted" so that it cannot be confused with the standalone-host steps.
/// </remarks>
[Binding]
public sealed class ApiHostedMcpSteps
{
    private const string ZeroGuid = "00000000-0000-0000-0000-000000000000";

    private readonly ScenarioContext _scenarioContext;
    private readonly string _suffix = Guid.NewGuid().ToString("N")[..8];
    private readonly List<Guid> _apiCreatedIds = [];

    private McpClient? _apiClient;
    private HttpClient? _httpClient;
    private IList<McpClientTool>? _apiTools;
    private CallOutcome? _apiOutcome;
    private CallOutcome? _standaloneOutcome;
    private string _apiProjectName = string.Empty;
    private string _apiProjectId = string.Empty;
    private HttpStatusCode? _probeStatus;

    public ApiHostedMcpSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    private McpClient ApiClient => _apiClient
        ?? throw new InvalidOperationException("The API-hosted MCP client has not been connected.");

    private McpClient StandaloneClient => SpecFlowHooks.GetMcpClient(_scenarioContext);

    private CallOutcome ApiOutcome => _apiOutcome
        ?? throw new InvalidOperationException("No API-hosted tool call has been made by this scenario.");

    private CallOutcome StandaloneOutcome => _standaloneOutcome
        ?? throw new InvalidOperationException("No standalone MCP tool call has been made by this scenario.");

    /// <summary>Calls a tool and captures either the result or the protocol error thrown by the server.</summary>
    private static async Task<CallOutcome> CallAsync(McpClient client, string tool, Dictionary<string, object?> arguments)
    {
        try
        {
            return new CallOutcome(await client.CallToolAsync(tool, arguments), null);
        }
        catch (McpException ex)
        {
            return new CallOutcome(null, ex);
        }
    }

    /// <summary>Connects a client and a plain HTTP client to the API image and makes sure its database exists.</summary>
    [Given(@"the API-hosted MCP client is connected")]
    public async Task GivenTheApiHostedMcpClientIsConnected()
    {
        var environment = ApiHostedMcpHooks.Current;

        var transport = new HttpClientTransport(new HttpClientTransportOptions
        {
            Endpoint = new Uri($"{environment.AppUrl}/mcp"),
            TransportMode = HttpTransportMode.StreamableHttp,
        });

        _apiClient = await McpClient.CreateAsync(transport);
        _httpClient = new HttpClient { BaseAddress = new Uri(environment.AppUrl) };
        _scenarioContext["ApiHostedMcpClient"] = _apiClient;
    }

    /// <summary>Releases the clients and removes the rows this scenario created inside the API image's database.</summary>
    [AfterScenario]
    public async Task AfterScenarioAsync()
    {
        (_apiClient as IDisposable)?.Dispose();
        _apiClient = null;
        _httpClient?.Dispose();
        _httpClient = null;

        if (_apiCreatedIds.Count > 0)
        {
            await ScenarioDataCleaner.DeleteAsync(ApiHostedMcpHooks.Current.PostgresConnectionString, _apiCreatedIds);
            _apiCreatedIds.Clear();
        }
    }

    [When(@"I request the API-hosted tool list")]
    public async Task WhenIRequestTheApiHostedToolList()
    {
        _apiTools = await ApiClient.ListToolsAsync();
    }

    [Then(@"the API-hosted tools are exactly the tools of the standalone MCP host")]
    public async Task ThenTheApiHostedToolsAreExactlyTheToolsOfTheStandaloneMcpHost()
    {
        _apiTools.Should().NotBeNull("the API-hosted tool list must have been requested first");

        var standaloneTools = await StandaloneClient.ListToolsAsync();

        _apiTools!.Select(t => t.Name).Should().BeEquivalentTo(standaloneTools.Select(t => t.Name));
    }

    [When(@"I create an API-hosted project named ""(.*)""")]
    public async Task WhenICreateAnApiHostedProjectNamed(string label)
    {
        _apiProjectName = $"[DeleteAfterTest] API-hosted {label} {_suffix}";

        var outcome = await CallAsync(ApiClient, "create_project", new Dictionary<string, object?>
        {
            ["name"] = _apiProjectName,
            ["repository"] = $"https://github.com/deleteaftertest/{_suffix}",
            ["description"] = "Project created by the API-hosted MCP scenarios",
        });

        _apiOutcome = outcome;
        _apiProjectId = McpToolResult.RequiredStringProperty(McpToolResult.Data(outcome.SuccessResult), "Id");
        _apiCreatedIds.Add(Guid.Parse(_apiProjectId));
    }

    [Then(@"the API-hosted project can be read back with get_project")]
    public async Task ThenTheApiHostedProjectCanBeReadBackWithGetProject()
    {
        ApiOutcome.Error.Should().BeNull($"create_project on the API host should succeed: {ApiOutcome.Error?.Message}");
        ApiOutcome.SuccessResult.IsError.Should().NotBeTrue("create_project on the API host should not report a tool error");

        var payload = await CallAsync(ApiClient, "get_project", new Dictionary<string, object?> { ["id"] = _apiProjectId });

        payload.Error.Should().BeNull($"get_project on the API host should succeed: {payload.Error?.Message}");
        payload.SuccessResult.IsError.Should().NotBeTrue();

        var data = McpToolResult.Data(payload.SuccessResult);
        McpToolResult.RequiredStringProperty(data, "Id").Should().Be(_apiProjectId);
        McpToolResult.RequiredStringProperty(data, "Name").Should().Be(_apiProjectName);
    }

    [When(@"I call get_project with the zero guid on both hosts")]
    public async Task WhenICallGetProjectWithTheZeroGuidOnBothHosts()
    {
        var arguments = new Dictionary<string, object?> { ["id"] = ZeroGuid };
        _standaloneOutcome = await CallAsync(StandaloneClient, "get_project", arguments);
        _apiOutcome = await CallAsync(ApiClient, "get_project", arguments);
    }

    [Then(@"both hosts report the hard error as a protocol error containing ""(.*)""")]
    public void ThenBothHostsReportTheHardErrorAsAProtocolErrorContaining(string expectedMessage)
    {
        foreach (var (host, outcome) in new[] { ("standalone", StandaloneOutcome), ("API-hosted", ApiOutcome) })
        {
            outcome.Result.Should().BeNull($"{host} must answer with a JSON-RPC error");
            outcome.Error.Should().BeOfType<McpProtocolException>($"{host} must raise a protocol error");
            outcome.Error!.Message.Should().Contain(expectedMessage, $"{host} must report the same validation message");
            outcome.Error!.Message.Should().NotContain("at DevStack.", $"{host} must not leak a stack trace");
        }
    }

    [When(@"I probe the API-hosted endpoint ""(.*)""")]
    public async Task WhenIProbeTheApiHostedEndpoint(string path)
    {
        _httpClient.Should().NotBeNull("the API-hosted HTTP client must have been connected first");
        using var response = await _httpClient!.GetAsync(path);
        _probeStatus = response.StatusCode;
    }

    [Then(@"the API-hosted endpoint responds with 200")]
    public void ThenTheApiHostedEndpointRespondsWith200()
    {
        _probeStatus.Should().Be(HttpStatusCode.OK);
    }

    [When(@"I create a project with an array name on both hosts")]
    public async Task WhenICreateAProjectWithAnArrayNameOnBothHosts()
    {
        var arguments = new Dictionary<string, object?>
        {
            ["name"] = new[] { "alpha", "beta" },
            ["repository"] = $"https://github.com/deleteaftertest/array-{_suffix}",
        };

        _standaloneOutcome = await CallAsync(StandaloneClient, "create_project", arguments);
        _apiOutcome = await CallAsync(ApiClient, "create_project", arguments);

        RegisterStandaloneProjectIfCreated();

        Console.WriteLine($"[MCP-09] array name on standalone host -> {Describe(_standaloneOutcome)}");
        Console.WriteLine($"[MCP-09] array name on API host        -> {Describe(_apiOutcome)}");
    }

    /// <summary>Registers the project the standalone host created from the normalised array so MCP-10 can delete it.</summary>
    private void RegisterStandaloneProjectIfCreated()
    {
        var result = _standaloneOutcome?.Result;
        if (result is null || result.IsError is true)
        {
            return;
        }

        CreatedRowRegistry.RegisterFromPayload(_scenarioContext, McpToolResult.Json(result));
    }

    /// <summary>Describes an outcome in one line so that the observed behaviour ends up in the test output.</summary>
    private static string Describe(CallOutcome outcome)
    {
        return outcome.Error is not null
            ? $"protocol error {outcome.Error.GetType().Name}: {outcome.Error.Message}"
            : $"IsError={outcome.SuccessResult.IsError} text={McpToolResult.Text(outcome.SuccessResult)}";
    }

    [Then(@"the standalone MCP host normalises the array name into a string")]
    public void ThenTheStandaloneMcpHostNormalisesTheArrayNameIntoAString()
    {
        StandaloneOutcome.Error.Should().BeNull($"the standalone host should accept the array name: {StandaloneOutcome.Error?.Message}");
        StandaloneOutcome.SuccessResult.IsError.Should().NotBeTrue("the standalone host should create the project");

        var data = McpToolResult.Data(StandaloneOutcome.SuccessResult);
        McpToolResult.RequiredStringProperty(data, "Name").Should().Be("alpha\nbeta");
    }

    [Then(@"the API-hosted MCP host rejects the array name")]
    public void ThenTheApiHostedMcpHostRejectsTheArrayName()
    {
        // BUG-09 (ledger §6): pinned from the observed response: without McpArgumentNormalizationFilter the JSON
        // array cannot bind
        // to the string parameter, so the invocation fails exactly like every other wrong-argument-type call,
        // i.e. as a tool result with IsError set and the generic invocation message. It is not a protocol
        // error and it does not leak a stack trace.
        ApiOutcome.Error.Should().BeNull($"the API host must answer with a tool result, not a JSON-RPC error. Observed: {Describe(ApiOutcome)}");
        ApiOutcome.SuccessResult.IsError.Should().BeTrue("the array must not bind to the string parameter");
        McpToolResult.Text(ApiOutcome.SuccessResult).Should().Be("An error occurred invoking 'create_project'.");
    }
}
