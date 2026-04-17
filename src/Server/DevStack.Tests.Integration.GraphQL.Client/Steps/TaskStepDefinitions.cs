using System.Text.Json.Nodes;
using DevStack.Domain.Enums;
using DevStack.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.GraphQL.Client.Steps;

[Binding]
public class TaskStepDefinitions
{
    private readonly GraphQLContext _context;
    private Guid _projectId;
    private Guid _featureId;
    private Guid _taskId;
    private JsonNode? _lastMutationResult;

    public TaskStepDefinitions(GraphQLContext context)
    {
        _context = context;
    }

    [Given("a parent project exists")]
    public async Task GivenAParentProjectExists()
    {
        _projectId = Guid.NewGuid();
        
        _lastMutationResult = await _context.SendMutationAsync("CreateProject", new
        {
            input = new { id = _projectId, name = "Test Project" }
        });

        _lastMutationResult!["createProject"]!["errors"]!.AsArray().Should().BeEmpty();
    }

    [Given("a parent feature exists")]
    public async Task GivenAParentFeatureExists()
    {
        _featureId = Guid.NewGuid();

        _lastMutationResult = await _context.SendMutationAsync("CreateFeature", new
        {
            input = new
            {
                id = _featureId,
                projectId = _projectId,
                title = "Parent Feature"
            }
        });

        _lastMutationResult!["createFeature"]!["errors"]!.AsArray().Should().BeEmpty();
    }

    [Given("a task \"(.*)\" exists")]
    public async Task GivenATaskExists(string title)
    {
        _taskId = Guid.NewGuid();

        _lastMutationResult = await _context.SendMutationAsync("CreateTask", new
        {
            input = new
            {
                id = _taskId,
                projectId = _projectId,
                featureId = _featureId,
                title,
                complexityRating = 3
            }
        });

        _lastMutationResult!["createTask"]!["errors"]!.AsArray().Should().BeEmpty();
    }

    [Given("a task with status \"(.*)\" exists")]
    public async Task GivenATaskWithStatusExists(string status)
    {
        _taskId = Guid.NewGuid();

        _lastMutationResult = await _context.SendMutationAsync("CreateTask", new
        {
            input = new
            {
                id = _taskId,
                projectId = _projectId,
                featureId = _featureId,
                title = "Test Task",
                complexityRating = 5,
                initialStatus = status
            }
        });

        _lastMutationResult!["createTask"]!["errors"]!.AsArray().Should().BeEmpty();
    }

    [When("I create a task with title \"(.*)\" and complexity rating (.*)")]
    public async Task WhenICreateTask(string title, int complexityRating)
    {
        _taskId = Guid.NewGuid();

        _lastMutationResult = await _context.SendMutationAsync("CreateTask", new
        {
            input = new
            {
                id = _taskId,
                projectId = _projectId,
                featureId = _featureId,
                title,
                deliverable = "Deliverable description",
                acceptanceCriteria = "Acceptance criteria",
                complexityRating
            }
        });
    }

    [Then("the task should be created successfully")]
    public void ThenTheTaskShouldBeCreatedSuccessfully()
    {
        _lastMutationResult.Should().NotBeNull();
        _lastMutationResult!["createTask"]!["errors"]!.AsArray().Should().BeEmpty();
    }

    [Then("the task should exist in the database")]
    public async Task ThenTheTaskShouldExistInTheDatabase()
    {
        await using var ctx = _context.CreateDbContext();
        var fetched = await ctx.Tasks.FindAsync(_taskId);
        fetched.Should().NotBeNull();
        fetched!.Title.Should().NotBeNull();
    }

    [When("I update the task title to \"(.*)\" and complexity rating to (.*)")]
    public async Task WhenIUpdateTaskTitleTo(string newTitle, int newComplexityRating)
    {
        _lastMutationResult = await _context.SendMutationAsync("UpdateTask", new
        {
            input = new
            {
                id = _taskId,
                title = newTitle,
                complexityRating = newComplexityRating,
                deliverable = "Updated Deliverable"
            }
        });
    }

    [Then("the task should be updated successfully")]
    public void ThenTheTaskShouldBeUpdatedSuccessfully()
    {
        _lastMutationResult.Should().NotBeNull();
        _lastMutationResult!["updateTask"]!["errors"]!.AsArray().Should().BeEmpty();
    }

    [When("I transition the task status to \"(.*)\"")]
    public async Task WhenITransitionTaskStatusTo(string targetStatus)
    {
        _lastMutationResult = await _context.SendMutationAsync("TransitionTaskStatus", new
        {
            input = new
            {
                id = _taskId,
                targetStatus,
                actor = "test-user"
            }
        });
    }

    [Then("the task status should be \"(.*)\"")]
    public async Task ThenTheTaskStatusShouldBe(string expectedStatus)
    {
        _lastMutationResult.Should().NotBeNull();
        _lastMutationResult!["transitionTaskStatus"]!["errors"]!.AsArray().Should().BeEmpty();

        await using var ctx = _context.CreateDbContext();
        var fetched = await ctx.Tasks.FindAsync(_taskId);
        var statusEnum = Enum.Parse<DevStack.Domain.Enums.TaskStatus>(expectedStatus);
        fetched!.Status.Should().Be(statusEnum);
    }

    [When("I delete the task")]
    public async Task WhenIDeleteTheTask()
    {
        _lastMutationResult = await _context.SendMutationAsync("DeleteTask", new
        {
            input = new { id = _taskId }
        });
    }

    [Then("the task should be deleted successfully")]
    public void ThenTheTaskShouldBeDeletedSuccessfully()
    {
        _lastMutationResult.Should().NotBeNull();
        _lastMutationResult!["deleteTask"]!["errors"]!.AsArray().Should().BeEmpty();
    }

    [Then("the task should not exist in the database")]
    public async Task ThenTheTaskShouldNotExistInTheDatabase()
    {
        await using var ctx = _context.CreateDbContext();
        var fetched = await ctx.Tasks.FindAsync(_taskId);
        fetched.Should().BeNull();
    }
}