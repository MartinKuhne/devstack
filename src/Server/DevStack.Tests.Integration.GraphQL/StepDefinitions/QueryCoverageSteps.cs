using System.Net.Http;
using System.Text.Json;

using AwesomeAssertions;

using DevStack.Tests.Integration.GraphQL.Client.Hooks;
using DevStack.Tests.Integration.Shared;

using Reqnroll;

namespace DevStack.Tests.Integration.GraphQL.Client.StepDefinitions;

/// <summary>
/// Step definitions for the GraphQL connection query surface (GQL-08 .. GQL-12):
/// cursor paging, filtering, sorting, pageInfo/edges shape and relation fields.
/// Every scenario works on rows tagged with <c>[DeleteAfterTest]</c> plus a per-scenario
/// marker, and every connection query is additionally filtered on that marker so that no
/// assertion can be influenced by rows leaked by other scenarios.
/// </summary>
[Binding]
public sealed class QueryCoverageSteps
{
    /// <summary>Literal marker that <c>deleteTestData</c> looks for when reclaiming test rows.</summary>
    private const string TestMarker = "[DeleteAfterTest]";

    private const string FixtureKey = "QcFixture";
    private const string PageKey = "QcPage";
    private const string ResultKey = "QcResult";
    private const string PreviousPageIdsKey = "QcPreviousPageIds";
    private const string StartCursorKey = "QcStartCursor";
    private const string EndCursorKey = "QcEndCursor";
    private const string RelationNodesKey = "QcRelationNodes";
    private const string RelationKindKey = "QcRelationKind";
    private const string TaskObjectsKey = "QcRelationTaskObjects";
    private const string TaskPageNodesKey = "QcRelationTaskPageNodes";
    private const string DeliverableObjectKey = "QcRelationDeliverableObject";

    private const string CreateProjectDocument = """
        mutation QcCreateProject($input: CreateProjectInput!) {
          createProject(input: $input) { id name }
        }
        """;

    private const string CreateDeliverableDocument = """
        mutation QcCreateDeliverable($input: CreateDeliverableInput!) {
          createDeliverable(input: $input) { id title status }
        }
        """;

    private const string CreateAgentTaskDocument = """
        mutation QcCreateAgentTask($input: CreateAgentTaskInput!) {
          createAgentTask(input: $input) { id title status }
        }
        """;

    private const string UpdateAgentTaskStatusDocument = """
        mutation QcUpdateAgentTaskStatus($id: UUID!, $targetStatus: AgentTaskStatus!) {
          updateAgentTaskStatus(id: $id, targetStatus: $targetStatus)
        }
        """;

    private const string CreateLargeLanguageModelDocument = """
        mutation QcCreateLargeLanguageModel($input: CreateLargeLanguageModelInput!) {
          createLargeLanguageModel(input: $input) { id url }
        }
        """;

    private const string ProjectRelationsDocument = """
        query QcProjectRelations($id: UUID!) {
          project(id: $id) {
            id
            name
            deliverables { id title }
          }
        }
        """;

    private const string DeliverableRelationsDocument = """
        query QcDeliverableRelations($id: UUID!) {
          deliverable(id: $id) {
            id
            title
            agentTasks { id title }
          }
        }
        """;

    private const string AgentTaskRelationsDocument = """
        query QcAgentTaskRelations($id: UUID!) {
          agentTask(id: $id) {
            id
            title
            deliverableId
            projectId
            dependsOnAgentTaskId
          }
        }
        """;

    // Mirrors src/AdminUi/src/graphql/queries/agentTask.graphql (GetAgentTask, lines 18-26):
    // the UI selects the deliverable and project objects next to the foreign keys.
    private const string AgentTaskRelationObjectsDocument = """
        query QcAgentTaskRelationObjects($id: UUID!) {
          agentTask(id: $id) {
            id
            title
            deliverableId
            projectId
            dependsOnAgentTaskId
            deliverable { id title }
            project { id name }
            dependsOnAgentTask { id title }
          }
        }
        """;

    // Mirrors src/AdminUi/src/graphql/queries/agentTask.graphql (GetAgentTasks, lines 48-55):
    // the UI selects the same objects for every node of the deliverable's task list.
    private const string AgentTaskPageRelationObjectsDocument = """
        query QcAgentTaskPageRelationObjects($where: AgentTaskFilterInput, $first: Int) {
          agentTasks(where: $where, first: $first) {
            nodes {
              id
              title
              deliverableId
              projectId
              deliverable { id title }
              project { id name }
            }
          }
        }
        """;

    // The deliverable-by-id query the AdminUi uses to render a deliverable's project.
    private const string DeliverableRelationObjectDocument = """
        query QcDeliverableRelationObject($id: UUID!) {
          deliverable(id: $id) {
            id
            title
            projectId
            project { id name }
          }
        }
        """;

    private readonly ScenarioContext _scenarioContext;
    private readonly HttpClient _httpClient;

    public QueryCoverageSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _httpClient = SpecFlowHooks.GetHttpClient(scenarioContext);
    }

    /// <summary>A row created by this scenario, addressable by its short token.</summary>
    private sealed record CreatedEntity(string Token, string Id, string Label, IReadOnlyDictionary<string, string> Fields);

    /// <summary>A node of the connection returned by the most recent connection query.</summary>
    private sealed record ConnectionNode(string Id, string Label, JsonElement Raw);

    /// <summary>An edge of the connection returned by the most recent connection query.</summary>
    private sealed record ConnectionEdge(string Cursor, string Id);

    /// <summary>Parsed view of the connection returned by the most recent connection query.</summary>
    private sealed class ConnectionPage
    {
        /// <summary>Schema field name of the connection (projects, deliverables, agentTasks, largeLanguageModels).</summary>
        public required string Kind { get; init; }

        /// <summary>Nodes in the order the server returned them.</summary>
        public required IReadOnlyList<ConnectionNode> Nodes { get; init; }

        /// <summary>Edges in the order the server returned them.</summary>
        public required IReadOnlyList<ConnectionEdge> Edges { get; init; }

        /// <summary>Value of pageInfo.hasNextPage.</summary>
        public required bool HasNextPage { get; init; }

        /// <summary>Value of pageInfo.hasPreviousPage.</summary>
        public required bool HasPreviousPage { get; init; }

        /// <summary>Value of pageInfo.startCursor.</summary>
        public string? StartCursor { get; init; }

        /// <summary>Value of pageInfo.endCursor.</summary>
        public string? EndCursor { get; init; }
    }

    /// <summary>All rows created by the current scenario plus the per-scenario marker.</summary>
    private sealed class ScenarioFixture(string marker, string slug)
    {
        /// <summary>Full <c>[DeleteAfterTest] A-&lt;slug&gt;</c> prefix used in names, titles and urls.</summary>
        public string Marker { get; } = marker;

        /// <summary>Random suffix that makes the marker unique per scenario.</summary>
        public string Slug { get; } = slug;

        /// <summary>Projects created by the scenario.</summary>
        public List<CreatedEntity> Projects { get; } = [];

        /// <summary>Deliverables created by the scenario.</summary>
        public List<CreatedEntity> Deliverables { get; } = [];

        /// <summary>Agent tasks created by the scenario.</summary>
        public List<CreatedEntity> Tasks { get; } = [];

        /// <summary>Large language models created by the scenario.</summary>
        public List<CreatedEntity> Models { get; } = [];
    }

    // ------------------------------------------------------------------ setup

    /// <summary>Verifies that the scenario has a GraphQL endpoint to talk to.</summary>
    [Given(@"^the query coverage GraphQL endpoint is reachable$")]
    public void GivenTheQueryCoverageGraphQlEndpointIsReachable()
    {
        SpecFlowHooks.GetGraphQLUrl(_scenarioContext).Should().EndWith("/graphql");
    }

    /// <summary>Generates the unique per-scenario <c>[DeleteAfterTest]</c> marker used by every created row.</summary>
    [Given(@"^a unique query coverage marker$")]
    public void GivenAUniqueQueryCoverageMarker()
    {
        var slug = Guid.NewGuid().ToString("N")[..8];
        _scenarioContext[FixtureKey] = new ScenarioFixture($"{TestMarker} A-{slug}", slug);
    }

    /// <summary>Creates the listed projects, each named <c>&lt;marker&gt; &lt;token&gt;</c>.</summary>
    [Given(@"^I create query coverage projects ""(.*)""$")]
    public async Task GivenICreateQueryCoverageProjects(string tokens)
    {
        var fixture = Fixture();

        foreach (var token in SplitList(tokens))
        {
            var name = $"{fixture.Marker} {token}";
            var variables = new
            {
                input = new
                {
                    name,
                    description = $"{fixture.Marker} desc {token}",
                    repository = $"https://example.com/{fixture.Slug}/{token}"
                }
            };

            var result = await ExecuteAsync(CreateProjectDocument, variables, "QcCreateProject");
            result.HasErrors.Should().BeFalse(result.RawBody);

            var created = result.Field("createProject");
            created.GetProperty("name").GetString().Should().Be(name);
            fixture.Projects.Add(new CreatedEntity(
                token,
                created.GetProperty("id").GetString()!,
                name,
                new Dictionary<string, string> { ["name"] = name }));
        }
    }

    /// <summary>Creates the listed deliverables inside the first project of the scenario.</summary>
    [Given(@"^I create query coverage deliverables ""(.*)""$")]
    public async Task GivenICreateQueryCoverageDeliverables(string specs)
    {
        foreach (var spec in SplitList(specs))
        {
            await CreateDeliverableAsync(spec, 1);
        }
    }

    /// <summary>Creates a single deliverable inside the n-th project of the scenario.</summary>
    [Given(@"^I create a query coverage deliverable ""(.*)"" in project (\d+)$")]
    public async Task GivenICreateAQueryCoverageDeliverableInProject(string spec, int projectIndex)
    {
        await CreateDeliverableAsync(spec, projectIndex);
    }

    /// <summary>Creates the listed agent tasks inside the first deliverable of the scenario.</summary>
    [Given(@"^I create query coverage agent tasks ""(.*)""$")]
    public async Task GivenICreateQueryCoverageAgentTasks(string specs)
    {
        foreach (var spec in SplitList(specs))
        {
            await CreateAgentTaskAsync(spec, 1, null);
        }
    }

    /// <summary>Creates a single agent task inside the n-th deliverable of the scenario.</summary>
    [Given(@"^I create a query coverage agent task ""(.*)"" in deliverable (\d+)$")]
    public async Task GivenICreateAQueryCoverageAgentTaskInDeliverable(string spec, int deliverableIndex)
    {
        await CreateAgentTaskAsync(spec, deliverableIndex, null);
    }

    /// <summary>Creates an agent task that depends on another agent task of the scenario.</summary>
    [Given(@"^I create a query coverage agent task ""(.*)"" in deliverable (\d+) depending on task (\d+)$")]
    public async Task GivenICreateAQueryCoverageAgentTaskDependingOnTask(string spec, int deliverableIndex, int dependencyIndex)
    {
        await CreateAgentTaskAsync(spec, deliverableIndex, dependencyIndex);
    }

    /// <summary>Creates the listed large language models with distinct url, model, alias, key and numeric fields.</summary>
    [Given(@"^I create query coverage large language models ""(.*)""$")]
    public async Task GivenICreateQueryCoverageLargeLanguageModels(string specs)
    {
        var fixture = Fixture();

        foreach (var spec in SplitList(specs))
        {
            var parts = spec.Split(':');
            parts.Should().HaveCount(4, $"a model spec is TOKEN:MAXCOMPLEXITY:MAXCONCURRENCY:COST but was '{spec}'");

            var token = parts[0].Trim();
            var maxComplexity = int.Parse(parts[1].Trim());
            var maxConcurrency = int.Parse(parts[2].Trim());
            var cost = int.Parse(parts[3].Trim());

            // The url keeps the bare [DeleteAfterTest] anchor because deleteTestData reclaims
            // large language models by url; the per-scenario marker goes into model / modelAlias /
            // apiKey, which is what the connection queries filter on (see MarkerField).
            var url = $"https://example.com/{TestMarker}/{fixture.Slug}/{token}";
            var model = $"{fixture.Marker} model-{token}";
            var modelAlias = $"{fixture.Marker} alias-{token}";
            var apiKey = $"{fixture.Marker} key-{token}";

            var variables = new
            {
                input = new
                {
                    url,
                    model,
                    modelAlias,
                    apiKey,
                    cost,
                    maxComplexity,
                    maxConcurrency
                }
            };

            var result = await ExecuteAsync(CreateLargeLanguageModelDocument, variables, "QcCreateLargeLanguageModel");
            result.HasErrors.Should().BeFalse(result.RawBody);

            var created = result.Field("createLargeLanguageModel");
            created.GetProperty("url").GetString().Should().Be(url);

            fixture.Models.Add(new CreatedEntity(
                token,
                created.GetProperty("id").GetString()!,
                url,
                new Dictionary<string, string>
                {
                    ["url"] = url,
                    ["model"] = model,
                    ["modelAlias"] = modelAlias,
                    ["apiKey"] = apiKey,
                    ["cost"] = cost.ToString(),
                    ["maxComplexity"] = maxComplexity.ToString(),
                    ["maxConcurrency"] = maxConcurrency.ToString()
                }));
        }
    }

    // ------------------------------------------------------------- GQL-08 queries

    /// <summary>Queries the projects connection with a forward page size and a name ordering.</summary>
    [When(@"^I query projects with first (\d+) ordered by name ""(.*)""$")]
    public async Task WhenIQueryProjectsWithFirstOrderedByName(int first, string direction)
    {
        await RunConnectionAsync("projects", new Dictionary<string, object?>(), Sort("name", direction), first, null, null, null);
    }

    /// <summary>Queries the projects connection with a backwards page size and a name ordering.</summary>
    [When(@"^I query projects with last (\d+) ordered by name ""(.*)""$")]
    public async Task WhenIQueryProjectsWithLastOrderedByName(int last, string direction)
    {
        await RunConnectionAsync("projects", new Dictionary<string, object?>(), Sort("name", direction), null, last, null, null);
    }

    /// <summary>Queries the next projects page after the cursor stored by the previous projects query.</summary>
    [When(@"^I query projects after the stored cursor with first (\d+) ordered by name ""(.*)""$")]
    public async Task WhenIQueryProjectsAfterTheStoredCursor(int first, string direction)
    {
        var after = Cursor(EndCursorKey);
        await RunConnectionAsync("projects", new Dictionary<string, object?>(), Sort("name", direction), first, null, after, null);
    }

    /// <summary>Queries the previous projects page before the cursor stored by the previous projects query.</summary>
    [When(@"^I query projects before the stored cursor with last (\d+) ordered by name ""(.*)""$")]
    public async Task WhenIQueryProjectsBeforeTheStoredCursor(int last, string direction)
    {
        var before = Cursor(StartCursorKey);
        await RunConnectionAsync("projects", new Dictionary<string, object?>(), Sort("name", direction), null, last, null, before);
    }

    /// <summary>Filters projects on the name field with a single string operator.</summary>
    [When(@"^I query projects filtered by name ""(.*)"" ""(.*)""$")]
    public async Task WhenIQueryProjectsFilteredByName(string op, string value)
    {
        var where = new Dictionary<string, object?> { ["name"] = Operation(op, StringValues("projects", op, value)) };
        await RunConnectionAsync("projects", where, null, null, null, null, null);
    }

    /// <summary>Filters projects with an <c>and</c> combination across the name and repository fields.</summary>
    [When(@"^I query projects filtered by name contains ""(.*)"" and repository contains ""(.*)""$")]
    public async Task WhenIQueryProjectsFilteredByNameAndRepository(string name, string repository)
    {
        var where = new Dictionary<string, object?>
        {
            ["and"] = new List<object?>
            {
                new Dictionary<string, object?> { ["name"] = new Dictionary<string, object?> { ["contains"] = StringValue("projects", name) } },
                new Dictionary<string, object?> { ["repository"] = new Dictionary<string, object?> { ["contains"] = repository } }
            }
        };

        await RunConnectionAsync("projects", where, null, null, null, null, null);
    }

    /// <summary>Filters projects with an <c>or</c> combination over the name field.</summary>
    [When(@"^I query projects filtered by name eq ""(.*)"" or name eq ""(.*)""$")]
    public async Task WhenIQueryProjectsFilteredByNameOr(string first, string second)
    {
        var where = new Dictionary<string, object?>
        {
            ["or"] = new List<object?>
            {
                new Dictionary<string, object?> { ["name"] = new Dictionary<string, object?> { ["eq"] = StringValue("projects", first) } },
                new Dictionary<string, object?> { ["name"] = new Dictionary<string, object?> { ["eq"] = StringValue("projects", second) } }
            }
        };

        await RunConnectionAsync("projects", where, null, null, null, null, null);
    }

    /// <summary>Filters projects on the nested filter <c>deliverables { some { status } }</c>.</summary>
    [When(@"^I query projects filtered by deliverables with status ""(.*)""$")]
    public async Task WhenIQueryProjectsFilteredByDeliverablesWithStatus(string status)
    {
        var where = new Dictionary<string, object?>
        {
            ["deliverables"] = new Dictionary<string, object?>
            {
                ["some"] = StatusFilter(status)
            }
        };

        await RunConnectionAsync("projects", where, null, null, null, null, null);
    }

    /// <summary>Filters projects on the nested filter <c>deliverables { none { status } }</c>.</summary>
    [When(@"^I query projects with no deliverables in status ""(.*)""$")]
    public async Task WhenIQueryProjectsWithNoDeliverablesInStatus(string status)
    {
        var where = new Dictionary<string, object?>
        {
            ["deliverables"] = new Dictionary<string, object?>
            {
                ["none"] = StatusFilter(status)
            }
        };

        await RunConnectionAsync("projects", where, null, null, null, null, null);
    }

    /// <summary>Filters projects on <c>deliverables { any: false }</c>, i.e. projects without any deliverable.</summary>
    [When(@"^I query projects that have no deliverables$")]
    public async Task WhenIQueryProjectsWithoutDeliverables()
    {
        var where = new Dictionary<string, object?>
        {
            ["deliverables"] = new Dictionary<string, object?> { ["any"] = false }
        };

        await RunConnectionAsync("projects", where, null, null, null, null, null);
    }

    // ------------------------------------------------------------- GQL-09 queries

    /// <summary>Filters deliverables on their status with eq / in / nin.</summary>
    [When(@"^I query deliverables filtered by status ""(.*)"" ""(.*)""$")]
    public async Task WhenIQueryDeliverablesFilteredByStatus(string op, string value)
    {
        var where = new Dictionary<string, object?> { ["status"] = Operation(op, EnumValues(value)) };
        await RunConnectionAsync("deliverables", where, null, null, null, null, null);
    }

    /// <summary>Filters deliverables on their type.</summary>
    [When(@"^I query deliverables filtered by type ""(.*)""$")]
    public async Task WhenIQueryDeliverablesFilteredByType(string type)
    {
        var where = new Dictionary<string, object?>
        {
            ["type"] = new Dictionary<string, object?> { ["eq"] = type }
        };

        await RunConnectionAsync("deliverables", where, null, null, null, null, null);
    }

    /// <summary>Filters deliverables on a substring of their title.</summary>
    [When(@"^I query deliverables filtered by title contains ""(.*)""$")]
    public async Task WhenIQueryDeliverablesFilteredByTitleContains(string value)
    {
        var where = new Dictionary<string, object?>
        {
            ["title"] = new Dictionary<string, object?> { ["contains"] = StringValue("deliverables", value) }
        };

        await RunConnectionAsync("deliverables", where, null, null, null, null, null);
    }

    /// <summary>Filters deliverables on the projectId foreign key of one of the created deliverables.</summary>
    [When(@"^I query deliverables filtered by the project id of deliverable (\d+)$")]
    public async Task WhenIQueryDeliverablesFilteredByProjectIdOfDeliverable(int index)
    {
        var projectId = FieldOf("deliverables", index, "projectId");
        var where = new Dictionary<string, object?>
        {
            ["projectId"] = new Dictionary<string, object?> { ["eq"] = projectId }
        };

        await RunConnectionAsync("deliverables", where, null, null, null, null, null);
    }

    /// <summary>Filters deliverables on the nested filter <c>agentTasks { some { status } }</c>.</summary>
    [When(@"^I query deliverables filtered by agent tasks with status ""(.*)""$")]
    public async Task WhenIQueryDeliverablesFilteredByAgentTasksWithStatus(string status)
    {
        var where = new Dictionary<string, object?>
        {
            ["agentTasks"] = new Dictionary<string, object?> { ["some"] = StatusFilter(status) }
        };

        await RunConnectionAsync("deliverables", where, null, null, null, null, null);
    }

    /// <summary>Filters deliverables on the nested filter <c>agentTasks { none { status } }</c>.</summary>
    [When(@"^I query deliverables with no agent tasks in status ""(.*)""$")]
    public async Task WhenIQueryDeliverablesWithNoAgentTasksInStatus(string status)
    {
        var where = new Dictionary<string, object?>
        {
            ["agentTasks"] = new Dictionary<string, object?> { ["none"] = StatusFilter(status) }
        };

        await RunConnectionAsync("deliverables", where, null, null, null, null, null);
    }

    /// <summary>Filters deliverables on <c>agentTasks { any: false }</c>, i.e. deliverables without any task.</summary>
    [When(@"^I query deliverables that have no agent tasks$")]
    public async Task WhenIQueryDeliverablesWithoutAgentTasks()
    {
        var where = new Dictionary<string, object?>
        {
            ["agentTasks"] = new Dictionary<string, object?> { ["any"] = false }
        };

        await RunConnectionAsync("deliverables", where, null, null, null, null, null);
    }

    /// <summary>Queries the deliverables connection with a forward page size and a title ordering.</summary>
    [When(@"^I query deliverables with first (\d+) ordered by title ""(.*)""$")]
    public async Task WhenIQueryDeliverablesWithFirstOrderedByTitle(int first, string direction)
    {
        await RunConnectionAsync("deliverables", new Dictionary<string, object?>(), Sort("title", direction), first, null, null, null);
    }

    /// <summary>Queries the next deliverables page after the cursor stored by the previous deliverables query.</summary>
    [When(@"^I query deliverables after the stored cursor with first (\d+) ordered by title ""(.*)""$")]
    public async Task WhenIQueryDeliverablesAfterTheStoredCursor(int first, string direction)
    {
        await RunConnectionAsync("deliverables", new Dictionary<string, object?>(), Sort("title", direction), first, null, Cursor(EndCursorKey), null);
    }

    /// <summary>Queries all deliverables of the scenario ordered by status.</summary>
    [When(@"^I query deliverables ordered by status ""(.*)""$")]
    public async Task WhenIQueryDeliverablesOrderedByStatus(string direction)
    {
        await RunConnectionAsync("deliverables", new Dictionary<string, object?>(), Sort("status", direction), null, null, null, null);
    }

    /// <summary>Filters deliverables through the nested <c>project { name }</c> object filter.</summary>
    [When(@"^I query deliverables filtered by the project name of deliverable (\d+)$")]
    public async Task WhenIQueryDeliverablesFilteredByProjectNameOfDeliverable(int index)
    {
        var projectName = FieldOf("projects", ProjectIndexOfDeliverable(index), "name");
        var where = new Dictionary<string, object?>
        {
            ["project"] = new Dictionary<string, object?>
            {
                ["name"] = new Dictionary<string, object?> { ["eq"] = projectName }
            }
        };

        await RunConnectionAsync("deliverables", where, null, null, null, null, null);
    }

    // ------------------------------------------------------------- GQL-10 queries

    /// <summary>Filters agent tasks on their status with eq / in / nin.</summary>
    [When(@"^I query agent tasks filtered by status ""(.*)"" ""(.*)""$")]
    public async Task WhenIQueryAgentTasksFilteredByStatus(string op, string value)
    {
        var where = new Dictionary<string, object?> { ["status"] = Operation(op, EnumValues(value)) };
        await RunConnectionAsync("agentTasks", where, null, null, null, null, null);
    }

    /// <summary>Filters agent tasks on complexityRating with gt / lt / eq.</summary>
    [When(@"^I query agent tasks filtered by complexity rating ""(.*)"" (\d+)$")]
    public async Task WhenIQueryAgentTasksFilteredByComplexityRating(string op, int value)
    {
        var where = new Dictionary<string, object?> { ["complexityRating"] = Operation(op, value) };
        await RunConnectionAsync("agentTasks", where, null, null, null, null, null);
    }

    /// <summary>Filters agent tasks on a substring of their title.</summary>
    [When(@"^I query agent tasks filtered by title contains ""(.*)""$")]
    public async Task WhenIQueryAgentTasksFilteredByTitleContains(string value)
    {
        var where = new Dictionary<string, object?>
        {
            ["title"] = new Dictionary<string, object?> { ["contains"] = StringValue("agentTasks", value) }
        };

        await RunConnectionAsync("agentTasks", where, null, null, null, null, null);
    }

    /// <summary>Filters agent tasks on the deliverableId foreign key of one of the created tasks.</summary>
    [When(@"^I query agent tasks filtered by the deliverable id of task (\d+)$")]
    public async Task WhenIQueryAgentTasksFilteredByDeliverableIdOfTask(int index)
    {
        var where = new Dictionary<string, object?>
        {
            ["deliverableId"] = new Dictionary<string, object?> { ["eq"] = FieldOf("agentTasks", index, "deliverableId") }
        };

        await RunConnectionAsync("agentTasks", where, null, null, null, null, null);
    }

    /// <summary>Filters agent tasks on the projectId foreign key of one of the created tasks.</summary>
    [When(@"^I query agent tasks filtered by the project id of task (\d+)$")]
    public async Task WhenIQueryAgentTasksFilteredByProjectIdOfTask(int index)
    {
        var where = new Dictionary<string, object?>
        {
            ["projectId"] = new Dictionary<string, object?> { ["eq"] = FieldOf("agentTasks", index, "projectId") }
        };

        await RunConnectionAsync("agentTasks", where, null, null, null, null, null);
    }

    /// <summary>Filters agent tasks on the dependsOnAgentTaskId foreign key of one of the created tasks.</summary>
    [When(@"^I query agent tasks filtered by the dependency of task (\d+)$")]
    public async Task WhenIQueryAgentTasksFilteredByDependencyOfTask(int index)
    {
        var where = new Dictionary<string, object?>
        {
            ["dependsOnAgentTaskId"] = new Dictionary<string, object?> { ["eq"] = FieldOf("agentTasks", index, "dependsOnAgentTaskId") }
        };

        await RunConnectionAsync("agentTasks", where, null, null, null, null, null);
    }

    /// <summary>Filters agent tasks through the nested <c>deliverable { title }</c> object filter.</summary>
    [When(@"^I query agent tasks filtered by the deliverable title of task (\d+)$")]
    public async Task WhenIQueryAgentTasksFilteredByDeliverableTitleOfTask(int index)
    {
        var title = FieldOf("agentTasks", index, "deliverableTitle");
        var where = new Dictionary<string, object?>
        {
            ["deliverable"] = new Dictionary<string, object?>
            {
                ["title"] = new Dictionary<string, object?> { ["eq"] = title }
            }
        };

        await RunConnectionAsync("agentTasks", where, null, null, null, null, null);
    }

    /// <summary>Filters agent tasks through the nested <c>project { name }</c> object filter.</summary>
    [When(@"^I query agent tasks filtered by the project name of task (\d+)$")]
    public async Task WhenIQueryAgentTasksFilteredByProjectNameOfTask(int index)
    {
        var name = FieldOf("agentTasks", index, "projectName");
        var where = new Dictionary<string, object?>
        {
            ["project"] = new Dictionary<string, object?>
            {
                ["name"] = new Dictionary<string, object?> { ["eq"] = name }
            }
        };

        await RunConnectionAsync("agentTasks", where, null, null, null, null, null);
    }

    /// <summary>Filters agent tasks through the nested <c>dependsOnAgentTask { title }</c> object filter.</summary>
    [When(@"^I query agent tasks filtered by the dependency title of task (\d+)$")]
    public async Task WhenIQueryAgentTasksFilteredByDependencyTitleOfTask(int index)
    {
        var title = FieldOf("agentTasks", index, "dependsOnAgentTaskTitle");
        var where = new Dictionary<string, object?>
        {
            ["dependsOnAgentTask"] = new Dictionary<string, object?>
            {
                ["title"] = new Dictionary<string, object?> { ["eq"] = title }
            }
        };

        await RunConnectionAsync("agentTasks", where, null, null, null, null, null);
    }

    /// <summary>Queries the agent tasks connection with a forward page size and a title ordering.</summary>
    [When(@"^I query agent tasks with first (\d+) ordered by title ""(.*)""$")]
    public async Task WhenIQueryAgentTasksWithFirstOrderedByTitle(int first, string direction)
    {
        await RunConnectionAsync("agentTasks", new Dictionary<string, object?>(), Sort("title", direction), first, null, null, null);
    }

    /// <summary>Queries the next agent tasks page after the cursor stored by the previous agent tasks query.</summary>
    [When(@"^I query agent tasks after the stored cursor with first (\d+) ordered by title ""(.*)""$")]
    public async Task WhenIQueryAgentTasksAfterTheStoredCursor(int first, string direction)
    {
        await RunConnectionAsync("agentTasks", new Dictionary<string, object?>(), Sort("title", direction), first, null, Cursor(EndCursorKey), null);
    }

    /// <summary>Queries all agent tasks of the scenario ordered by complexityRating.</summary>
    [When(@"^I query agent tasks ordered by complexity rating ""(.*)""$")]
    public async Task WhenIQueryAgentTasksOrderedByComplexityRating(string direction)
    {
        await RunConnectionAsync("agentTasks", new Dictionary<string, object?>(), Sort("complexityRating", direction), null, null, null, null);
    }

    // ------------------------------------------------------------- GQL-11 queries

    /// <summary>Queries all large language models of the scenario ordered by cost.</summary>
    [When(@"^I query large language models ordered by cost ""(.*)""$")]
    public async Task WhenIQueryLargeLanguageModelsOrderedByCost(string direction)
    {
        await RunConnectionAsync("largeLanguageModels", new Dictionary<string, object?>(), Sort("cost", direction), null, null, null, null);
    }

    /// <summary>Filters large language models on cost with eq / gt.</summary>
    [When(@"^I query large language models filtered by cost ""(.*)"" (\d+)$")]
    public async Task WhenIQueryLargeLanguageModelsFilteredByCost(string op, int value)
    {
        var where = new Dictionary<string, object?> { ["cost"] = Operation(op, value) };
        await RunConnectionAsync("largeLanguageModels", where, null, null, null, null, null);
    }

    /// <summary>Filters large language models on <c>maxComplexity { gte }</c>.</summary>
    [When(@"^I query large language models filtered by max complexity gte (\d+)$")]
    public async Task WhenIQueryLargeLanguageModelsFilteredByMaxComplexityGte(int value)
    {
        var where = new Dictionary<string, object?>
        {
            ["maxComplexity"] = new Dictionary<string, object?> { ["gte"] = value }
        };

        await RunConnectionAsync("largeLanguageModels", where, null, null, null, null, null);
    }

    /// <summary>Filters large language models on a substring of the model name.</summary>
    [When(@"^I query large language models filtered by model contains ""(.*)""$")]
    public async Task WhenIQueryLargeLanguageModelsFilteredByModelContains(string value)
    {
        var where = new Dictionary<string, object?>
        {
            ["model"] = new Dictionary<string, object?> { ["contains"] = value }
        };

        await RunConnectionAsync("largeLanguageModels", where, null, null, null, null, null);
    }

    // ------------------------------------------------------------- GQL-12 queries

    /// <summary>Reads the n-th created project together with its deliverables relation.</summary>
    [When(@"^I query the query coverage project (\d+) with its deliverables$")]
    public async Task WhenIQueryTheQueryCoverageProjectWithItsDeliverables(int index)
    {
        var project = Entity("projects", index);
        var result = await ExecuteAsync(ProjectRelationsDocument, new { id = project.Id }, "QcProjectRelations");

        result.HasErrors.Should().BeFalse(result.RawBody);
        var node = result.Field("project");
        node.ValueKind.Should().NotBe(JsonValueKind.Null, result.RawBody);
        node.GetProperty("id").GetString().Should().Be(project.Id);
        node.GetProperty("name").GetString().Should().Be(project.Label);

        StoreRelation("deliverables", node.GetProperty("deliverables"));
    }

    /// <summary>Reads the n-th created deliverable together with its agentTasks relation.</summary>
    [When(@"^I query the query coverage deliverable (\d+) with its agent tasks$")]
    public async Task WhenIQueryTheQueryCoverageDeliverableWithItsAgentTasks(int index)
    {
        var deliverable = Entity("deliverables", index);
        var result = await ExecuteAsync(DeliverableRelationsDocument, new { id = deliverable.Id }, "QcDeliverableRelations");

        result.HasErrors.Should().BeFalse(result.RawBody);
        var node = result.Field("deliverable");
        node.ValueKind.Should().NotBe(JsonValueKind.Null, result.RawBody);
        node.GetProperty("id").GetString().Should().Be(deliverable.Id);
        node.GetProperty("title").GetString().Should().Be(deliverable.Label);

        StoreRelation("agentTasks", node.GetProperty("agentTasks"));
    }

    /// <summary>Reads the n-th created agent task together with its foreign keys.</summary>
    [When(@"^I query the query coverage agent task (\d+) with its relations$")]
    public async Task WhenIQueryTheQueryCoverageAgentTaskWithItsRelations(int index)
    {
        var task = Entity("agentTasks", index);
        var result = await ExecuteAsync(AgentTaskRelationsDocument, new { id = task.Id }, "QcAgentTaskRelations");

        result.HasErrors.Should().BeFalse(result.RawBody);
        var node = result.Field("agentTask");
        node.ValueKind.Should().NotBe(JsonValueKind.Null, result.RawBody);
        node.GetProperty("id").GetString().Should().Be(task.Id);
        node.GetProperty("title").GetString().Should().Be(task.Label);

        _scenarioContext["QcRelationTask"] = node;
    }

    /// <summary>
    /// Reads the n-th created agent task with its relation <em>objects</em> (deliverable, project,
    /// dependsOnAgentTask) exactly as the AdminUi query does, and keeps the payload for assertion.
    /// </summary>
    [When(@"^I query the query coverage agent task (\d+) with its relation objects$")]
    public async Task WhenIQueryTheQueryCoverageAgentTaskWithItsRelationObjects(int index)
    {
        var task = Entity("agentTasks", index);
        var result = await ExecuteAsync(AgentTaskRelationObjectsDocument, new { id = task.Id }, "QcAgentTaskRelationObjects");

        result.HasErrors.Should().BeFalse(result.RawBody);
        var node = result.Field("agentTask");
        node.ValueKind.Should().NotBe(JsonValueKind.Null, result.RawBody);
        node.GetProperty("id").GetString().Should().Be(task.Id);

        _scenarioContext[TaskObjectsKey] = node;
    }

    /// <summary>
    /// Reads the agent tasks of the n-th created deliverable through the AdminUi connection query
    /// (deliverableId filter plus relation objects) and keeps the nodes for assertion.
    /// </summary>
    [When(@"^I query agent tasks in deliverable (\d+) with their relation objects$")]
    public async Task WhenIQueryAgentTasksInDeliverableWithTheirRelationObjects(int deliverableIndex)
    {
        var fixture = Fixture();
        var deliverable = Entity("deliverables", deliverableIndex);

        var where = new Dictionary<string, object?>
        {
            ["and"] = new List<object?>
            {
                new Dictionary<string, object?>
                {
                    ["deliverableId"] = new Dictionary<string, object?> { ["eq"] = deliverable.Id }
                },
                new Dictionary<string, object?>
                {
                    ["title"] = new Dictionary<string, object?> { ["contains"] = fixture.Marker }
                }
            }
        };

        var result = await ExecuteAsync(
            AgentTaskPageRelationObjectsDocument,
            new Dictionary<string, object?> { ["where"] = where, ["first"] = 10 },
            "QcAgentTaskPageRelationObjects");

        result.HasErrors.Should().BeFalse(result.RawBody);
        result.IsFieldNull("agentTasks").Should().BeFalse(result.RawBody);

        var nodes = result.Field("agentTasks").GetProperty("nodes");
        nodes.ValueKind.Should().Be(JsonValueKind.Array, result.RawBody);

        _scenarioContext[TaskPageNodesKey] = nodes.EnumerateArray().ToList();
    }

    /// <summary>
    /// Reads the n-th created deliverable with its project object and keeps the payload for assertion.
    /// </summary>
    [When(@"^I query the query coverage deliverable (\d+) with its project object$")]
    public async Task WhenIQueryTheQueryCoverageDeliverableWithItsProjectObject(int index)
    {
        var deliverable = Entity("deliverables", index);
        var result = await ExecuteAsync(DeliverableRelationObjectDocument, new { id = deliverable.Id }, "QcDeliverableRelationObject");

        result.HasErrors.Should().BeFalse(result.RawBody);
        var node = result.Field("deliverable");
        node.ValueKind.Should().NotBe(JsonValueKind.Null, result.RawBody);
        node.GetProperty("id").GetString().Should().Be(deliverable.Id);

        _scenarioContext[DeliverableObjectKey] = node;
    }

    // ---------------------------------------------------------------- assertions

    /// <summary>Asserts the number of nodes returned by the last connection query.</summary>
    [Then(@"^the ""(.*)"" connection should contain (\d+) nodes$")]
    public void ThenConnectionShouldContainNodes(string kind, int count)
    {
        var page = Page(kind);
        page.Nodes.Count.Should().Be(count, LastResult.RawBody);
    }

    /// <summary>Asserts that the returned nodes are exactly the referenced rows, in any order.</summary>
    [Then(@"^the ""(.*)"" connection nodes should be ""(.*)""$")]
    public void ThenConnectionNodesShouldBe(string kind, string expected)
    {
        var page = Page(kind);
        var expectedIds = ExpectedIds(kind, expected);

        page.Nodes.Select(n => n.Id).Should().BeEquivalentTo(expectedIds, $"the {kind} connection returned {LastResult.RawBody}");
    }

    /// <summary>Asserts that the returned nodes are exactly the referenced rows in the given order.</summary>
    [Then(@"^the ""(.*)"" connection nodes should be in order ""(.*)""$")]
    public void ThenConnectionNodesShouldBeInOrder(string kind, string expected)
    {
        var page = Page(kind);
        var expectedIds = ExpectedIds(kind, expected);

        page.Nodes.Select(n => n.Id).Should().Equal(expectedIds, $"the {kind} connection returned {LastResult.RawBody}");
    }

    /// <summary>Asserts hasNextPage / hasPreviousPage of the connection's pageInfo.</summary>
    [Then(@"^the ""(.*)"" connection pageInfo should report hasNextPage (.*) and hasPreviousPage (.*)$")]
    public void ThenConnectionPageInfoShouldReport(string kind, bool hasNextPage, bool hasPreviousPage)
    {
        var page = Page(kind);
        page.HasNextPage.Should().Be(hasNextPage, LastResult.RawBody);
        page.HasPreviousPage.Should().Be(hasPreviousPage, LastResult.RawBody);
    }

    /// <summary>Asserts that both pageInfo cursors are populated.</summary>
    [Then(@"^the ""(.*)"" connection pageInfo cursors should be present$")]
    public void ThenConnectionPageInfoCursorsShouldBePresent(string kind)
    {
        var page = Page(kind);
        page.StartCursor.Should().NotBeNullOrEmpty(LastResult.RawBody);
        page.EndCursor.Should().NotBeNullOrEmpty(LastResult.RawBody);
    }

    /// <summary>Asserts that edges carry a cursor per node and reference the same rows in the same order.</summary>
    [Then(@"^the ""(.*)"" connection edges should mirror the nodes with cursors$")]
    public void ThenConnectionEdgesShouldMirrorNodes(string kind)
    {
        var page = Page(kind);

        page.Edges.Should().HaveCount(page.Nodes.Count, LastResult.RawBody);
        page.Edges.Should().OnlyContain(edge => !string.IsNullOrEmpty(edge.Cursor), LastResult.RawBody);
        page.Edges.Select(e => e.Id).Should().Equal(page.Nodes.Select(n => n.Id), LastResult.RawBody);
    }

    /// <summary>Asserts that the current page shares no row with the connection query before it.</summary>
    [Then(@"^the ""(.*)"" connection nodes should not overlap the previous page$")]
    public void ThenConnectionNodesShouldNotOverlapPreviousPage(string kind)
    {
        var page = Page(kind);
        var previous = _scenarioContext.TryGetValue<List<string>>(PreviousPageIdsKey, out var ids) && ids is not null
            ? ids
            : [];

        page.Nodes.Select(n => n.Id).Intersect(previous).Should().BeEmpty(LastResult.RawBody);
    }

    /// <summary>Asserts that the returned nodes are monotonically ordered by a raw node field.</summary>
    [Then(@"^the ""(.*)"" connection nodes should be ordered by ""(.*)"" ""(.*)""$")]
    public void ThenConnectionNodesShouldBeOrderedBy(string kind, string field, string direction)
    {
        var page = Page(kind);
        var values = page.Nodes.Select(node => ReadValue(node.Raw, field)).ToList();

        var sorted = values.OrderBy(v => v, ValueComparer.Instance).ToList();
        if (direction.Equals("DESC", StringComparison.OrdinalIgnoreCase))
        {
            sorted.Reverse();
        }

        values.Should().Equal(sorted, $"the {kind} connection should be ordered by {field} {direction}: {LastResult.RawBody}");
    }

    /// <summary>Asserts that every created large language model comes back with all scalar fields unchanged.</summary>
    [Then(@"^the ""largeLanguageModels"" connection should round-trip every created model field$")]
    public void ThenLargeLanguageModelsShouldRoundTripEveryField()
    {
        var page = Page("largeLanguageModels");
        var nodesById = page.Nodes.ToDictionary(n => n.Id, n => n.Raw);

        foreach (var model in Fixture().Models)
        {
            nodesById.Should().ContainKey(model.Id, $"model {model.Token} was not returned: {LastResult.RawBody}");
            var node = nodesById[model.Id];

            node.GetProperty("url").GetString().Should().Be(model.Fields["url"]);
            node.GetProperty("model").GetString().Should().Be(model.Fields["model"]);
            node.GetProperty("modelAlias").GetString().Should().Be(model.Fields["modelAlias"]);
            node.GetProperty("apiKey").GetString().Should().Be(model.Fields["apiKey"]);
            ReadValue(node, "cost").Should().Be(model.Fields["cost"]);
            ReadValue(node, "maxComplexity").Should().Be(model.Fields["maxComplexity"]);
            ReadValue(node, "maxConcurrency").Should().Be(model.Fields["maxConcurrency"]);
        }
    }

    /// <summary>Asserts the deliverables returned by the project-by-id relation query.</summary>
    [Then(@"^the query coverage project relation should expose (\d+) deliverables ""(.*)""$")]
    public void ThenProjectRelationShouldExposeDeliverables(int count, string expected)
    {
        AssertRelation("deliverables", count, expected);
    }

    /// <summary>Asserts the agent tasks returned by the deliverable-by-id relation query.</summary>
    [Then(@"^the query coverage deliverable relation should expose (\d+) agent tasks ""(.*)""$")]
    public void ThenDeliverableRelationShouldExposeAgentTasks(int count, string expected)
    {
        AssertRelation("agentTasks", count, expected);
    }

    /// <summary>Asserts that an agent task references the expected deliverable, project and dependency.</summary>
    [Then(@"^the query coverage agent task (\d+) relation should reference deliverable (\d+) project (\d+) and dependency task (\d+)$")]
    public void ThenAgentTaskRelationShouldReference(int taskIndex, int deliverableIndex, int projectIndex, int dependencyIndex)
    {
        var node = _scenarioContext.TryGetValue<JsonElement>("QcRelationTask", out var stored)
            ? stored
            : throw new InvalidOperationException("No agent task relation has been queried.");

        var task = Entity("agentTasks", taskIndex);
        node.GetProperty("id").GetString().Should().Be(task.Id);
        node.GetProperty("deliverableId").GetString().Should().Be(FieldOf("agentTasks", taskIndex, "deliverableId"));
        node.GetProperty("projectId").GetString().Should().Be(FieldOf("agentTasks", taskIndex, "projectId"));
        node.GetProperty("dependsOnAgentTaskId").GetString().Should().Be(Entity("agentTasks", dependencyIndex).Id);
        FieldOf("agentTasks", taskIndex, "dependsOnAgentTaskId").Should().Be(Entity("agentTasks", dependencyIndex).Id);
        FieldOf("deliverables", deliverableIndex, "projectId").Should().Be(FieldOf("agentTasks", taskIndex, "projectId"));
    }

    // ---------------------------------------------------------------------------------------
    // PRODUCT DEFECT BUG-01 (doc/planning/integration-test-coverage-ledger.md §6) pinned by the three
    // relation-object assertions below.
    //
    // The relation OBJECT fields of agentTask and deliverable resolve to null, even though the
    // foreign-key scalars are correct. Observed payloads (Release, docker image devstack-api):
    //
    //   agentTask(id:)  -> {"id":"...","title":"...","deliverableId":"27df7f72-...",
    //                       "projectId":"d998b201-...","dependsOnAgentTaskId":"64e3dd9a-...",
    //                       "deliverable":null,"project":null,"dependsOnAgentTask":null}
    //   agentTasks(...) -> {"id":"...","deliverableId":"2004f5dc-...","projectId":"2e096c9d-...",
    //                       "deliverable":null,"project":null}
    //   deliverable(id:)-> {"id":"...","projectId":"5599fe0d-...","project":null}
    //
    // Cause: Query.cs loads only the root entity. GetAgentTask has no Include at all, GetDeliverable
    // includes AgentTasks but not Project, and the connection resolvers carry
    // [UsePaging][UseFiltering][UseSorting] without [UseProjection], so HotChocolate never projects
    // the navigation and EF Core leaves the CLR navigation unloaded (lazy loading proxies are not
    // enabled - Program.cs registers the context with a plain UseNpgsql). HotChocolate then serialises
    // the unloaded navigation as null instead of loading it.
    //
    // AdminUi impact: src/AdminUi/src/graphql/queries/agentTask.graphql selects exactly these fields
    //   GetAgentTask  (lines 18-26): deliverable { id title } project { id name repository }
    //   GetAgentTasks (lines 48-55): deliverable { id title } project { id name }
    // so the task detail view and the task list render empty deliverable / project columns although
    // the relations exist in the database.
    //
    // Server-side fix: add [UseProjection] to the connection resolvers and Include the navigations in
    // the by-id resolvers (or resolve them through dedicated field resolvers). Once fixed, flip these
    // three assertions back to asserting the resolved objects' ids and labels.
    // ---------------------------------------------------------------------------------------

    /// <summary>
    /// Asserts that the relation objects of an agent task-by-id query are NOT resolved while its
    /// foreign keys reference the created rows (see the product-defect note above the step).
    /// </summary>
    [Then(@"^the query coverage agent task (\d+) relation objects are not resolved but its foreign keys reference deliverable (\d+) project (\d+) and dependency task (\d+)$")]
    public void ThenAgentTaskRelationObjectsAreNotResolved(int taskIndex, int deliverableIndex, int projectIndex, int dependencyIndex)
    {
        var node = StoredObject(TaskObjectsKey);

        node.GetProperty("id").GetString().Should().Be(Entity("agentTasks", taskIndex).Id);
        AssertUnresolvedObject(node, "deliverable", "deliverableId", Entity("deliverables", deliverableIndex).Id);
        AssertUnresolvedObject(node, "project", "projectId", Entity("projects", projectIndex).Id);
        AssertUnresolvedObject(node, "dependsOnAgentTask", "dependsOnAgentTaskId", Entity("agentTasks", dependencyIndex).Id);
    }

    /// <summary>
    /// Asserts that no node of the deliverable's agent task connection resolves its deliverable or
    /// project object, while the foreign keys of every node reference the created rows.
    /// </summary>
    [Then(@"^the query coverage agent task relation objects in deliverable (\d+) are not resolved but reference its project (\d+)$")]
    public void ThenAgentTaskRelationObjectsInDeliverableAreNotResolved(int deliverableIndex, int projectIndex)
    {
        var nodes = StoredNodes(TaskPageNodesKey);
        var deliverable = Entity("deliverables", deliverableIndex);
        var project = Entity("projects", projectIndex);

        nodes.Should().HaveCount(KindList("agentTasks").Count, $"the connection returned {nodes.Count} nodes");
        nodes.Select(n => n.GetProperty("id").GetString()).Should()
            .BeEquivalentTo(KindList("agentTasks").Select(t => t.Id), $"the connection returned {nodes.Count} nodes");

        foreach (var node in nodes)
        {
            AssertUnresolvedObject(node, "deliverable", "deliverableId", deliverable.Id);
            AssertUnresolvedObject(node, "project", "projectId", project.Id);
        }
    }

    /// <summary>
    /// Asserts that the project object of a deliverable-by-id query is not resolved while its
    /// projectId references the created project.
    /// </summary>
    [Then(@"^the query coverage deliverable (\d+) project object is not resolved but its projectId references project (\d+)$")]
    public void ThenDeliverableProjectObjectIsNotResolved(int deliverableIndex, int projectIndex)
    {
        var node = StoredObject(DeliverableObjectKey);

        AssertUnresolvedObject(node, "project", "projectId", Entity("projects", projectIndex).Id);
        node.GetProperty("projectId").GetString().Should().Be(FieldOf("deliverables", deliverableIndex, "projectId"));
    }

    // ------------------------------------------------------------------ internals

    /// <summary>Creates one deliverable from a TOKEN:TYPE:STATUS spec.</summary>
    private async Task CreateDeliverableAsync(string spec, int projectIndex)
    {
        var fixture = Fixture();
        var parts = spec.Split(':');
        parts.Should().HaveCount(3, $"a deliverable spec is TOKEN:TYPE:STATUS but was '{spec}'");

        var token = parts[0].Trim();
        var type = parts[1].Trim();
        var status = parts[2].Trim();
        var project = Entity("projects", projectIndex);
        var title = $"{fixture.Marker} {token}";

        var variables = new
        {
            input = new
            {
                projectId = project.Id,
                title,
                type,
                description = $"{fixture.Marker} desc {token}",
                initialStatus = status
            }
        };

        var result = await ExecuteAsync(CreateDeliverableDocument, variables, "QcCreateDeliverable");
        result.HasErrors.Should().BeFalse(result.RawBody);

        var created = result.Field("createDeliverable");
        created.GetProperty("title").GetString().Should().Be(title);
        created.GetProperty("status").GetString().Should().Be(status);

        fixture.Deliverables.Add(new CreatedEntity(
            token,
            created.GetProperty("id").GetString()!,
            title,
            new Dictionary<string, string> { ["title"] = title, ["projectId"] = project.Id }));
    }

    /// <summary>Creates one agent task from a TOKEN:COMPLEXITY:STATUS spec and moves it to the requested status.</summary>
    private async Task CreateAgentTaskAsync(string spec, int deliverableIndex, int? dependencyIndex)
    {
        var fixture = Fixture();
        var parts = spec.Split(':');
        parts.Should().HaveCount(3, $"a task spec is TOKEN:COMPLEXITY:STATUS but was '{spec}'");

        var token = parts[0].Trim();
        var complexity = int.Parse(parts[1].Trim());
        var status = parts[2].Trim();
        var deliverable = Entity("deliverables", deliverableIndex);
        var projectId = deliverable.Fields["projectId"];
        var title = $"{fixture.Marker} {token}";

        var dependsOnId = dependencyIndex.HasValue ? Entity("agentTasks", dependencyIndex.Value).Id : null;

        var variables = new
        {
            input = new
            {
                deliverableId = deliverable.Id,
                projectId,
                title,
                description = $"{fixture.Marker} desc {token}",
                complexityRating = complexity,
                dependsOnAgentTaskId = dependsOnId
            }
        };

        var result = await ExecuteAsync(CreateAgentTaskDocument, variables, "QcCreateAgentTask");
        result.HasErrors.Should().BeFalse(result.RawBody);

        var created = result.Field("createAgentTask");
        created.GetProperty("title").GetString().Should().Be(title);

        var id = created.GetProperty("id").GetString()!;

        if (!status.Equals("READY", StringComparison.OrdinalIgnoreCase))
        {
            var statusResult = await ExecuteAsync(
                UpdateAgentTaskStatusDocument,
                new { id, targetStatus = status },
                "QcUpdateAgentTaskStatus");
            statusResult.HasErrors.Should().BeFalse(statusResult.RawBody);
        }

        fixture.Tasks.Add(new CreatedEntity(
            token,
            id,
            title,
            new Dictionary<string, string>
            {
                ["title"] = title,
                ["deliverableId"] = deliverable.Id,
                ["deliverableTitle"] = deliverable.Label,
                ["projectId"] = projectId,
                ["projectName"] = Entity("projects", IndexOf("projects", projectId)).Label,
                ["dependsOnAgentTaskId"] = dependsOnId ?? string.Empty,
                ["dependsOnAgentTaskTitle"] = dependencyIndex.HasValue ? Entity("agentTasks", dependencyIndex.Value).Label : string.Empty,
                ["complexityRating"] = complexity.ToString()
            }));
    }

    /// <summary>Runs a connection query, always combined with the per-scenario marker filter.</summary>
    private async Task RunConnectionAsync(
        string kind,
        Dictionary<string, object?> where,
        Dictionary<string, object?>? order,
        int? first,
        int? last,
        string? after,
        string? before)
    {
        var fixture = Fixture();

        var markerFilter = new Dictionary<string, object?>
        {
            [MarkerField(kind)] = new Dictionary<string, object?> { ["contains"] = fixture.Marker }
        };

        var combinedWhere = where.Count == 0
            ? markerFilter
            : new Dictionary<string, object?>
            {
                ["and"] = new List<object?> { where, markerFilter }
            };

        var variables = new Dictionary<string, object?>
        {
            ["where"] = combinedWhere,
            ["first"] = first,
            ["last"] = last,
            ["after"] = after,
            ["before"] = before,
            ["order"] = order is null ? null : new List<object?> { order }
        };

        var result = await ExecuteAsync(ConnectionDocument(kind), variables, "Qc" + UpperFirst(kind));
        result.HasErrors.Should().BeFalse(result.RawBody);
        result.HasData.Should().BeTrue(result.RawBody);
        result.IsFieldNull(kind).Should().BeFalse(result.RawBody);

        _scenarioContext[ResultKey] = result;
        StorePage(kind, result);
    }

    /// <summary>Parses the connection payload of a result into the scenario context.</summary>
    private void StorePage(string kind, GraphQLResult result)
    {
        var previous = _scenarioContext.TryGetValue<ConnectionPage>(PageKey, out var stored) && stored is not null
            ? stored.Nodes.Select(n => n.Id).ToList()
            : [];

        _scenarioContext[PreviousPageIdsKey] = previous;

        var connection = result.Field(kind);
        var pageInfo = connection.GetProperty("pageInfo");

        var nodes = ReadNodes(connection.GetProperty("nodes"), LabelField(kind));
        var edges = ReadEdges(connection.GetProperty("edges"));

        var page = new ConnectionPage
        {
            Kind = kind,
            Nodes = nodes,
            Edges = edges,
            HasNextPage = pageInfo.GetProperty("hasNextPage").GetBoolean(),
            HasPreviousPage = pageInfo.GetProperty("hasPreviousPage").GetBoolean(),
            StartCursor = ReadString(pageInfo, "startCursor"),
            EndCursor = ReadString(pageInfo, "endCursor")
        };

        _scenarioContext[PageKey] = page;

        if (!string.IsNullOrEmpty(page.StartCursor))
        {
            _scenarioContext[StartCursorKey] = page.StartCursor;
        }

        if (!string.IsNullOrEmpty(page.EndCursor))
        {
            _scenarioContext[EndCursorKey] = page.EndCursor;
        }
    }

    /// <summary>Reads the nodes array of a connection into comparable node records.</summary>
    private static List<ConnectionNode> ReadNodes(JsonElement nodes, string labelField)
    {
        if (nodes.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return nodes.EnumerateArray()
            .Select(node => new ConnectionNode(
                node.GetProperty("id").GetString()!,
                node.GetProperty(labelField).GetString()!,
                node))
            .ToList();
    }

    /// <summary>Reads the edges array of a connection into comparable edge records.</summary>
    private static List<ConnectionEdge> ReadEdges(JsonElement edges)
    {
        if (edges.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return edges.EnumerateArray()
            .Select(edge => new ConnectionEdge(
                edge.GetProperty("cursor").GetString()!,
                edge.GetProperty("node").GetProperty("id").GetString()!))
            .ToList();
    }

    /// <summary>Posts a GraphQL document and returns the parsed result.</summary>
    private async Task<GraphQLResult> ExecuteAsync(string query, object? variables, string operationName)
        => await GraphQLRequest.ExecuteAsync(_httpClient, query, variables, operationName);

    /// <summary>Builds the connection document for a schema field with all paging arguments as variables.</summary>
    private static string ConnectionDocument(string kind) => $$"""
        query Qc{{UpperFirst(kind)}}($where: {{FilterInput(kind)}}, $first: Int, $after: String, $last: Int, $before: String, $order: [{{SortInput(kind)}}!]) {
          {{kind}}(where: $where, first: $first, after: $after, last: $last, before: $before, order: $order) {
            pageInfo { hasNextPage hasPreviousPage startCursor endCursor }
            edges { cursor node { id {{LabelField(kind)}} } }
            nodes { {{NodeFields(kind)}} }
          }
        }
        """;

    /// <summary>The GraphQL input type used to filter a connection.</summary>
    private static string FilterInput(string kind) => kind switch
    {
        "projects" => "ProjectFilterInput",
        "deliverables" => "DeliverableFilterInput",
        "agentTasks" => "AgentTaskFilterInput",
        "largeLanguageModels" => "LargeLanguageModelFilterInput",
        _ => throw new ArgumentException($"Unknown connection '{kind}'.", nameof(kind))
    };

    /// <summary>The GraphQL input type used to sort a connection.</summary>
    private static string SortInput(string kind) => kind switch
    {
        "projects" => "ProjectSortInput",
        "deliverables" => "DeliverableSortInput",
        "agentTasks" => "AgentTaskSortInput",
        "largeLanguageModels" => "LargeLanguageModelSortInput",
        _ => throw new ArgumentException($"Unknown connection '{kind}'.", nameof(kind))
    };

    /// <summary>The scalar field that identifies a node of a connection.</summary>
    private static string LabelField(string kind) => kind switch
    {
        "projects" => "name",
        "deliverables" => "title",
        "agentTasks" => "title",
        "largeLanguageModels" => "url",
        _ => throw new ArgumentException($"Unknown connection '{kind}'.", nameof(kind))
    };

    /// <summary>
    /// The field the per-scenario marker filter is applied to. It is the label field for every
    /// connection except <c>largeLanguageModels</c>: a model's url carries only the bare
    /// <c>[DeleteAfterTest]</c> anchor that <c>deleteTestData</c> reclaims rows by, while the full
    /// per-scenario marker is written into <c>model</c>. Filtering the url on the full marker
    /// would therefore exclude the rows the scenario has just created.
    /// </summary>
    private static string MarkerField(string kind) => kind == "largeLanguageModels" ? "model" : LabelField(kind);

    /// <summary>The node selection set used for a connection.</summary>
    private static string NodeFields(string kind) => kind switch
    {
        "projects" => "id name",
        "deliverables" => "id title status",
        "agentTasks" => "id title status complexityRating",
        "largeLanguageModels" => "id url model modelAlias apiKey maxComplexity maxConcurrency cost",
        _ => throw new ArgumentException($"Unknown connection '{kind}'.", nameof(kind))
    };

    /// <summary>Capitalises the first letter of a schema field name for the operation name.</summary>
    private static string UpperFirst(string value) => char.ToUpperInvariant(value[0]) + value[1..];

    /// <summary>Builds a single-field sort input.</summary>
    private static Dictionary<string, object?> Sort(string field, string direction)
        => new() { [field] = direction };

    /// <summary>Builds a status filter shared by the project and deliverable nested filters.</summary>
    private static Dictionary<string, object?> StatusFilter(string status)
        => new() { ["status"] = new Dictionary<string, object?> { ["eq"] = status } };

    /// <summary>Builds an operation input such as <c>{ eq: "value" }</c> or <c>{ in: [ ... ] }</c>.</summary>
    private static Dictionary<string, object?> Operation(string op, object value)
        => new() { [op] = value };

    /// <summary>Splits a comma separated feature-table cell into trimmed, non-empty entries.</summary>
    private static string[] SplitList(string raw) => raw
        .Split(',')
        .Select(entry => entry.Trim())
        .Where(entry => entry.Length > 0)
        .ToArray();

    /// <summary>Resolves a filter value that may reference a created row by token or index.</summary>
    private string StringValue(string kind, string value)
    {
        if (value == "@MARKER")
        {
            return Fixture().Marker;
        }

        return TryEntity(kind, value, out var entity) ? entity.Label : value;
    }

    /// <summary>
    /// Resolves the value of a string operation: <c>in</c> / <c>nin</c> take a comma separated
    /// list of row references, every other operator a single value.
    /// </summary>
    private object StringValues(string kind, string op, string value)
    {
        if (!op.Equals("in", StringComparison.OrdinalIgnoreCase) && !op.Equals("nin", StringComparison.OrdinalIgnoreCase))
        {
            return StringValue(kind, value);
        }

        return SplitList(value).Select(reference => StringValue(kind, reference)).ToList();
    }

    /// <summary>Resolves a comma separated list of enum literals into a list for <c>in</c> / <c>nin</c> filters.</summary>
    private static object EnumValues(string value)
    {
        var values = SplitList(value);
        return values.Length == 1 ? values[0] : values.ToList();
    }

    /// <summary>Reads the fixture of the current scenario.</summary>
    private ScenarioFixture Fixture()
        => _scenarioContext.TryGetValue<ScenarioFixture>(FixtureKey, out var fixture) && fixture is not null
            ? fixture
            : throw new InvalidOperationException("No query coverage marker has been generated for this scenario.");

    /// <summary>Reads the connection page stored by the last connection query and checks its kind.</summary>
    private ConnectionPage Page(string kind)
    {
        var page = _scenarioContext.TryGetValue<ConnectionPage>(PageKey, out var stored) && stored is not null
            ? stored
            : throw new InvalidOperationException("No connection query has been executed.");

        page.Kind.Should().Be(kind, "the last connection query was a different connection");
        return page;
    }

    /// <summary>Reads the GraphQL result of the last connection query for assertion messages.</summary>
    private GraphQLResult LastResult
        => _scenarioContext.TryGetValue<GraphQLResult>(ResultKey, out var result) && result is not null
            ? result
            : throw new InvalidOperationException("No connection query has been executed.");

    /// <summary>Reads a stored cursor and fails when no cursor has been captured yet.</summary>
    private string Cursor(string key)
        => _scenarioContext.TryGetValue<string>(key, out var cursor) && !string.IsNullOrEmpty(cursor)
            ? cursor
            : throw new InvalidOperationException($"No {key} has been stored by a previous connection query.");

    /// <summary>Resolves the n-th created row of a connection kind (1-based).</summary>
    private CreatedEntity Entity(string kind, int index)
    {
        var list = KindList(kind);
        index.Should().BeInRange(1, list.Count, $"only {list.Count} {kind} have been created in this scenario");
        return list[index - 1];
    }

    /// <summary>Returns the created rows tracked for a connection kind.</summary>
    private List<CreatedEntity> KindList(string kind) => kind switch
    {
        "projects" => Fixture().Projects,
        "deliverables" => Fixture().Deliverables,
        "agentTasks" => Fixture().Tasks,
        "largeLanguageModels" => Fixture().Models,
        _ => throw new ArgumentException($"Unknown connection '{kind}'.", nameof(kind))
    };

    /// <summary>Looks up a created row by <c>@n</c> index or by its creation token.</summary>
    private bool TryEntity(string kind, string reference, out CreatedEntity entity)
    {
        entity = default!;
        var list = KindList(kind);

        if (reference.StartsWith('@') && int.TryParse(reference[1..], out var index))
        {
            if (index < 1 || index > list.Count)
            {
                return false;
            }

            entity = list[index - 1];
            return true;
        }

        var match = list.FirstOrDefault(candidate => candidate.Token == reference);
        if (match is null)
        {
            return false;
        }

        entity = match;
        return true;
    }

    /// <summary>Reads a JSON payload stored by a relation-object query.</summary>
    private JsonElement StoredObject(string key)
        => _scenarioContext.TryGetValue<JsonElement>(key, out var stored) && stored.ValueKind != JsonValueKind.Undefined
            ? stored
            : throw new InvalidOperationException($"No relation-object query has been executed for '{key}'.");

    /// <summary>Reads the node payloads stored by a relation-object connection query.</summary>
    private List<JsonElement> StoredNodes(string key)
        => _scenarioContext.TryGetValue<List<JsonElement>>(key, out var stored) && stored is not null
            ? stored
            : throw new InvalidOperationException("No relation-object connection query has been executed.");

    /// <summary>
    /// Asserts that a relation object field comes back as JSON null while its foreign key scalar
    /// carries the expected id. This pins the CURRENT server behaviour, which is product defect
    /// BUG-01 (ledger §6) -
    /// see the note on <see cref="ThenAgentTaskRelationObjectsAreNotResolved"/>.
    /// </summary>
    private static void AssertUnresolvedObject(JsonElement payload, string field, string foreignKeyField, string expectedId)
    {
        payload.GetProperty(foreignKeyField).GetString().Should().Be(expectedId, $"payload: {payload}");

        payload.GetProperty(field).ValueKind.Should().Be(
            JsonValueKind.Null,
            $"{field} is currently resolved as null by the server (product defect, see " +
            $"ThenAgentTaskRelationObjectsAreNotResolved); payload: {payload}");
    }

    /// <summary>Reads a tracked field of a created row and fails with a clear message when it is missing.</summary>
    private string FieldOf(string kind, int index, string field)
    {
        var value = Entity(kind, index).Fields.TryGetValue(field, out var stored) ? stored : null;
        value.Should().NotBeNullOrEmpty($"'{field}' was not tracked for {kind} #{index}");
        return value!;
    }

    /// <summary>Finds the 1-based index of a created project by its id.</summary>
    private int IndexOf(string kind, string id)
    {
        var index = KindList(kind).FindIndex(entity => entity.Id == id) + 1;
        index.Should().BeGreaterThan(0, $"no {kind} with id {id} was created in this scenario");
        return index;
    }

    /// <summary>Resolves the index of the project that owns one of the created deliverables.</summary>
    private int ProjectIndexOfDeliverable(int deliverableIndex)
        => IndexOf("projects", FieldOf("deliverables", deliverableIndex, "projectId"));

    /// <summary>Resolves a comma separated list of row references into the matching ids.</summary>
    private List<string> ExpectedIds(string kind, string references)
    {
        var ids = new List<string>();

        foreach (var reference in SplitList(references))
        {
            if (!TryEntity(kind, reference, out var entity))
            {
                throw new InvalidOperationException($"'{reference}' does not reference a created {kind}.");
            }

            ids.Add(entity.Id);
        }

        return ids;
    }

    /// <summary>Remembers the nodes of a by-id relation selection for the matching assertion step.</summary>
    private void StoreRelation(string kind, JsonElement nodes)
    {
        _scenarioContext[RelationKindKey] = kind;
        _scenarioContext[RelationNodesKey] = ReadNodes(nodes, LabelField(kind));
    }

    /// <summary>Asserts a by-id relation selection against the rows created for the scenario.</summary>
    private void AssertRelation(string kind, int count, string expected)
    {
        var storedKind = _scenarioContext.TryGetValue<string>(RelationKindKey, out var value) ? value : null;
        storedKind.Should().Be(kind, "no relation query for this kind has been executed");

        var nodes = _scenarioContext.TryGetValue<List<ConnectionNode>>(RelationNodesKey, out var storedNodes) && storedNodes is not null
            ? storedNodes
            : throw new InvalidOperationException("No relation query has been executed.");

        nodes.Should().HaveCount(count);
        nodes.Select(n => n.Id).Should().BeEquivalentTo(ExpectedIds(kind, expected));

        foreach (var node in nodes)
        {
            var created = KindList(kind).First(entity => entity.Id == node.Id);
            node.Label.Should().Be(created.Label);
        }
    }

    /// <summary>Reads a scalar node field as a comparable string, keeping numbers comparable.</summary>
    private static string ReadValue(JsonElement node, string field)
    {
        var value = node.GetProperty(field);

        return value.ValueKind switch
        {
            JsonValueKind.Number => value.GetInt64().ToString(),
            JsonValueKind.Null => string.Empty,
            _ => value.GetString() ?? string.Empty
        };
    }

    /// <summary>Reads an optional string field from a JSON element.</summary>
    private static string? ReadString(JsonElement element, string field)
        => element.GetProperty(field).ValueKind == JsonValueKind.Null ? null : element.GetProperty(field).GetString();

    /// <summary>Compares node values numerically when both sides are numbers, otherwise ordinally.</summary>
    private sealed class ValueComparer : IComparer<string>
    {
        /// <summary>Shared instance.</summary>
        public static readonly ValueComparer Instance = new();

        /// <summary>Compares two node values.</summary>
        public int Compare(string? x, string? y)
        {
            var leftIsNumber = long.TryParse(x, out var left);
            var rightIsNumber = long.TryParse(y, out var right);

            return leftIsNumber && rightIsNumber ? left.CompareTo(right) : string.CompareOrdinal(x, y);
        }
    }
}
