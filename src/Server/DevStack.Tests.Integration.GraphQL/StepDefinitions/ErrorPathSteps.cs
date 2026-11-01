using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

using AwesomeAssertions;

using DevStack.Tests.Integration.GraphQL.Client.Hooks;
using DevStack.Tests.Integration.Shared;

using Reqnroll;

namespace DevStack.Tests.Integration.GraphQL.Client.StepDefinitions;

/// <summary>
/// Error and exception path bindings for <c>GraphQLErrorPaths.feature</c>.
/// Every assertion is pinned against an observed response payload, not an assumed contract.
/// </summary>
[Binding]
public sealed class ErrorPathSteps
{
    /// <summary>Marker that makes <c>deleteTestData</c> reclaim a row.</summary>
    private const string Marker = "[DeleteAfterTest]";

    /// <summary>The message HotChocolate substitutes for a masked resolver exception.</summary>
    private const string MaskedMessage = "Unexpected Execution Error";

    private readonly ScenarioContext _scenarioContext;
    private readonly HttpClient _httpClient;

    public ErrorPathSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _httpClient = SpecFlowHooks.GetHttpClient(scenarioContext);
    }

    private GraphQLResult Response => Get<GraphQLResult>("Response");

    /// <summary>
    /// Creates a project, deliverable and agent task so validation failures cannot be confused with missing foreign keys.
    /// </summary>
    [Given(@"a project, deliverable and agent task exist for error testing")]
    public void GivenExistingEntitiesForErrorTesting()
    {
        var project = Execute(
            "mutation($i: CreateProjectInput!) { createProject(input: $i) { id } }",
            new { i = new { name = $"{Marker} error-path project", repository = "https://example.com", description = (string?)null } });

        var projectId = project.Field("createProject").GetProperty("id").ToString();

        var deliverable = Execute(
            "mutation($i: CreateDeliverableInput!) { createDeliverable(input: $i) { id } }",
            new { i = new { projectId, title = $"{Marker} error-path deliverable", type = "Feature", description = "", initialStatus = "PLAN" } });

        var deliverableId = deliverable.Field("createDeliverable").GetProperty("id").ToString();

        var task = Execute(
            "mutation($i: CreateAgentTaskInput!) { createAgentTask(input: $i) { id } }",
            new { i = new { deliverableId, projectId, title = $"{Marker} error-path task", description = "", complexityRating = 5 } });

        var agentTaskId = task.Field("createAgentTask").GetProperty("id").ToString();

        _scenarioContext["ErrorPathProjectId"] = projectId;
        _scenarioContext["ErrorPathDeliverableId"] = deliverableId;
        _scenarioContext["ErrorPathAgentTaskId"] = agentTaskId;
    }

    /// <summary>
    /// Sends a mutation whose id (or parent id) points at a row that does not exist.
    /// </summary>
    [When(@"I attempt ""([^""]+)"" against a missing entity")]
    public void WhenIAttemptAgainstMissingEntity(string mutation)
    {
        if (!MissingEntityCases.TryGetValue(mutation, out var build))
        {
            throw new InvalidOperationException($"Unknown missing-entity case '{mutation}'.");
        }

        var (query, variables) = build(Guid.NewGuid());
        Store(Execute(query, variables));
    }

    /// <summary>
    /// Sends a mutation carrying a value the domain model must reject.
    /// </summary>
    [When(@"I attempt the invalid input case ""([^""]+)"" on the existing entities")]
    public void WhenIAttemptInvalidInput(string testCase)
    {
        if (!InvalidInputCases.TryGetValue(testCase, out var build))
        {
            throw new InvalidOperationException($"Unknown invalid-input case '{testCase}'.");
        }

        var projectId = Get<string>("ErrorPathProjectId");
        var deliverableId = Get<string>("ErrorPathDeliverableId");
        var agentTaskId = Get<string>("ErrorPathAgentTaskId");

        var (query, variables) = build(projectId, deliverableId, agentTaskId);
        Store(Execute(query, variables));
    }

    /// <summary>
    /// Sends a malformed or guarded document that must be rejected by the GraphQL protocol layer.
    /// </summary>
    [When(@"I attempt the protocol case ""([^""]+)""")]
    public void WhenIAttemptProtocolCase(string testCase)
    {
        if (!ProtocolCases.TryGetValue(testCase, out var build))
        {
            throw new InvalidOperationException($"Unknown protocol case '{testCase}'.");
        }

        var (query, variables) = build();
        Store(Execute(query, variables));
    }

    /// <summary>
    /// Sends a by-id query for a row that does not exist.
    /// </summary>
    [When(@"I query ""([^""]+)"" by an unknown id")]
    public void WhenIQueryByUnknownId(string field)
    {
        var selection = field switch
        {
            "project" => "id name",
            "deliverable" or "agentTask" => "id title",
            "largeLanguageModel" => "id model",
            _ => throw new InvalidOperationException($"Unknown by-id query '{field}'.")
        };

        Store(Execute(
            $"query($id: UUID!) {{ {field}(id: $id) {{ {selection} }} }}",
            new { id = Guid.NewGuid() }));
    }

    /// <summary>
    /// Sends three different not-found mutations so their error classification can be compared.
    /// </summary>
    [When(@"I attempt three not-found mutations")]
    public void WhenIAttemptThreeNotFoundMutations()
    {
        var results = new List<GraphQLResult>();
        foreach (var mutation in new[] { "updateProject", "deleteDeliverable", "deleteAgentTask" })
        {
            var (query, variables) = MissingEntityCases[mutation](Guid.NewGuid());
            results.Add(Execute(query, variables));
        }

        _scenarioContext["ErrorPathResponses"] = results;
    }

    /// <summary>
    /// Asserts the observed masked-exception contract: HTTP 200, one error, generic message, path and data shape.
    /// </summary>
    [Then(@"the response should report a masked server error for ""([^""]+)"" with ""([^""]+)""")]
    public void ThenMaskedServerError(string field, string dataShape)
    {
        var response = Response;

        response.StatusCode.Should().Be(HttpStatusCode.OK, $"a resolver exception is still a successful HTTP call: {response}");
        response.HasErrors.Should().BeTrue($"an unknown entity must be reported: {response}");
        response.ErrorCount.Should().Be(1, $"exactly one error is expected: {response}");
        response.FirstErrorMessage.Should().Be(MaskedMessage, "HotChocolate masks the underlying exception message");
        FirstErrorPath().Should().Be(field, "the error must be attributed to the failing field");
        ErrorExtensions().TryGetProperty("code", out _).Should().BeFalse("masked execution errors expose no extensions.code");

        switch (dataShape)
        {
            case "null-data":
                response.DataIsNull.Should().BeTrue($"a non-null field failure nulls the whole data element: {response}");
                break;
            case "null-field":
                response.HasData.Should().BeTrue($"a nullable field keeps partial data: {response}");
                response.IsFieldNull(field).Should().BeTrue($"'{field}' should be null: {response}");
                break;
            default:
                throw new InvalidOperationException($"Unknown data shape '{dataShape}'.");
        }
    }

    /// <summary>
    /// Asserts the request never reached execution: HTTP 400, one error and no data element.
    /// </summary>
    [Then(@"the response should report a request validation error")]
    public void ThenRequestValidationError()
    {
        var response = Response;

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, $"an invalid document is rejected with 400: {response}");
        response.HasErrors.Should().BeTrue($"a validation failure must be reported: {response}");
        response.ErrorCount.Should().Be(1, $"exactly one error is expected: {response}");
        response.HasData.Should().BeFalse($"a rejected document carries no data element: {response}");
    }

    /// <summary>
    /// Asserts the payload carries no errors array.
    /// </summary>
    [Then(@"the response should report no errors")]
    public void ThenNoErrors()
    {
        Response.HasErrors.Should().BeFalse($"expected a clean response: {Response}");
    }

    /// <summary>
    /// Asserts the HTTP status code of the response.
    /// </summary>
    [Then(@"the response should have HTTP status (\d+)")]
    public void ThenHttpStatus(int status)
    {
        ((int)Response.StatusCode).Should().Be(status, $"observed payload: {Response}");
    }

    /// <summary>
    /// Asserts a top-level data field exists and is null.
    /// </summary>
    [Then(@"the response data field ""([^""]+)"" should be null")]
    public void ThenDataFieldIsNull(string field)
    {
        Response.HasData.Should().BeTrue($"expected a data element: {Response}");
        Response.IsFieldNull(field).Should().BeTrue($"'{field}' should be null: {Response}");
    }

    /// <summary>
    /// Asserts a top-level data field exists and carries a value.
    /// </summary>
    [Then(@"the response data field ""([^""]+)"" should not be null")]
    public void ThenDataFieldIsNotNull(string field)
    {
        Response.HasData.Should().BeTrue($"expected a data element: {Response}");
        Response.IsFieldNull(field).Should().BeFalse($"'{field}' should carry a value: {Response}");
    }

    /// <summary>
    /// Asserts a non-null data element is present.
    /// </summary>
    [Then(@"the response should have non-null data")]
    public void ThenHasData()
    {
        Response.HasData.Should().BeTrue($"expected a data element: {Response}");
    }

    /// <summary>
    /// Asserts the payload has no data element at all (rejected document).
    /// </summary>
    [Then(@"the response should have no data element")]
    public void ThenNoDataElement()
    {
        Response.Body.TryGetProperty("data", out _).Should().BeFalse($"no data element is expected: {Response}");
    }

    /// <summary>
    /// Asserts the first error message contains the pinned fragment.
    /// </summary>
    [Then(@"the response error message should contain ""([^""]+)""")]
    public void ThenErrorMessageContains(string fragment)
    {
        Response.HasErrors.Should().BeTrue($"expected an error: {Response}");
        Response.FirstErrorMessage.Should().Contain(fragment, $"observed payload: {Response}");
    }

    /// <summary>
    /// Asserts the first error exposes the pinned stable extensions.code.
    /// </summary>
    [Then(@"the response error code should be ""([^""]+)""")]
    public void ThenErrorCodeIs(string code)
    {
        Response.HasErrors.Should().BeTrue($"expected an error: {Response}");
        Response.FirstErrorCode.Should().Be(code, $"observed payload: {Response}");
    }

    /// <summary>
    /// Asserts the first error exposes no extensions.code.
    /// </summary>
    [Then(@"the response error code should be absent")]
    public void ThenErrorCodeIsAbsent()
    {
        Response.HasErrors.Should().BeTrue($"expected an error: {Response}");
        Response.FirstErrorCode.Should().BeNull($"observed payload: {Response}");
    }

    /// <summary>
    /// Asserts the payload leaks no exception type, message or stack frame.
    /// </summary>
    [Then(@"the response should not leak exception details")]
    public void ThenNoExceptionDetails()
    {
        var raw = Response.RawBody;

        raw.Should().NotContain("at DevStack.", "stack frames must not reach the client");
        raw.Should().NotContain("System.InvalidOperationException", "exception types must not reach the client");
        raw.Should().NotContain("System.ArgumentException", "exception types must not reach the client");
        raw.Should().NotContain("not found.", "handler messages must be masked");
        raw.Should().NotContain("is required", "handler messages must be masked");
        raw.Should().NotContain("characters or less", "handler messages must be masked");
        raw.Should().NotContain("between 1 and 10", "handler messages must be masked");
        raw.Should().NotContain("stackTrace", "no stack trace must be serialised");
    }

    /// <summary>
    /// Asserts three not-found mutations produce an identical error classification.
    /// </summary>
    [Then(@"the three responses should share the same error classification")]
    public void ThenSameClassification()
    {
        var results = Get<List<GraphQLResult>>("ErrorPathResponses");

        results.Should().HaveCount(3);
        results.Should().OnlyContain(result => result.HasErrors, "every not-found mutation must report an error");

        var classifications = results.Select(ClassificationOf).Distinct().ToList();

        classifications.Should().HaveCount(1, $"expected one classification but found: {string.Join(" | ", classifications)}");
        classifications[0].Should().Be($"{MaskedMessage}|no-code|path-present");
    }

    /// <summary>
    /// Reduces a response to the classification triple used to compare error shapes.
    /// </summary>
    private static string ClassificationOf(GraphQLResult result)
    {
        var code = result.FirstErrorCode ?? "no-code";
        var path = result.ErrorsOrEmpty[0].TryGetProperty("path", out _) ? "path-present" : "path-absent";
        return $"{result.FirstErrorMessage}|{code}|{path}";
    }

    /// <summary>
    /// Builds a syntactically valid query that nests <paramref name="levels"/> relation levels
    /// under <c>projects(first: 100)</c> so the computed type cost grows with each level.
    /// </summary>
    private static string BuildCostLimitedQuery(int levels)
    {
        var cycle = new[] { "deliverables { id title ", "agentTasks { id title ", "deliverable { id title ", "project { id name " };

        var query = new System.Text.StringBuilder("{ projects(first: 100) { nodes { id name ");
        for (var i = 0; i < levels; i++)
        {
            query.Append(cycle[i % cycle.Length]);
        }

        query.Append("id ");
        for (var i = 0; i < levels; i++)
        {
            query.Append("} ");
        }

        query.Append("} } }");
        return query.ToString();
    }

    /// <summary>
    /// Mutation documents that address a row which does not exist.
    /// </summary>
    private static readonly Dictionary<string, Func<Guid, (string Query, object Variables)>> MissingEntityCases = new()
    {
        ["updateProject"] = id => (
            "mutation($i: UpdateProjectInput!) { updateProject(input: $i) { id } }",
            new { i = new { id, name = $"{Marker} ghost", description = (string?)null, repository = (string?)null } }),
        ["deleteProject"] = id => (
            "mutation($id: UUID!) { deleteProject(id: $id) }",
            new { id }),
        ["createDeliverable"] = id => (
            "mutation($i: CreateDeliverableInput!) { createDeliverable(input: $i) { id } }",
            new { i = new { projectId = id, title = $"{Marker} ghost", type = "Feature", description = "", initialStatus = "PLAN" } }),
        ["updateDeliverable"] = id => (
            "mutation($i: UpdateDeliverableInput!) { updateDeliverable(input: $i) { id } }",
            new { i = new { id, title = $"{Marker} ghost" } }),
        ["updateDeliverableStatus"] = id => (
            "mutation($id: UUID!, $s: DeliverableStatus!) { updateDeliverableStatus(id: $id, targetStatus: $s) }",
            new { id, s = "DONE" }),
        ["deleteDeliverable"] = id => (
            "mutation($id: UUID!) { deleteDeliverable(id: $id) }",
            new { id }),
        ["createAgentTask"] = id => (
            "mutation($i: CreateAgentTaskInput!) { createAgentTask(input: $i) { id } }",
            new { i = new { deliverableId = id, projectId = id, title = $"{Marker} ghost", description = "", complexityRating = 5 } }),
        ["createAgentTaskDependency"] = id => (
            "mutation($i: CreateAgentTaskInput!) { createAgentTask(input: $i) { id } }",
            new { i = new { deliverableId = id, projectId = id, title = $"{Marker} ghost", description = "", complexityRating = 5, dependsOnAgentTaskId = Guid.NewGuid() } }),
        ["updateAgentTask"] = id => (
            "mutation($i: UpdateAgentTaskInput!) { updateAgentTask(input: $i) { id } }",
            new { i = new { id, title = $"{Marker} ghost" } }),
        ["updateAgentTaskStatus"] = id => (
            "mutation($id: UUID!, $s: AgentTaskStatus!) { updateAgentTaskStatus(id: $id, targetStatus: $s) }",
            new { id, s = "DONE" }),
        ["deleteAgentTask"] = id => (
            "mutation($id: UUID!) { deleteAgentTask(id: $id) }",
            new { id }),
        ["updateLargeLanguageModel"] = id => (
            "mutation($i: UpdateLargeLanguageModelInput!) { updateLargeLanguageModel(input: $i) { id } }",
            new { i = new { id, model = $"{Marker} ghost" } }),
        ["deleteLargeLanguageModel"] = id => (
            "mutation($id: UUID!) { deleteLargeLanguageModel(id: $id) }",
            new { id }),
        ["checkAndMarkDeliverableDone"] = id => (
            "mutation($id: UUID!) { checkAndMarkDeliverableDone(deliverableId: $id) }",
            new { id }),
    };

    /// <summary>
    /// Mutation documents that carry a value the domain model rejects.
    /// </summary>
    private static readonly Dictionary<string, Func<string, string, string, (string Query, object Variables)>> InvalidInputCases = new()
    {
        ["create-project-blank-name"] = (_, _, _) => (
            "mutation($i: CreateProjectInput!) { createProject(input: $i) { id } }",
            new { i = new { name = "   ", repository = "https://example.com", description = (string?)null } }),
        ["create-project-201-name"] = (_, _, _) => (
            "mutation($i: CreateProjectInput!) { createProject(input: $i) { id } }",
            new { i = new { name = new string('n', 201), repository = "https://example.com", description = (string?)null } }),
        ["create-deliverable-empty-title"] = (projectId, _, _) => (
            "mutation($i: CreateDeliverableInput!) { createDeliverable(input: $i) { id } }",
            new { i = new { projectId, title = "", type = "Feature", description = "", initialStatus = "PLAN" } }),
        ["create-deliverable-blank-title"] = (projectId, _, _) => (
            "mutation($i: CreateDeliverableInput!) { createDeliverable(input: $i) { id } }",
            new { i = new { projectId, title = "   ", type = "Feature", description = "", initialStatus = "PLAN" } }),
        ["create-deliverable-201-title"] = (projectId, _, _) => (
            "mutation($i: CreateDeliverableInput!) { createDeliverable(input: $i) { id } }",
            new { i = new { projectId, title = new string('x', 201), type = "Feature", description = "", initialStatus = "PLAN" } }),
        ["update-deliverable-empty-title"] = (_, deliverableId, _) => (
            "mutation($i: UpdateDeliverableInput!) { updateDeliverable(input: $i) { id } }",
            new { i = new { id = deliverableId, title = "" } }),
        ["update-deliverable-201-title"] = (_, deliverableId, _) => (
            "mutation($i: UpdateDeliverableInput!) { updateDeliverable(input: $i) { id } }",
            new { i = new { id = deliverableId, title = new string('x', 201) } }),
        ["create-agent-task-empty-title"] = (projectId, deliverableId, _) => (
            "mutation($i: CreateAgentTaskInput!) { createAgentTask(input: $i) { id } }",
            new { i = new { deliverableId, projectId, title = "", description = "", complexityRating = 5 } }),
        ["create-agent-task-201-title"] = (projectId, deliverableId, _) => (
            "mutation($i: CreateAgentTaskInput!) { createAgentTask(input: $i) { id } }",
            new { i = new { deliverableId, projectId, title = new string('x', 201), description = "", complexityRating = 5 } }),
        ["create-agent-task-complexity-0"] = (projectId, deliverableId, _) => (
            "mutation($i: CreateAgentTaskInput!) { createAgentTask(input: $i) { id } }",
            new { i = new { deliverableId, projectId, title = $"{Marker} rating zero", description = "", complexityRating = 0 } }),
        ["create-agent-task-complexity-11"] = (projectId, deliverableId, _) => (
            "mutation($i: CreateAgentTaskInput!) { createAgentTask(input: $i) { id } }",
            new { i = new { deliverableId, projectId, title = $"{Marker} rating eleven", description = "", complexityRating = 11 } }),
        ["update-agent-task-empty-title"] = (_, _, agentTaskId) => (
            "mutation($i: UpdateAgentTaskInput!) { updateAgentTask(input: $i) { id } }",
            new { i = new { id = agentTaskId, title = "" } }),
        ["update-agent-task-201-title"] = (_, _, agentTaskId) => (
            "mutation($i: UpdateAgentTaskInput!) { updateAgentTask(input: $i) { id } }",
            new { i = new { id = agentTaskId, title = new string('x', 201) } }),
        ["update-agent-task-301-title"] = (_, _, agentTaskId) => (
            "mutation($i: UpdateAgentTaskInput!) { updateAgentTask(input: $i) { id } }",
            new { i = new { id = agentTaskId, title = new string('x', 301) } }),
        ["update-agent-task-complexity-0"] = (_, _, agentTaskId) => (
            "mutation($i: UpdateAgentTaskInput!) { updateAgentTask(input: $i) { id } }",
            new { i = new { id = agentTaskId, complexityRating = 0 } }),
        ["update-agent-task-complexity-11"] = (_, _, agentTaskId) => (
            "mutation($i: UpdateAgentTaskInput!) { updateAgentTask(input: $i) { id } }",
            new { i = new { id = agentTaskId, complexityRating = 11 } }),
        ["create-deliverable-type-lc"] = (projectId, _, _) => (
            "mutation($i: CreateDeliverableInput!) { createDeliverable(input: $i) { id type } }",
            new { i = new { projectId, title = $"{Marker} type lowercase", type = "feature", description = "", initialStatus = "PLAN" } }),
        ["create-deliverable-type-mix"] = (projectId, _, _) => (
            "mutation($i: CreateDeliverableInput!) { createDeliverable(input: $i) { id type } }",
            new { i = new { projectId, title = $"{Marker} type mixed", type = "DeFeCt", description = "", initialStatus = "PLAN" } }),
        ["create-deliverable-type-spike"] = (projectId, _, _) => (
            "mutation($i: CreateDeliverableInput!) { createDeliverable(input: $i) { id type } }",
            new { i = new { projectId, title = $"{Marker} type spike", type = "SPIKE", description = "", initialStatus = "PLAN" } }),
        ["create-deliverable-type-bogus"] = (projectId, _, _) => (
            "mutation($i: CreateDeliverableInput!) { createDeliverable(input: $i) { id type } }",
            new { i = new { projectId, title = $"{Marker} type bogus", type = "NOT_A_TYPE", description = "", initialStatus = "PLAN" } }),
        ["create-deliverable-type-numeric"] = (projectId, _, _) => (
            "mutation($i: CreateDeliverableInput!) { createDeliverable(input: $i) { id type } }",
            new { i = new { projectId, title = $"{Marker} type numeric", type = "999", description = "", initialStatus = "PLAN" } }),
    };

    /// <summary>
    /// Documents that must be rejected (or guarded) by the GraphQL protocol layer.
    /// </summary>
    private static readonly Dictionary<string, Func<(string Query, object? Variables)>> ProtocolCases = new()
    {
        ["malformed-uuid-literal"] = () => (
            "mutation($id: UUID!) { deleteProject(id: $id) }",
            new { id = "not-a-guid" }),
        ["unknown-selection-field"] = () => (
            "query($id: UUID!) { project(id: $id) { id totallyBogusField } }",
            new { id = Guid.NewGuid() }),
        ["missing-required-argument"] = () => (
            "mutation { deleteProject }",
            null),
        ["unknown-argument"] = () => (
            "mutation($id: UUID!) { deleteProject(id: $id, bogus: 1) }",
            new { id = Guid.NewGuid() }),
        ["invalid-enum-literal"] = () => (
            "mutation($id: UUID!) { updateDeliverableStatus(id: $id, targetStatus: BOGUS) }",
            new { id = Guid.NewGuid() }),
        ["invalid-enum-variable"] = () => (
            "mutation($id: UUID!, $s: DeliverableStatus!) { updateDeliverableStatus(id: $id, targetStatus: $s) }",
            new { id = Guid.NewGuid(), s = "BOGUS" }),
        ["missing-required-input"] = () => (
            "mutation($i: CreateLargeLanguageModelInput!) { createLargeLanguageModel(input: $i) { id } }",
            new { i = new { url = "https://example.com" } }),
        ["ambiguous-operation-name"] = () => (
            "query A { projects { nodes { id } } } query B { projects { nodes { id } } }",
            null),
        ["syntax-error"] = () => (
            "{ projects { nodes { ",
            null),
        ["paging-first-101"] = () => ("{ projects(first: 101) { nodes { id } } }", null),
        ["paging-last-101"] = () => ("{ projects(last: 101) { nodes { id } } }", null),
        ["paging-first-100"] = () => ("{ projects(first: 100) { nodes { id } } }", null),
        ["cost-limit"] = () => (BuildCostLimitedQuery(9), null),
        ["introspection"] = () => ("{ __schema { queryType { name } } }", null),
    };

    /// <summary>
    /// Posts a document and records the result for the assertion steps.
    /// </summary>
    private GraphQLResult Execute(string query, object? variables)
        => GraphQLRequest.Execute(_httpClient, query, variables);

    /// <summary>
    /// Stores a response so the following Then step can assert on it.
    /// </summary>
    private void Store(GraphQLResult result) => _scenarioContext["Response"] = result;

    /// <summary>
    /// Reads a required value from the scenario context.
    /// </summary>
    private T Get<T>(string key)
        => _scenarioContext.TryGetValue<T>(key, out var value) && value is not null
            ? value
            : throw new InvalidOperationException($"'{key}' is not set. Check the preceding step.");

    /// <summary>
    /// Reads the path of the first error as a dotted string.
    /// </summary>
    private string FirstErrorPath()
    {
        var errors = Response.ErrorsOrEmpty;
        if (!errors[0].TryGetProperty("path", out var path) || path.ValueKind != System.Text.Json.JsonValueKind.Array)
        {
            return string.Empty;
        }

        return string.Join(".", path.EnumerateArray().Select(segment => segment.ToString()));
    }

    /// <summary>
    /// Reads the extensions element of the first error, or an empty object when there is none.
    /// </summary>
    private System.Text.Json.JsonElement ErrorExtensions()
    {
        var errors = Response.ErrorsOrEmpty;
        return errors[0].TryGetProperty("extensions", out var extensions) &&
               extensions.ValueKind == System.Text.Json.JsonValueKind.Object
            ? extensions
            : System.Text.Json.JsonDocument.Parse("{}").RootElement;
    }
}
