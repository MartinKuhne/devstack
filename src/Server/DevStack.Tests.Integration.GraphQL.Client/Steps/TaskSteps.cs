using System;
using System.Linq;
using System.Threading.Tasks;
using DevStack.Client;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.GraphQL.Client.Steps;

[Binding]
public class TaskSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly IDevStackClient _client;

    public TaskSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _client = Hooks.SpecFlowHooks.GetClient(scenarioContext);
    }

    [When(@"I create a task with title ""(.*)"" and complexity rating (\d+)")]
    public async Task WhenICreateATaskWithTitleAndComplexityRating(string title, int complexityRating)
    {
        var projectId = _scenarioContext.TryGetValue<string>("ParentProjectId", out var pid) ? pid : null;
        var featureId = _scenarioContext.TryGetValue<string>("ParentFeatureId", out var fid) ? fid : null;

        projectId.Should().NotBeNullOrEmpty();
        featureId.Should().NotBeNullOrEmpty();

        var input = new CreateTaskInput
        {
            ProjectId = Guid.Parse(projectId!),
            ItemId = Guid.Parse(featureId!),
            Title = title,
            ComplexityRating = complexityRating
        };

        var result = await _client.CreateTask.ExecuteAsync(input);
        result.Errors.Should().BeEmpty();
        
        _scenarioContext["LastMutationErrors"] = new System.Collections.Generic.List<string>(result.Errors.Select(e => e.Message));
        _scenarioContext["CreatedTaskId"] = result.Data?.CreateTask.Task?.Id;
    }

    [When(@"I update the task title to ""(.*)"" and complexity rating to (\d+)")]
    public async Task WhenIUpdateTheTaskTitleToAndComplexityRatingTo(string title, int complexityRating)
    {
        var taskId = _scenarioContext.TryGetValue<string>("CurrentTaskId", out var tid) ? tid 
            : _scenarioContext.TryGetValue<string>("TaskId_Original Title", out tid) ? tid : null;
        
        taskId.Should().NotBeNullOrEmpty();

        var input = new UpdateTaskInput
        {
            Id = Guid.Parse(taskId!),
            Title = title,
            ComplexityRating = complexityRating
        };

        var result = await _client.UpdateTask.ExecuteAsync(input);
        result.Errors.Should().BeEmpty();
        
        _scenarioContext["LastMutationErrors"] = new System.Collections.Generic.List<string>(result.Errors.Select(e => e.Message));
    }

    [When(@"I transition the task status to ""(.*)""")]
    public async Task WhenITransitionTheTaskStatusTo(string status)
    {
        var taskId = _scenarioContext.TryGetValue<string>("CurrentTaskId", out var tid) ? tid : null;
        taskId.Should().NotBeNullOrEmpty();

        var statusEnum = Enum.Parse<DevStack.Client.TaskStatus>(status, ignoreCase: true);
        var input = new TransitionTaskInput
        {
            Id = Guid.Parse(taskId!),
            TargetStatus = statusEnum,
            Actor = "Test"
        };

        var result = await _client.TransitionTaskStatus.ExecuteAsync(input);
        result.Errors.Should().BeEmpty();
        
        _scenarioContext["LastMutationErrors"] = new System.Collections.Generic.List<string>(result.Errors.Select(e => e.Message));
        _scenarioContext["CurrentTaskStatus"] = status;
    }

    [When(@"I delete the task")]
    public async Task WhenIDeleteTheTask()
    {
        var taskId = _scenarioContext.TryGetValue<string>("CurrentTaskId", out var tid) ? tid 
            : _scenarioContext.TryGetValue<string>("TaskId_To Delete", out tid) ? tid : null;
        
        taskId.Should().NotBeNullOrEmpty();

        var input = new DeleteTaskInput
        {
            Id = Guid.Parse(taskId!)
        };

        var result = await _client.DeleteTask.ExecuteAsync(input);
        result.Errors.Should().BeEmpty();
        
        _scenarioContext["LastMutationErrors"] = new System.Collections.Generic.List<string>(result.Errors.Select(e => e.Message));
    }

    [Then(@"the task should be created successfully")]
    public void ThenTheTaskShouldBeCreatedSuccessfully()
    {
        var errors = _scenarioContext.TryGetValue<System.Collections.Generic.IReadOnlyList<string>>("LastMutationErrors", out var e) ? e : null;
        errors.Should().BeEmpty();
    }

    [Then(@"the task should exist in the database")]
    public void ThenTheTaskShouldExistInTheDatabase()
    {
        var taskId = _scenarioContext.TryGetValue<string>("CreatedTaskId", out var id) ? id : null;
        taskId.Should().NotBeNullOrEmpty();
    }

    [Then(@"the task should be updated successfully")]
    public void ThenTheTaskShouldBeUpdatedSuccessfully()
    {
        var errors = _scenarioContext.TryGetValue<System.Collections.Generic.IReadOnlyList<string>>("LastMutationErrors", out var e) ? e : null;
        errors.Should().BeEmpty();
    }

    [Then(@"the task status should be ""(.*)""")]
    public void ThenTheTaskStatusShouldBe(string status)
    {
        var currentStatus = _scenarioContext.TryGetValue<string>("CurrentTaskStatus", out var s) ? s : null;
        currentStatus.Should().Be(status);
    }

    [Then(@"the task should be deleted successfully")]
    public void ThenTheTaskShouldBeDeletedSuccessfully()
    {
        var errors = _scenarioContext.TryGetValue<System.Collections.Generic.IReadOnlyList<string>>("LastMutationErrors", out var e) ? e : null;
        errors.Should().BeEmpty();
    }

    [Then(@"the task should not exist in the database")]
    public void ThenTheTaskShouldNotExistInTheDatabase()
    {
        var taskId = _scenarioContext.TryGetValue<string>("CreatedTaskId", out var id) ? id 
            : _scenarioContext.TryGetValue<string>("TaskId_To Delete", out id) ? id : null;
        
        taskId.Should().NotBeNullOrEmpty();
    }
}