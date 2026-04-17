using System.Text.Json.Nodes;
using DevStack.Domain.Enums;
using DevStack.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.GraphQL.Client.Steps;

[Binding]
public class FeatureStepDefinitions
{
    private readonly GraphQLContext _context;
    private Guid _projectId;
    private Guid _featureId;
    private JsonNode? _lastMutationResult;

    public FeatureStepDefinitions(GraphQLContext context)
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

    [Given("a feature \"(.*)\" exists")]
    public async Task GivenAFeatureExists(string title)
    {
        _featureId = Guid.NewGuid();

        _lastMutationResult = await _context.SendMutationAsync("CreateFeature", new
        {
            input = new
            {
                id = _featureId,
                projectId = _projectId,
                title,
                description = "Test description"
            }
        });

        _lastMutationResult!["createFeature"]!["errors"]!.AsArray().Should().BeEmpty();
    }

    [Given("a feature with status \"(.*)\" exists")]
    public async Task GivenAFeatureWithStatusExists(string status)
    {
        _featureId = Guid.NewGuid();

        _lastMutationResult = await _context.SendMutationAsync("CreateFeature", new
        {
            input = new
            {
                id = _featureId,
                projectId = _projectId,
                title = "Test Feature",
                initialStatus = status
            }
        });

        _lastMutationResult!["createFeature"]!["errors"]!.AsArray().Should().BeEmpty();
    }

    [When("I create a feature with title \"(.*)\" and description \"(.*)\"")]
    public async Task WhenICreateFeature(string title, string description)
    {
        _featureId = Guid.NewGuid();

        _lastMutationResult = await _context.SendMutationAsync("CreateFeature", new
        {
            input = new
            {
                id = _featureId,
                projectId = _projectId,
                title,
                description,
                acceptanceCriteria = "Acceptance criteria",
                initialStatus = "Planning"
            }
        });
    }

    [Then("the feature should be created successfully")]
    public void ThenTheFeatureShouldBeCreatedSuccessfully()
    {
        _lastMutationResult.Should().NotBeNull();
        _lastMutationResult!["createFeature"]!["errors"]!.AsArray().Should().BeEmpty();
    }

    [Then("the feature should exist in the database")]
    public async Task ThenTheFeatureShouldExistInTheDatabase()
    {
        await using var ctx = _context.CreateDbContext();
        var fetched = await ctx.Items.FindAsync(_featureId);
        fetched.Should().NotBeNull();
        fetched!.Title.Should().NotBeNull();
    }

    [When("I update the feature title to \"(.*)\"")]
    public async Task WhenIUpdateFeatureTitleTo(string newTitle)
    {
        _lastMutationResult = await _context.SendMutationAsync("UpdateFeature", new
        {
            input = new
            {
                id = _featureId,
                title = newTitle,
                description = "Updated Description"
            }
        });
    }

    [Then("the feature should be updated successfully")]
    public void ThenTheFeatureShouldBeUpdatedSuccessfully()
    {
        _lastMutationResult.Should().NotBeNull();
        _lastMutationResult!["updateFeature"]!["errors"]!.AsArray().Should().BeEmpty();
    }

    [When("I transition the feature status to \"(.*)\"")]
    public async Task WhenITransitionFeatureStatusTo(string targetStatus)
    {
        _lastMutationResult = await _context.SendMutationAsync("TransitionFeatureStatus", new
        {
            input = new
            {
                id = _featureId,
                targetStatus,
                actor = "test-user"
            }
        });
    }

    [Then("the feature status should be \"(.*)\"")]
    public async Task ThenTheFeatureStatusShouldBe(string expectedStatus)
    {
        _lastMutationResult.Should().NotBeNull();
        _lastMutationResult!["transitionFeatureStatus"]!["errors"]!.AsArray().Should().BeEmpty();

        await using var ctx = _context.CreateDbContext();
        var fetched = await ctx.Items.FindAsync(_featureId);
        var statusEnum = Enum.Parse<FeatureStatus>(expectedStatus);
        fetched!.Status.Should().Be(statusEnum);
    }

    [When("I delete the feature")]
    public async Task WhenIDeleteTheFeature()
    {
        _lastMutationResult = await _context.SendMutationAsync("DeleteFeature", new
        {
            input = new { id = _featureId }
        });
    }

    [Then("the feature should be deleted successfully")]
    public void ThenTheFeatureShouldBeDeletedSuccessfully()
    {
        _lastMutationResult.Should().NotBeNull();
        _lastMutationResult!["deleteFeature"]!["errors"]!.AsArray().Should().BeEmpty();
    }

    [Then("the feature should not exist in the database")]
    public async Task ThenTheFeatureShouldNotExistInTheDatabase()
    {
        await using var ctx = _context.CreateDbContext();
        var fetched = await ctx.Items.FindAsync(_featureId);
        fetched.Should().BeNull();
    }
}