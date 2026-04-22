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

    public AgentTaskSteps(ScenarioContext scenarioContext)
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

    private static JsonElement GetMutationResult(JsonElement data, string mutationName)
    {
        if (!data.TryGetProperty(mutationName, out var result) || result.ValueKind == JsonValueKind.Null)
        {
            throw new InvalidOperationException($"GraphQL mutation '{mutationName}' returned null: " + data.ToString());
        }
        return result;
    }

    private static JsonElement GetNonNullData(JsonElement parent, string propertyName, string mutationName)
    {
        if (!parent.TryGetProperty(propertyName, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            var errors = parent.TryGetProperty("errors", out var errorsElem) && errorsElem.ValueKind != JsonValueKind.Null
                ? string.Join("; ", errorsElem.EnumerateArray().Select(e => $"{e.GetProperty("field")}: {e.GetProperty("message")}".ToString()))
                : "no errors";
            throw new InvalidOperationException($"GraphQL mutation '{mutationName}' returned null for '{propertyName}': {errors}. Full response: {parent.ToString()}");
        }
        return value;
    }

    private static bool HasErrors(JsonElement response, string mutationName)
    {
        var data = GetData(response);
        var result = GetMutationResult(data, mutationName);
        var errors = result.GetProperty("errors");
        return errors.ValueKind != JsonValueKind.Null && errors.GetArrayLength() > 0;
    }

    [Given(@"an agent task ""(.*)"" exists")]
    public void GivenAnAgentTaskExists(string title)
    {
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString()!;
        CreateAgentTask(deliverableId, title, 5);
    }

    [Given(@"an agent task with status ""(.*)"" exists")]
    public void GivenAnAgentTaskWithStatusExists(string status)
    {
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString()!;
        CreateAgentTask(deliverableId, "Test Task", 5);
        var targetStatus = MapAgentTaskStatus(status);
        if (targetStatus != "READY")
        {
            TransitionToTargetStatus(deliverableId, targetStatus);
        }
    }

    private void TransitionToTargetStatus(string deliverableId, string targetStatus)
    {
        var currentStatus = "READY";
        
        while (currentStatus != targetStatus)
        {
            string nextStatus;
            if (currentStatus == "READY")
            {
                if (targetStatus == "IN_PROGRESS" || targetStatus == "FAILED" || targetStatus == "REJECTED")
                {
                    nextStatus = targetStatus;
                }
                else if (targetStatus == "NEEDS_REVIEW" || targetStatus == "DONE")
                {
                    nextStatus = "IN_PROGRESS";
                }
                else
                {
                    throw new InvalidOperationException($"Cannot transition from {currentStatus} to {targetStatus}");
                }
            }
            else if (currentStatus == "IN_PROGRESS")
            {
                if (targetStatus == "NEEDS_REVIEW")
                {
                    nextStatus = "NEEDS_REVIEW";
                }
                else if (targetStatus == "FAILED" || targetStatus == "REJECTED")
                {
                    nextStatus = targetStatus;
                }
                else if (targetStatus == "DONE")
                {
                    nextStatus = "NEEDS_REVIEW";
                }
                else
                {
                    throw new InvalidOperationException($"Cannot transition from {currentStatus} to {targetStatus}");
                }
            }
            else if (currentStatus == "NEEDS_REVIEW")
            {
                if (targetStatus == "IN_PROGRESS" || targetStatus == "DONE" || targetStatus == "REJECTED")
                {
                    nextStatus = targetStatus;
                }
                else
                {
                    throw new InvalidOperationException($"Cannot transition from {currentStatus} to {targetStatus}");
                }
            }
            else
            {
                throw new InvalidOperationException($"Cannot transition from {currentStatus} to {targetStatus}");
            }
            
            TransitionAgentTask(deliverableId, nextStatus);
            currentStatus = nextStatus;
        }
    }

    private void TransitionAgentTask(string deliverableId, string targetStatus)
    {
        var taskId = _scenarioContext["AgentTaskId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation TransitionAgentTaskStatus($input: TransitionAgentTaskInput!) { transitionAgentTaskStatus(input: $input) { agentTask { id status } errors { field message } } }",
            variables = new { input = new { id = taskId, targetStatus, actor = "test-user" } },
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
        var transitionResponse = GetMutationResult(GetData(result), "transitionAgentTaskStatus");
        var errors = transitionResponse.GetProperty("errors");
        if (errors.ValueKind != JsonValueKind.Null && errors.GetArrayLength() > 0)
        {
            var errorMessages = new StringBuilder();
            foreach (var error in errors.EnumerateArray())
            {
                errorMessages.Append(error.GetString());
            }
            throw new InvalidOperationException($"TransitionAgentTaskStatus failed: {errorMessages}");
        }
        var agentTask = transitionResponse.GetProperty("agentTask");
        if (agentTask.ValueKind == JsonValueKind.Null)
        {
            throw new InvalidOperationException("TransitionAgentTaskStatus returned null agentTask. Response: " + content);
        }
    }

    [Given(@"the agent task has errors set")]
    public void GivenTheAgentTaskHasErrorsSet()
    {
        var taskId = _scenarioContext["AgentTaskId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation UpdateAgentTask($input: UpdateAgentTaskInput!) { updateAgentTask(input: $input) { agentTask { id } errors { field message } } }",
            variables = new { input = new { id = taskId, title = (string?)null, result = (string?)null, errors = "Test error message", commitHash = (string?)null, dependsOnAgentTaskId = (Guid?)null, description = (string?)null, agent = (string?)null, complexityRating = (int?)null, promptTokens = (int?)null, completionTokens = (int?)null, executionDurationInSeconds = (int?)null } },
            operationName = "UpdateAgentTask"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"HTTP {response.StatusCode}: {content}");
        }
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var errors = GetMutationResult(GetData(result), "updateAgentTask").GetProperty("errors");
        if (errors.ValueKind != JsonValueKind.Null && errors.GetArrayLength() > 0)
        {
            var errorMessages = new StringBuilder();
            foreach (var error in errors.EnumerateArray())
            {
                errorMessages.Append(error.GetString());
            }
            throw new InvalidOperationException($"UpdateAgentTask failed: {errorMessages}");
        }
    }

    [Given(@"the agent task has result set")]
    public void GivenTheAgentTaskHasResultSet()
    {
        var taskId = _scenarioContext["AgentTaskId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation UpdateAgentTask($input: UpdateAgentTaskInput!) { updateAgentTask(input: $input) { agentTask { id } errors { field message } } }",
            variables = new { input = new { id = taskId, title = (string?)null, result = "Task completed successfully", errors = (string?)null, commitHash = (string?)null, dependsOnAgentTaskId = (Guid?)null, description = (string?)null, agent = (string?)null, complexityRating = (int?)null, promptTokens = (int?)null, completionTokens = (int?)null, executionDurationInSeconds = (int?)null } },
            operationName = "UpdateAgentTask"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"HTTP {response.StatusCode}: {content}");
        }
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var errors = GetMutationResult(GetData(result), "updateAgentTask").GetProperty("errors");
        if (errors.ValueKind != JsonValueKind.Null && errors.GetArrayLength() > 0)
        {
            var errorMessages = new StringBuilder();
            foreach (var error in errors.EnumerateArray())
            {
                errorMessages.Append(error.GetString());
            }
            throw new InvalidOperationException($"UpdateAgentTask failed: {errorMessages}");
        }
    }

    private void CreateAgentTask(string deliverableId, string title, int complexityRating)
    {
        var mutation = new
        {
            query = @"mutation CreateAgentTask($input: CreateAgentTaskInput!) { createAgentTask(input: $input) { agentTask { id } errors { field message } } }",
            variables = new { input = new { deliverableId, title, complexityRating, description = "Integration test task", result = (string?)null, errors = (string?)null, commitHash = (string?)null, dependsOnAgentTaskId = (Guid?)null, agent = (string?)null, promptTokens = (int?)null, completionTokens = (int?)null, executionDurationInSeconds = (int?)null } },
            operationName = "CreateAgentTask"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"HTTP {response.StatusCode}: {content}");
        }
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var agentTask = GetNonNullData(GetMutationResult(GetData(result), "createAgentTask"), "agentTask", "createAgentTask");
        if (agentTask.ValueKind == JsonValueKind.Null)
        {
            throw new InvalidOperationException("CreateAgentTask returned null agentTask");
        }
        var taskId = agentTask.GetProperty("id").ToString();
        _scenarioContext["AgentTaskId"] = taskId;
    }

    [When(@"I create an agent task with title ""(.*)"" and complexity rating (.*)")]
    public void WhenICreateAnAgentTaskWithTitleAndComplexityRating(string title, int complexityRating)
    {
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString()!;
        CreateAgentTaskWithFullFields(deliverableId, title, complexityRating, null, null, null, null, null, null, null, null, null);
    }

    [When(@"I create an agent task with title ""(.*)"" complexity (.*) result ""(.*)"" errors (.*) commit hash ""(.*)"" model ""(.*)""")]
    public void WhenICreateAnAgentTaskWithAllFields(string title, int complexityRating, string? result, string? errors, string? commitHash, string? model)
    {
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString()!;
        CreateAgentTaskWithFullFields(deliverableId, title, complexityRating, result, errors, commitHash, null, null, model, null, null, null);
    }

    [When(@"I create an agent task with title ""(.*)"" complexity (.*) and depends on ""(.*)""")]
    public void WhenICreateAnAgentTaskWithDependency(string title, int complexityRating, string dependencyTitle)
    {
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString()!;
        var dependencyTaskId = _scenarioContext.Get<string>("AgentTaskId") ?? "";
        Guid? depId = Guid.TryParse(dependencyTaskId, out var parsed) ? parsed : null;
        CreateAgentTaskWithFullFields(deliverableId, title, complexityRating, null, null, null, depId, null, null, null, null, null);
    }

    private void CreateAgentTaskWithFullFields(string deliverableId, string title, int complexityRating, string? result, string? errors, string? commitHash, Guid? dependsOnAgentTaskId, string? description, string? agent, int? promptTokens, int? completionTokens, int? executionDurationInSeconds)
    {
        description ??= "Integration test task";

        var mutation = new
        {
            query = @"mutation CreateAgentTask($input: CreateAgentTaskInput!) { createAgentTask(input: $input) { agentTask { id } errors { field message } } }",
            variables = new { input = new { deliverableId, title, description, complexityRating, result, errors, commitHash, dependsOnAgentTaskId, promptTokens, completionTokens, executionDurationInSeconds, agent } },
            operationName = "CreateAgentTask"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"HTTP {response.StatusCode}: {content}");
        }
        var resultJson = JsonSerializer.Deserialize<JsonElement>(content)!;
        var createResult = GetMutationResult(GetData(resultJson), "createAgentTask");
        var errs = createResult.GetProperty("errors");
        if (errs.ValueKind != JsonValueKind.Null && errs.GetArrayLength() > 0)
        {
            var errorMessages = new StringBuilder();
            foreach (var error in errs.EnumerateArray())
            {
                errorMessages.Append(error.GetString());
            }
            throw new InvalidOperationException($"CreateAgentTask failed: {errorMessages}");
        }
        var agentTask = createResult.GetProperty("agentTask");
        if (agentTask.ValueKind == JsonValueKind.Null)
        {
            throw new InvalidOperationException("CreateAgentTask returned null agentTask");
        }
        var taskId = agentTask.GetProperty("id").ToString();
        _scenarioContext["AgentTaskId"] = taskId;
        _scenarioContext["Response"] = resultJson;
    }

    [When(@"I update the agent task title to ""(.*)""")]
    public void WhenIUpdateTheAgentTaskTitleTo(string title)
    {
        var taskId = _scenarioContext["AgentTaskId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation UpdateAgentTask($input: UpdateAgentTaskInput!) { updateAgentTask(input: $input) { agentTask { id } errors { field message } } }",
            variables = new { input = new { id = taskId, title, result = (string?)null, errors = (string?)null, commitHash = (string?)null, dependsOnAgentTaskId = (Guid?)null, description = (string?)null, agent = (string?)null, complexityRating = (int?)null, promptTokens = (int?)null, completionTokens = (int?)null, executionDurationInSeconds = (int?)null } },
            operationName = "UpdateAgentTask"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [When(@"I update the agent task complexity rating to (.*)")]
    public void WhenIUpdateTheAgentTaskComplexityRatingTo(int complexityRating)
    {
        var taskId = _scenarioContext["AgentTaskId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation UpdateAgentTask($input: UpdateAgentTaskInput!) { updateAgentTask(input: $input) { agentTask { id } errors { field message } } }",
            variables = new { input = new { id = taskId, title = (string?)null, result = (string?)null, errors = (string?)null, commitHash = (string?)null, dependsOnAgentTaskId = (Guid?)null, description = (string?)null, agent = (string?)null, complexityRating, promptTokens = (int?)null, completionTokens = (int?)null, executionDurationInSeconds = (int?)null } },
            operationName = "UpdateAgentTask"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [When(@"I update the agent task result to ""(.*)""")]
    public void WhenIUpdateTheAgentTaskResultTo(string resultValue)
    {
        var taskId = _scenarioContext["AgentTaskId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation UpdateAgentTask($input: UpdateAgentTaskInput!) { updateAgentTask(input: $input) { agentTask { id } errors { field message } } }",
            variables = new { input = new { id = taskId, title = (string?)null, result = resultValue, errors = (string?)null, commitHash = (string?)null, dependsOnAgentTaskId = (Guid?)null, description = (string?)null, agent = (string?)null, complexityRating = (int?)null, promptTokens = (int?)null, completionTokens = (int?)null, executionDurationInSeconds = (int?)null } },
            operationName = "UpdateAgentTask"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [When(@"I update the agent task commit hash to ""(.*)""")]
    public void WhenIUpdateTheAgentTaskCommitHashTo(string commitHash)
    {
        var taskId = _scenarioContext["AgentTaskId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation UpdateAgentTask($input: UpdateAgentTaskInput!) { updateAgentTask(input: $input) { agentTask { id } errors { field message } } }",
            variables = new { input = new { id = taskId, title = (string?)null, result = (string?)null, errors = (string?)null, commitHash, dependsOnAgentTaskId = (Guid?)null, description = (string?)null, agent = (string?)null, complexityRating = (int?)null, promptTokens = (int?)null, completionTokens = (int?)null, executionDurationInSeconds = (int?)null } },
            operationName = "UpdateAgentTask"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [When(@"I update the agent task model to ""(.*)""")]
    public void WhenIUpdateTheAgentTaskModelTo(string model)
    {
        var taskId = _scenarioContext["AgentTaskId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation UpdateAgentTask($input: UpdateAgentTaskInput!) { updateAgentTask(input: $input) { agentTask { id } errors { field message } } }",
            variables = new { input = new { id = taskId, title = (string?)null, result = (string?)null, errors = (string?)null, commitHash = (string?)null, dependsOnAgentTaskId = (Guid?)null, description = (string?)null, agent = model, complexityRating = (int?)null, promptTokens = (int?)null, completionTokens = (int?)null, executionDurationInSeconds = (int?)null } },
            operationName = "UpdateAgentTask"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [When(@"I transition the agent task status to ""(.*)""")]
    public void WhenITransitionTheAgentTaskStatusTo(string targetStatus)
    {
        var taskId = _scenarioContext["AgentTaskId"]?.ToString()!;
        var targetStatusMapped = MapAgentTaskStatus(targetStatus);
        var mutation = new
        {
            query = @"mutation TransitionAgentTaskStatus($input: TransitionAgentTaskInput!) { transitionAgentTaskStatus(input: $input) { agentTask { id status } errors { field message } } }",
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
        var transitionResponse = GetMutationResult(GetData(result), "transitionAgentTaskStatus");
        var errors = transitionResponse.GetProperty("errors");
        if (errors.ValueKind != JsonValueKind.Null && errors.GetArrayLength() > 0)
        {
            var errorMessages = new StringBuilder();
            foreach (var error in errors.EnumerateArray())
            {
                errorMessages.Append(error.GetString());
            }
            throw new InvalidOperationException($"TransitionAgentTaskStatus failed: {errorMessages}");
        }
        _scenarioContext["Response"] = result;
    }

    [When(@"I delete the agent task")]
    public void WhenIDeleteTheAgentTask()
    {
        var taskId = _scenarioContext["AgentTaskId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation DeleteAgentTask($input: DeleteAgentTaskInput!) { deleteAgentTask(input: $input) { agentTask { id } errors { field message } } }",
            variables = new { input = new { id = taskId } },
            operationName = "DeleteAgentTask"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [When(@"I query the agent task by id")]
    public void WhenIQueryTheAgentTaskById()
    {
        var taskId = _scenarioContext["AgentTaskId"]?.ToString()!;
        var query = new
        {
            query = @"query GetAgentTaskById($id: UUID!) { agentTaskById(id: $id) { id title status complexityRating } }",
            variables = new { id = taskId },
            operationName = "GetAgentTaskById"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [When(@"I query agent tasks by deliverable id")]
    public void WhenIQueryAgentTasksByDeliverableId()
    {
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString()!;
        var query = new
        {
            query = @"query GetAgentTasksByDeliverableId($deliverableId: UUID!) { agentTasksByDeliverableId(deliverableId: $deliverableId) { id title status } }",
            variables = new { deliverableId },
            operationName = "GetAgentTasksByDeliverableId"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [Then(@"the agent task should be created successfully")]
    public void ThenTheAgentTaskShouldBeCreatedSuccessfully()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        HasErrors(response, "createAgentTask").Should().BeFalse("errors should be empty");
    }

    [Then(@"the agent task should be updated successfully")]
    public void ThenTheAgentTaskShouldBeUpdatedSuccessfully()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        HasErrors(response, "updateAgentTask").Should().BeFalse("errors should be empty");
    }

    [Then(@"the agent task should be deleted successfully")]
    public void ThenTheAgentTaskShouldBeDeletedSuccessfully()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        HasErrors(response, "deleteAgentTask").Should().BeFalse("errors should be empty");
    }

    [Then(@"the agent task should exist in the database")]
    public void ThenTheAgentTaskShouldExistInTheDatabase()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        var agentTask = GetNonNullData(GetMutationResult(GetData(response), "createAgentTask"), "agentTask", "createAgentTask");
        agentTask.ValueKind.Should().NotBe(JsonValueKind.Null);
        var taskId = agentTask.GetProperty("id").ToString();
        taskId.Should().NotBeNullOrEmpty();
        _scenarioContext["AgentTaskId"] = taskId;
    }

    [Then(@"the agent task status should be ""(.*)""")]
    public void ThenTheAgentTaskStatusShouldBe(string expectedStatus)
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        var agentTask = GetNonNullData(GetMutationResult(GetData(response), "transitionAgentTaskStatus"), "agentTask", "transitionAgentTaskStatus");
        var status = agentTask.GetProperty("status").ToString();
        var expectedMapped = MapAgentTaskStatus(expectedStatus);
        status.Should().BeEquivalentTo(expectedMapped);
    }

    [Then(@"the agent task should not exist in the database")]
    public void ThenTheAgentTaskShouldNotExistInDatabase()
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
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var agentTask = result.GetProperty("data").GetProperty("agentTaskById");
        agentTask.ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Then(@"the agent task should be returned with correct data")]
    public void ThenTheAgentTaskShouldBeReturnedWithCorrectData()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        var agentTask = response.GetProperty("data").GetProperty("agentTaskById");
        agentTask.ValueKind.Should().NotBe(JsonValueKind.Null);
        var taskId = agentTask.GetProperty("id").ToString();
        taskId.Should().NotBeNullOrEmpty();
        taskId.Should().Be(_scenarioContext["AgentTaskId"]?.ToString());
    }

    [Then(@"the agent tasks list should contain the created task")]
    public void ThenTheAgentTasksListShouldContainTheCreatedTask()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        var tasks = response.GetProperty("data").GetProperty("agentTasksByDeliverableId");
        tasks.ValueKind.Should().Be(JsonValueKind.Array);
        var taskId = _scenarioContext["AgentTaskId"]?.ToString();
        var found = false;
        foreach (var t in tasks.EnumerateArray())
        {
            if (t.GetProperty("id").ToString() == taskId)
            {
                found = true;
                break;
            }
        }
        found.Should().BeTrue("The agent tasks list should contain the created task");
    }

    private static string MapAgentTaskStatus(string status)
    {
        var lower = status.ToLowerInvariant();
        return lower switch
        {
            "ready" => "READY",
            "in_progress" or "inprogress" or "in progress" => "IN_PROGRESS",
            "done" => "DONE",
            "failed" => "FAILED",
            "rejected" => "REJECTED",
            "needs_review" or "needsreview" or "needs review" => "NEEDS_REVIEW",
            _ => status.ToUpperInvariant().Replace(" ", "_")
        };
    }
}
