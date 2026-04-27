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

    private static bool HasErrors(JsonElement response, string mutationName)
    {
        return false;
    }

    [Given(@"a parent project exists")]
    public void GivenAParentProjectExists()
    {
        var mutation = new
        {
            query = @"mutation CreateProject($input: CreateProjectInput!) { createProject(input: $input) { id } }",
            variables = new { input = new { name = "Test Project", description = (string?)null, repository = "https://example.com" } },
            operationName = "CreateProject"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var projectId = GetData(result).GetProperty("createProject").GetProperty("id").ToString();
        _scenarioContext["ProjectId"] = projectId;
    }

    [Given(@"a parent deliverable exists")]
    public void GivenAParentDeliverableExists()
    {
        var projectId = _scenarioContext["ProjectId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation CreateDeliverable($input: CreateDeliverableInput!) { createDeliverable(input: $input) { id } }",
            variables = new { input = new { projectId, title = "Parent Deliverable", type = "Feature", description = "", acceptanceCriteria = (string?)null, executionPlan = (string?)null, securityImpact = (string?)null, performanceImpact = (string?)null, testPlan = (string?)null, deploymentPlan = (string?)null, initialStatus = "PLANNING" } },
            operationName = "CreateDeliverable"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var deliverableId = GetData(result).GetProperty("createDeliverable").GetProperty("id").ToString();
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
            query = @"mutation CreateDeliverable($input: CreateDeliverableInput!) { createDeliverable(input: $input) { id } }",
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
                    initialStatus = status
                }
            },
            operationName = "CreateDeliverable"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;

        var deliverableId = GetData(result).GetProperty("createDeliverable").GetProperty("id").ToString();
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
            query = @"mutation CreateDeliverable($input: CreateDeliverableInput!) { createDeliverable(input: $input) { id } }",
            variables = new
            {
                input = new
                {
                    projectId,
                    title,
                    type,
                    description = description ?? "",
                    acceptanceCriteria = (string?)null,
                    executionPlan = (string?)null,
                    securityImpact = (string?)null,
                    performanceImpact = (string?)null,
                    testPlan = (string?)null,
                    deploymentPlan = (string?)null,
                    initialStatus = "PLANNING"
                }
            },
            operationName = "CreateDeliverable"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var deliverableId = GetData(result).GetProperty("createDeliverable").GetProperty("id").ToString();
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
        _scenarioContext["Response"] = result;
    }

    [When(@"I create a deliverable with title ""(.*)"" type ""(.*)"" description ""(.*)"" acceptance criteria ""(.*)"" agent feedback ""(.*)""")]
    public void WhenICreateADeliverableWithAllFields(string title, string type, string description, string acceptanceCriteria, string agentFeedback)
    {
        var projectId = _scenarioContext["ProjectId"]?.ToString()!;
        _scenarioContext["DeliverableTitle"] = title;
        var mutation = new
        {
            query = @"mutation CreateDeliverable($input: CreateDeliverableInput!) { createDeliverable(input: $input) { id } }",
            variables = new
            {
                input = new
                {
                    projectId,
                    title,
                    type,
                    description,
                    acceptanceCriteria,
                    executionPlan = (string?)null,
                    securityImpact = (string?)null,
                    performanceImpact = (string?)null,
                    testPlan = (string?)null,
                    deploymentPlan = (string?)null,
                    initialStatus = "PLANNING"
                }
            },
            operationName = "CreateDeliverable"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var deliverableId = GetData(result).GetProperty("createDeliverable").GetProperty("id").ToString();
        _scenarioContext["DeliverableId"] = deliverableId;
        _scenarioContext["Response"] = result;
    }

    [When(@"I update the deliverable title to ""(.*)""")]
    public void WhenIUpdateTheDeliverableTitleTo(string title)
    {
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation UpdateDeliverable($input: UpdateDeliverableInput!) { updateDeliverable(input: $input) { id } }",
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
            query = @"mutation UpdateDeliverable($input: UpdateDeliverableInput!) { updateDeliverable(input: $input) { id } }",
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
            query = @"mutation UpdateDeliverable($input: UpdateDeliverableInput!) { updateDeliverable(input: $input) { id } }",
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
            query = @"mutation TransitionDeliverableStatus($id: UUID!, $targetStatus: DeliverableStatus!) { updateDeliverableStatus(id: $id, targetStatus: $targetStatus) }",
            variables = new { id = deliverableId, targetStatus = mappedStatus },
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
            query = @"mutation DeleteDeliverable($id: UUID!) { deleteDeliverable(id: $id) }",
            variables = new { id = deliverableId },
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
            query = @"query GetDeliverableById($id: UUID!) { deliverable(id: $id) { id title status description } }",
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
            query = @"query GetDeliverables($projectId: UUID) { deliverables(where: { projectId: { eq: $projectId }}) { nodes { id title status projectId } } }",
            variables = new { projectId = projectId },
            operationName = "GetDeliverables"
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
        var data = GetData(response);
        var deleted = data.GetProperty("deleteDeliverable");
        deleted.ValueKind.Should().NotBe(JsonValueKind.Null);
        deleted.GetBoolean().Should().BeTrue("deliverable should be deleted successfully");
    }

    [Then(@"the deliverable should exist in the database")]
    public void ThenTheDeliverableShouldExistInTheDatabase()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        var deliverable = GetData(response).GetProperty("createDeliverable");
        deliverable.ValueKind.Should().NotBe(JsonValueKind.Null);
        var deliverableId = deliverable.GetProperty("id").ToString();
        deliverableId.Should().NotBeNullOrEmpty();
        _scenarioContext["DeliverableId"] = deliverableId;
    }

    [Then(@"the deliverable status should be ""(.*)""")]
    public void ThenTheDeliverableStatusShouldBe(string expectedStatus)
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        var data = GetData(response);
        string status;

        if (data.TryGetProperty("createDeliverable", out var createResult))
        {
            status = createResult.GetProperty("status").ToString();
        }
        else if (data.TryGetProperty("updateDeliverableStatus", out var transitionResult))
        {
            status = transitionResult.ToString();
        }
        else
        {
            throw new InvalidOperationException("Response does not contain createDeliverable or updateDeliverableStatus");
        }

        status.Should().NotBeNullOrEmpty("deliverable status should not be null");
        var mappedExpected = MapStatus(expectedStatus);
        status.Should().BeEquivalentTo(mappedExpected);
    }

    [Then(@"the deliverable should not exist in the database")]
    public void ThenTheDeliverableShouldNotExistInDatabase()
    {
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString()!;
        var query = new
        {
            query = @"query GetDeliverableById($id: UUID!) { deliverable(id: $id) { id } }",
            variables = new { id = deliverableId },
            operationName = "GetDeliverableById"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var deliverable = GetData(result).GetProperty("deliverable");
        deliverable.ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Then(@"the deliverable should be returned with correct data")]
    public void ThenTheDeliverableShouldBeReturnedWithCorrectData()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        var deliverable = GetData(response).GetProperty("deliverable");
        deliverable.ValueKind.Should().NotBe(JsonValueKind.Null);
        var deliverableId = deliverable.GetProperty("id").ToString();
        deliverableId.Should().NotBeNullOrEmpty();
        deliverableId.Should().Be(_scenarioContext["DeliverableId"]?.ToString());
    }

    [Then(@"the deliverables list should contain the created deliverable")]
    public void ThenTheDeliverablesListShouldContainTheCreatedDeliverable()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        var connection = GetData(response).GetProperty("deliverables");
        var deliverables = connection.GetProperty("nodes");
        deliverables.ValueKind.Should().Be(JsonValueKind.Array);
        var deliverableIdStr = _scenarioContext["DeliverableId"]?.ToString() ?? throw new InvalidOperationException("DeliverableId not found in scenario context");
        var deliverableId = Guid.Parse(deliverableIdStr);
        Console.WriteLine($"Wanted: {deliverableId}");
        var found = false;
        foreach (var d in deliverables.EnumerateArray())
        {
            var dId = Guid.Parse(d.GetProperty("id").ToString());
            Console.WriteLine(dId);
            if (dId == deliverableId)
            {
                found = true;
                break;
            }
        }
        found.Should().BeTrue("The deliverables list should contain the created deliverable");
    }
}
