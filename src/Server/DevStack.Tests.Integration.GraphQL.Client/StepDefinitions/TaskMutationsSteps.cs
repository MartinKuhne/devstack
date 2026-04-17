using System.Net.Http.Json;
using System.Text.Json.Nodes;
using DevStack.Infrastructure.Persistence;
using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
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

    [BeforeScenario]
    public async Task BeforeTaskScenario()
    {
        _projectId = await _fixture.CreateTestProjectAsync("Test Project");
        _featureId = await _fixture.CreateTestFeatureAsync(_projectId.Value, "Parent Feature");
    }

    [Given("a parent feature exists")]
    public void GivenAParentFeatureExists()
    {
        // No-op: Feature setup handled in BeforeScenario
    }

    [When(@"I create a task with title ""([^""]*)"" and complexity rating (\d+)")]
    public async Task WhenICreateATaskWithTitleAndComplexityRating(string title, int complexityRating)
    {
        var data = await SendMutationAsync("""
            mutation CreateTask($input: CreateTaskInput!) {
              createTask(input: $input) {
                task { id }
                errors
              }
            }
            """,
            new { input = new { ProjectId = _projectId, ItemId = _featureId, Title = title, ComplexityRating = complexityRating } });
        
        _scenarioContext.Add("result", data);
        _createdTaskId = Guid.Parse(data!["createTask"]!["task"]!["id"]!.GetValue<string>());
    }

    [Then("the task should be created successfully")]
    public void ThenTheTaskShouldBeCreatedSuccessfully()
    {
        var data = _scenarioContext.Get<JsonNode>("result");
        data!["createTask"]!["errors"]!.AsArray().Should().BeEmpty();
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
        var data = await SendMutationAsync("""
            mutation CreateTask($input: CreateTaskInput!) {
              createTask(input: $input) {
                task { id }
                errors
              }
            }
            """,
            new { input = new { ProjectId = _projectId, ItemId = _featureId, Title = title, ComplexityRating = 5 } });
        
        _createdTaskId = Guid.Parse(data!["createTask"]!["task"]!["id"]!.GetValue<string>());
    }

    [When(@"I update the task title to ""([^""]*)"" and complexity rating to (\d+)")]
    public async Task WhenIUpdateTheTaskTitleToAndComplexityRatingTo(string newTitle, int newComplexity)
    {
        var data = await SendMutationAsync("""
            mutation UpdateTask($input: UpdateTaskInput!) {
              updateTask(input: $input) {
                task { id title }
                errors
              }
            }
            """,
            new { input = new { Id = _createdTaskId, Title = newTitle, ComplexityRating = newComplexity, Deliverable = "Updated Deliverable" } });
        
        _scenarioContext.Add("updateResult", data);
    }

    [Then("the task should be updated successfully")]
    public async Task ThenTheTaskShouldBeUpdatedSuccessfully()
    {
        var data = _scenarioContext.Get<JsonNode>("updateResult");
        data!["updateTask"]!["errors"]!.AsArray().Should().BeEmpty();
        
        await using var ctx = _fixture.CreateDbContext();
        var task = await ctx.Tasks.FindAsync(_createdTaskId);
        task!.Title.Should().Be("Updated Title");
        task.ComplexityRating.Should().Be(7);
    }

    [Given(@"a task with status ""([^""]*)"" exists")]
    public async Task GivenATaskWithStatusExists(string status)
    {
        var data = await SendMutationAsync("""
            mutation CreateTask($input: CreateTaskInput!) {
              createTask(input: $input) {
                task { id }
                errors
              }
            }
            """,
            new { input = new { ProjectId = _projectId, ItemId = _featureId, Title = "Test Task", ComplexityRating = 5 } });
        
        _createdTaskId = Guid.Parse(data!["createTask"]!["task"]!["id"]!.GetValue<string>());
    }

    [When(@"I transition the task status to ""([^""]*)""")]
    public async Task WhenITransitionTheTaskStatusTo(string targetStatus)
    {
        var data = await SendMutationAsync("""
            mutation TransitionTaskStatus($input: TransitionTaskInput!) {
              transitionTaskStatus(input: $input) {
                task { id status }
                errors
              }
            }
            """,
            new { input = new { Id = _createdTaskId, TargetStatus = targetStatus, Actor = "test-user" } });
        
        _scenarioContext.Add("transitionResult", data);
    }

    [Then(@"the task status should be ""([^""]*)""")]
    public async Task ThenTheTaskStatusShouldBe(string expectedStatus)
    {
        var data = _scenarioContext.Get<JsonNode>("transitionResult");
        data!["transitionTaskStatus"]!["errors"]!.AsArray().Should().BeEmpty();
        
        await using var ctx = _fixture.CreateDbContext();
        var task = await ctx.Tasks.FindAsync(_createdTaskId);
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
            "Todo" => Domain.Enums.TaskStatus.Planning,
            _ => Domain.Enums.TaskStatus.Planning
        };
        task!.Status.Should().Be(expected);
    }

    [Given(@"a task ""([^""]*)"" exists for deletion")]
    public async Task GivenATaskExistsForDeletion(string title)
    {
        var data = await SendMutationAsync("""
            mutation CreateTask($input: CreateTaskInput!) {
              createTask(input: $input) {
                task { id }
                errors
              }
            }
            """,
            new { input = new { ProjectId = _projectId, ItemId = _featureId, Title = title, ComplexityRating = 3 } });
        
        _createdTaskId = Guid.Parse(data!["createTask"]!["task"]!["id"]!.GetValue<string>());
    }

    [When("I delete the task")]
    public async Task WhenIDeleteTheTask()
    {
        var data = await SendMutationAsync("""
            mutation DeleteTask($input: DeleteTaskInput!) {
              deleteTask(input: $input) {
                task { id }
                errors
              }
            }
            """,
            new { input = new { Id = _createdTaskId } });
        
        _scenarioContext.Add("deleteResult", data);
    }

    [Then("the task should be deleted successfully")]
    public void ThenTheTaskShouldBeDeletedSuccessfully()
    {
        var data = _scenarioContext.Get<JsonNode>("deleteResult");
        data!["deleteTask"]!["errors"]!.AsArray().Should().BeEmpty();
    }

    [Then("the task should not exist in the database")]
    public async Task ThenTheTaskShouldNotExistInTheDatabase()
    {
        await using var ctx = _fixture.CreateDbContext();
        var task = await ctx.Tasks.FindAsync(_createdTaskId);
        task.Should().BeNull();
    }

    private async Task<JsonNode?> SendMutationAsync(string query, object? variables = null)
    {
        var response = await _fixture.HttpClient.PostAsJsonAsync("_fixture.GraphQlUrl", new { query, variables });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonNode.Parse(json)?["data"];
    }
}
