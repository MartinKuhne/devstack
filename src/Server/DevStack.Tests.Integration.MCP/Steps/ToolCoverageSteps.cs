using System.Text.Json;

using AwesomeAssertions;

using DevStack.Tests.Integration.MCP.Hooks;

using ModelContextProtocol;
using ModelContextProtocol.Client;

using Reqnroll;

namespace DevStack.Tests.Integration.MCP.Steps;

/// <summary>
/// Step definitions for the happy paths of the tools that had no integration coverage (MCP-01).
/// </summary>
/// <remarks>
/// Every scenario works on its own freshly created project so that lookups such as
/// <c>get_next_deliverable</c> and <c>get_next_task</c> resolve exactly one candidate entity,
/// regardless of what other scenarios left behind in the shared database.
/// </remarks>
[Binding]
public sealed class ToolCoverageSteps
{
    private const string ProjectKey = "CoverageProjectId";
    private const string DeliverableKey = "CoverageDeliverableId";
    private const string TaskKey = "CoverageTaskId";
    private const string ProjectDataKey = "CoverageGetProjectPayload";

    private readonly ScenarioContext _scenarioContext;
    private readonly string _suffix = Guid.NewGuid().ToString("N")[..8];
    private string? _createdProjectName;
    private string? _createdProjectRepository;

    public ToolCoverageSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    private McpClient Client => SpecFlowHooks.GetMcpClient(_scenarioContext);

    private string CoverageProjectId => _scenarioContext.GetString(ProjectKey)
        ?? throw new InvalidOperationException("No coverage project has been created for this scenario.");

    private string CoverageDeliverableId => _scenarioContext.GetString(DeliverableKey)
        ?? throw new InvalidOperationException("No coverage deliverable has been created for this scenario.");

    private string CoverageTaskId => _scenarioContext.GetString(TaskKey)
        ?? throw new InvalidOperationException("No coverage task has been created for this scenario.");

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

    /// <summary>Calls a tool that is expected to succeed and returns the JSON payload of its result.</summary>
    private async Task<JsonElement> CallForDataAsync(string tool, Dictionary<string, object?> arguments)
    {
        var outcome = await CallAsync(tool, arguments);
        outcome.Error.Should().BeNull($"{tool} should not throw, but threw {outcome.Error?.GetType().Name}: {outcome.Error?.Message}");
        outcome.SuccessResult.IsError.Should().NotBeTrue($"{tool} should not report a tool error: {McpToolResult.Text(outcome.SuccessResult)}");
        return McpToolResult.Data(outcome.SuccessResult);
    }

    /// <summary>Creates a uniquely named project that the cleanup predicate can reclaim and registers it for scenario cleanup.</summary>
    private async Task<string> CreateProjectAsync(string label)
    {
        _createdProjectName = $"[DeleteAfterTest] MCP {label} {_suffix}";
        _createdProjectRepository = $"https://github.com/deleteaftertest/{_suffix}";

        var outcome = await CallAsync("create_project", new Dictionary<string, object?>
        {
            ["name"] = _createdProjectName,
            ["repository"] = _createdProjectRepository,
            ["description"] = "Project created by the MCP tool coverage scenarios",
        });

        outcome.Error.Should().BeNull($"create_project should succeed: {outcome.Error?.Message}");
        outcome.SuccessResult.IsError.Should().NotBeTrue("create_project should not report a tool error");

        var id = McpToolResult.RequiredStringProperty(McpToolResult.Data(outcome.SuccessResult), "Id");
        CreatedRowRegistry.Register(_scenarioContext, id);
        return id;
    }

    /// <summary>Creates a deliverable in the scenario project, optionally filling every documentation field, and registers it for scenario cleanup.</summary>
    private async Task<string> CreateDeliverableAsync(bool allFields)
    {
        var arguments = new Dictionary<string, object?>
        {
            ["projectId"] = CoverageProjectId,
            ["title"] = $"[DeleteAfterTest] Deliverable {_suffix}",
            ["description"] = "Deliverable created by the MCP tool coverage scenarios",
        };

        if (allFields)
        {
            arguments["design"] = $"design-{_suffix}";
            arguments["acceptanceCriteria"] = $"criteria-{_suffix}";
            arguments["executionPlan"] = $"plan-{_suffix}";
            arguments["securityImpact"] = $"security-{_suffix}";
            arguments["performanceImpact"] = $"performance-{_suffix}";
            arguments["testPlan"] = $"test-{_suffix}";
            arguments["deploymentPlan"] = $"deployment-{_suffix}";
        }

        var data = await CallForDataAsync("create_deliverable", arguments);
        var id = McpToolResult.RequiredStringProperty(data, "Id");
        _scenarioContext[DeliverableKey] = id;
        CreatedRowRegistry.Register(_scenarioContext, id);
        return id;
    }

    /// <summary>Transitions an entity using one of the status transition tools and asserts the call succeeded.</summary>
    private async Task TransitionAsync(string tool, string id, string status)
    {
        var outcome = await CallAsync(tool, new Dictionary<string, object?>
        {
            ["id"] = id,
            ["targetStatus"] = status,
            ["actor"] = "coverage-test",
        });

        outcome.Error.Should().BeNull($"{tool} should succeed: {outcome.Error?.Message}");
        outcome.SuccessResult.IsError.Should().NotBeTrue($"{tool} should not report a tool error: {McpToolResult.Text(outcome.SuccessResult)}");
    }

    [When(@"I call get_project for the seeded project")]
    public async Task WhenICallGetProjectForTheSeededProject()
    {
        var outcome = await CallAsync("get_project", new Dictionary<string, object?>
        {
            ["id"] = SpecFlowHooks.GetSeededProjectId().ToString(),
        });

        outcome.Error.Should().BeNull($"get_project should not fail: {outcome.Error?.Message}");
        outcome.SuccessResult.IsError.Should().NotBeTrue();
        _scenarioContext[ProjectDataKey] = McpToolResult.Json(outcome.SuccessResult);
    }

    [Then(@"the returned project id is the seeded project")]
    public void ThenTheReturnedProjectIdIsTheSeededProject()
    {
        var payload = JsonDocument.Parse(_scenarioContext.GetString(ProjectDataKey)!).RootElement;
        McpToolResult.RequiredStringProperty(payload, "Id").Should().Be(SpecFlowHooks.GetSeededProjectId().ToString());
    }

    [Then(@"the returned project name contains ""(.*)""")]
    public void ThenTheReturnedProjectNameContains(string expectedNamePart)
    {
        var payload = JsonDocument.Parse(_scenarioContext.GetString(ProjectDataKey)!).RootElement;
        McpToolResult.RequiredStringProperty(payload, "Name").Should().Contain(expectedNamePart);
        McpToolResult.RequiredStringProperty(payload, "Repository").Should().NotBeNullOrWhiteSpace();
    }

    [When(@"I create a coverage project named ""(.*)""")]
    public async Task WhenICreateACoverageProjectNamed(string label)
    {
        _scenarioContext["CreatedProjectId"] = await CreateProjectAsync(label);
    }

    [Then(@"the created coverage project can be read back with get_project")]
    public async Task ThenTheCreatedCoverageProjectCanBeReadBackWithGetProject()
    {
        var id = _scenarioContext.GetString("CreatedProjectId")
            ?? throw new InvalidOperationException("No project was created by this scenario.");

        var payload = await CallForDataAsync("get_project", new Dictionary<string, object?> { ["id"] = id });

        McpToolResult.RequiredStringProperty(payload, "Id").Should().Be(id);
        McpToolResult.RequiredStringProperty(payload, "Name").Should().Be(_createdProjectName);
        McpToolResult.RequiredStringProperty(payload, "Repository").Should().Be(_createdProjectRepository);
    }

    [Given(@"a coverage project")]
    public async Task GivenACoverageProject()
    {
        _scenarioContext[ProjectKey] = await CreateProjectAsync("Coverage");
    }

    [Given(@"a coverage deliverable")]
    public async Task GivenACoverageDeliverable()
    {
        await CreateDeliverableAsync(allFields: false);
    }

    [Given(@"a coverage deliverable in ""(.*)"" status")]
    public async Task GivenACoverageDeliverableInStatus(string status)
    {
        var id = await CreateDeliverableAsync(allFields: false);
        await TransitionAsync("update_deliverable_status", id, status);
    }

    [When(@"I create a fully documented deliverable")]
    public async Task WhenICreateAFullyDocumentedDeliverable()
    {
        await CreateDeliverableAsync(allFields: true);
    }

    [Then(@"get_deliverable returns the documented deliverable fields")]
    public async Task ThenGetDeliverableReturnsTheDocumentedDeliverableFields()
    {
        var payload = await CallForDataAsync("get_deliverable", new Dictionary<string, object?> { ["id"] = CoverageDeliverableId });

        McpToolResult.RequiredStringProperty(payload, "Id").Should().Be(CoverageDeliverableId);
        McpToolResult.RequiredStringProperty(payload, "ProjectId").Should().Be(CoverageProjectId);
        McpToolResult.RequiredStringProperty(payload, "Title").Should().Be($"[DeleteAfterTest] Deliverable {_suffix}");
        McpToolResult.RequiredStringProperty(payload, "Design").Should().Be($"design-{_suffix}");
        McpToolResult.RequiredStringProperty(payload, "AcceptanceCriteria").Should().Be($"criteria-{_suffix}");
        McpToolResult.RequiredStringProperty(payload, "ExecutionPlan").Should().Be($"plan-{_suffix}");
        McpToolResult.RequiredStringProperty(payload, "SecurityImpact").Should().Be($"security-{_suffix}");
        McpToolResult.RequiredStringProperty(payload, "PerformanceImpact").Should().Be($"performance-{_suffix}");
        McpToolResult.RequiredStringProperty(payload, "TestPlan").Should().Be($"test-{_suffix}");
        McpToolResult.RequiredStringProperty(payload, "DeploymentPlan").Should().Be($"deployment-{_suffix}");
    }

    [Then(@"get_next_deliverable finds the coverage deliverable in ""(.*)"" status")]
    public async Task ThenGetNextDeliverableFindsTheCoverageDeliverableInStatus(string status)
    {
        var payload = await CallForDataAsync("get_next_deliverable", new Dictionary<string, object?>
        {
            ["projectId"] = CoverageProjectId,
            ["status"] = status,
        });

        McpToolResult.RequiredStringProperty(payload, "Id").Should().Be(CoverageDeliverableId);
    }

    [When(@"I move the coverage deliverable to ""(.*)"" status")]
    public async Task WhenIMoveTheCoverageDeliverableToStatus(string status)
    {
        await TransitionAsync("update_deliverable_status", CoverageDeliverableId, status);
    }

    [Given(@"a coverage task")]
    public async Task GivenACoverageTask()
    {
        var payload = await CallForDataAsync("create_task", new Dictionary<string, object?>
        {
            ["deliverableId"] = CoverageDeliverableId,
            ["title"] = $"[DeleteAfterTest] Task {_suffix}",
            ["description"] = $"task-description-{_suffix}",
        });

        var taskId = McpToolResult.RequiredStringProperty(payload, "Id");
        _scenarioContext[TaskKey] = taskId;
        CreatedRowRegistry.Register(_scenarioContext, taskId);
    }

    [Then(@"get_task returns the coverage task")]
    public async Task ThenGetTaskReturnsTheCoverageTask()
    {
        var payload = await CallForDataAsync("get_task", new Dictionary<string, object?> { ["id"] = CoverageTaskId });

        McpToolResult.RequiredStringProperty(payload, "Id").Should().Be(CoverageTaskId);
        McpToolResult.RequiredStringProperty(payload, "ProjectId").Should().Be(CoverageProjectId);
        McpToolResult.RequiredStringProperty(payload, "Title").Should().Be($"[DeleteAfterTest] Task {_suffix}");
        McpToolResult.RequiredStringProperty(payload, "Description").Should().Be($"task-description-{_suffix}");
        McpToolResult.RequiredStringProperty(payload, "Status").Should().Be("Ready");
    }

    [Then(@"get_next_task returns the coverage task")]
    public async Task ThenGetNextTaskReturnsTheCoverageTask()
    {
        var payload = await CallForDataAsync("get_next_task", new Dictionary<string, object?>
        {
            ["projectId"] = CoverageProjectId,
        });

        McpToolResult.RequiredStringProperty(payload, "Id").Should().Be(CoverageTaskId);
        McpToolResult.RequiredStringProperty(payload, "Status").Should().Be("Ready");
    }

    [Given(@"the coverage task is moved to ""(.*)"" status")]
    public async Task GivenTheCoverageTaskIsMovedToStatus(string status)
    {
        await TransitionAsync("update_task_status", CoverageTaskId, status);
    }

    [Then(@"get_next_task reports all tasks are completed")]
    public async Task ThenGetNextTaskReportsAllTasksAreCompleted()
    {
        var outcome = await CallAsync("get_next_task", new Dictionary<string, object?>
        {
            ["projectId"] = CoverageProjectId,
        });

        outcome.Error.Should().BeNull($"get_next_task should not fail: {outcome.Error?.Message}");
        outcome.SuccessResult.IsError.Should().NotBeTrue();
        McpToolResult.Text(outcome.SuccessResult).Should().Contain("No Pending Tasks");

        var payload = McpToolResult.Data(outcome.SuccessResult);
        McpToolResult.RequiredStringProperty(payload, "Message").Should().Be("All tasks are completed for the selected deliverable");
        // BUG-06 (ledger §6): this asserts the DEFECTIVE payload shape, not an intended contract. On the
        // "all tasks completed" branch get_next_task returns the DELIVERABLE id under the key "Id", in the same
        // shape a task payload uses, so a client following the usage hint would pass it to get_task. See the
        // BUG-06 block in DevStack.Mcp/Tools/TaskTools.cs. Rename the key to DeliverableId and update this
        // assertion in the same change.
        McpToolResult.RequiredStringProperty(payload, "Id").Should().Be(CoverageDeliverableId);
    }
}
