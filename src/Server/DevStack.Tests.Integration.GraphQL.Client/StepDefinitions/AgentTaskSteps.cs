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
        CreateTask(deliverableId, title, 1);
    }

    [Given(@"a (?:task|agent task) with status ""(.*)"" exists")]
    public void GivenATaskWithStatusExists(string status)
    {
        var deliverableIdObj = _scenarioContext["DeliverableId"];
        var deliverableId = deliverableIdObj?.ToString() ?? throw new InvalidOperationException($"DeliverableId not set. Value: {deliverableIdObj ?? (object)"null"}");
        CreateTask(deliverableId, "Test Task", 1);
    }

    [When(@"I create a (?:task|agent task) with title ""(.*)"" and complexity rating (.*)")]
    public void WhenICreateATaskWithComplexityRating(string title, int complexityRating)
    {
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString()!;
        CreateTask(deliverableId, title, complexityRating);
        var mutation = new
        {
            query = @"mutation CreateAgentTask($input: CreateAgentTaskInput!) { createAgentTask(input: $input) { agentTask { id } errors } }",
            variables = new { input = new { deliverableId, title, complexityRating, result = (string?)null, errors = (string?)null, commitHash = (string?)null, dependsOnDevTask = (string?)null, promptTokens = (int?)null, completionTokens = (int?)null, executionDurationInSeconds = (double?)null, model = (string?)null } },
            operationName = "CreateAgentTask"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var data = result.GetProperty("data");
        var createAgentTask = data.GetProperty("createAgentTask");
        var errors = createAgentTask.GetProperty("errors");
        if (errors.GetArrayLength() > 0)
        {
            var errorMessages = new StringBuilder();
            foreach (var error in errors.EnumerateArray())
            {
                errorMessages.Append(error.GetString()!);
            }
            throw new InvalidOperationException($"CreateAgentTask failed: {errorMessages}");
        }
        var agentTask = createAgentTask.GetProperty("agentTask");
        var taskId = agentTask.GetProperty("id").ToString();
        _scenarioContext["AgentTaskId"] = taskId;
        _scenarioContext["Response"] = result;
    }

    private void CreateTask(string deliverableId, string title, int complexityRating)
    {
        var mutation = new
        {
            query = @"mutation CreateAgentTask($input: CreateAgentTaskInput!) { createAgentTask(input: $input) { agentTask { id } errors } }",
            variables = new { input = new { deliverableId, title, complexityRating, result = (string?)null, errors = (string?)null, commitHash = (string?)null, dependsOnDevTask = (string?)null, promptTokens = (int?)null, completionTokens = (int?)null, executionDurationInSeconds = (double?)null, model = (string?)null } },
            operationName = "CreateAgentTask"
        };

        var contentBody = JsonSerializer.Serialize(mutation);
        var response = _httpClient.PostAsync("", new StringContent(contentBody, Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"HTTP {response.StatusCode}: {content}");
        }
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var data = result.GetProperty("data");
        var createAgentTask = data.GetProperty("createAgentTask");
        var errors = createAgentTask.GetProperty("errors");
        if (errors.GetArrayLength() > 0)
        {
            var errorMessages = new StringBuilder();
            foreach (var error in errors.EnumerateArray())
            {
                errorMessages.Append(error.GetString()!);
            }
            throw new InvalidOperationException($"CreateAgentTask failed: {errorMessages}");
        }
        var agentTask = createAgentTask.GetProperty("agentTask");
        if (agentTask.ValueKind == JsonValueKind.Null)
        {
            throw new InvalidOperationException("CreateAgentTask returned null agentTask");
        }
        var taskId = agentTask.GetProperty("id").ToString();
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
        var targetStatusMapped = targetStatus switch
        {
            "DONE" or "Done" => "DONE",
            "FAILED" or "Failed" => "FAILED",
            "REJECTED" or "Rejected" => "REJECTED",
            "NEEDSREVIEW" or "NeedsReview" or "Needs Review" => "NEEDS_REVIEW",
            "INPROGRESS" or "InProgress" => "IN_PROGRESS",
            "READY" or "Ready" => "READY",
            _ => targetStatus.ToUpperInvariant().Replace(" ", "_")
        };
        var mutation = new
        {
            query = @"mutation TransitionAgentTaskStatus($input: TransitionAgentTaskInput!) { transitionAgentTaskStatus(input: $input) { agentTask { id status } errors } }",
            variables = new { input = new { id = taskId, targetStatus = targetStatusMapped, actor = "test-user" } },
            operationName = "TransitionAgentTaskStatus"
        };

        var contentBody = JsonSerializer.Serialize(mutation);
        var response = _httpClient.PostAsync("", new StringContent(contentBody, Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"HTTP {response.StatusCode}: {content}");
        }
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var data = result.GetProperty("data");
        var transitionResponse = data.GetProperty("transitionAgentTaskStatus");
        var errors = transitionResponse.GetProperty("errors");
        if (errors.ValueKind != JsonValueKind.Null && errors.GetArrayLength() > 0)
        {
            var errorMessages = new StringBuilder();
            foreach (var error in errors.EnumerateArray())
            {
                errorMessages.Append(error.GetString()!);
            }
            throw new InvalidOperationException($"TransitionAgentTaskStatus failed: {errorMessages}");
        }
        var agentTask = transitionResponse.GetProperty("agentTask");
        if (agentTask.ValueKind == JsonValueKind.Null)
        {
            throw new InvalidOperationException("TransitionAgentTaskStatus returned null agentTask. Response: " + content);
        }
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
        var expectedMapped = expectedStatus switch
        {
            "DONE" or "Done" => "DONE",
            "FAILED" or "Failed" => "FAILED",
            "REJECTED" or "Rejected" => "REJECTED",
            "NEEDSREVIEW" or "NeedsReview" or "Needs Review" => "NEEDS_REVIEW",
            "INPROGRESS" or "InProgress" => "IN_PROGRESS",
            "READY" or "Ready" => "READY",
            _ => expectedStatus.ToUpperInvariant()
        };
        status.Should().BeEquivalentTo(expectedMapped);
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
