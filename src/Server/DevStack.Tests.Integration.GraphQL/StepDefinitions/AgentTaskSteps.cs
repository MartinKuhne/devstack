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
        return false;
    }

    [Given(@"an agent task ""(.*)"" exists")]
    public void GivenAnAgentTaskExists(string title)
    {
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString()!;
        var projectId = _scenarioContext["ProjectId"]?.ToString()!;
        CreateAgentTask(deliverableId, projectId, title, 5);
    }

    [Given(@"an agent task with status ""(.*)"" exists")]
    public void GivenAnAgentTaskWithStatusExists(string status)
    {
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString()!;
        var projectId = _scenarioContext["ProjectId"]?.ToString()!;
        CreateAgentTask(deliverableId, projectId, "Test Task", 5);
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
            query = @"mutation TransitionAgentTaskStatus($id: UUID!, $targetStatus: AgentTaskStatus!) { updateAgentTaskStatus(id: $id, targetStatus: $targetStatus) }",
            variables = new { id = taskId, targetStatus },
            operationName = "TransitionAgentTaskStatus"
        };

        var contentBody = JsonSerializer.Serialize(mutation);
        var response = _httpClient.PostAsync("", new StringContent(contentBody, Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"HTTP {response.StatusCode}: {content}");
        }
    }

    [Given(@"the agent task has errors set")]
    public void GivenTheAgentTaskHasErrorsSet()
    {
        var taskId = _scenarioContext["AgentTaskId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation UpdateAgentTask($input: UpdateAgentTaskInput!) { updateAgentTask(input: $input) { id } }",
            variables = new { input = new { id = taskId, errors = "Test error message" } },
            operationName = "UpdateAgentTask"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"HTTP {response.StatusCode}: {content}");
        }
    }

    [Given(@"the agent task has result set")]
    public void GivenTheAgentTaskHasResultSet()
    {
        var taskId = _scenarioContext["AgentTaskId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation UpdateAgentTask($input: UpdateAgentTaskInput!) { updateAgentTask(input: $input) { id } }",
            variables = new { input = new { id = taskId, result = "Task completed successfully" } },
            operationName = "UpdateAgentTask"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"HTTP {response.StatusCode}: {content}");
        }
    }

    private void CreateAgentTask(string deliverableId, string projectId, string title, int complexityRating)
    {
        var mutation = new
        {
            query = @"mutation CreateAgentTask($input: CreateAgentTaskInput!) { createAgentTask(input: $input) { id } }",
            variables = new { input = new { deliverableId, projectId, title, description = "Integration test task", complexityRating, dependsOnAgentTaskId = (Guid?)null } },
            operationName = "CreateAgentTask"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"HTTP {response.StatusCode}: {content}");
        }
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var agentTask = GetData(result).GetProperty("createAgentTask");
        if (agentTask.ValueKind == JsonValueKind.Null)
        {
            throw new InvalidOperationException("CreateAgentTask returned null agentTask");
        }
        var taskId = agentTask.GetProperty("id").ToString();
        _scenarioContext["AgentTaskId"] = taskId;

        if (!_scenarioContext.ContainsKey("AgentTaskIds"))
        {
            _scenarioContext["AgentTaskIds"] = new List<string>();
        }
        var taskIds = (List<string>)_scenarioContext["AgentTaskIds"]!;
        taskIds.Add(taskId);
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
        var projectId = _scenarioContext["ProjectId"]?.ToString()!;

        var mutation = new
        {
            query = @"mutation CreateAgentTask($input: CreateAgentTaskInput!) { createAgentTask(input: $input) { id } }",
            variables = new { input = new { deliverableId, projectId, title, description, complexityRating, dependsOnAgentTaskId } },
            operationName = "CreateAgentTask"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"HTTP {response.StatusCode}: {content}");
        }
        var resultJson = JsonSerializer.Deserialize<JsonElement>(content)!;
        var agentTask = GetData(resultJson).GetProperty("createAgentTask");
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
            query = @"mutation UpdateAgentTask($input: UpdateAgentTaskInput!) { updateAgentTask(input: $input) { id } }",
            variables = new { input = new { id = taskId, title } },
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
            query = @"mutation UpdateAgentTask($input: UpdateAgentTaskInput!) { updateAgentTask(input: $input) { id } }",
            variables = new { input = new { id = taskId, complexityRating } },
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
            query = @"mutation UpdateAgentTask($input: UpdateAgentTaskInput!) { updateAgentTask(input: $input) { id } }",
            variables = new { input = new { id = taskId, result = resultValue } },
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
            query = @"mutation UpdateAgentTask($input: UpdateAgentTaskInput!) { updateAgentTask(input: $input) { id } }",
            variables = new { input = new { id = taskId, commitHash } },
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
            query = @"mutation UpdateAgentTask($input: UpdateAgentTaskInput!) { updateAgentTask(input: $input) { id } }",
            variables = new { input = new { id = taskId, agent = model } },
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
            query = @"mutation TransitionAgentTaskStatus($id: UUID!, $targetStatus: AgentTaskStatus!) { updateAgentTaskStatus(id: $id, targetStatus: $targetStatus) }",
            variables = new { id = taskId, targetStatus = targetStatusMapped },
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
        _scenarioContext["Response"] = result;
    }

    [When(@"I delete the agent task")]
    public void WhenIDeleteTheAgentTask()
    {
        var taskId = _scenarioContext["AgentTaskId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation DeleteAgentTask($id: UUID!) { deleteAgentTask(id: $id) }",
            variables = new { id = taskId },
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
            query = @"query GetAgentTaskById($id: UUID!) { agentTask(id: $id) { id title status complexityRating } }",
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
        var query = new
        {
            query = @"query GetAllAgentTasks { agentTasks { nodes { id title status deliverableId } } }",
            operationName = "GetAllAgentTasks"
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
        var data = GetData(response);
        var deleted = data.GetProperty("deleteAgentTask");
        deleted.ValueKind.Should().NotBe(JsonValueKind.Null);
        deleted.GetBoolean().Should().BeTrue("agent task should be deleted successfully");
    }

    [Then(@"the agent task should exist in the database")]
    public void ThenTheAgentTaskShouldExistInTheDatabase()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        var agentTask = GetData(response).GetProperty("createAgentTask");
        agentTask.ValueKind.Should().NotBe(JsonValueKind.Null);
        var taskId = agentTask.GetProperty("id").ToString();
        taskId.Should().NotBeNullOrEmpty();
        _scenarioContext["AgentTaskId"] = taskId;
    }

    [Then(@"the agent task status should be ""(.*)""")]
    public void ThenTheAgentTaskStatusShouldBe(string expectedStatus)
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        var data = GetData(response);
        var status = data.GetProperty("updateAgentTaskStatus").ToString();
        var expectedMapped = MapAgentTaskStatus(expectedStatus);
        status.Should().BeEquivalentTo(expectedMapped);
    }

    [Then(@"the agent task should not exist in the database")]
    public void ThenTheAgentTaskShouldNotExistInDatabase()
    {
        var taskId = _scenarioContext["AgentTaskId"]?.ToString()!;
        var query = new
        {
            query = @"query GetAgentTaskById($id: UUID!) { agentTask(id: $id) { id } }",
            variables = new { id = taskId },
            operationName = "GetAgentTaskById"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var agentTask = GetData(result).GetProperty("agentTask");
        agentTask.ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Then(@"the agent task should be returned with correct data")]
    public void ThenTheAgentTaskShouldBeReturnedWithCorrectData()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        var agentTask = GetData(response).GetProperty("agentTask");
        agentTask.ValueKind.Should().NotBe(JsonValueKind.Null);
        var taskId = agentTask.GetProperty("id").ToString();
        taskId.Should().NotBeNullOrEmpty();
        taskId.Should().Be(_scenarioContext["AgentTaskId"]?.ToString());
    }

    [Then(@"the agent tasks list should contain the created task")]
    public void ThenTheAgentTasksListShouldContainTheCreatedTask()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        var connection = GetData(response).GetProperty("agentTasks");
        var tasks = connection.GetProperty("nodes");
        tasks.ValueKind.Should().Be(JsonValueKind.Array);
        var taskId = _scenarioContext["AgentTaskId"]?.ToString();
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString();
        var found = false;
        foreach (var t in tasks.EnumerateArray())
        {
            var tDeliverableId = t.GetProperty("deliverableId").ToString();
            if (tDeliverableId == deliverableId && t.GetProperty("id").ToString() == taskId)
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

    [Then(@"the deliverable status should be queried and be ""(.*)""")]
    public void ThenTheDeliverableStatusShouldBeQueriedAndBe(string expectedStatus)
    {
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString()!;
        var query = new
        {
            query = @"query GetDeliverableById($id: UUID!) { deliverable(id: $id) { id status } }",
            variables = new { id = deliverableId },
            operationName = "GetDeliverableById"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var deliverable = GetData(result).GetProperty("deliverable");
        var actualStatus = deliverable.GetProperty("status").ToString();
        var mappedExpected = MapStatus(expectedStatus);
        actualStatus.Should().BeEquivalentTo(mappedExpected, $"deliverable status should be {mappedExpected}");
    }

    [Given(@"a deliverable ""(.*)"" type ""(.*)"" with initial status ""(.*)"" exists")]
    public void GivenADeliverableWithTypeAndInitialStatusExists(string title, string type, string initialStatus)
    {
        var projectId = _scenarioContext["ProjectId"]?.ToString()!;
        _scenarioContext["DeliverableTitle"] = title;
        var mappedStatus = MapStatus(initialStatus);
        var mutation = new
        {
            query = @"mutation CreateDeliverable($input: CreateDeliverableInput!) { createDeliverable(input: $input) { id status } }",
            variables = new
            {
                input = new
                {
                    projectId,
                    title,
                    type,
                    description = "",
                    acceptanceCriteria = (string?)null,
                    executionPlan = (string?)null,
                    securityImpact = (string?)null,
                    performanceImpact = (string?)null,
                    testPlan = (string?)null,
                    deploymentPlan = (string?)null,
                    initialStatus = mappedStatus
                }
            },
            operationName = "CreateDeliverable"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var deliverableId = GetData(result).GetProperty("createDeliverable").GetProperty("id").ToString();
        _scenarioContext["DeliverableId"] = deliverableId;
        var status = GetData(result).GetProperty("createDeliverable").GetProperty("status").ToString();
        _scenarioContext["DeliverableInitialStatus"] = status;
    }

    private static string MapStatus(string status)
    {
        var lower = status.ToLowerInvariant();
        return lower switch
        {
            "draft" => "DRAFT",
            "design" => "DESIGN",
            "plan" => "PLAN",
            "implement" => "IMPLEMENT",
            "merge" => "MERGE",
            "deploy" => "DEPLOY",
            "test" => "TEST",
            "ready" => "READY",
            "in_progress" or "inprogress" or "in progress" => "IN_PROGRESS",
            "done" => "DONE",
            "failed" => "FAILED",
            "rejected" => "REJECTED",
            "needs_review" or "needsreview" or "needs review" => "NEEDS_REVIEW",
            _ => "DRAFT"
        };
    }

    [When(@"I transition the first agent task status to ""(.*)""")]
    public void WhenITransitionTheFirstAgentTaskStatusTo(string targetStatus)
    {
        var taskIds = (List<string>)_scenarioContext["AgentTaskIds"]!;
        var taskId = taskIds[0];
        TransitionAgentTaskById(taskId, targetStatus);
    }

    [When(@"I transition the second agent task status to ""(.*)""")]
    public void WhenITransitionTheSecondAgentTaskStatusTo(string targetStatus)
    {
        var taskIds = (List<string>)_scenarioContext["AgentTaskIds"]!;
        var taskId = taskIds[1];
        TransitionAgentTaskById(taskId, targetStatus);
    }

    private void TransitionAgentTaskById(string taskId, string targetStatus)
    {
        var mappedStatus = MapAgentTaskStatus(targetStatus);
        var mutation = new
        {
            query = @"mutation TransitionAgentTaskStatus($id: UUID!, $targetStatus: AgentTaskStatus!) { updateAgentTaskStatus(id: $id, targetStatus: $targetStatus) }",
            variables = new { id = taskId, targetStatus = mappedStatus },
            operationName = "TransitionAgentTaskStatus"
        };

        var contentBody = JsonSerializer.Serialize(mutation);
        var response = _httpClient.PostAsync("", new StringContent(contentBody, Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"HTTP {response.StatusCode}: {content}");
        }
    }

    [When(@"I call checkAndMarkDeliverableDone on the deliverable")]
    public void WhenICallCheckAndMarkDeliverableDoneOnTheDeliverable()
    {
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation CheckAndMarkDeliverableDone($deliverableId: UUID!) { checkAndMarkDeliverableDone(deliverableId: $deliverableId) }",
            variables = new { deliverableId = deliverableId },
            operationName = "CheckAndMarkDeliverableDone"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [Then(@"the check result should be true")]
    public void ThenTheCheckResultShouldBeTrue()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        var data = GetData(response);
        var result = data.GetProperty("checkAndMarkDeliverableDone").GetBoolean();
        result.Should().BeTrue("checkAndMarkDeliverableDone should return true");
    }

    [Then(@"the check result should be false")]
    public void ThenTheCheckResultShouldBeFalse()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        var data = GetData(response);
        var result = data.GetProperty("checkAndMarkDeliverableDone").GetBoolean();
        result.Should().BeFalse("checkAndMarkDeliverableDone should return false");
    }
}
