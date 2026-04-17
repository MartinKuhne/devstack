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
public class FeatureMutationsSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly IntegrationTestFixture _fixture;
    private Guid? _projectId;
    private Guid? _createdFeatureId;

    public FeatureMutationsSteps(ScenarioContext scenarioContext, IntegrationTestFixture fixture)
    {
        _scenarioContext = scenarioContext;
        _fixture = fixture;
    }

    [BeforeScenario]
    public async Task BeforeFeatureScenario()
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

    [When(@"I create a feature with title ""([^""]*)"" and description ""([^""]*)""")]
    public async Task WhenICreateAFeatureWithTitleAndDescription(string title, string description)
    {
        var data = await SendMutationAsync("""
            mutation CreateFeature($input: CreateFeatureInput!) {
              createFeature(input: $input) {
                item { id }
                errors
              }
            }
            """,
            new { input = new { ProjectId = _projectId, Title = title, Description = description } });
        
        _scenarioContext.Add("result", data);
        _createdFeatureId = Guid.Parse(data!["createFeature"]!["item"]!["id"]!.GetValue<string>());
    }

    [Then("the feature should be created successfully")]
    public void ThenTheFeatureShouldBeCreatedSuccessfully()
    {
        var data = _scenarioContext.Get<JsonNode>("result");
        data!["createFeature"]!["errors"]!.AsArray().Should().BeEmpty();
    }

    [Then("the feature should exist in the database")]
    public async Task ThenTheFeatureShouldExistInTheDatabase()
    {
        await using var ctx = _fixture.CreateDbContext();
        var feature = await ctx.Items.FindAsync(_createdFeatureId);
        feature.Should().NotBeNull();
        feature!.Subtype.Should().Be(ItemSubtype.Feature);
    }

    [Given(@"a feature ""([^""]*)"" exists")]
    public async Task GivenAFeatureExists(string title)
    {
        var data = await SendMutationAsync("""
            mutation CreateFeature($input: CreateFeatureInput!) {
              createFeature(input: $input) {
                item { id }
                errors
              }
            }
            """,
            new { input = new { ProjectId = _projectId, Title = title, Description = "Original description" } });
        
        _createdFeatureId = Guid.Parse(data!["createFeature"]!["item"]!["id"]!.GetValue<string>());
    }

    [When(@"I update the feature title to ""([^""]*)""")]
    public async Task WhenIUpdateTheFeatureTitleTo(string newTitle)
    {
        var data = await SendMutationAsync("""
            mutation UpdateFeature($input: UpdateFeatureInput!) {
              updateFeature(input: $input) {
                item { id title }
                errors
              }
            }
            """,
            new { input = new { Id = _createdFeatureId, Title = newTitle, Description = "Updated Description" } });
        
        _scenarioContext.Add("updateResult", data);
    }

    [Then("the feature should be updated successfully")]
    public async Task ThenTheFeatureShouldBeUpdatedSuccessfully()
    {
        var data = _scenarioContext.Get<JsonNode>("updateResult");
        data!["updateFeature"]!["errors"]!.AsArray().Should().BeEmpty();
        
        await using var ctx = _fixture.CreateDbContext();
        var feature = await ctx.Items.FindAsync(_createdFeatureId);
        feature!.Title.Should().Be("Updated Title");
    }

    [Given(@"a feature with status ""([^""]*)"" exists")]
    public async Task GivenAFeatureWithStatusExists(string status)
    {
        var data = await SendMutationAsync("""
            mutation CreateFeature($input: CreateFeatureInput!) {
              createFeature(input: $input) {
                item { id }
                errors
              }
            }
            """,
            new { input = new { ProjectId = _projectId, Title = "Test Feature", InitialStatus = status } });
        
        _createdFeatureId = Guid.Parse(data!["createFeature"]!["item"]!["id"]!.GetValue<string>());
    }

    [When(@"I transition the feature status to ""([^""]*)""")]
    public async Task WhenITransitionTheFeatureStatusTo(string targetStatus)
    {
        var data = await SendMutationAsync("""
            mutation TransitionFeatureStatus($input: TransitionFeatureInput!) {
              transitionFeatureStatus(input: $input) {
                item { id status }
                errors
              }
            }
            """,
            new { input = new { Id = _createdFeatureId, TargetStatus = targetStatus, Actor = "test-user" } });
        
        _scenarioContext.Add("transitionResult", data);
    }

    [Then(@"the feature status should be ""([^""]*)""")]
    public async Task ThenTheFeatureStatusShouldBe(string expectedStatus)
    {
        var data = _scenarioContext.Get<JsonNode>("transitionResult");
        data!["transitionFeatureStatus"]!["errors"]!.AsArray().Should().BeEmpty();
        
        await using var ctx = _fixture.CreateDbContext();
        var feature = await ctx.Items.FindAsync(_createdFeatureId);
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
        feature!.Status.Should().Be(expected);
    }

    [Given(@"a feature ""([^""]*)"" exists for deletion")]
    public async Task GivenAFeatureExistsForDeletion(string title)
    {
        var data = await SendMutationAsync("""
            mutation CreateFeature($input: CreateFeatureInput!) {
              createFeature(input: $input) {
                item { id }
                errors
              }
            }
            """,
            new { input = new { ProjectId = _projectId, Title = title } });
        
        _createdFeatureId = Guid.Parse(data!["createFeature"]!["item"]!["id"]!.GetValue<string>());
    }

    [When("I delete the feature")]
    public async Task WhenIDeleteTheFeature()
    {
        var data = await SendMutationAsync("""
            mutation DeleteFeature($input: DeleteFeatureInput!) {
              deleteFeature(input: $input) {
                item { id }
                errors
              }
            }
            """,
            new { input = new { Id = _createdFeatureId } });
        
        _scenarioContext.Add("deleteResult", data);
    }

    [Then("the feature should be deleted successfully")]
    public void ThenTheFeatureShouldBeDeletedSuccessfully()
    {
        var data = _scenarioContext.Get<JsonNode>("deleteResult");
        data!["deleteFeature"]!["errors"]!.AsArray().Should().BeEmpty();
    }

    [Then("the feature should not exist in the database")]
    public async Task ThenTheFeatureShouldNotExistInTheDatabase()
    {
        await using var ctx = _fixture.CreateDbContext();
        var feature = await ctx.Items.FindAsync(_createdFeatureId);
        feature.Should().BeNull();
    }

    private async Task<JsonNode?> SendMutationAsync(string query, object? variables = null)
    {
        var response = await _fixture.HttpClient.PostAsJsonAsync("_fixture.GraphQlUrl", new { query, variables });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonNode.Parse(json)?["data"];
    }
}
