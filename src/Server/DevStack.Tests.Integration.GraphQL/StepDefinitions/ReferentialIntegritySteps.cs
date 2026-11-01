using System.Net.Http;
using System.Text.Json;

using AwesomeAssertions;

using DevStack.Tests.Integration.GraphQL.Client.Hooks;
using DevStack.Tests.Integration.Shared;

using Reqnroll;

namespace DevStack.Tests.Integration.GraphQL.Client.StepDefinitions;

/// <summary>
/// Referential integrity bindings for <c>GraphQLReferentialIntegrity.feature</c> (GQL-05) and the
/// <c>deleteTestData</c> contract (GQL-06).
/// Every expectation is pinned against an observed payload: the delete behaviours follow the EF
/// configurations in <c>DevStack.Persistence/Persistence/Configurations</c> (project -&gt; deliverables
/// and deliverable -&gt; agent tasks cascade, <c>DependsOnAgentTaskId</c> is set to null), and the
/// <c>projectId</c> of a created agent task is derived by the handler from its deliverable.
/// </summary>
[Binding]
public sealed class ReferentialIntegritySteps
{
    /// <summary>Marker that makes <c>deleteTestData</c> reclaim a row.</summary>
    private const string Marker = "[DeleteAfterTest]";

    private const string FixtureKey = "RiFixture";
    private const string ResponseKey = "RiResponse";
    private const string DeleteFieldKey = "RiDeleteField";

    private const string CreateProjectDocument = """
        mutation RiCreateProject($input: CreateProjectInput!) {
          createProject(input: $input) { id name }
        }
        """;

    private const string CreateDeliverableDocument = """
        mutation RiCreateDeliverable($input: CreateDeliverableInput!) {
          createDeliverable(input: $input) { id title projectId }
        }
        """;

    private const string CreateAgentTaskDocument = """
        mutation RiCreateAgentTask($input: CreateAgentTaskInput!) {
          createAgentTask(input: $input) { id title projectId deliverableId dependsOnAgentTaskId }
        }
        """;

    private const string CreateModelDocument = """
        mutation RiCreateLargeLanguageModel($input: CreateLargeLanguageModelInput!) {
          createLargeLanguageModel(input: $input) { id url }
        }
        """;

    private const string ProjectByIdDocument = """
        query RiProject($id: UUID!) { project(id: $id) { id name } }
        """;

    private const string DeliverableByIdDocument = """
        query RiDeliverable($id: UUID!) { deliverable(id: $id) { id title projectId } }
        """;

    private const string AgentTaskByIdDocument = """
        query RiAgentTask($id: UUID!) {
          agentTask(id: $id) { id title projectId deliverableId dependsOnAgentTaskId }
        }
        """;

    private const string ModelByIdDocument = """
        query RiLargeLanguageModel($id: UUID!) { largeLanguageModel(id: $id) { id url } }
        """;

    private const string DeleteProjectDocument = "mutation RiDeleteProject($id: UUID!) { deleteProject(id: $id) }";
    private const string DeleteDeliverableDocument = "mutation RiDeleteDeliverable($id: UUID!) { deleteDeliverable(id: $id) }";
    private const string DeleteAgentTaskDocument = "mutation RiDeleteAgentTask($id: UUID!) { deleteAgentTask(id: $id) }";
    private const string DeleteModelDocument = "mutation RiDeleteLargeLanguageModel($id: UUID!) { deleteLargeLanguageModel(id: $id) }";

    private const string DeleteTestDataDocument = """
        mutation RiDeleteTestData {
          deleteTestData { success message }
        }
        """;

    /// <summary>The message the <c>deleteTestData</c> resolver returns on the happy path.</summary>
    private const string CleanupSuccessMessage = "Test data cleaned up successfully";

    private readonly ScenarioContext _scenarioContext;
    private readonly HttpClient _httpClient;

    public ReferentialIntegritySteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _httpClient = SpecFlowHooks.GetHttpClient(scenarioContext);
    }

    /// <summary>Rows created by the current scenario, addressable by their short token.</summary>
    private sealed class Fixture(string marker, string slug)
    {
        /// <summary>Full <c>[DeleteAfterTest] R-&lt;slug&gt;</c> prefix used in names, titles and urls.</summary>
        public string Marker { get; } = marker;

        /// <summary>Random suffix that makes the marker unique per scenario.</summary>
        public string Slug { get; } = slug;

        /// <summary>Marked projects, keyed by token.</summary>
        public Dictionary<string, string> Projects { get; } = new(StringComparer.Ordinal);

        /// <summary>Marked deliverables, keyed by token.</summary>
        public Dictionary<string, string> Deliverables { get; } = new(StringComparer.Ordinal);

        /// <summary>Marked agent tasks, keyed by token.</summary>
        public Dictionary<string, string> Tasks { get; } = new(StringComparer.Ordinal);

        /// <summary>Marked large language models, keyed by token.</summary>
        public Dictionary<string, string> Models { get; } = new(StringComparer.Ordinal);

        /// <summary>Projects created without the marker; <c>deleteTestData</c> must leave them alone.</summary>
        public Dictionary<string, string> UnmarkedProjects { get; } = new(StringComparer.Ordinal);

        /// <summary>Deliverables created without the marker.</summary>
        public Dictionary<string, string> UnmarkedDeliverables { get; } = new(StringComparer.Ordinal);

        /// <summary>Agent tasks created without the marker.</summary>
        public Dictionary<string, string> UnmarkedTasks { get; } = new(StringComparer.Ordinal);

        /// <summary>Large language models created without the marker.</summary>
        public Dictionary<string, string> UnmarkedModels { get; } = new(StringComparer.Ordinal);
    }

    // ------------------------------------------------------------------ setup

    /// <summary>Generates the unique per-scenario <c>[DeleteAfterTest]</c> marker used by every marked row.</summary>
    [Given(@"^a unique referential integrity marker$")]
    public void GivenAUniqueReferentialIntegrityMarker()
    {
        var slug = Guid.NewGuid().ToString("N")[..8];
        _scenarioContext[FixtureKey] = new Fixture($"{Marker} R-{slug}", slug);
    }

    /// <summary>Creates a project whose name carries the scenario marker.</summary>
    [Given(@"^a referential integrity project ""([^""]+)""$")]
    public void GivenAReferentialIntegrityProject(string token)
    {
        var fixture = Current();
        var name = $"{fixture.Marker} project {token}";

        var result = Execute(
            CreateProjectDocument,
            new { input = new { name, description = $"{fixture.Marker} description", repository = $"https://example.com/{fixture.Slug}/{token}" } },
            "RiCreateProject");

        NoErrors(result);
        fixture.Projects[token] = result.Field("createProject").GetProperty("id").GetString()!;
    }

    /// <summary>Creates a deliverable whose title carries the scenario marker inside a marked project.</summary>
    [Given(@"^a referential integrity deliverable ""([^""]+)"" in project ""([^""]+)""$")]
    public void GivenAReferentialIntegrityDeliverable(string token, string projectToken)
    {
        var fixture = Current();
        fixture.Deliverables[token] = CreateDeliverable($"{fixture.Marker} deliverable {token}", ProjectId(projectToken));
    }

    /// <summary>Creates an agent task whose title carries the scenario marker inside a marked deliverable.</summary>
    [Given(@"^a referential integrity agent task ""([^""]+)"" in deliverable ""([^""]+)""$")]
    public void GivenAReferentialIntegrityAgentTask(string token, string deliverableToken)
    {
        var fixture = Current();
        fixture.Tasks[token] = CreateAgentTask(
            DeliverableId(deliverableToken),
            $"{fixture.Marker} task {token}",
            dependsOnAgentTaskId: null,
            projectIdOverride: null);
    }

    /// <summary>Creates a marked agent task that depends on another marked task of the scenario.</summary>
    [Given(@"^a referential integrity agent task ""([^""]+)"" in deliverable ""([^""]+)"" depending on task ""([^""]+)""$")]
    public void GivenAReferentialIntegrityAgentTaskDependingOnTask(string token, string deliverableToken, string dependencyToken)
    {
        var fixture = Current();
        fixture.Tasks[token] = CreateAgentTask(
            DeliverableId(deliverableToken),
            $"{fixture.Marker} task {token}",
            dependsOnAgentTaskId: TaskId(dependencyToken),
            projectIdOverride: null);
    }

    /// <summary>Creates a large language model whose url carries the scenario marker.</summary>
    [Given(@"^a referential integrity large language model ""([^""]+)""$")]
    public void GivenAReferentialIntegrityLargeLanguageModel(string token)
    {
        var fixture = Current();
        fixture.Models[token] = CreateModel($"{fixture.Marker} model {token}", $"https://example.com/{Marker}/{fixture.Slug}/{token}");
    }

    // ------------------------------------------------- unmarked fixtures (GQL-06)

    /// <summary>Creates a project whose name deliberately does NOT carry the marker.</summary>
    [Given(@"^an unmarked referential integrity project ""([^""]+)""$")]
    public void GivenAnUnmarkedReferentialIntegrityProject(string token)
    {
        var fixture = Current();
        var name = $"Ri unmarked project {token} {fixture.Slug}";

        var result = Execute(
            CreateProjectDocument,
            new { input = new { name, description = "unmarked description", repository = $"https://example.com/{fixture.Slug}/unmarked-{token}" } },
            "RiCreateProject");

        NoErrors(result);
        name.Should().NotContain(Marker, "an unmarked fixture must not be reclaimable by deleteTestData");
        fixture.UnmarkedProjects[token] = result.Field("createProject").GetProperty("id").GetString()!;
    }

    /// <summary>Creates a deliverable without the marker inside an unmarked project.</summary>
    [Given(@"^an unmarked referential integrity deliverable ""([^""]+)"" in project ""([^""]+)""$")]
    public void GivenAnUnmarkedReferentialIntegrityDeliverable(string token, string projectToken)
    {
        var fixture = Current();
        fixture.UnmarkedDeliverables[token] = CreateDeliverable($"Ri unmarked deliverable {token} {fixture.Slug}", UnmarkedProjectId(projectToken));
    }

    /// <summary>Creates an agent task without the marker inside an unmarked deliverable.</summary>
    [Given(@"^an unmarked referential integrity agent task ""([^""]+)"" in deliverable ""([^""]+)""$")]
    public void GivenAnUnmarkedReferentialIntegrityAgentTask(string token, string deliverableToken)
    {
        var fixture = Current();
        fixture.UnmarkedTasks[token] = CreateAgentTask(
            UnmarkedDeliverableId(deliverableToken),
            $"Ri unmarked task {token} {fixture.Slug}",
            dependsOnAgentTaskId: null,
            projectIdOverride: null);
    }

    /// <summary>Creates a large language model whose url deliberately does NOT carry the marker.</summary>
    [Given(@"^an unmarked referential integrity large language model ""([^""]+)""$")]
    public void GivenAnUnmarkedReferentialIntegrityLargeLanguageModel(string token)
    {
        var fixture = Current();
        var url = $"https://example.com/unmarked/{fixture.Slug}/{token}";
        url.Should().NotContain(Marker, "an unmarked fixture must not be reclaimable by deleteTestData");
        fixture.UnmarkedModels[token] = CreateModel($"Ri unmarked model {token} {fixture.Slug}", url);
    }

    // ------------------------------------------------------------------ mutations

    /// <summary>Deletes a marked project of the scenario.</summary>
    [When(@"^I delete the referential integrity project ""([^""]+)""$")]
    public void WhenIDeleteTheReferentialIntegrityProject(string token)
        => RunDelete(DeleteProjectDocument, "RiDeleteProject", "deleteProject", ProjectId(token));

    /// <summary>Deletes a marked deliverable of the scenario.</summary>
    [When(@"^I delete the referential integrity deliverable ""([^""]+)""$")]
    public void WhenIDeleteTheReferentialIntegrityDeliverable(string token)
        => RunDelete(DeleteDeliverableDocument, "RiDeleteDeliverable", "deleteDeliverable", DeliverableId(token));

    /// <summary>Deletes a marked agent task of the scenario.</summary>
    [When(@"^I delete the referential integrity agent task ""([^""]+)""$")]
    public void WhenIDeleteTheReferentialIntegrityAgentTask(string token)
        => RunDelete(DeleteAgentTaskDocument, "RiDeleteAgentTask", "deleteAgentTask", TaskId(token));

    /// <summary>Creates a marked agent task and reports the payload so the create itself can be asserted.</summary>
    [When(@"^I create a referential integrity agent task ""([^""]+)"" in deliverable ""([^""]+)""$")]
    public void WhenICreateAReferentialIntegrityAgentTask(string token, string deliverableToken)
    {
        var fixture = Current();
        var id = CreateAgentTask(DeliverableId(deliverableToken), $"{fixture.Marker} task {token}", null, null, out var result);
        fixture.Tasks[token] = id;
        Store(result, null);
    }

    /// <summary>Creates a marked agent task that depends on a task of another deliverable or project.</summary>
    [When(@"^I create a referential integrity agent task ""([^""]+)"" in deliverable ""([^""]+)"" depending on task ""([^""]+)""$")]
    public void WhenICreateAReferentialIntegrityAgentTaskDependingOnTask(string token, string deliverableToken, string dependencyToken)
    {
        var fixture = Current();
        var id = CreateAgentTask(
            DeliverableId(deliverableToken),
            $"{fixture.Marker} task {token}",
            TaskId(dependencyToken),
            null,
            out var result);
        fixture.Tasks[token] = id;
        Store(result, null);
    }

    /// <summary>
    /// Creates a marked agent task whose input <c>projectId</c> is the project of another deliverable,
    /// i.e. a projectId that contradicts the deliverable it is attached to.
    /// </summary>
    [When(@"^I create a referential integrity agent task ""([^""]+)"" in deliverable ""([^""]+)"" claiming project ""([^""]+)""$")]
    public void WhenICreateAReferentialIntegrityAgentTaskClaimingProject(string token, string deliverableToken, string projectToken)
    {
        var fixture = Current();
        var id = CreateAgentTask(
            DeliverableId(deliverableToken),
            $"{fixture.Marker} task {token}",
            null,
            ProjectId(projectToken),
            out var result);
        fixture.Tasks[token] = id;
        Store(result, null);
    }

    /// <summary>Creates a marked agent task whose input <c>projectId</c> points at a project that does not exist.</summary>
    [When(@"^I create a referential integrity agent task ""([^""]+)"" in deliverable ""([^""]+)"" claiming a project that does not exist$")]
    public void WhenICreateAReferentialIntegrityAgentTaskClaimingAnUnknownProject(string token, string deliverableToken)
    {
        var fixture = Current();
        var id = CreateAgentTask(
            DeliverableId(deliverableToken),
            $"{fixture.Marker} task {token}",
            null,
            Guid.NewGuid().ToString(),
            out var result);
        fixture.Tasks[token] = id;
        Store(result, null);
    }

    /// <summary>Creates a marked deliverable inside a project that is not the project of any other deliverable of the scenario.</summary>
    [When(@"^I create a referential integrity deliverable ""([^""]+)"" in project ""([^""]+)""$")]
    public void WhenICreateAReferentialIntegrityDeliverable(string token, string projectToken)
    {
        var fixture = Current();
        var title = $"{fixture.Marker} deliverable {token}";
        var result = Execute(
            CreateDeliverableDocument,
            new
            {
                input = new
                {
                    projectId = ProjectId(projectToken),
                    title,
                    type = "Feature",
                    description = "",
                    initialStatus = "DRAFT"
                }
            },
            "RiCreateDeliverable");

        fixture.Deliverables[token] = result.Field("createDeliverable").GetProperty("id").GetString()!;
        Store(result, null);
    }

    /// <summary>Runs the <c>deleteTestData</c> mutation and keeps the payload for assertions.</summary>
    [When(@"^I call deleteTestData$")]
    public void WhenICallDeleteTestData()
    {
        var result = Execute(DeleteTestDataDocument, null, "RiDeleteTestData");
        Store(result, null);
    }

    /// <summary>Reclaims the unmarked rows of the scenario with the by-id delete mutations.</summary>
    [When(@"^I delete the unmarked referential integrity rows$")]
    public void WhenIDeleteTheUnmarkedReferentialIntegrityRows()
    {
        var fixture = Current();

        foreach (var id in fixture.UnmarkedProjects.Values)
        {
            var result = Execute(DeleteProjectDocument, new { id }, "RiDeleteProject");
            NoErrors(result);
            result.Field("deleteProject").GetBoolean().Should().BeTrue($"deleting the unmarked project {id} should report true");
        }

        foreach (var id in fixture.UnmarkedModels.Values)
        {
            var result = Execute(DeleteModelDocument, new { id }, "RiDeleteLargeLanguageModel");
            NoErrors(result);
            result.Field("deleteLargeLanguageModel").GetBoolean().Should().BeTrue($"deleting the unmarked model {id} should report true");
        }
    }

    // ------------------------------------------------------------------ assertions

    /// <summary>Asserts the last delete mutation returned no errors and <c>true</c>.</summary>
    [Then(@"^the referential integrity delete mutation should return true$")]
    public void ThenTheReferentialIntegrityDeleteMutationShouldReturnTrue()
    {
        var result = Last;
        var field = _scenarioContext.TryGetValue<string>(DeleteFieldKey, out var name) && !string.IsNullOrEmpty(name)
            ? name
            : throw new InvalidOperationException("No delete mutation has been executed.");

        NoErrors(result);
        result.Field(field).GetBoolean().Should().BeTrue($"{field} should report true");
    }

    /// <summary>Asserts the last create mutation returned no errors.</summary>
    [Then(@"^the referential integrity mutation should report no errors$")]
    public void ThenTheReferentialIntegrityMutationShouldReportNoErrors() => NoErrors(Last);

    /// <summary>Asserts a marked project is gone.</summary>
    [Then(@"^the referential integrity project ""([^""]+)"" should no longer exist$")]
    public void ThenTheReferentialIntegrityProjectShouldNoLongerExist(string token)
        => Absent(ProjectByIdDocument, "RiProject", "project", ProjectId(token));

    /// <summary>Asserts a marked deliverable is gone.</summary>
    [Then(@"^the referential integrity deliverable ""([^""]+)"" should no longer exist$")]
    public void ThenTheReferentialIntegrityDeliverableShouldNoLongerExist(string token)
        => Absent(DeliverableByIdDocument, "RiDeliverable", "deliverable", DeliverableId(token));

    /// <summary>Asserts a marked agent task is gone.</summary>
    [Then(@"^the referential integrity agent task ""([^""]+)"" should no longer exist$")]
    public void ThenTheReferentialIntegrityAgentTaskShouldNoLongerExist(string token)
        => Absent(AgentTaskByIdDocument, "RiAgentTask", "agentTask", TaskId(token));

    /// <summary>Asserts a marked deliverable still exists and persists the given project as its projectId.</summary>
    [Then(@"^the referential integrity deliverable ""([^""]+)"" should persist project ""([^""]+)""$")]
    public void ThenTheReferentialIntegrityDeliverableShouldPersistProject(string token, string projectToken)
    {
        var deliverable = Present(DeliverableByIdDocument, "RiDeliverable", "deliverable", DeliverableId(token));
        deliverable.GetProperty("projectId").GetString().Should().Be(ProjectId(projectToken));
    }

    /// <summary>Asserts a marked agent task still exists and persists the given project as its projectId.</summary>
    [Then(@"^the referential integrity agent task ""([^""]+)"" should persist project ""([^""]+)""$")]
    public void ThenTheReferentialIntegrityAgentTaskShouldPersistProject(string token, string projectToken)
    {
        var task = Present(AgentTaskByIdDocument, "RiAgentTask", "agentTask", TaskId(token));
        task.GetProperty("projectId").GetString().Should().Be(
            ProjectId(projectToken),
            "the handler derives the projectId from the deliverable, not from the input");
    }

    /// <summary>Asserts a marked agent task still exists and still references the given deliverable.</summary>
    [Then(@"^the referential integrity agent task ""([^""]+)"" should persist deliverable ""([^""]+)""$")]
    public void ThenTheReferentialIntegrityAgentTaskShouldPersistDeliverable(string token, string deliverableToken)
    {
        var task = Present(AgentTaskByIdDocument, "RiAgentTask", "agentTask", TaskId(token));
        task.GetProperty("deliverableId").GetString().Should().Be(DeliverableId(deliverableToken));
    }

    /// <summary>Asserts a marked agent task still exists and still references the given dependency task.</summary>
    [Then(@"^the referential integrity agent task ""([^""]+)"" should persist dependency task ""([^""]+)""$")]
    public void ThenTheReferentialIntegrityAgentTaskShouldPersistDependencyTask(string token, string dependencyToken)
    {
        var task = Present(AgentTaskByIdDocument, "RiAgentTask", "agentTask", TaskId(token));
        task.GetProperty("dependsOnAgentTaskId").GetString().Should().Be(TaskId(dependencyToken));
    }

    /// <summary>
    /// Asserts a marked agent task survives the deletion of the task it depended on and that its
    /// <c>dependsOnAgentTaskId</c> was cleared (<c>DeleteBehavior.SetNull</c>).
    /// </summary>
    [Then(@"^the referential integrity agent task ""([^""]+)"" should survive with its dependency cleared$")]
    public void ThenTheReferentialIntegrityAgentTaskShouldSurviveWithDependencyCleared(string token)
    {
        var task = Present(AgentTaskByIdDocument, "RiAgentTask", "agentTask", TaskId(token));
        task.GetProperty("dependsOnAgentTaskId").ValueKind.Should().Be(
            JsonValueKind.Null,
            "the dependency FK is configured with DeleteBehavior.SetNull but was {0}",
            task.GetProperty("dependsOnAgentTaskId").GetRawText());
    }

    /// <summary>Asserts every marked row of the scenario was reclaimed by <c>deleteTestData</c>.</summary>
    [Then(@"^the marked referential integrity rows should all be gone$")]
    public void ThenTheMarkedRowsShouldAllBeGone()
    {
        var fixture = Current();

        foreach (var id in fixture.Projects.Values)
        {
            Absent(ProjectByIdDocument, "RiProject", "project", id);
        }

        foreach (var id in fixture.Deliverables.Values)
        {
            Absent(DeliverableByIdDocument, "RiDeliverable", "deliverable", id);
        }

        foreach (var id in fixture.Tasks.Values)
        {
            Absent(AgentTaskByIdDocument, "RiAgentTask", "agentTask", id);
        }

        foreach (var id in fixture.Models.Values)
        {
            Absent(ModelByIdDocument, "RiLargeLanguageModel", "largeLanguageModel", id);
        }
    }

    /// <summary>Asserts every unmarked row of the scenario survived <c>deleteTestData</c>.</summary>
    [Then(@"^the unmarked referential integrity rows should all still exist$")]
    public void ThenTheUnmarkedRowsShouldAllStillExist()
    {
        var fixture = Current();

        foreach (var (token, id) in fixture.UnmarkedProjects)
        {
            Present(ProjectByIdDocument, "RiProject", "project", id).GetProperty("name").GetString()
                .Should().Be($"Ri unmarked project {token} {fixture.Slug}");
        }

        foreach (var id in fixture.UnmarkedDeliverables.Values)
        {
            Present(DeliverableByIdDocument, "RiDeliverable", "deliverable", id);
        }

        foreach (var id in fixture.UnmarkedTasks.Values)
        {
            Present(AgentTaskByIdDocument, "RiAgentTask", "agentTask", id);
        }

        foreach (var id in fixture.UnmarkedModels.Values)
        {
            Present(ModelByIdDocument, "RiLargeLanguageModel", "largeLanguageModel", id);
        }
    }

    /// <summary>Asserts the unmarked rows are gone after the scenario deleted them by id.</summary>
    [Then(@"^the unmarked referential integrity rows should all be gone$")]
    public void ThenTheUnmarkedRowsShouldAllBeGone()
    {
        var fixture = Current();

        foreach (var id in fixture.UnmarkedProjects.Values)
        {
            Absent(ProjectByIdDocument, "RiProject", "project", id);
        }

        foreach (var id in fixture.UnmarkedDeliverables.Values)
        {
            Absent(DeliverableByIdDocument, "RiDeliverable", "deliverable", id);
        }

        foreach (var id in fixture.UnmarkedTasks.Values)
        {
            Absent(AgentTaskByIdDocument, "RiAgentTask", "agentTask", id);
        }

        foreach (var id in fixture.UnmarkedModels.Values)
        {
            Absent(ModelByIdDocument, "RiLargeLanguageModel", "largeLanguageModel", id);
        }
    }

    /// <summary>Asserts the <c>deleteTestData</c> payload reports success with the pinned message.</summary>
    [Then(@"^the deleteTestData payload should report success$")]
    public void ThenTheDeleteTestDataPayloadShouldReportSuccess()
    {
        var result = Last;
        NoErrors(result);
        result.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var payload = result.Field("deleteTestData");
        payload.GetProperty("success").GetBoolean().Should().BeTrue("the payload is {0}", result.RawBody);
        payload.GetProperty("message").GetString().Should().Be(CleanupSuccessMessage);
    }

    // ------------------------------------------------------------------ helpers

    /// <summary>Creates a deliverable and returns its id.</summary>
    private string CreateDeliverable(string title, string projectId)
    {
        var result = Execute(
            CreateDeliverableDocument,
            new { input = new { projectId, title, type = "Feature", description = "", initialStatus = "DRAFT" } },
            "RiCreateDeliverable");

        NoErrors(result);
        return result.Field("createDeliverable").GetProperty("id").GetString()!;
    }

    /// <summary>Creates an agent task and returns its id, optionally overriding the input projectId.</summary>
    private string CreateAgentTask(string deliverableId, string title, string? dependsOnAgentTaskId, string? projectIdOverride)
        => CreateAgentTask(deliverableId, title, dependsOnAgentTaskId, projectIdOverride, out _);

    /// <summary>Creates an agent task, exposes the raw payload and returns the new id.</summary>
    private string CreateAgentTask(
        string deliverableId,
        string title,
        string? dependsOnAgentTaskId,
        string? projectIdOverride,
        out GraphQLResult result)
    {
        var deliverable = Present(DeliverableByIdDocument, "RiDeliverable", "deliverable", deliverableId);
        var projectId = projectIdOverride ?? deliverable.GetProperty("projectId").GetString()!;

        result = Execute(
            CreateAgentTaskDocument,
            new { input = new { deliverableId, projectId, title, description = "referential integrity task", complexityRating = 5, dependsOnAgentTaskId } },
            "RiCreateAgentTask");

        NoErrors(result);
        return result.Field("createAgentTask").GetProperty("id").GetString()!;
    }

    /// <summary>Creates a large language model and returns its id.</summary>
    private string CreateModel(string model, string url)
    {
        var result = Execute(
            CreateModelDocument,
            new { input = new { url, model, modelAlias = (string?)null, apiKey = "ri-test-key", maxComplexity = 10 } },
            "RiCreateLargeLanguageModel");

        NoErrors(result);
        return result.Field("createLargeLanguageModel").GetProperty("id").GetString()!;
    }

    /// <summary>Runs a delete mutation and stores both the payload and the schema field it wrote.</summary>
    private void RunDelete(string document, string operationName, string field, string id)
    {
        var result = Execute(document, new { id }, operationName);
        Store(result, field);
    }

    /// <summary>Stores a payload as the scenario's last response, optionally recording the deleted field.</summary>
    private void Store(GraphQLResult result, string? deleteField)
    {
        _scenarioContext[ResponseKey] = result;
        if (deleteField is not null)
        {
            _scenarioContext[DeleteFieldKey] = deleteField;
        }
    }

    /// <summary>Fails when the payload carries errors, using the raw body as the failure message.</summary>
    private static void NoErrors(GraphQLResult result)
        => result.HasErrors.Should().BeFalse("the payload must not carry errors: {0}", result.RawBody);

    /// <summary>Queries a row by id and asserts it is null.</summary>
    private void Absent(string document, string operationName, string field, string id)
    {
        var result = Execute(document, new { id }, operationName);
        NoErrors(result);
        result.IsFieldNull(field).Should().BeTrue("'{0}' should be null for {1} but the payload was: {2}", field, id, result.RawBody);
    }

    /// <summary>Queries a row by id and asserts it is present, returning its fields.</summary>
    private JsonElement Present(string document, string operationName, string field, string id)
    {
        var result = Execute(document, new { id }, operationName);
        NoErrors(result);
        result.IsFieldNull(field).Should().BeFalse("'{0}' should still exist for {1} but the payload was: {2}", field, id, result.RawBody);
        return result.Field(field);
    }

    /// <summary>Posts a GraphQL document and returns the parsed result.</summary>
    private GraphQLResult Execute(string query, object? variables, string operationName)
        => GraphQLRequest.Execute(_httpClient, query, variables, operationName);

    /// <summary>Reads the payload of the last mutation of the scenario.</summary>
    private GraphQLResult Last
        => _scenarioContext.TryGetValue<GraphQLResult>(ResponseKey, out var result) && result is not null
            ? result
            : throw new InvalidOperationException("No referential integrity mutation has been executed.");

    /// <summary>Reads the fixture of the current scenario.</summary>
    private Fixture Current()
        => _scenarioContext.TryGetValue<Fixture>(FixtureKey, out var fixture) && fixture is not null
            ? fixture
            : throw new InvalidOperationException("No referential integrity marker has been generated for this scenario.");

    /// <summary>Resolves the id of a marked project token.</summary>
    private string ProjectId(string token) => Lookup(Current().Projects, "project", token);

    /// <summary>Resolves the id of a marked deliverable token.</summary>
    private string DeliverableId(string token) => Lookup(Current().Deliverables, "deliverable", token);

    /// <summary>Resolves the id of a marked agent task token.</summary>
    private string TaskId(string token) => Lookup(Current().Tasks, "task", token);

    /// <summary>Resolves the id of an unmarked project token.</summary>
    private string UnmarkedProjectId(string token) => Lookup(Current().UnmarkedProjects, "unmarked project", token);

    /// <summary>Resolves the id of an unmarked deliverable token.</summary>
    private string UnmarkedDeliverableId(string token) => Lookup(Current().UnmarkedDeliverables, "unmarked deliverable", token);

    /// <summary>Looks a created row id up by token and fails with a readable message when it is unknown.</summary>
    private static string Lookup(IReadOnlyDictionary<string, string> rows, string kind, string token)
        => rows.TryGetValue(token, out var id)
            ? id
            : throw new InvalidOperationException($"No {kind} '{token}' has been created by this scenario. Known: {string.Join(", ", rows.Keys)}");
}
