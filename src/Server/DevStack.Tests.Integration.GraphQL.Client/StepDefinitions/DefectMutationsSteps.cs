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
public class DefectMutationsSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly IntegrationTestFixture _fixture;
    private Guid? _projectId;
    private Guid? _createdDefectId;

    public DefectMutationsSteps(ScenarioContext scenarioContext, IntegrationTestFixture fixture)
    {
        _scenarioContext = scenarioContext;
        _fixture = fixture;
    }

    [BeforeScenario]
    public async Task BeforeDefectScenario()
    {
        _projectId = await _fixture.CreateTestProjectAsync("Test Project");
    }

    [Given("the API is available")]
    public void GivenTheApiIsAvailable()
    {
    }

    [Given("a parent project exists")]
    public void GivenAParentProjectExists()
    {
    }

    [When(@"I create a defect with title ""([^""]*)"" and severity ""([^""]*)""")]
    public async Task WhenICreateADefectWithTitleAndSeverity(string title, string severity)
    {
        var data = await SendMutationAsync("""
            mutation CreateDefect($input: CreateDefectInput!) {
              createDefect(input: $input) {
                item { id }
                errors
              }
            }
            """,
            new { input = new { ProjectId = _projectId, Title = title, Severity = severity } });
        
        _scenarioContext.Add("result", data);
        _createdDefectId = Guid.Parse(data!["createDefect"]!["item"]!["id"]!.GetValue<string>());
    }

    [Then("the defect should be created successfully")]
    public void ThenTheDefectShouldBeCreatedSuccessfully()
    {
        var data = _scenarioContext.Get<JsonNode>("result");
        data!["createDefect"]!["errors"]!.AsArray().Should().BeEmpty();
    }

    [Then("the defect should exist in the database")]
    public async Task ThenTheDefectShouldExistInTheDatabase()
    {
        await using var ctx = _fixture.CreateDbContext();
        var defect = await ctx.Items.FindAsync(_createdDefectId);
        defect.Should().NotBeNull();
        defect!.Subtype.Should().Be(ItemSubtype.Defect);
    }

    [Given(@"a defect ""([^""]*)"" exists")]
    public async Task GivenADefectExists(string title)
    {
        var data = await SendMutationAsync("""
            mutation CreateDefect($input: CreateDefectInput!) {
              createDefect(input: $input) {
                item { id }
                errors
              }
            }
            """,
            new { input = new { ProjectId = _projectId, Title = title, Description = "Original description", Severity = "Low" } });
        
        _createdDefectId = Guid.Parse(data!["createDefect"]!["item"]!["id"]!.GetValue<string>());
    }

    [When(@"I update the defect title to ""([^""]*)""")]
    public async Task WhenIUpdateTheDefectTitleTo(string newTitle)
    {
        var data = await SendMutationAsync("""
            mutation UpdateDefect($input: UpdateDefectInput!) {
              updateDefect(input: $input) {
                item { id title }
                errors
              }
            }
            """,
            new { input = new { Id = _createdDefectId, Title = newTitle, Description = "Updated Description" } });
        
        _scenarioContext.Add("updateResult", data);
    }

    [Then("the defect should be updated successfully")]
    public async Task ThenTheDefectShouldBeUpdatedSuccessfully()
    {
        var data = _scenarioContext.Get<JsonNode>("updateResult");
        data!["updateDefect"]!["errors"]!.AsArray().Should().BeEmpty();
        
        await using var ctx = _fixture.CreateDbContext();
        var defect = await ctx.Items.FindAsync(_createdDefectId);
        defect!.Title.Should().Be("Updated Title");
    }

    [Given(@"a defect with status ""([^""]*)"" exists")]
    public async Task GivenADefectWithStatusExists(string status)
    {
        var data = await SendMutationAsync("""
            mutation CreateDefect($input: CreateDefectInput!) {
              createDefect(input: $input) {
                item { id }
                errors
              }
            }
            """,
            new { input = new { ProjectId = _projectId, Title = "Test Defect", Severity = "Low", InitialStatus = status } });
        
        _createdDefectId = Guid.Parse(data!["createDefect"]!["item"]!["id"]!.GetValue<string>());
    }

    [When(@"I transition the defect status to ""([^""]*)""")]
    public async Task WhenITransitionTheDefectStatusTo(string targetStatus)
    {
        var data = await SendMutationAsync("""
            mutation TransitionDefectStatus($input: TransitionDefectInput!) {
              transitionDefectStatus(input: $input) {
                item { id status }
                errors
              }
            }
            """,
            new { input = new { Id = _createdDefectId, TargetStatus = targetStatus, Actor = "test-user" } });
        
        _scenarioContext.Add("transitionResult", data);
    }

    [Then(@"the defect status should be ""([^""]*)""")]
    public async Task ThenTheDefectStatusShouldBe(string expectedStatus)
    {
        var data = _scenarioContext.Get<JsonNode>("transitionResult");
        data!["transitionDefectStatus"]!["errors"]!.AsArray().Should().BeEmpty();
        
        await using var ctx = _fixture.CreateDbContext();
        var defect = await ctx.Items.FindAsync(_createdDefectId);
        var expected = expectedStatus switch
        {
            "Planning" => Domain.Enums.FeatureStatus.Planning,
            "InProgress" => Domain.Enums.FeatureStatus.InProgress,
            "Ready" => Domain.Enums.FeatureStatus.Ready,
            "ReadyForTest" => Domain.Enums.FeatureStatus.ReadyForTest,
            "Testing" => Domain.Enums.FeatureStatus.Testing,
            "Done" => Domain.Enums.FeatureStatus.Done,
            "Failed" => Domain.Enums.FeatureStatus.Failed,
            "Rejected" => Domain.Enums.FeatureStatus.Rejected,
            "InReview" => Domain.Enums.FeatureStatus.InReview,
            _ => Domain.Enums.FeatureStatus.Planning
        };
        defect!.Status.Should().Be(expected);
    }

    [Given(@"a defect ""([^""]*)"" exists for deletion")]
    public async Task GivenADefectExistsForDeletion(string title)
    {
        var data = await SendMutationAsync("""
            mutation CreateDefect($input: CreateDefectInput!) {
              createDefect(input: $input) {
                item { id }
                errors
              }
            }
            """,
            new { input = new { ProjectId = _projectId, Title = title, Severity = "Low" } });
        
        _createdDefectId = Guid.Parse(data!["createDefect"]!["item"]!["id"]!.GetValue<string>());
    }

    [When("I delete the defect")]
    public async Task WhenIDeleteTheDefect()
    {
        var data = await SendMutationAsync("""
            mutation DeleteDefect($input: DeleteDefectInput!) {
              deleteDefect(input: $input) {
                item { id }
                errors
              }
            }
            """,
            new { input = new { Id = _createdDefectId } });
        
        _scenarioContext.Add("deleteResult", data);
    }

    [Then("the defect should be deleted successfully")]
    public void ThenTheDefectShouldBeDeletedSuccessfully()
    {
        var data = _scenarioContext.Get<JsonNode>("deleteResult");
        data!["deleteDefect"]!["errors"]!.AsArray().Should().BeEmpty();
    }

    [Then("the defect should not exist in the database")]
    public async Task ThenTheDefectShouldNotExistInTheDatabase()
    {
        await using var ctx = _fixture.CreateDbContext();
        var defect = await ctx.Items.FindAsync(_createdDefectId);
        defect.Should().BeNull();
    }

    private async Task<JsonNode?> SendMutationAsync(string query, object? variables = null)
    {
        var response = await _fixture.HttpClient.PostAsJsonAsync("_fixture.GraphQlUrl", new { query, variables });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonNode.Parse(json)?["data"];
    }
}
