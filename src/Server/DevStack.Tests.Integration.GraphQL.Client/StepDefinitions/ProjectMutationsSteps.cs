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
    public void GivenTheApiIsAvailable()
    {
    }

    [When(@"I create a project with name ""([^""]*)"" and description ""([^""]*)""")]
    public async Task WhenICreateAProjectWithNameAndDescription(string name, string description)
    {
        var data = await SendMutationAsync("""
            mutation CreateProject($input: CreateProjectInput!) {
              createProject(input: $input) {
                project { id }
                errors
              }
            }
            """,
            new { input = new { name, description } });
        
        _scenarioContext.Add("result", data);
        _createdProjectId = Guid.Parse(data!["createProject"]!["project"]!["id"]!.GetValue<string>());
    }

    [Then("the project should be created successfully")]
    public void ThenTheProjectShouldBeCreatedSuccessfully()
    {
        var data = _scenarioContext.Get<JsonNode>("result");
        data!["createProject"]!["errors"]!.AsArray().Should().BeEmpty();
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
        var data = await SendMutationAsync("""
            mutation CreateProject($input: CreateProjectInput!) {
              createProject(input: $input) {
                project { id }
                errors
              }
            }
            """,
            new { input = new { name, description = "Original description" } });
        
        _createdProjectId = Guid.Parse(data!["createProject"]!["project"]!["id"]!.GetValue<string>());
    }

    [When(@"I update the project name to ""([^""]*)""")]
    public async Task WhenIUpdateTheProjectNameTo(string newName)
    {
        var data = await SendMutationAsync("""
            mutation UpdateProject($input: UpdateProjectInput!) {
              updateProject(input: $input) {
                project { id name }
                errors
              }
            }
            """,
            new { input = new { Id = _createdProjectId, Name = newName } });
        
        _scenarioContext.Add("updateResult", data);
    }

    [Then("the project should be updated successfully")]
    public async Task ThenTheProjectShouldBeUpdatedSuccessfully()
    {
        var data = _scenarioContext.Get<JsonNode>("updateResult");
        data!["updateProject"]!["errors"]!.AsArray().Should().BeEmpty();
        
        await using var ctx = _fixture.CreateDbContext();
        var project = await ctx.Projects.FindAsync(_createdProjectId);
        project!.Name.Should().Be("Updated Name");
    }

    [Given(@"a project ""([^""]*)"" exists for deletion")]
    public async Task GivenAProjectExistsForDeletion(string name)
    {
        var data = await SendMutationAsync("""
            mutation CreateProject($input: CreateProjectInput!) {
              createProject(input: $input) {
                project { id }
                errors
              }
            }
            """,
            new { input = new { name } });
        
        _createdProjectId = Guid.Parse(data!["createProject"]!["project"]!["id"]!.GetValue<string>());
    }

    [When("I delete the project")]
    public async Task WhenIDeleteTheProject()
    {
        var data = await SendMutationAsync("""
            mutation DeleteProject($input: DeleteProjectInput!) {
              deleteProject(input: $input) {
                project { id }
                errors
              }
            }
            """,
            new { input = new { Id = _createdProjectId } });
        
        _scenarioContext.Add("deleteResult", data);
    }

    [Then("the project should be deleted successfully")]
    public void ThenTheProjectShouldBeDeletedSuccessfully()
    {
        var data = _scenarioContext.Get<JsonNode>("deleteResult");
        data!["deleteProject"]!["errors"]!.AsArray().Should().BeEmpty();
    }

    [Then("the project should not exist in the database")]
    public async Task ThenTheProjectShouldNotExistInTheDatabase()
    {
        await using var ctx = _fixture.CreateDbContext();
        var project = await ctx.Projects.FindAsync(_createdProjectId);
        project.Should().BeNull();
    }

    private async Task<JsonNode?> SendMutationAsync(string query, object? variables = null)
    {
        var response = await _fixture.HttpClient.PostAsJsonAsync("_fixture.GraphQlUrl", new { query, variables });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonNode.Parse(json)?["data"];
    }
}
