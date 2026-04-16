using TechTalk.SpecFlow;
using DevStack.Tests.Integration.MCP.Client;
using FluentAssertions;
using System.Text.Json;
using System.Threading;

namespace DevStack.Tests.Integration.MCP.Steps;

[Binding]
public sealed class DevStackToolsSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly IMcpJsonRpcClient _client;
    private JsonRpcResponse? _response;
    private string? _createdResourceId;

    public DevStackToolsSteps(ScenarioContext scenarioContext, IMcpJsonRpcClient client)
    {
        _scenarioContext = scenarioContext;
        _client = client;
    }

    #region Project Steps

    [Given(@"a valid project creation request with name ""(.*)""")]
    public void GivenAValidProjectCreationRequest(string projectName)
    {
        _scenarioContext["ProjectName"] = projectName;
    }

    [Given(@"an existing project ID")]
    public async Task GivenAnExistingProjectID()
    {
        if (_createdResourceId == null)
        {
            _createdResourceId = await CreateTestProjectAsync();
        }
        _scenarioContext["ProjectId"] = _createdResourceId;
    }

    [When(@"I call devstack_createProject")]
    public async Task WhenICallDevstackCreateProject()
    {
        var projectName = _scenarioContext.GetString("ProjectName") ?? "Test Project";
        var request = new { name = projectName, description = "Test project description" };
        _response = await _client.SendRequestAsync("devstack_createProject", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_getProjectById with the ID")]
    public async Task WhenICallDevstackGetProjectById()
    {
        var projectId = _scenarioContext.GetString("ProjectId") ?? "";
        var request = new { id = projectId };
        _response = await _client.SendRequestAsync("devstack_getProjectById", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_getProjects")]
    public async Task WhenICallDevstackGetProjects()
    {
        _response = await _client.SendRequestAsync("devstack_getProjects", default(CancellationToken));
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_updateProject with updated name ""(.*)""")]
    public async Task WhenICallDevstackUpdateProject(string updatedName)
    {
        var projectId = _scenarioContext.GetString("ProjectId") ?? "";
        var request = new { id = projectId, name = updatedName };
        _response = await _client.SendRequestAsync("devstack_updateProject", request);
        _scenarioContext["Response"] = _response;
    }

    [When(@"I call devstack_deleteProject with the ID")]
    public async Task WhenICallDevstackDeleteProject()
    {
        var projectId = _scenarioContext.GetString("ProjectId") ?? "";
        var request = new { id = projectId };
        _response = await _client.SendRequestAsync("devstack_deleteProject", request);
        _scenarioContext["Response"] = _response;
    }

    [Then(@"the response should contain the created project")]
    public void ThenTheResponseShouldContainTheCreatedProject()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
        _response!.Result.Should().NotBeNull();
    }

    [Then(@"the project should have a valid ID")]
    public void ThenTheProjectShouldHaveAValidID()
    {
        var result = _response!.Result!.ToString();
        result.Should().NotBeNullOrEmpty();
        
        var jsonDoc = JsonDocument.Parse(result!);
        if (jsonDoc.RootElement.TryGetProperty("id", out var idElement))
        {
            _createdResourceId = idElement.GetString();
            _createdResourceId.Should().NotBeNullOrEmpty();
        }
    }

    [Then(@"the response should contain the project details")]
    public void ThenTheResponseShouldContainTheProjectDetails()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
    }

    [Then(@"the project name should match")]
    public void ThenTheProjectNameShouldMatch()
    {
        var expectedName = _scenarioContext.GetString("ProjectName");
        var result = _response!.Result!.ToString();
        result.Should().Contain(expectedName ?? "");
    }

    [Then(@"the response should contain a list of projects")]
    public void ThenTheResponseShouldContainAListOfProjects()
    {
        _response.Should().NotBeNull();
        _response!.Result.Should().NotBeNull();
    }

    [Then(@"the list should not be empty")]
    public void ThenTheListShouldNotBeEmpty()
    {
        var result = _response!.Result!.ToString();
        result.Should().NotBe("[]");
    }

    [Then(@"the response should contain the updated project")]
    public void ThenTheResponseShouldContainTheUpdatedProject()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
    }

    [Then(@"the project name should be ""(.*)""")]
    public void ThenTheProjectNameShouldBe(string expectedName)
    {
        var result = _response!.Result!.ToString();
        result.Should().Contain(expectedName);
    }

    [Then(@"the response should confirm deletion")]
    public void ThenTheResponseShouldConfirmDeletion()
    {
        _response.Should().NotBeNull();
        _response!.Error.Should().BeNull();
    }

    #endregion

    #region Helper Methods

    private async Task<string> CreateTestProjectAsync()
    {
        var request = new { name = $"Test Project {Guid.NewGuid()}", description = "Auto-generated test project" };
        var response = await _client.SendRequestAsync("devstack_createProject", request);
        var result = response.Result!.ToString()!;
        var jsonDoc = JsonDocument.Parse(result);
        return jsonDoc.RootElement.GetProperty("id").GetString() ?? "";
    }

    #endregion
}
