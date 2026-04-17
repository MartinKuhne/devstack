using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Infrastructure.Persistence;
using DevStack.Infrastructure.Projects;
using DevStack.Infrastructure.Features;
using DevStack.Infrastructure.Defects;
using DevStack.Infrastructure.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.GraphQL.Client.Features;

[Binding]
public class TaskMutationsSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly IntegrationTestFixture _fixture;
    private Guid? _projectId;
    private Guid? _featureId;
    private Guid? _createdTaskId;

    public TaskMutationsSteps(ScenarioContext scenarioContext, IntegrationTestFixture fixture)
    {
        _scenarioContext = scenarioContext;
        _fixture = fixture;
    }

    [Given("the API is available")]
    public void GivenTheApiIsAvailable() { }

    [Given("a parent project exists")]
    public void GivenAParentProjectExists() { }

    [Given("a parent feature exists")]
    public void GivenAParentFeatureExists() { }

    [BeforeScenario]
    public async Task BeforeTaskScenario()
    {
        _projectId = await _fixture.CreateTestProjectAsync("Test Project");
        _featureId = await _fixture.CreateTestFeatureAsync(_projectId.Value, "Parent Feature");
    }

    [When(@"I create a task with title ""([^""]*)"" and complexity rating (\d+)")]
    public async Task WhenICreateATaskWithTitleAndComplexityRating(string title, int complexityRating)
    {
        _createdTaskId = await _fixture.CreateTestTaskAsync(_projectId!.Value, _featureId!.Value, title, complexityRating);
    }

    [Then("the task should be created successfully")]
    public void ThenTheTaskShouldBeCreatedSuccessfully()
    {
        _createdTaskId.Should().NotBe(Guid.Empty);
    }

    [Then("the task should exist in the database")]
    public async Task ThenTheTaskShouldExistInTheDatabase()
    {
        await using var ctx = _fixture.CreateDbContext();
        var task = await ctx.Tasks.FindAsync(_createdTaskId);
        task.Should().NotBeNull();
    }

    [Given(@"a task ""([^""]*)"" exists")]
    public async Task GivenATaskExists(string title)
    {
        _createdTaskId = await _fixture.CreateTestTaskAsync(_projectId!.Value, _featureId!.Value, title, 5);
    }

    [When(@"I update the task title to ""([^""]*)"" and complexity rating to (\d+)")]
    public async Task WhenIUpdateTheTaskTitleToAndComplexityRatingTo(string newTitle, int newComplexity)
    {
        var mutation = new DevStack.Api.GraphQL.Types.Mutation();
        var input = new DevStack.Api.GraphQL.Types.UpdateTaskInput(_createdTaskId!.Value, newTitle, "Updated Deliverable", null, null, null, null, newComplexity);
        var handler = new UpdateTaskHandler(_fixture.CreateDbContext());
        
        var result = await mutation.UpdateTaskAsync(input, handler, CancellationToken.None);
        _scenarioContext.Add("updateResult", result);
    }

    [Then("the task should be updated successfully")]
    public async Task ThenTheTaskShouldBeUpdatedSuccessfully()
    {
        var result = _scenarioContext.Get<DevStack.Api.GraphQL.Types.TaskPayload>("updateResult");
        result.Errors.Should().BeEmpty();
        
        await using var ctx = _fixture.CreateDbContext();
        var task = await ctx.Tasks.FindAsync(_createdTaskId);
        task!.Title.Should().Be("Updated Title");
        task.ComplexityRating.Should().Be(7);
    }

    [Given(@"a task with status ""([^""]*)"" exists")]
    public async Task GivenATaskWithStatusExists(string status)
    {
        _createdTaskId = await _fixture.CreateTestTaskAsync(_projectId!.Value, _featureId!.Value, "Test Task", 5);
    }

    [When(@"I transition the task status to ""([^""]*)""")]
    public async Task WhenITransitionTheTaskStatusTo(string targetStatus)
    {
        var target = targetStatus switch
        {
            "Planning" => Domain.Enums.TaskStatus.Planning,
            "Ready" => Domain.Enums.TaskStatus.Ready,
            "Prepare" => Domain.Enums.TaskStatus.Prepare,
            "Code" => Domain.Enums.TaskStatus.Code,
            "Review" => Domain.Enums.TaskStatus.Review,
            "ReadyForTest" => Domain.Enums.TaskStatus.ReadyForTest,
            "Testing" => Domain.Enums.TaskStatus.Testing,
            "Done" => Domain.Enums.TaskStatus.Done,
            "Failed" => Domain.Enums.TaskStatus.Failed,
            "Rejected" => Domain.Enums.TaskStatus.Rejected,
            "InReview" => Domain.Enums.TaskStatus.InReview,
            "Todo" => Domain.Enums.TaskStatus.Planning,
            _ => Domain.Enums.TaskStatus.Planning
        };
        
        await _fixture.UpdateTaskStatusAsync(_createdTaskId!.Value, target, "test-user");
    }

    [Then(@"the task status should be ""([^""]*)""")]
    public async Task ThenTheTaskStatusShouldBe(string expectedStatus)
    {
        var expected = expectedStatus switch
        {
            "Planning" => Domain.Enums.TaskStatus.Planning,
            "Ready" => Domain.Enums.TaskStatus.Ready,
            "Prepare" => Domain.Enums.TaskStatus.Prepare,
            "Code" => Domain.Enums.TaskStatus.Code,
            "Review" => Domain.Enums.TaskStatus.Review,
            "ReadyForTest" => Domain.Enums.TaskStatus.ReadyForTest,
            "Testing" => Domain.Enums.TaskStatus.Testing,
            "Done" => Domain.Enums.TaskStatus.Done,
            "Failed" => Domain.Enums.TaskStatus.Failed,
            "Rejected" => Domain.Enums.TaskStatus.Rejected,
            "InReview" => Domain.Enums.TaskStatus.InReview,
            _ => Domain.Enums.TaskStatus.Planning
        };
        
        await using var ctx = _fixture.CreateDbContext();
        var task = await ctx.Tasks.FindAsync(_createdTaskId);
        task!.Status.Should().Be(expected);
    }

    [Given(@"a task ""([^""]*)"" exists for deletion")]
    public async Task GivenATaskExistsForDeletion(string title)
    {
        _createdTaskId = await _fixture.CreateTestTaskAsync(_projectId!.Value, _featureId!.Value, title, 3);
    }

    [When("I delete the task")]
    public async Task WhenIDeleteTheTask()
    {
        var mutation = new DevStack.Api.GraphQL.Types.Mutation();
        var input = new DevStack.Api.GraphQL.Types.DeleteTaskInput(_createdTaskId!.Value);
        var handler = new DeleteTaskHandler(_fixture.CreateDbContext());
        
        var result = await mutation.DeleteTaskAsync(input, handler, CancellationToken.None);
        _scenarioContext.Add("deleteResult", result);
    }

    [Then("the task should be deleted successfully")]
    public void ThenTheTaskShouldBeDeletedSuccessfully()
    {
        var result = _scenarioContext.Get<DevStack.Api.GraphQL.Types.TaskPayload>("deleteResult");
        result.Errors.Should().BeEmpty();
    }

    [Then("the task should not exist in the database")]
    public async Task ThenTheTaskShouldNotExistInTheDatabase()
    {
        await using var ctx = _fixture.CreateDbContext();
        var task = await ctx.Tasks.FindAsync(_createdTaskId);
        task.Should().BeNull();
    }
}