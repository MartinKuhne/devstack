using System.Net.Http;
using System.Text;
using System.Text.Json;
using DevStack.Tests.Integration.GraphQL.Client.Hooks;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.GraphQL.Client.StepDefinitions;

[Binding]
public sealed class AgentTaskSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly HttpClient _httpClient;

    private static bool HasErrors(JsonElement response, string mutationName)
    {
        var errors = response.GetProperty("data").GetProperty(mutationName).GetProperty("errors");
        return errors.ValueKind != JsonValueKind.Null && errors.GetArrayLength() > 0;
    }

    public AgentTaskSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _httpClient = SpecFlowHooks.GetHttpClient(scenarioContext);
    }

 [Given(@"a deliverable exists")]
    public void GivenADeliverableExists()
    {
        var projectId = _scenarioContext["ProjectId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation CreateDeliverable($input: CreateDeliverableInput!) { createDeliverable(input: $input) { deliverable { id } errors } }",
            variables = new { input = new { projectId, title = "Parent Deliverable", type = "Feature", description = (string?)null, acceptanceCriteria = (string?)null, agentFeedback = (string?)null, executionPlan = (string?)null, securityImpact = (string?)null, performanceImpact = (string?)null, testPlan = (string?)null, deploymentPlan = (string?)null, blocking = (string?)null, initialStatus = "PLANNING" } },
            operationName = "CreateDeliverable"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var deliverableId = result.GetProperty("data").GetProperty("createDeliverable").GetProperty("deliverable").GetProperty("id").ToString();
        _scenarioContext["DeliverableId"] = deliverableId;
    }

    [Given(@"a (?:task|agent task) ""(.*)"" exists")]
    public void GivenATaskExists(string title)
    {
        var projectId = _scenarioContext["ProjectId"]?.ToString()!;
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString()!;
        CreateTask(projectId, deliverableId, title, 1);
    }

    [Given(@"a (?:task|agent task) with status ""(.*)"" exists")]
    public void GivenATaskWithStatusExists(string status)
    {
        if (!_scenarioContext.ContainsKey("ProjectId") || string.IsNullOrEmpty(_scenarioContext["ProjectId"]?.ToString()))
            throw new InvalidOperationException("ProjectId not set. Ensure 'a parent project exists' has been executed.");
        if (!_scenarioContext.ContainsKey("DeliverableId") || string.IsNullOrEmpty(_scenarioContext["DeliverableId"]?.ToString()))
            throw new InvalidOperationException("DeliverableId not set. Ensure 'a parent feature exists' has been executed.");
        var projectId = _scenarioContext["ProjectId"]?.ToString()!;
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString()!;
        CreateTask(projectId, deliverableId, "Test Task", 1);
    }

    [When(@"I create a (?:task|agent task) with title ""(.*)"" and complexity rating (.*)")]
    public void WhenICreateATaskWithComplexityRating(string title, int complexityRating)
    {
        var projectId = _scenarioContext["ProjectId"]?.ToString()!;
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString()!;
        CreateTask(projectId, deliverableId, title, complexityRating);
        var mutation = new
        {
            query = @"mutation CreateAgentTask($input: CreateAgentTaskInput!) { createAgentTask(input: $input) { agentTask { id } errors } }",
            variables = new { input = new { projectId, deliverableId, title, complexityRating, result = (string?)null, errors = (string?)null, commitHash = (string?)null, dependsOnDevTask = (string?)null, promptTokens = (int?)null, completionTokens = (int?)null, executionDurationInSeconds = (double?)null, model = (string?)null } },
            operationName = "CreateAgentTask"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        _scenarioContext["Response"] = result;
    }

    private void CreateTask(string projectId, string deliverableId, string title, int complexityRating)
    {
        var mutation = new
        {
            query = @"mutation CreateAgentTask($input: CreateAgentTaskInput!) { createAgentTask(input: $input) { agentTask { id } errors } }",
            variables = new { input = new { projectId, deliverableId, title, complexityRating, result = (string?)null, errors = (string?)null, commitHash = (string?)null, dependsOnDevTask = (string?)null, promptTokens = (int?)null, completionTokens = (int?)null, executionDurationInSeconds = (double?)null, model = (string?)null } },
            operationName = "CreateAgentTask"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        var taskId = result!.GetProperty("data").GetProperty("createAgentTask").GetProperty("agentTask").GetProperty("id").ToString();
        _scenarioContext["AgentTaskId"] = taskId;
    }

    [When(@"I update the (?:task|agent task) title to ""(.*)"" and complexity rating to (.*)")]
    public void WhenIUpdateTheTaskTitleAndComplexity(string title, int complexityRating)
    {
        var taskId = _scenarioContext["AgentTaskId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation UpdateAgentTask($input: UpdateAgentTaskInput!) { updateAgentTask(input: $input) { agentTask { id } errors } }",
            variables = new { input = new { id = taskId, title, result = (string?)null, errors = (string?)null, commitHash = (string?)null, dependsOnDevTask = (string?)null, complexityRating, promptTokens = (int?)null, completionTokens = (int?)null, executionDurationInSeconds = (double?)null, model = (string?)null } },
            operationName = "UpdateAgentTask"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        _scenarioContext["Response"] = result;
    }

    [When(@"I transition the (?:task|agent task) status to ""(.*)""")]
    public void WhenITransitionTheTaskStatusTo(string targetStatus)
    {
        var taskId = _scenarioContext["AgentTaskId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation TransitionAgentTaskStatus($input: TransitionAgentTaskInput!) { transitionAgentTaskStatus(input: $input) { agentTask { id status } errors } }",
            variables = new { input = new { id = taskId, targetStatus = targetStatus.ToUpperInvariant(), actor = "test-user" } },
            operationName = "TransitionAgentTaskStatus"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        _scenarioContext["Response"] = result;
    }

    [When(@"I delete the (?:task|agent task)")]
    public void WhenIDeleteTheTask()
    {
        var taskId = _scenarioContext["AgentTaskId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation DeleteAgentTask($input: DeleteAgentTaskInput!) { deleteAgentTask(input: $input) { agentTask { id } errors } }",
            variables = new { input = new { id = taskId } },
            operationName = "DeleteAgentTask"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        _scenarioContext["Response"] = result;
    }

    [Then(@"the (?:task|agent task) should be created successfully")]
    public void ThenTheTaskShouldBeCreatedSuccessfully()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        HasErrors(response, "createAgentTask").Should().BeFalse("errors should be empty");
    }

    [Then(@"the (?:task|agent task) should be updated successfully")]
    public void ThenTheTaskShouldBeUpdatedSuccessfully()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        HasErrors(response, "updateAgentTask").Should().BeFalse("errors should be empty");
    }

    [Then(@"the (?:task|agent task) should be deleted successfully")]
    public void ThenTheTaskShouldBeDeletedSuccessfully()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        HasErrors(response, "deleteAgentTask").Should().BeFalse("errors should be empty");
    }

   [Then(@"the (?:task|agent task) should exist in the database")]
    public void ThenTheTaskShouldExistInTheDatabase()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        var agentTask = response.GetProperty("data").GetProperty("createAgentTask").GetProperty("agentTask");
        agentTask.ValueKind.Should().NotBe(JsonValueKind.Null);
        var taskId = agentTask.GetProperty("id").ToString();
        taskId.Should().NotBeNullOrEmpty();
        _scenarioContext["AgentTaskId"] = taskId;
    }

    [Then(@"the (?:task|agent task) status should be ""(.*)""")]
    public void ThenTheTaskStatusShouldBe(string expectedStatus)
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        var agentTask = response.GetProperty("data").GetProperty("transitionAgentTaskStatus").GetProperty("agentTask");
        var status = agentTask.GetProperty("status").ToString();
        status.Should().BeEquivalentTo(expectedStatus.ToUpperInvariant());
    }

    [Then(@"the (?:task|agent task) should not exist in the database")]
    public void ThenTheTaskShouldNotExistInDatabase()
    {
        var taskId = _scenarioContext["AgentTaskId"]?.ToString()!;
        var query = new
        {
            query = @"query GetAgentTaskById($id: UUID!) { agentTaskById(id: $id) { id } }",
            variables = new { id = taskId },
            operationName = "GetAgentTaskById"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        var agentTask = result!.GetProperty("data").GetProperty("agentTaskById");
        agentTask.ValueKind.Should().Be(JsonValueKind.Null);
    }
}
