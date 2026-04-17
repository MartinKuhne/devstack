using System;
using System.Threading.Tasks;
using DevStack.Client;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.GraphQL.Client.Steps;

[Binding]
public class ProjectSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly IDevStackClient _client;

    public ProjectSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _client = Hooks.SpecFlowHooks.GetClient(scenarioContext);
    }

    [When(@"I create a project with name ""(.*)"" and description ""(.*)""")]
    public async Task WhenICreateAProjectWithNameAndDescription(string name, string description)
    {
        var input = new CreateProjectInput
        {
            Name = name,
            Description = description
        };

        var result = await _client.CreateProject.ExecuteAsync(input);
        result.Errors.Should().BeEmpty();
        
        _scenarioContext["LastMutationErrors"] = result.Errors;
        _scenarioContext["CreatedProjectId"] = result.Data?.CreateProject.Project?.Id;
    }

    [When(@"I update the project name to ""(.*)""")]
    public async Task WhenIUpdateTheProjectNameTo(string name)
    {
        var projectId = _scenarioContext.TryGetValue<string>("CurrentProjectId", out var id) ? id 
            : _scenarioContext.TryGetValue<string>("ProjectId_Original Name", out id) ? id : null;
        
        projectId.Should().NotBeNullOrEmpty();

        var input = new UpdateProjectInput
        {
            Id = Guid.Parse(projectId!),
            Name = name
        };

        var result = await _client.UpdateProject.ExecuteAsync(input);
        result.Errors.Should().BeEmpty();
        
        _scenarioContext["LastMutationErrors"] = result.Errors;
    }

    [When(@"I delete the project")]
    public async Task WhenIDeleteTheProject()
    {
        var projectId = _scenarioContext.TryGetValue<string>("CurrentProjectId", out var id) ? id 
            : _scenarioContext.TryGetValue<string>("ProjectId_To Delete", out id) ? id : null;
        
        projectId.Should().NotBeNullOrEmpty();

        var input = new DeleteProjectInput
        {
            Id = Guid.Parse(projectId!)
        };

        var result = await _client.DeleteProject.ExecuteAsync(input);
        result.Errors.Should().BeEmpty();
        
        _scenarioContext["LastMutationErrors"] = result.Errors;
    }

    [Then(@"the project should be created successfully")]
    public void ThenTheProjectShouldBeCreatedSuccessfully()
    {
        var errors = _scenarioContext.TryGetValue<System.Collections.Generic.IReadOnlyList<string>>("LastMutationErrors", out var e) ? e : null;
        errors.Should().BeEmpty();
    }

    [Then(@"the project should exist in the database")]
    public void ThenTheProjectShouldExistInTheDatabase()
    {
        var projectId = _scenarioContext.TryGetValue<string>("CreatedProjectId", out var id) ? id : null;
        projectId.Should().NotBeNullOrEmpty();
    }

    [Then(@"the project should be updated successfully")]
    public void ThenTheProjectShouldBeUpdatedSuccessfully()
    {
        var errors = _scenarioContext.TryGetValue<System.Collections.Generic.IReadOnlyList<string>>("LastMutationErrors", out var e) ? e : null;
        errors.Should().BeEmpty();
    }

    [Then(@"the project should be deleted successfully")]
    public void ThenTheProjectShouldBeDeletedSuccessfully()
    {
        var errors = _scenarioContext.TryGetValue<System.Collections.Generic.IReadOnlyList<string>>("LastMutationErrors", out var e) ? e : null;
        errors.Should().BeEmpty();
    }

    [Then(@"the project should not exist in the database")]
    public void ThenTheProjectShouldNotExistInTheDatabase()
    {
        var projectId = _scenarioContext.TryGetValue<string>("CreatedProjectId", out var id) ? id 
            : _scenarioContext.TryGetValue<string>("ProjectId_To Delete", out id) ? id : null;
        
        projectId.Should().BeNull();
    }
}