using System.Net.Http;
using System.Text;
using System.Text.Json;
using DevStack.Domain.Enums;
using DevStack.Tests.Integration.GraphQL.Client.Hooks;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.GraphQL.Client.StepDefinitions;

[Binding]
public sealed class DeliverableSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly HttpClient _httpClient;

    public DeliverableSteps(ScenarioContext scenarioContext)
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

    [Given(@"a parent project exists")]
    public void GivenAParentProjectExists()
    {
        var mutation = new
        {
            query = @"mutation CreateProject($input: CreateProjectInput!) { createProject(input: $input) { project { id } errors { field message } } }",
            variables = new { input = new { name = "Test Project", description = (string?)null, repository = (string?)null } },
            operationName = "CreateProject"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var projectId = GetNonNullData(GetMutationResult(GetData(result), "createProject"), "project", "createProject").GetProperty("id").ToString();
        _scenarioContext["ProjectId"] = projectId;
    }

    [Given(@"a parent deliverable exists")]
    public void GivenAParentDeliverableExists()
    {
        var projectId = _scenarioContext["ProjectId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation CreateDeliverable($input: CreateDeliverableInput!) { createDeliverable(input: $input) { deliverable { id } errors { field message } } }",
            variables = new { input = new { projectId, title = "Parent Deliverable", type = "Feature", description = (string?)null, acceptanceCriteria = (string?)null, agentFeedback = (string?)null, executionPlan = (string?)null, securityImpact = (string?)null, performanceImpact = (string?)null, testPlan = (string?)null, deploymentPlan = (string?)null, blocking = (string?)null, initialStatus = "PLANNING" } },
            operationName = "CreateDeliverable"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var deliverableId = GetNonNullData(GetMutationResult(GetData(result), "createDeliverable"), "deliverable", "createDeliverable").GetProperty("id").ToString();
        _scenarioContext["DeliverableId"] = deliverableId;
    }

    [Given(@"a deliverable ""(.*)"" type ""(.*)"" exists")]
    public void GivenADeliverableWithTitleAndTypeExists(string title, string type)
    {
        var projectId = _scenarioContext["ProjectId"]?.ToString()!;
        _scenarioContext["DeliverableTitle"] = title;
        CreateDeliverable(projectId, title, type, null);
    }

    [Given(@"a deliverable with status ""(.*)"" type ""(.*)"" exists")]
    public void GivenADeliverableWithStatusAndTypeExists(string status, string type)
    {
        var projectId = _scenarioContext["ProjectId"]?.ToString()!;
        var title = "Test Deliverable";
        CreateDeliverable(projectId, title, type, status);
    }

    private void CreateDeliverable(string projectId, string title, string type, string? initialStatus)
    {
        _scenarioContext["DeliverableTitle"] = title;

        var statusStr = initialStatus ?? "Planning";
        var status = MapStatus(statusStr);
        var mutation = new
        {
            query = @"mutation CreateDeliverable($input: CreateDeliverableInput!) { createDeliverable(input: $input) { deliverable { id } errors { field message } } }",
            variables = new
            {
                input = new
                {
                    projectId,
                    title,
                    type,
                    description = (string?)null,
                    acceptanceCriteria = (string?)null,
                    agentFeedback = (string?)null,
                    executionPlan = (string?)null,
                    securityImpact = (string?)null,
                    performanceImpact = (string?)null,
                    testPlan = (string?)null,
                    deploymentPlan = (string?)null,
                    blocking = (string?)null,
                    initialStatus = status
                }
            },
            operationName = "CreateDeliverable"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;

        var deliverableData = GetNonNullData(GetMutationResult(GetData(result), "createDeliverable"), "deliverable", "createDeliverable");
        var deliverableId = deliverableData.GetProperty("id").ToString();
        _scenarioContext["DeliverableId"] = deliverableId;
    }

    private string MapStatus(string status)
    {
        var lower = status.ToLowerInvariant();
        return lower switch
        {
            "planning" => "PLANNING",
            "ready" => "READY",
            "in_progress" or "inprogress" or "in progress" => "IN_PROGRESS",
            "done" => "DONE",
            "failed" => "FAILED",
            "rejected" => "REJECTED",
            "needs_review" or "needsreview" or "needs review" => "NEEDS_REVIEW",
            "draft" => "DRAFT",
            _ => "PLANNING"
        };
    }

    [When(@"I create a deliverable with title ""(.*)"" type ""(.*)"" and description ""(.*)""")]
    public void WhenICreateADeliverableWithTitleTypeAndDescription(string title, string type, string description)
    {
        var projectId = _scenarioContext["ProjectId"]?.ToString()!;
        _scenarioContext["DeliverableTitle"] = title;
        var mutation = new
        {
            query = @"mutation CreateDeliverable($input: CreateDeliverableInput!) { createDeliverable(input: $input) { deliverable { id } errors { field message } } }",
            variables = new
            {
                input = new
                {
                    projectId,
                    title,
                    type,
                    description,
                    acceptanceCriteria = (string?)null,
                    agentFeedback = (string?)null,
                    executionPlan = (string?)null,
                    securityImpact = (string?)null,
                    performanceImpact = (string?)null,
                    testPlan = (string?)null,
                    deploymentPlan = (string?)null,
                    blocking = (string?)null,
                    initialStatus = "PLANNING"
                }
            },
            operationName = "CreateDeliverable"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var deliverableId = GetNonNullData(GetMutationResult(GetData(result), "createDeliverable"), "deliverable", "createDeliverable").GetProperty("id").ToString();
        _scenarioContext["DeliverableId"] = deliverableId;
        _scenarioContext["Response"] = result;
    }

    [When(@"I create a deliverable with title ""(.*)"" type ""(.*)"" and initial status ""(.*)""")]
    public void WhenICreateADeliverableWithTitleTypeAndInitialStatus(string title, string type, string initialStatus)
    {
        var projectId = _scenarioContext["ProjectId"]?.ToString()!;
        _scenarioContext["DeliverableTitle"] = title;
        var mappedStatus = MapStatus(initialStatus);
        var mutation = new
        {
            query = @"mutation CreateDeliverable($input: CreateDeliverableInput!) { createDeliverable(input: $input) { deliverable { id status } errors { field message } } }",
            variables = new
            {
                input = new
                {
                    projectId,
                    title,
                    type,
                    description = (string?)null,
                    acceptanceCriteria = (string?)null,
                    agentFeedback = (string?)null,
                    executionPlan = (string?)null,
                    securityImpact = (string?)null,
                    performanceImpact = (string?)null,
                    testPlan = (string?)null,
                    deploymentPlan = (string?)null,
                    blocking = (string?)null,
                    initialStatus = mappedStatus
                }
            },
            operationName = "CreateDeliverable"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var deliverableData = GetNonNullData(GetMutationResult(GetData(result), "createDeliverable"), "deliverable", "createDeliverable");
        var deliverableId = deliverableData.GetProperty("id").ToString();
        _scenarioContext["DeliverableId"] = deliverableId;
        _scenarioContext["Response"] = result;
    }

    [When(@"I create a deliverable with title ""(.*)"" type ""(.*)"" description ""(.*)"" acceptance criteria ""(.*)"" agent feedback ""(.*)""")]
    public void WhenICreateADeliverableWithAllFields(string title, string type, string description, string acceptanceCriteria, string agentFeedback)
    {
        var projectId = _scenarioContext["ProjectId"]?.ToString()!;
        _scenarioContext["DeliverableTitle"] = title;
        var mutation = new
        {
            query = @"mutation CreateDeliverable($input: CreateDeliverableInput!) { createDeliverable(input: $input) { deliverable { id } errors { field message } } }",
            variables = new
            {
                input = new
                {
                    projectId,
                    title,
                    type,
                    description,
                    acceptanceCriteria,
                    agentFeedback,
                    executionPlan = (string?)null,
                    securityImpact = (string?)null,
                    performanceImpact = (string?)null,
                    testPlan = (string?)null,
                    deploymentPlan = (string?)null,
                    blocking = (string?)null,
                    initialStatus = "PLANNING"
                }
            },
            operationName = "CreateDeliverable"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var deliverableId = GetNonNullData(GetMutationResult(GetData(result), "createDeliverable"), "deliverable", "createDeliverable").GetProperty("id").ToString();
        _scenarioContext["DeliverableId"] = deliverableId;
        _scenarioContext["Response"] = result;
    }

    [When(@"I update the deliverable title to ""(.*)""")]
    public void WhenIUpdateTheDeliverableTitleTo(string title)
    {
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation UpdateDeliverable($input: UpdateDeliverableInput!) { updateDeliverable(input: $input) { deliverable { id } errors { field message } } }",
            variables = new { input = new { id = deliverableId, title, description = (string?)null, acceptanceCriteria = (string?)null, agentFeedback = (string?)null, executionPlan = (string?)null, securityImpact = (string?)null, performanceImpact = (string?)null, testPlan = (string?)null, deploymentPlan = (string?)null, blocking = (string?)null } },
            operationName = "UpdateDeliverable"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [When(@"I update the deliverable description to ""(.*)""")]
    public void WhenIUpdateTheDeliverableDescriptionTo(string description)
    {
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation UpdateDeliverable($input: UpdateDeliverableInput!) { updateDeliverable(input: $input) { deliverable { id } errors { field message } } }",
            variables = new { input = new { id = deliverableId, title = (string?)null, description, acceptanceCriteria = (string?)null, agentFeedback = (string?)null, executionPlan = (string?)null, securityImpact = (string?)null, performanceImpact = (string?)null, testPlan = (string?)null, deploymentPlan = (string?)null, blocking = (string?)null } },
            operationName = "UpdateDeliverable"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [When(@"I update the deliverable acceptance criteria to ""(.*)""")]
    public void WhenIUpdateTheDeliverableAcceptanceCriteriaTo(string acceptanceCriteria)
    {
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation UpdateDeliverable($input: UpdateDeliverableInput!) { updateDeliverable(input: $input) { deliverable { id } errors { field message } } }",
            variables = new { input = new { id = deliverableId, title = (string?)null, description = (string?)null, acceptanceCriteria, agentFeedback = (string?)null, executionPlan = (string?)null, securityImpact = (string?)null, performanceImpact = (string?)null, testPlan = (string?)null, deploymentPlan = (string?)null, blocking = (string?)null } },
            operationName = "UpdateDeliverable"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [When(@"I transition the deliverable status to ""(.*)""")]
    public void WhenITransitionTheDeliverableStatusTo(string targetStatus)
    {
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString()!;
        var mappedStatus = MapStatus(targetStatus);
        var mutation = new
        {
            query = @"mutation TransitionDeliverableStatus($input: TransitionDeliverableInput!) { transitionDeliverableStatus(input: $input) { deliverable { id status } errors { field message } } }",
            variables = new { input = new { id = deliverableId, targetStatus = mappedStatus, actor = "test-user" } },
            operationName = "TransitionDeliverableStatus"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [When(@"I delete the deliverable")]
    public void WhenIDeleteTheDeliverable()
    {
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation DeleteDeliverable($input: DeleteDeliverableInput!) { deleteDeliverable(input: $input) { deliverable { id } errors { field message } } }",
            variables = new { input = new { id = deliverableId } },
            operationName = "DeleteDeliverable"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [When(@"I query the deliverable by id")]
    public void WhenIQueryTheDeliverableById()
    {
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString()!;
        var query = new
        {
            query = @"query GetDeliverableById($id: UUID!) { deliverableById(id: $id) { id title type status description } }",
            variables = new { id = deliverableId },
            operationName = "GetDeliverableById"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [When(@"I query deliverables by project id")]
    public void WhenIQueryDeliverablesByProjectId()
    {
        var projectId = _scenarioContext["ProjectId"]?.ToString()!;
        var query = new
        {
            query = @"query GetDeliverablesByProjectId($projectId: UUID!) { deliverablesByProjectId(projectId: $projectId) { id title type status } }",
            variables = new { projectId },
            operationName = "GetDeliverablesByProjectId"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [Then(@"the deliverable should be created successfully")]
    public void ThenTheDeliverableShouldBeCreatedSuccessfully()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        HasErrors(response, "createDeliverable").Should().BeFalse("errors should be empty");
    }

    [Then(@"the deliverable should be updated successfully")]
    public void ThenTheDeliverableShouldBeUpdatedSuccessfully()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        HasErrors(response, "updateDeliverable").Should().BeFalse("errors should be empty");
    }

    [Then(@"the deliverable should be deleted successfully")]
    public void ThenTheDeliverableShouldBeDeletedSuccessfully()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        HasErrors(response, "deleteDeliverable").Should().BeFalse("errors should be empty");
    }

    [Then(@"the deliverable should exist in the database")]
    public void ThenTheDeliverableShouldExistInTheDatabase()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        var deliverable = GetData(response).GetProperty("createDeliverable").GetProperty("deliverable");
        deliverable.ValueKind.Should().NotBe(JsonValueKind.Null);
        var deliverableId = deliverable.GetProperty("id").ToString();
        deliverableId.Should().NotBeNullOrEmpty();
        _scenarioContext["DeliverableId"] = deliverableId;
    }

    [Then(@"the deliverable status should be ""(.*)""")]
    public void ThenTheDeliverableStatusShouldBe(string expectedStatus)
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        JsonElement deliverable;
        
        var data = GetData(response);
        if (data.TryGetProperty("createDeliverable", out var createResult))
        {
            deliverable = createResult.GetProperty("deliverable");
        }
        else if (data.TryGetProperty("transitionDeliverableStatus", out var transitionResult))
        {
            var errors = transitionResult.GetProperty("errors");
            if (errors.ValueKind != JsonValueKind.Null && errors.GetArrayLength() > 0)
            {
                throw new InvalidOperationException($"Transition failed: {errors}");
            }
            deliverable = transitionResult.GetProperty("deliverable");
        }
        else
        {
            throw new InvalidOperationException("Response does not contain createDeliverable or transitionDeliverableStatus");
        }
        
        deliverable.ValueKind.Should().NotBe(JsonValueKind.Null, "deliverable should not be null");
        var status = deliverable.GetProperty("status").ToString();
        var mappedExpected = MapStatus(expectedStatus);
        status.Should().BeEquivalentTo(mappedExpected);
    }

    [Then(@"the deliverable should not exist in the database")]
    public void ThenTheDeliverableShouldNotExistInDatabase()
    {
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString()!;
        var query = new
        {
            query = @"query GetDeliverableById($id: UUID!) { deliverableById(id: $id) { id } }",
            variables = new { id = deliverableId },
            operationName = "GetDeliverableById"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var deliverable = GetData(result).GetProperty("deliverableById");
        deliverable.ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Then(@"the deliverable should be returned with correct data")]
    public void ThenTheDeliverableShouldBeReturnedWithCorrectData()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        var deliverable = GetData(response).GetProperty("deliverableById");
        deliverable.ValueKind.Should().NotBe(JsonValueKind.Null);
        var deliverableId = deliverable.GetProperty("id").ToString();
        deliverableId.Should().NotBeNullOrEmpty();
        deliverableId.Should().Be(_scenarioContext["DeliverableId"]?.ToString());
    }

    [Then(@"the deliverables list should contain the created deliverable")]
    public void ThenTheDeliverablesListShouldContainTheCreatedDeliverable()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        var deliverables = GetData(response).GetProperty("deliverablesByProjectId");
        deliverables.ValueKind.Should().Be(JsonValueKind.Array);
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString();
        var found = false;
        foreach (var d in deliverables.EnumerateArray())
        {
            if (d.GetProperty("id").ToString() == deliverableId)
            {
                found = true;
                break;
            }
        }
        found.Should().BeTrue("The deliverables list should contain the created deliverable");
    }
}
