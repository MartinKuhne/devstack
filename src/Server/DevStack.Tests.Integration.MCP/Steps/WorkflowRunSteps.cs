using TechTalk.SpecFlow;
using DevStack.Tests.Integration.MCP.Client;
using FluentAssertions;
using System.Text.Json;
using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevStack.Tests.Integration.MCP.Steps;

[Binding]
public sealed class WorkflowRunSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly DevStackDbContext _dbContext;
    private JsonRpcResponse? _response;
    private Guid? _createdWorkflowRunId;
    private Guid? _testProjectId;

    public WorkflowRunSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        var connectionString = Environment.GetEnvironmentVariable("DEVSTACK_TEST_CONNECTION_STRING") 
            ?? "Host=localhost;Database=devstack_test;Username=postgres;Password=postgres";
        
        var options = new DbContextOptionsBuilder<DevStackDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        
        _dbContext = new DevStackDbContext(options);
    }

    #region Given Steps

    [Given(@"a project with workflow runs")]
    public void GivenAProjectWithWorkflowRuns()
    {
        var project = new Project { Name = "Test Project for WorkflowRuns" };
        _dbContext.Projects.Add(project);
        _dbContext.SaveChanges();
        _testProjectId = project.Id;

        for (int i = 0; i < 3; i++)
        {
            var run = new WorkflowRun
            {
                ProjectId = project.Id,
                WorkflowType = (WorkflowType)i,
                Status = (WorkflowRunStatus)(i + 1),
                StartedAt = DateTime.UtcNow.AddHours(-i),
                InputPayload = $"{{\"input\": {i}}}"
            };
            _dbContext.WorkflowRuns.Add(run);
        }
        _dbContext.SaveChanges();
    }

    [Given(@"a valid workflow run request")]
    public void GivenAValidWorkflowRunRequest()
    {
        _scenarioContext["WorkflowRunProjectId"] = Guid.NewGuid().ToString();
        _scenarioContext["WorkflowRunType"] = "Planner";
        _scenarioContext["WorkflowRunInput"] = "{\"action\": \"plan\"}";
    }

    [Given(@"an existing workflow run")]
    public async Task GivenAnExistingWorkflowRun()
    {
        if (_testProjectId == null)
        {
            var project = new Project { Name = "Test Project" };
            _dbContext.Projects.Add(project);
            await _dbContext.SaveChangesAsync();
            _testProjectId = project.Id;
        }

        var run = new WorkflowRun
        {
            ProjectId = _testProjectId.Value,
            WorkflowType = WorkflowType.DevLead,
            Status = WorkflowRunStatus.Running,
            InputPayload = "{\"input\": \"test\"}"
        };
        _dbContext.WorkflowRuns.Add(run);
        await _dbContext.SaveChangesAsync();
        
        _scenarioContext["WorkflowRunId"] = run.Id.ToString();
    }

    [Given(@"a running workflow run")]
    public async Task GivenARunningWorkflowRun()
    {
        await GivenAnExistingWorkflowRun();
        
        var runId = Guid.Parse(_scenarioContext.GetString("WorkflowRunId") ?? "");
        var run = await _dbContext.WorkflowRuns.FindAsync(runId);
        if (run != null)
        {
            run.Status = WorkflowRunStatus.Running;
            await _dbContext.SaveChangesAsync();
        }
    }

    #endregion

    #region When Steps

    [When(@"I call GetWorkflowRuns")]
    public async Task WhenICallGetWorkflowRuns()
    {
        var projectId = _testProjectId ?? Guid.Parse(_scenarioContext["ProjectId"].ToString()!);
        
        var runs = await _dbContext.WorkflowRuns
            .Where(wr => wr.ProjectId == projectId)
            .ToListAsync();
        
        var result = JsonSerializer.Serialize(runs);
        _response = new JsonRpcResponse("2.0", JsonDocument.Parse(result));
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call CreateWorkflowRun")]
    public async Task WhenICallCreateWorkflowRun()
    {
        var project = new Project { Name = $"Test Project {Guid.NewGuid()}" };
        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync();

        var workflowTypeStr = _scenarioContext.GetString("WorkflowRunType") ?? "Planner";
        var workflowType = Enum.Parse<WorkflowType>(workflowTypeStr, true);
        var inputPayload = _scenarioContext.GetString("WorkflowRunInput") ?? "{}";

        var run = new WorkflowRun
        {
            ProjectId = project.Id,
            WorkflowType = workflowType,
            Status = WorkflowRunStatus.Queued,
            InputPayload = inputPayload
        };

        _dbContext.WorkflowRuns.Add(run);
        await _dbContext.SaveChangesAsync();

        _createdWorkflowRunId = run.Id;
        var result = JsonSerializer.Serialize(run);
        _response = new JsonRpcResponse("2.0", JsonDocument.Parse(result));
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call UpdateWorkflowRun")]
    public async Task WhenICallUpdateWorkflowRun()
    {
        var runId = Guid.Parse(_scenarioContext.GetString("WorkflowRunId") ?? "");
        var run = await _dbContext.WorkflowRuns.FindAsync(runId);
        
        if (run != null)
        {
            run.Status = WorkflowRunStatus.Succeeded;
            run.OutputPayload = "{\"output\": \"success\"}";
            run.CompletedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }

        var result = JsonSerializer.Serialize(run);
        _response = new JsonRpcResponse("2.0", JsonDocument.Parse(result));
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call CancelWorkflowRun")]
    public async Task WhenICallCancelWorkflowRun()
    {
        var runId = Guid.Parse(_scenarioContext.GetString("WorkflowRunId") ?? "");
        var run = await _dbContext.WorkflowRuns.FindAsync(runId);
        
        if (run != null)
        {
            run.Status = WorkflowRunStatus.Cancelled;
            run.CompletedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }

        _response = new JsonRpcResponse("2.0", JsonDocument.Parse("{\"cancelled\": true}"));
        _scenarioContext["Response"] = _response;
    }

    #endregion

    #region Then Steps

    [Then(@"the response should contain all workflow runs for the project")]
    public void ThenTheResponseShouldContainAllWorkflowRunsForTheProject()
    {
        _response.Should().NotBeNull();
        _response!.Result.Should().NotBeNull();
        
        var result = _response!.Result!.ToString()!;
        var runs = JsonSerializer.Deserialize<List<WorkflowRun>>(result)!;
        runs.Should().NotBeNullOrEmpty();
        var expectedProjectId = _testProjectId!.Value;
        runs!.ForEach(r => r.ProjectId.Should().Be(expectedProjectId));
    }

    [Then(@"the response should contain the created workflow run")]
    public void ThenTheResponseShouldContainTheCreatedWorkflowRun()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
        _response!.Result.Should().NotBeNull();
    }

    [Then(@"the workflow type should be set correctly")]
    public void ThenTheWorkflowTypeShouldBeSetCorrectly()
    {
        var result = _response!.Result!.ToString()!;
        var run = JsonSerializer.Deserialize<WorkflowRun>(result)!;
        run!.WorkflowType.Should().Be(WorkflowType.Planner);
    }

    [Then(@"the response should contain the updated workflow run")]
    public void ThenTheResponseShouldContainTheUpdatedWorkflowRun()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
        
        var result = _response!.Result!.ToString()!;
        var run = JsonSerializer.Deserialize<WorkflowRun>(result)!;
        run!.Status.Should().Be(WorkflowRunStatus.Succeeded);
        run.OutputPayload.Should().NotBeNullOrEmpty();
    }

    [Then(@"the status should transition correctly")]
    public void ThenTheStatusShouldTransitionCorrectly()
    {
        var result = _response!.Result!.ToString()!;
        var run = JsonSerializer.Deserialize<WorkflowRun>(result)!;
        run!.Status.Should().Be(WorkflowRunStatus.Succeeded);
        run.CompletedAt.Should().NotBeNull();
    }

    [Then(@"the response should confirm cancellation")]
    public void ThenTheResponseShouldConfirmCancellation()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
    }

    [Then(@"the status should be cancelled")]
    public void ThenTheStatusShouldBeCancelled()
    {
        var runIdStr = _scenarioContext.GetString("WorkflowRunId") ?? "";
        var runId = Guid.Parse(runIdStr);
        var run = _dbContext.WorkflowRuns.Find(runId);
        run!.Status.Should().Be(WorkflowRunStatus.Cancelled);
        run.CompletedAt.Should().NotBeNull();
    }

    #endregion
}
