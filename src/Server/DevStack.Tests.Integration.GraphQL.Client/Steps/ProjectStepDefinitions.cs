using System.Text.Json.Nodes;
using DevStack.Domain.Enums;
using DevStack.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.GraphQL.Client.Steps;

[Binding]
public class ProjectStepDefinitions
{
    private readonly GraphQLContext _context;
    private Guid _projectId;
    private JsonNode? _lastMutationResult;

    public ProjectStepDefinitions(GraphQLContext context)
    {
        _context = context;
    }

    [Given("the API is available")]
    public void GivenTheApiIsAvailable()
    {
        _context.HttpClient.Should().NotBeNull();
    }

    [Given("a project \"(.*)\" exists")]
    public async Task GivenAProjectExists(string name)
    {
        var projectId = Guid.NewGuid();
        _projectId = projectId;

        _lastMutationResult = await _context.SendMutationAsync("CreateProject", new
        {
            input = new
            {
                id = projectId,
                name,
                description = "Test description"
            }
        });

        _lastMutationResult!["createProject"]!["errors"]!.AsArray().Should().BeEmpty();
    }

    [When("I create a project with name \"(.*)\" and description \"(.*)\"")]
    public async Task WhenICreateProject(string name, string description)
    {
        _projectId = Guid.NewGuid();
        
        _lastMutationResult = await _context.SendMutationAsync("CreateProject", new
        {
            input = new
            {
                id = _projectId,
                name,
                description,
                architecture = "Microservices",
                memory = "8GB",
                githubUrl = "https://github.com/test/repo"
            }
        });
    }

    [Then("the project should be created successfully")]
    public void ThenTheProjectShouldBeCreatedSuccessfully()
    {
        _lastMutationResult.Should().NotBeNull();
        _lastMutationResult!["createProject"]!["errors"]!.AsArray().Should().BeEmpty();
    }

    [Then("the project should exist in the database")]
    public async Task ThenTheProjectShouldExistInTheDatabase()
    {
        await using var ctx = _context.CreateDbContext();
        var fetched = await ctx.Projects.FindAsync(_projectId);
        fetched.Should().NotBeNull();
        fetched!.Name.Should().NotBeNull();
    }

    [When("I update the project name to \"(.*)\"")]
    public async Task WhenIUpdateProjectNameTo(string newName)
    {
        _lastMutationResult = await _context.SendMutationAsync("UpdateProject", new
        {
            input = new
            {
                id = _projectId,
                name = newName,
                description = "Updated description"
            }
        });
    }

    [Then("the project should be updated successfully")]
    public void ThenTheProjectShouldBeUpdatedSuccessfully()
    {
        _lastMutationResult.Should().NotBeNull();
        _lastMutationResult!["updateProject"]!["errors"]!.AsArray().Should().BeEmpty();
    }

    [When("I delete the project")]
    public async Task WhenIDeleteTheProject()
    {
        _lastMutationResult = await _context.SendMutationAsync("DeleteProject", new
        {
            input = new { id = _projectId }
        });
    }

    [Then("the project should be deleted successfully")]
    public void ThenTheProjectShouldBeDeletedSuccessfully()
    {
        _lastMutationResult.Should().NotBeNull();
        _lastMutationResult!["deleteProject"]!["errors"]!.AsArray().Should().BeEmpty();
    }

    [Then("the project should not exist in the database")]
    public async Task ThenTheProjectShouldNotExistInTheDatabase()
    {
        await using var ctx = _context.CreateDbContext();
        var fetched = await ctx.Projects.FindAsync(_projectId);
        fetched.Should().BeNull();
    }
}