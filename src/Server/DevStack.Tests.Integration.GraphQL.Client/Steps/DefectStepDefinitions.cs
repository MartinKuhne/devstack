using System.Text.Json.Nodes;
using DevStack.Domain.Enums;
using DevStack.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.GraphQL.Client.Steps;

[Binding]
public class DefectStepDefinitions
{
    private readonly GraphQLContext _context;
    private Guid _projectId;
    private Guid _defectId;
    private JsonNode? _lastMutationResult;

    public DefectStepDefinitions(GraphQLContext context)
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

    [Given("a defect \"(.*)\" exists")]
    public async Task GivenADefectExists(string title)
    {
        _defectId = Guid.NewGuid();

        _lastMutationResult = await _context.SendMutationAsync("CreateDefect", new
        {
            input = new
            {
                id = _defectId,
                projectId = _projectId,
                title,
                severity = "Low"
            }
        });

        _lastMutationResult!["createDefect"]!["errors"]!.AsArray().Should().BeEmpty();
    }

    [Given("a defect with status \"(.*)\" exists")]
    public async Task GivenADefectWithStatusExists(string status)
    {
        _defectId = Guid.NewGuid();

        _lastMutationResult = await _context.SendMutationAsync("CreateDefect", new
        {
            input = new
            {
                id = _defectId,
                projectId = _projectId,
                title = "Test Defect",
                severity = "Medium",
                initialStatus = status
            }
        });

        _lastMutationResult!["createDefect"]!["errors"]!.AsArray().Should().BeEmpty();
    }

    [When("I create a defect with title \"(.*)\" and severity \"(.*)\"")]
    public async Task WhenICreateDefect(string title, string severity)
    {
        _defectId = Guid.NewGuid();

        _lastMutationResult = await _context.SendMutationAsync("CreateDefect", new
        {
            input = new
            {
                id = _defectId,
                projectId = _projectId,
                title,
                severity,
                description = "Defect description",
                initialStatus = "Planning"
            }
        });
    }

    [Then("the defect should be created successfully")]
    public void ThenTheDefectShouldBeCreatedSuccessfully()
    {
        _lastMutationResult.Should().NotBeNull();
        _lastMutationResult!["createDefect"]!["errors"]!.AsArray().Should().BeEmpty();
    }

    [Then("the defect should exist in the database")]
    public async Task ThenTheDefectShouldExistInTheDatabase()
    {
        await using var ctx = _context.CreateDbContext();
        var fetched = await ctx.Items.FindAsync(_defectId);
        fetched.Should().NotBeNull();
        fetched!.Title.Should().NotBeNull();
    }

    [When("I update the defect title to \"(.*)\"")]
    public async Task WhenIUpdateDefectTitleTo(string newTitle)
    {
        _lastMutationResult = await _context.SendMutationAsync("UpdateDefect", new
        {
            input = new
            {
                id = _defectId,
                title = newTitle,
                description = "Updated Description"
            }
        });
    }

    [Then("the defect should be updated successfully")]
    public void ThenTheDefectShouldBeUpdatedSuccessfully()
    {
        _lastMutationResult.Should().NotBeNull();
        _lastMutationResult!["updateDefect"]!["errors"]!.AsArray().Should().BeEmpty();
    }

    [When("I transition the defect status to \"(.*)\"")]
    public async Task WhenITransitionDefectStatusTo(string targetStatus)
    {
        _lastMutationResult = await _context.SendMutationAsync("TransitionDefectStatus", new
        {
            input = new
            {
                id = _defectId,
                targetStatus,
                actor = "test-user"
            }
        });
    }

    [Then("the defect status should be \"(.*)\"")]
    public async Task ThenTheDefectStatusShouldBe(string expectedStatus)
    {
        _lastMutationResult.Should().NotBeNull();
        _lastMutationResult!["transitionDefectStatus"]!["errors"]!.AsArray().Should().BeEmpty();

        await using var ctx = _context.CreateDbContext();
        var fetched = await ctx.Items.FindAsync(_defectId);
        var statusEnum = Enum.Parse<FeatureStatus>(expectedStatus);
        fetched!.Status.Should().Be(statusEnum);
    }

    [When("I delete the defect")]
    public async Task WhenIDeleteTheDefect()
    {
        _lastMutationResult = await _context.SendMutationAsync("DeleteDefect", new
        {
            input = new { id = _defectId }
        });
    }

    [Then("the defect should be deleted successfully")]
    public void ThenTheDefectShouldBeDeletedSuccessfully()
    {
        _lastMutationResult.Should().NotBeNull();
        _lastMutationResult!["deleteDefect"]!["errors"]!.AsArray().Should().BeEmpty();
    }

    [Then("the defect should not exist in the database")]
    public async Task ThenTheDefectShouldNotExistInTheDatabase()
    {
        await using var ctx = _context.CreateDbContext();
        var fetched = await ctx.Items.FindAsync(_defectId);
        fetched.Should().BeNull();
    }
}