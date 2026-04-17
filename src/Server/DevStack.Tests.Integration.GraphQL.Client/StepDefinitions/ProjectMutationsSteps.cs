using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.GraphQL.Client.Features;

[Binding]
public class ProjectMutationsSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly IntegrationTestFixture _fixture;
    private Guid? _createdProjectId;

    public ProjectMutationsSteps(ScenarioContext scenarioContext, IntegrationTestFixture fixture)
    {
        _scenarioContext = scenarioContext;
        _fixture = fixture;
    }

    [Given("the API is available")]
    public void GivenTheApiIsAvailable() { }

    [Given("a parent project exists")]
    public void GivenAParentProjectExists() { }

    [When(@"I create a project with name ""([^""]*)"" and description ""([^""]*)""")]
    public async Task WhenICreateAProjectWithNameAndDescription(string name, string description)
    {
        _createdProjectId = await _fixture.CreateTestProjectAsync(name, description);
    }

    [Then("the project should be created successfully")]
    public void ThenTheProjectShouldBeCreatedSuccessfully()
    {
        _createdProjectId.Should().NotBe(Guid.Empty);
    }

    [Then("the project should exist in the database")]
    public async Task ThenTheProjectShouldExistInTheDatabase()
    {
        var projectId = _createdProjectId!.Value;
        await using var ctx = _fixture.CreateDbContext();
        var project = await ctx.Projects.FindAsync(projectId);
        project.Should().NotBeNull();
    }

    [Given(@"a project ""([^""]*)"" exists")]
    public async Task GivenAProjectExists(string name)
    {
        _createdProjectId = await _fixture.CreateTestProjectAsync(name, "Original description");
    }

    [When(@"I update the project name to ""([^""]*)""")]
    public async Task WhenIUpdateTheProjectNameTo(string newName)
    {
        var mutation = new DevStack.Api.GraphQL.Types.Mutation();
        var input = new DevStack.Api.GraphQL.Types.UpdateProjectInput(_createdProjectId!.Value, newName, null, null, null, null, null);
        var handler = new DevStack.Infrastructure.Projects.UpdateProjectHandler(_fixture.CreateDbContext());
        
        var result = await mutation.UpdateProjectAsync(input, handler, CancellationToken.None);
        _scenarioContext.Add("updateResult", result);
    }

    [Then("the project should be updated successfully")]
    public async Task ThenTheProjectShouldBeUpdatedSuccessfully()
    {
        var result = _scenarioContext.Get<DevStack.Api.GraphQL.Types.ProjectPayload>("updateResult");
        result.Errors.Should().BeEmpty();
        
        await using var ctx = _fixture.CreateDbContext();
        var project = await ctx.Projects.FindAsync(_createdProjectId);
        project!.Name.Should().Be("Updated Name");
    }

    [Given(@"a project ""([^""]*)"" exists for deletion")]
    public async Task GivenAProjectExistsForDeletion(string name)
    {
        _createdProjectId = await _fixture.CreateTestProjectAsync(name);
    }

    [When("I delete the project")]
    public async Task WhenIDeleteTheProject()
    {
        var mutation = new DevStack.Api.GraphQL.Types.Mutation();
        var input = new DevStack.Api.GraphQL.Types.DeleteProjectInput(_createdProjectId!.Value);
        var handler = new DevStack.Infrastructure.Projects.DeleteProjectHandler(_fixture.CreateDbContext());
        
        var result = await mutation.DeleteProjectAsync(input, handler, CancellationToken.None);
        _scenarioContext.Add("deleteResult", result);
    }

    [Then("the project should be deleted successfully")]
    public void ThenTheProjectShouldBeDeletedSuccessfully()
    {
        var result = _scenarioContext.Get<DevStack.Api.GraphQL.Types.ProjectPayload>("deleteResult");
        result.Errors.Should().BeEmpty();
    }

    [Then("the project should not exist in the database")]
    public async Task ThenTheProjectShouldNotExistInTheDatabase()
    {
        await using var ctx = _fixture.CreateDbContext();
        var project = await ctx.Projects.FindAsync(_createdProjectId);
        project.Should().BeNull();
    }
}