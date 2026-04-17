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

    [Given("the API is available")]
    public void GivenTheApiIsAvailable() { }

    [Given("a parent project exists")]
    public void GivenAParentProjectExists() { }

    [BeforeScenario]
    public async Task BeforeFeatureScenario()
    {
        _projectId = await _fixture.CreateTestProjectAsync("Test Project");
    }

    [When(@"I create a feature with title ""([^""]*)"" and description ""([^""]*)""")]
    public async Task WhenICreateAFeatureWithTitleAndDescription(string title, string description)
    {
        _createdFeatureId = await _fixture.CreateTestFeatureAsync(_projectId!.Value, title, description);
    }

    [Then("the feature should be created successfully")]
    public void ThenTheFeatureShouldBeCreatedSuccessfully()
    {
        _createdFeatureId.Should().NotBe(Guid.Empty);
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
        _createdFeatureId = await _fixture.CreateTestFeatureAsync(_projectId!.Value, title, "Original description");
    }

    [When(@"I update the feature title to ""([^""]*)""")]
    public async Task WhenIUpdateTheFeatureTitleTo(string newTitle)
    {
        var mutation = new DevStack.Api.GraphQL.Types.Mutation();
        var input = new DevStack.Api.GraphQL.Types.UpdateFeatureInput(_createdFeatureId!.Value, newTitle, "Updated Description", null, null, null, null, null, null, null);
        var handler = new UpdateFeatureHandler(_fixture.CreateDbContext());
        
        var result = await mutation.UpdateFeatureAsync(input, handler, CancellationToken.None);
        _scenarioContext.Add("updateResult", result);
    }

    [Then("the feature should be updated successfully")]
    public async Task ThenTheFeatureShouldBeUpdatedSuccessfully()
    {
        var result = _scenarioContext.Get<DevStack.Api.GraphQL.Types.FeaturePayload>("updateResult");
        result.Errors.Should().BeEmpty();
        
        await using var ctx = _fixture.CreateDbContext();
        var feature = await ctx.Items.FindAsync(_createdFeatureId);
        feature!.Title.Should().Be("Updated Title");
    }

    [Given(@"a feature with status ""([^""]*)"" exists")]
    public async Task GivenAFeatureWithStatusExists(string status)
    {
        var initialStatus = status switch
        {
            "Planning" => FeatureStatus.Planning,
            "InProgress" => FeatureStatus.InProgress,
            "Ready" => FeatureStatus.Ready,
            _ => FeatureStatus.Planning
        };
        
        var mutation = new DevStack.Api.GraphQL.Types.Mutation();
        var input = new DevStack.Api.GraphQL.Types.CreateFeatureInput(_projectId!.Value, "Test Feature", null, null, null, null, null, null, null, null, initialStatus);
        var handler = new CreateFeatureHandler(_fixture.CreateDbContext());
        
        var result = await mutation.CreateFeatureAsync(input, handler, CancellationToken.None);
        result.Errors.Should().BeEmpty();
        _createdFeatureId = result.Item!.Id;
    }

    [When(@"I transition the feature status to ""([^""]*)""")]
    public async Task WhenITransitionTheFeatureStatusTo(string targetStatus)
    {
        var target = targetStatus switch
        {
            "Planning" => FeatureStatus.Planning,
            "InProgress" => FeatureStatus.InProgress,
            "Ready" => FeatureStatus.Ready,
            "ReadyForTest" => FeatureStatus.ReadyForTest,
            "Testing" => FeatureStatus.Testing,
            "Done" => FeatureStatus.Done,
            "Failed" => FeatureStatus.Failed,
            "Rejected" => FeatureStatus.Rejected,
            "InReview" => FeatureStatus.InReview,
            _ => FeatureStatus.Planning
        };
        
        await _fixture.UpdateFeatureStatusAsync(_createdFeatureId!.Value, target, "test-user");
    }

    [Then(@"the feature status should be ""([^""]*)""")]
    public async Task ThenTheFeatureStatusShouldBe(string expectedStatus)
    {
        var expected = expectedStatus switch
        {
            "Planning" => FeatureStatus.Planning,
            "InProgress" => FeatureStatus.InProgress,
            "Ready" => FeatureStatus.Ready,
            "ReadyForTest" => FeatureStatus.ReadyForTest,
            "Testing" => FeatureStatus.Testing,
            "Done" => FeatureStatus.Done,
            "Failed" => FeatureStatus.Failed,
            "Rejected" => FeatureStatus.Rejected,
            "InReview" => FeatureStatus.InReview,
            _ => FeatureStatus.Planning
        };
        
        await using var ctx = _fixture.CreateDbContext();
        var feature = await ctx.Items.FindAsync(_createdFeatureId);
        feature!.Status.Should().Be(expected);
    }

    [Given(@"a feature ""([^""]*)"" exists for deletion")]
    public async Task GivenAFeatureExistsForDeletion(string title)
    {
        _createdFeatureId = await _fixture.CreateTestFeatureAsync(_projectId!.Value, title);
    }

    [When("I delete the feature")]
    public async Task WhenIDeleteTheFeature()
    {
        var mutation = new DevStack.Api.GraphQL.Types.Mutation();
        var input = new DevStack.Api.GraphQL.Types.DeleteFeatureInput(_createdFeatureId!.Value);
        var handler = new DeleteFeatureHandler(_fixture.CreateDbContext());
        
        var result = await mutation.DeleteFeatureAsync(input, handler, CancellationToken.None);
        _scenarioContext.Add("deleteResult", result);
    }

    [Then("the feature should be deleted successfully")]
    public void ThenTheFeatureShouldBeDeletedSuccessfully()
    {
        var result = _scenarioContext.Get<DevStack.Api.GraphQL.Types.FeaturePayload>("deleteResult");
        result.Errors.Should().BeEmpty();
    }

    [Then("the feature should not exist in the database")]
    public async Task ThenTheFeatureShouldNotExistInTheDatabase()
    {
        await using var ctx = _fixture.CreateDbContext();
        var feature = await ctx.Items.FindAsync(_createdFeatureId);
        feature.Should().BeNull();
    }
}