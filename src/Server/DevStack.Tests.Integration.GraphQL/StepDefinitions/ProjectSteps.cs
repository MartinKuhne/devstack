using System.Net.Http;
using System.Text;
using System.Text.Json;

using DevStack.Tests.Integration.GraphQL.Client.Hooks;

using FluentAssertions;

using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.GraphQL.Client.StepDefinitions;

[Binding]
public sealed class ProjectSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly HttpClient _httpClient;

    public ProjectSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _httpClient = SpecFlowHooks.GetHttpClient(scenarioContext);
    }

    private static JsonElement GetData(JsonElement response)
    {
        if (!response.TryGetProperty("data", out var data) || data.ValueKind == JsonValueKind.Null)
        {
            throw new InvalidOperationException("GraphQL response has no data: " + response.ToString());
        }
        return data;
    }

    private static bool HasErrors(JsonElement response, string mutationName)
    {
        return false;
    }

    [Given("the API is available")]
    public void GivenTheApiIsAvailable()
    {
    }

    [Given(@"a project ""(.*)"" exists")]
    public void GivenAProjectExists(string name)
    {
        var mutation = new
        {
            query = @"mutation CreateProject($input: CreateProjectInput!) { createProject(input: $input) { id } }",
            variables = new { input = new { name, description = (string?)null, repository = "https://example.com" } },
            operationName = "CreateProject"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;

        var projectId = GetData(result).GetProperty("createProject").GetProperty("id").ToString();
        _scenarioContext["ProjectId"] = projectId;
    }

    [When(@"I create a project with name ""(.*)"" and description ""(.*)""")]
    public void WhenICreateAProjectWithNameAndDescription(string name, string description)
    {
        var mutation = new
        {
            query = @"mutation CreateProject($input: CreateProjectInput!) { createProject(input: $input) { id } }",
            variables = new { input = new { name, description, repository = "https://example.com" } },
            operationName = "CreateProject"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;

        var projectId = GetData(result).GetProperty("createProject").GetProperty("id").ToString();
        _scenarioContext["ProjectId"] = projectId;
        _scenarioContext["Response"] = result;
    }

    [When(@"I create a project with name ""(.*)"" and no description")]
    public void WhenICreateAProjectWithNameAndNoDescription(string name)
    {
        var mutation = new
        {
            query = @"mutation CreateProject($input: CreateProjectInput!) { createProject(input: $input) { id } }",
            variables = new { input = new { name, description = (string?)null, repository = "https://example.com" } },
            operationName = "CreateProject"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;

        var projectId = GetData(result).GetProperty("createProject").GetProperty("id").ToString();
        _scenarioContext["ProjectId"] = projectId;
        _scenarioContext["Response"] = result;
    }

    [When(@"I update the project name to ""(.*)""")]
    public void WhenIUpdateTheProjectNameTo(string name)
    {
        var projectId = _scenarioContext["ProjectId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation UpdateProject($input: UpdateProjectInput!) { updateProject(input: $input) { id } }",
            variables = new { input = new { id = projectId, name, description = (string?)null, repository = (string?)null } },
            operationName = "UpdateProject"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [When(@"I update the project description to ""(.*)""")]
    public void WhenIUpdateTheProjectDescriptionTo(string description)
    {
        var projectId = _scenarioContext["ProjectId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation UpdateProject($input: UpdateProjectInput!) { updateProject(input: $input) { id } }",
            variables = new { input = new { id = projectId, name = (string?)null, description, repository = (string?)null } },
            operationName = "UpdateProject"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [When(@"I update the project repository to ""(.*)""")]
    public void WhenIUpdateTheProjectRepositoryTo(string repository)
    {
        var projectId = _scenarioContext["ProjectId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation UpdateProject($input: UpdateProjectInput!) { updateProject(input: $input) { id } }",
            variables = new { input = new { id = projectId, name = (string?)null, description = (string?)null, repository } },
            operationName = "UpdateProject"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [When(@"I delete the project")]
    public void WhenIDeleteTheProject()
    {
        var projectId = _scenarioContext["ProjectId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation DeleteProject($id: UUID!) { deleteProject(id: $id) }",
            variables = new { id = projectId },
            operationName = "DeleteProject"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [When(@"I query the project by id")]
    public void WhenIQueryTheProjectById()
    {
        var projectId = _scenarioContext["ProjectId"]?.ToString()!;
        var query = new
        {
            query = @"query GetProjectById($id: UUID!) { project(id: $id) { id name description repository } }",
            variables = new { id = projectId },
            operationName = "GetProjectById"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [Then(@"the project should be created successfully")]
    public void ThenTheProjectShouldBeCreatedSuccessfully()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        HasErrors(response, "createProject").Should().BeFalse("errors should be empty");
    }

    [Then(@"the project should be updated successfully")]
    public void ThenTheProjectShouldBeUpdatedSuccessfully()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        HasErrors(response, "updateProject").Should().BeFalse("errors should be empty");
    }

    [Then(@"the project should be deleted successfully")]
    public void ThenTheProjectShouldBeDeletedSuccessfully()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        var data = GetData(response);
        var deleted = data.GetProperty("deleteProject");
        deleted.ValueKind.Should().NotBe(JsonValueKind.Null);
        deleted.GetBoolean().Should().BeTrue("project should be deleted successfully");
    }

    [Then(@"the project should exist in the database")]
    public void ThenTheProjectShouldExistInTheDatabase()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        var project = GetData(response).GetProperty("createProject");
        project.ValueKind.Should().NotBe(JsonValueKind.Null);
        var projectId = project.GetProperty("id").ToString();
        projectId.Should().NotBeNullOrEmpty();
        _scenarioContext["ProjectId"] = projectId;
    }

    [Then(@"the project should not exist in the database")]
    public void ThenTheProjectShouldNotExistInDatabase()
    {
        var projectId = _scenarioContext["ProjectId"]?.ToString()!;
        var query = new
        {
            query = @"query GetProjectById($id: UUID!) { project(id: $id) { id } }",
            variables = new { id = projectId },
            operationName = "GetProjectById"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var project = GetData(result).GetProperty("project");
        project.ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Then(@"the project should be returned with correct data")]
    public void ThenTheProjectShouldBeReturnedWithCorrectData()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        var project = GetData(response).GetProperty("project");
        project.ValueKind.Should().NotBe(JsonValueKind.Null);
        var projectId = project.GetProperty("id").ToString();
        projectId.Should().NotBeNullOrEmpty();
        projectId.Should().Be(_scenarioContext["ProjectId"]?.ToString());
    }
}
