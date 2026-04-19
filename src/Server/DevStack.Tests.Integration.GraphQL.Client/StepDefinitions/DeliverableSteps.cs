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

    private static bool HasErrors(JsonElement response, string mutationName)
    {
        var errors = response.GetProperty("data").GetProperty(mutationName).GetProperty("errors");
        return errors.ValueKind != JsonValueKind.Null && errors.GetArrayLength() > 0;
    }

    public DeliverableSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _httpClient = SpecFlowHooks.GetHttpClient(scenarioContext);
    }

    private DeliverableType ResolveDeliverableType(string name)
    {
        var lower = name.ToLowerInvariant();
        if (lower.Contains("feature") || lower.Contains("deliverable")) return DeliverableType.Feature;
        if (lower.Contains("defect")) return DeliverableType.Defect;
        return DeliverableType.Maintenance;
    }

    [Given(@"a parent project exists")]
    public void GivenAParentProjectExists()
    {
        var mutation = new
        {
            query = @"mutation CreateProject($input: CreateProjectInput!) { createProject(input: $input) { project { id } errors } }",
            variables = new { input = new { name = "Test Project", description = "Test description", repository = string.Empty } },
            operationName = "CreateProject"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var projectId = result.GetProperty("data").GetProperty("createProject").GetProperty("project").GetProperty("id").ToString();
        _scenarioContext["ProjectId"] = projectId;
    }

    [Given(@"a parent feature exists")]
    public void GivenAParentFeatureExists()
    {
        var projectId = _scenarioContext["ProjectId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation CreateDeliverable($input: CreateDeliverableInput!) { createDeliverable(input: $input) { deliverable { id } errors } }",
            variables = new { input = new { projectId, title = "Parent Feature", type = "Feature", description = (string?)null, acceptanceCriteria = (string?)null, agentFeedback = (string?)null, executionPlan = (string?)null, securityImpact = (string?)null, performanceImpact = (string?)null, testPlan = (string?)null, deploymentPlan = (string?)null, blocking = (string?)null, initialStatus = "PLANNING" } },
            operationName = "CreateDeliverable"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var deliverableId = result.GetProperty("data").GetProperty("createDeliverable").GetProperty("deliverable").GetProperty("id").ToString();
        _scenarioContext["DeliverableId"] = deliverableId;
    }

    [Given(@"a (?:feature|deliverable|defect) ""(.*)"" exists")]
    public void GivenADeliverableExists(string title)
    {
        var projectId = _scenarioContext["ProjectId"]?.ToString()!;
        var type = ResolveDeliverableType(title);
        _scenarioContext["DeliverableTitle"] = title;
        CreateDeliverable(projectId, title, type, null);
    }

    [Given(@"a (?:feature|deliverable|defect) with status ""(.*)"" exists")]
    public void GivenADeliverableWithStatusExists(string status)
    {
        var projectId = _scenarioContext["ProjectId"]?.ToString()!;
        var title = _scenarioContext.ContainsKey("DeliverableTitle") ? _scenarioContext["DeliverableTitle"]?.ToString() ?? "Test Deliverable" : "Test Deliverable";
        var type = ResolveDeliverableType(title);
        CreateDeliverable(projectId, title, type, status);
    }

    [Given(@"a (?:feature|deliverable|defect) ""(.*)"" with type ""(.*)"" exists")]
    public void GivenADeliverableWithTypeExists(string title, string type)
    {
        var projectId = _scenarioContext["ProjectId"]?.ToString()!;
        var deliverableType = ResolveDeliverableType(type);
        CreateDeliverable(projectId, title, deliverableType, null);
    }

    private void CreateDeliverable(string projectId, string title, DeliverableType type, string? initialStatus)
    {
        _scenarioContext["DeliverableTitle"] = title;

        var statusStr = initialStatus ?? "Planning";
        var status = MapStatus(statusStr);
        var mutation = new
        {
            query = @"mutation CreateDeliverable($input: CreateDeliverableInput!) { createDeliverable(input: $input) { deliverable { id } errors } }",
            variables = new
            {
                input = new
                {
                    projectId,
                    title,
                    type = type.ToString(),
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

        var deliverableData = result.GetProperty("data").GetProperty("createDeliverable").GetProperty("deliverable");
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
            "inprogress" or "in progress" => "IN_PROGRESS",
            "done" => "DONE",
            "failed" => "FAILED",
            "rejected" => "REJECTED",
            "needsreview" or "needs review" => "NEEDS_REVIEW",
            "draft" => "DRAFT",
            _ => "PLANNING"
        };
    }

    [When(@"I create a (?:feature|deliverable|defect) with title ""(.*)"" and description ""(.*)""")]
    public void WhenICreateADeliverableWithDescription(string title, string description)
    {
        var projectId = _scenarioContext["ProjectId"]?.ToString()!;
        var type = ResolveDeliverableType(title);
        _scenarioContext["DeliverableTitle"] = title;
        var mutation = new
        {
            query = @"mutation CreateDeliverable($input: CreateDeliverableInput!) { createDeliverable(input: $input) { deliverable { id } errors } }",
            variables = new
            {
                input = new
                {
                    projectId,
                    title,
                    type = type.ToString(),
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
        var deliverableId = result.GetProperty("data").GetProperty("createDeliverable").GetProperty("deliverable").GetProperty("id").ToString();
        _scenarioContext["DeliverableId"] = deliverableId;
        _scenarioContext["Response"] = result;
    }

    [When(@"I update the (?:feature|deliverable|defect) title to ""(.*)""")]
    public void WhenIUpdateTheDeliverableTitleTo(string title)
    {
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation UpdateDeliverable($input: UpdateDeliverableInput!) { updateDeliverable(input: $input) { deliverable { id } errors } }",
            variables = new { input = new { id = deliverableId, title, description = (string?)null, acceptanceCriteria = (string?)null, agentFeedback = (string?)null, executionPlan = (string?)null, securityImpact = (string?)null, performanceImpact = (string?)null, testPlan = (string?)null, deploymentPlan = (string?)null, blocking = (string?)null } },
            operationName = "UpdateDeliverable"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [When(@"I update the (?:feature|deliverable|defect) title to ""(.*)"" and description to ""(.*)""")]
    public void WhenIUpdateTheDeliverableTitleAndDescriptionTo(string title, string description)
    {
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation UpdateDeliverable($input: UpdateDeliverableInput!) { updateDeliverable(input: $input) { deliverable { id } errors } }",
            variables = new { input = new { id = deliverableId, title, description, acceptanceCriteria = (string?)null, agentFeedback = (string?)null, executionPlan = (string?)null, securityImpact = (string?)null, performanceImpact = (string?)null, testPlan = (string?)null, deploymentPlan = (string?)null, blocking = (string?)null } },
            operationName = "UpdateDeliverable"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [When(@"I transition the (?:feature|deliverable|defect) status to ""(.*)""")]
    public void WhenITransitionTheDeliverableStatusTo(string targetStatus)
    {
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString()!;
        var mappedStatus = MapStatus(targetStatus);
        var mutation = new
        {
            query = @"mutation TransitionDeliverableStatus($input: TransitionDeliverableInput!) { transitionDeliverableStatus(input: $input) { deliverable { id status } errors } }",
            variables = new { input = new { id = deliverableId, targetStatus = mappedStatus, actor = "test-user" } },
            operationName = "TransitionDeliverableStatus"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [When(@"I delete the (?:feature|deliverable|defect)")]
    public void WhenIDeleteTheDeliverable()
    {
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString()!;
        var mutation = new
        {
            query = @"mutation DeleteDeliverable($input: DeleteDeliverableInput!) { deleteDeliverable(input: $input) { deliverable { id } errors } }",
            variables = new { input = new { id = deliverableId } },
            operationName = "DeleteDeliverable"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(mutation), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        _scenarioContext["Response"] = result;
    }

    [Then(@"the (?:feature|deliverable|defect) should be created successfully")]
    public void ThenTheDeliverableShouldBeCreatedSuccessfully()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        HasErrors(response, "createDeliverable").Should().BeFalse("errors should be empty");
    }

    [Then(@"the (?:feature|deliverable|defect) should be updated successfully")]
    public void ThenTheDeliverableShouldBeUpdatedSuccessfully()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        HasErrors(response, "updateDeliverable").Should().BeFalse("errors should be empty");
    }

    [Then(@"the (?:feature|deliverable|defect) should be deleted successfully")]
    public void ThenTheDeliverableShouldBeDeletedSuccessfully()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        HasErrors(response, "deleteDeliverable").Should().BeFalse("errors should be empty");
    }

    [Then(@"the (?:feature|deliverable|defect) should exist in the database")]
    public void ThenTheDeliverableShouldExistInTheDatabase()
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        var deliverable = response.GetProperty("data").GetProperty("createDeliverable").GetProperty("deliverable");
        deliverable.ValueKind.Should().NotBe(JsonValueKind.Null);
        var deliverableId = deliverable.GetProperty("id").ToString();
        deliverableId.Should().NotBeNullOrEmpty();
        _scenarioContext["DeliverableId"] = deliverableId;
    }

    [Then(@"the (?:feature|deliverable|defect) status should be ""(.*)""")]
    public void ThenTheDeliverableStatusShouldBe(string expectedStatus)
    {
        var response = (JsonElement)_scenarioContext["Response"]!;
        var deliverable = response.GetProperty("data").GetProperty("transitionDeliverableStatus").GetProperty("deliverable");
        var status = deliverable.GetProperty("status").ToString();
        var mappedExpected = MapStatus(expectedStatus);
        status.Should().BeEquivalentTo(mappedExpected);
    }

    [Then(@"the (?:feature|deliverable|defect) should not exist in the database")]
    public void ThenTheDeliverableShouldNotExistInDatabase()
    {
        var deliverableId = _scenarioContext["DeliverableId"]?.ToString()!;
        var query = new
        {
            query = @"query GetDeliverable($id: UUID!) { deliverableById(id: $id) { id } }",
            variables = new { id = deliverableId },
            operationName = "GetDeliverable"
        };

        var response = _httpClient.PostAsync("", new StringContent(JsonSerializer.Serialize(query), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
        var result = JsonSerializer.Deserialize<JsonElement>(content)!;
        var deliverable = result.GetProperty("data").GetProperty("deliverableById");
        deliverable.ValueKind.Should().Be(JsonValueKind.Null);
    }
}
