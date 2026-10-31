using System.Globalization;
using System.Net.Http;
using System.Text.Json;

using DevStack.Tests.Integration.GraphQL.Client.Hooks;
using DevStack.Tests.Integration.Shared;

using FluentAssertions;

using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.GraphQL.Client.StepDefinitions;

/// <summary>
/// Field round-trip bindings for <c>GraphQLFieldCoverage.feature</c> (GQL-13) and the
/// <c>DeliverableType</c> surface (GQL-14). Every value below is one that no earlier scenario
/// has ever sent, and every assertion compares the re-queried row against the exact value that
/// was written, including newlines, markdown-ish punctuation and non-ASCII characters.
/// </summary>
[Binding]
public sealed class FieldCoverageSteps
{
    /// <summary>Marker that makes <c>deleteTestData</c> reclaim a row.</summary>
    private const string Marker = "[DeleteAfterTest]";

    private const string FixtureKey = "FcFixture";
    private const string ResponseKey = "FcResponse";
    private const string ConnectionKey = "FcNodes";
    private const string CreatedDeliverableKey = "FcCreatedDeliverable";

    /// <summary>
    /// The document fields <c>updateDeliverable</c> accepts, in the order they are asserted.
    /// Values contain real control characters and non-ASCII text so the round-trip covers more than
    /// plain ASCII prose. The title is filled in per scenario because it has to carry the marker.
    /// </summary>
    private static readonly (string Field, string Value)[] DeliverableDocumentFields =
    [
        ("description", "Description line one\nline two — Grüße, 日本語, ✅"),
        ("acceptanceCriteria", "# Acceptance\n- [ ] criterion one\n- [ ] criterion two"),
        ("executionPlan", "1. first step\n2. second step **bold**"),
        ("agentFeedback", "agent feedback: `var x = 1;`\nsecond line\tindented"),
        ("securityImpact", "# Security\nimpact: none\tp95 < 200ms"),
        ("performanceImpact", "p95 < 200ms\nthroughput: 1000 rps (peak 1_200)"),
        ("testPlan", "```csharp\nAssert.True(result.IsSuccess);\n```"),
        ("deploymentPlan", "rollout: canary 10%\nrollback: revert #42"),
        ("blocking", "blocked by #7 — waiting for @team"),
        ("design", "# Design\n## Overview\n- item one\n- item two"),
    ];

    /// <summary>The number of fields the "every document field" scenario is expected to have sent.</summary>
    private static readonly int DeliverableDocumentFieldCount = DeliverableDocumentFields.Length + 1;

    /// <summary>
    /// KNOWN DEFECT of <c>updateDeliverable</c>, pinned here instead of fixed: <c>Mutation.UpdateDeliverableAsync</c>
    /// builds <c>UpdateDeliverableCommand</c> with positional arguments, but <c>UpdateDeliverableInput</c> declares
    /// <c>AgentFeedback</c> before <c>ExecutionPlan</c> while <c>UpdateDeliverableCommand</c> declares them the other
    /// way round. The mutation therefore writes the value of one of these fields into the other. The map names, for
    /// every affected column, the input field whose value it is observed to hold.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, string> CrossedDeliverableFields =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["agentFeedback"] = "executionPlan",
            ["executionPlan"] = "agentFeedback",
        };

    private const string CreateProjectDocument = """
        mutation FcCreateProject($input: CreateProjectInput!) {
          createProject(input: $input) { id name }
        }
        """;

    private const string CreateDeliverableDocument = """
        mutation FcCreateDeliverable($input: CreateDeliverableInput!) {
          createDeliverable(input: $input) { id title type design }
        }
        """;

    private const string UpdateDeliverableDocument = """
        mutation FcUpdateDeliverable($input: UpdateDeliverableInput!) {
          updateDeliverable(input: $input) { id }
        }
        """;

    private const string DeliverableByIdDocument = """
        query FcDeliverable($id: UUID!) {
          deliverable(id: $id) {
            id
            title
            description
            acceptanceCriteria
            executionPlan
            agentFeedback
            securityImpact
            performanceImpact
            testPlan
            deploymentPlan
            blocking
            design
          }
        }
        """;

    private const string CreateAgentTaskDocument = """
        mutation FcCreateAgentTask($input: CreateAgentTaskInput!) {
          createAgentTask(input: $input) { id title }
        }
        """;

    private const string UpdateAgentTaskDocument = """
        mutation FcUpdateAgentTask($input: UpdateAgentTaskInput!) {
          updateAgentTask(input: $input) { id }
        }
        """;

    private const string AgentTaskByIdDocument = """
        query FcAgentTask($id: UUID!) {
          agentTask(id: $id) {
            id
            title
            result
            errors
            commitHash
            promptTokens
            completionTokens
            executionDurationInSeconds
            agent
          }
        }
        """;

    private const string CreateModelDocument = """
        mutation FcCreateLargeLanguageModel($input: CreateLargeLanguageModelInput!) {
          createLargeLanguageModel(input: $input) { id url }
        }
        """;

    private const string UpdateModelDocument = """
        mutation FcUpdateLargeLanguageModel($input: UpdateLargeLanguageModelInput!) {
          updateLargeLanguageModel(input: $input) { id }
        }
        """;

    private const string ModelByIdDocument = """
        query FcLargeLanguageModel($id: UUID!) {
          largeLanguageModel(id: $id) {
            id
            url
            model
            modelAlias
            apiKey
            maxComplexity
            maxConcurrency
            cost
          }
        }
        """;

    private const string DeliverablesConnectionDocument = """
        query FcDeliverables($where: DeliverableFilterInput, $first: Int) {
          deliverables(where: $where, first: $first) {
            nodes { id title type }
          }
        }
        """;

    private readonly ScenarioContext _scenarioContext;
    private readonly HttpClient _httpClient;

    public FieldCoverageSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _httpClient = SpecFlowHooks.GetHttpClient(scenarioContext);
    }

    /// <summary>Rows and sent values of the current scenario.</summary>
    private sealed class Fixture(string marker, string slug)
    {
        /// <summary>Full <c>[DeleteAfterTest] F-&lt;slug&gt;</c> prefix used in names, titles and urls.</summary>
        public string Marker { get; } = marker;

        /// <summary>Random suffix that makes the marker unique per scenario.</summary>
        public string Slug { get; } = slug;

        /// <summary>The project every deliverable of the scenario belongs to.</summary>
        public string? ProjectId { get; set; }

        /// <summary>The deliverable the update scenarios write to.</summary>
        public string? DeliverableId { get; set; }

        /// <summary>The agent task the update scenarios write to.</summary>
        public string? AgentTaskId { get; set; }

        /// <summary>The large language model the update scenario writes to.</summary>
        public string? ModelId { get; set; }

        /// <summary>The exact values the last write step sent, keyed by GraphQL field name.</summary>
        public Dictionary<string, string> Sent { get; } = new(StringComparer.Ordinal);

        /// <summary>Deliverables created by the type scenarios, keyed by the type string that was sent.</summary>
        public Dictionary<string, string> DeliverablesByType { get; } = new(StringComparer.Ordinal);
    }

    // ------------------------------------------------------------------ setup

    /// <summary>Generates the unique per-scenario <c>[DeleteAfterTest]</c> marker.</summary>
    [Given(@"^a field coverage marker$")]
    public void GivenAFieldCoverageMarker()
    {
        var slug = Guid.NewGuid().ToString("N")[..8];
        _scenarioContext[FixtureKey] = new Fixture($"{Marker} F-{slug}", slug);
    }

    /// <summary>Creates the project all field coverage deliverables belong to.</summary>
    [Given(@"^a field coverage project$")]
    public void GivenAFieldCoverageProject()
    {
        var fixture = Current();
        var result = Execute(
            CreateProjectDocument,
            new
            {
                input = new
                {
                    name = $"{fixture.Marker} project {fixture.Slug}",
                    repository = $"https://example.com/{fixture.Slug}",
                    description = (string?)null
                }
            },
            "FcCreateProject");

        NoErrors(result);
        fixture.ProjectId = result.Field("createProject").GetProperty("id").GetString();
    }

    /// <summary>Creates a deliverable with only the mandatory fields, so the update scenarios start clean.</summary>
    [Given(@"^a field coverage deliverable$")]
    public void GivenAFieldCoverageDeliverable()
    {
        var fixture = Current();
        var result = Execute(
            CreateDeliverableDocument,
            new
            {
                input = new
                {
                    projectId = ProjectId(),
                    title = $"{fixture.Marker} deliverable {fixture.Slug}",
                    type = "Feature",
                    description = "seed",
                    initialStatus = "DRAFT"
                }
            },
            "FcCreateDeliverable");

        NoErrors(result);
        fixture.DeliverableId = result.Field("createDeliverable").GetProperty("id").GetString();
        Store(result);
    }

    /// <summary>Creates an agent task with only the mandatory fields.</summary>
    [Given(@"^a field coverage agent task$")]
    public void GivenAFieldCoverageAgentTask()
    {
        var fixture = Current();
        var result = Execute(
            CreateAgentTaskDocument,
            new
            {
                input = new
                {
                    deliverableId = DeliverableId(),
                    projectId = ProjectId(),
                    title = $"{fixture.Marker} task {fixture.Slug}",
                    description = "field coverage task",
                    complexityRating = 5,
                    dependsOnAgentTaskId = (string?)null
                }
            },
            "FcCreateAgentTask");

        NoErrors(result);
        fixture.AgentTaskId = result.Field("createAgentTask").GetProperty("id").GetString();
        Store(result);
    }

    /// <summary>Creates a large language model whose url carries the marker so cleanup can reclaim it.</summary>
    [Given(@"^a field coverage large language model$")]
    public void GivenAFieldCoverageLargeLanguageModel()
    {
        var fixture = Current();
        var result = Execute(
            CreateModelDocument,
            new
            {
                input = new
                {
                    url = $"https://example.com/{Marker}/{fixture.Slug}/llm",
                    model = $"{fixture.Marker} seed-model",
                    modelAlias = "seed-alias",
                    apiKey = "seed-key",
                    cost = 0,
                    maxComplexity = 10,
                    maxConcurrency = 1
                }
            },
            "FcCreateLargeLanguageModel");

        NoErrors(result);
        fixture.ModelId = result.Field("createLargeLanguageModel").GetProperty("id").GetString();
        Store(result);
    }

    // ------------------------------------------------------------- GQL-13 writes

    /// <summary>Creates a deliverable that sends the <c>design</c> field of <c>CreateDeliverableInput</c>.</summary>
    [When(@"^I create a field coverage deliverable with design ""([^""]+)""$")]
    public void WhenICreateAFieldCoverageDeliverableWithDesign(string design)
    {
        var fixture = Current();
        var value = Decode(design);

        var result = Execute(
            CreateDeliverableDocument,
            new
            {
                input = new
                {
                    projectId = ProjectId(),
                    title = $"{fixture.Marker} deliverable {fixture.Slug}",
                    type = "Feature",
                    description = "seed",
                    initialStatus = "DRAFT",
                    design = value
                }
            },
            "FcCreateDeliverable");

        NoErrors(result);
        fixture.DeliverableId = result.Field("createDeliverable").GetProperty("id").GetString();
        Remember("design", value);
        Store(result);
    }

    /// <summary>Sends a single <c>UpdateDeliverableInput</c> field; every other field stays null.</summary>
    [When(@"^I update the field coverage deliverable field ""([^""]+)"" to ""([^""]+)""$")]
    public void WhenIUpdateTheFieldCoverageDeliverableField(string field, string value)
    {
        var decoded = Decode(value);

        var input = new Dictionary<string, object?> { ["id"] = DeliverableId(), [field] = decoded };
        var result = Execute(UpdateDeliverableDocument, new { input }, "FcUpdateDeliverable");

        Remember(field, decoded);
        Store(result);
    }

    /// <summary>Sends every document field of <c>UpdateDeliverableInput</c> in a single mutation.</summary>
    [When(@"^I update the field coverage deliverable with every document field$")]
    public void WhenIUpdateTheFieldCoverageDeliverableWithEveryDocumentField()
    {
        var fixture = Current();
        fixture.Sent.Clear();

        var input = new Dictionary<string, object?> { ["id"] = DeliverableId() };

        foreach (var (field, value) in DeliverableDocumentFields)
        {
            input[field] = value;
            fixture.Sent[field] = value;
        }

        var title = $"{fixture.Marker} deliverable renamed";
        input["title"] = title;
        fixture.Sent["title"] = title;

        Store(Execute(UpdateDeliverableDocument, new { input }, "FcUpdateDeliverable"));
    }

    /// <summary>
    /// Sends <c>agentFeedback</c> and <c>executionPlan</c> in one mutation so that the crossing of the two
    /// columns can be pinned by <see cref="ThenTheFieldCoverageDeliverableFieldShouldHoldTheValueSentForAnother" />.
    /// </summary>
    [When(@"^I update the field coverage deliverable with agentFeedback ""([^""]+)"" and executionPlan ""([^""]+)""$")]
    public void WhenIUpdateTheFieldCoverageDeliverableCrossedPair(string agentFeedback, string executionPlan)
    {
        var fixture = Current();
        var feedback = Decode(agentFeedback);
        var plan = Decode(executionPlan);

        var input = new Dictionary<string, object?>
        {
            ["id"] = DeliverableId(),
            ["agentFeedback"] = feedback,
            ["executionPlan"] = plan,
        };

        fixture.Sent.Clear();
        fixture.Sent["agentFeedback"] = feedback;
        fixture.Sent["executionPlan"] = plan;

        Store(Execute(UpdateDeliverableDocument, new { input }, "FcUpdateDeliverable"));
    }

    /// <summary>Sends a single <c>UpdateAgentTaskInput</c> field; every other field stays null.</summary>
    [When(@"^I update the field coverage agent task field ""([^""]+)"" to ""([^""]+)""$")]
    public void WhenIUpdateTheFieldCoverageAgentTaskField(string field, string value)
    {
        var decoded = Decode(value);
        object sent = IsIntegerField(field) ? int.Parse(decoded, CultureInfo.InvariantCulture) : decoded;

        var input = new Dictionary<string, object?> { ["id"] = AgentTaskId(), [field] = sent };
        var result = Execute(UpdateAgentTaskDocument, new { input }, "FcUpdateAgentTask");

        Remember(field, decoded);
        Store(result);
    }

    /// <summary>Sends <c>apiKey</c>, <c>cost</c> and <c>modelAlias</c> of <c>UpdateLargeLanguageModelInput</c>.</summary>
    [When(@"^I update the field coverage large language model with apiKey ""([^""]+)"" cost (\d+) and modelAlias ""([^""]+)""$")]
    public void WhenIUpdateTheFieldCoverageLargeLanguageModel(string apiKey, int cost, string modelAlias)
    {
        var fixture = Current();
        var decodedKey = Decode(apiKey);
        var decodedAlias = Decode(modelAlias);

        var result = Execute(
            UpdateModelDocument,
            new { input = new { id = ModelId(), apiKey = decodedKey, cost, modelAlias = decodedAlias } },
            "FcUpdateLargeLanguageModel");

        fixture.Sent.Clear();
        fixture.Sent["apiKey"] = decodedKey;
        fixture.Sent["cost"] = cost.ToString(CultureInfo.InvariantCulture);
        fixture.Sent["modelAlias"] = decodedAlias;
        Store(result);
    }

    // ------------------------------------------------------------- GQL-13 reads

    /// <summary>Re-queries the deliverable of the scenario.</summary>
    [When(@"^I query the field coverage deliverable$")]
    public void WhenIQueryTheFieldCoverageDeliverable()
    {
        var result = Execute(DeliverableByIdDocument, new { id = DeliverableId() }, "FcDeliverable");
        NoErrors(result);
        result.IsFieldNull("deliverable").Should().BeFalse("the deliverable should exist: {0}", result.RawBody);
        Store(result);
    }

    /// <summary>Re-queries the agent task of the scenario.</summary>
    [When(@"^I query the field coverage agent task$")]
    public void WhenIQueryTheFieldCoverageAgentTask()
    {
        var result = Execute(AgentTaskByIdDocument, new { id = AgentTaskId() }, "FcAgentTask");
        NoErrors(result);
        result.IsFieldNull("agentTask").Should().BeFalse("the agent task should exist: {0}", result.RawBody);
        Store(result);
    }

    /// <summary>Re-queries the large language model of the scenario.</summary>
    [When(@"^I query the field coverage large language model$")]
    public void WhenIQueryTheFieldCoverageLargeLanguageModel()
    {
        var result = Execute(ModelByIdDocument, new { id = ModelId() }, "FcLargeLanguageModel");
        NoErrors(result);
        result.IsFieldNull("largeLanguageModel").Should().BeFalse("the model should exist: {0}", result.RawBody);
        Store(result);
    }

    // ------------------------------------------------------------- GQL-13 asserts

    /// <summary>Asserts the last mutation reported no errors.</summary>
    [Then(@"^the field coverage mutation should report no errors$")]
    public void ThenTheFieldCoverageMutationShouldReportNoErrors() => NoErrors(Last);

    /// <summary>Asserts a re-queried deliverable field still holds exactly the value that was sent.</summary>
    [Then(@"^the field coverage deliverable field ""([^""]+)"" should equal the value that was sent$")]
    public void ThenTheFieldCoverageDeliverableFieldShouldEqualTheSentValue(string field) => AssertField("deliverable", field);

    /// <summary>Asserts a re-queried agent task field still holds exactly the value that was sent.</summary>
    [Then(@"^the field coverage agent task field ""([^""]+)"" should equal the value that was sent$")]
    public void ThenTheFieldCoverageAgentTaskFieldShouldEqualTheSentValue(string field) => AssertField("agentTask", field);

    /// <summary>
    /// Asserts every document field of the deliverable holds the value that was sent for it. The crossed pair
    /// <c>agentFeedback</c> / <c>executionPlan</c> is compared with the value of the column the API really wrote
    /// it to, so the defect stays pinned (see <see cref="CrossedDeliverableFields" />).
    /// </summary>
    [Then(@"^the field coverage deliverable should expose every document field that was sent$")]
    public void ThenTheFieldCoverageDeliverableShouldExposeEveryDocumentField()
    {
        var sent = Current().Sent.Keys.ToList();
        sent.Should().HaveCount(
            DeliverableDocumentFieldCount,
            $"the write step should have sent all {DeliverableDocumentFieldCount} document fields");

        foreach (var field in sent)
        {
            AssertField("deliverable", field);
        }
    }

    /// <summary>
    /// Asserts the API echoes <c>apiKey</c> back verbatim next to <c>cost</c> and <c>modelAlias</c>.
    /// The key is returned in clear text by <c>largeLanguageModel(id:)</c>; this assertion pins the
    /// observed behaviour and is the security observation reported for GQL-13.
    /// </summary>
    [Then(@"^the field coverage large language model should return the sent apiKey, cost and modelAlias verbatim$")]
    public void ThenTheFieldCoverageModelShouldReturnTheSentValues()
    {
        foreach (var field in new[] { "apiKey", "cost", "modelAlias" })
        {
            AssertField("largeLanguageModel", field);
        }
    }

    // ------------------------------------------------------------------ GQL-14

    /// <summary>Creates a deliverable sending an arbitrary <c>type</c> string.</summary>
    [When(@"^I create a field coverage deliverable of type ""([^""]+)""$")]
    public void WhenICreateAFieldCoverageDeliverableOfType(string type)
    {
        var fixture = Current();
        var result = Execute(
            CreateDeliverableDocument,
            new
            {
                input = new
                {
                    projectId = ProjectId(),
                    title = $"{fixture.Marker} deliverable-{type}",
                    type,
                    description = "seed",
                    initialStatus = "DRAFT"
                }
            },
            "FcCreateDeliverable");

        NoErrors(result);
        fixture.DeliverablesByType[type] = result.Field("createDeliverable").GetProperty("id").GetString()!;
        _scenarioContext[CreatedDeliverableKey] = result.Field("createDeliverable");
        Store(result);
    }

    /// <summary>Queries the deliverables of the scenario filtered by an enum type literal.</summary>
    [When(@"^I query field coverage deliverables filtered by type ""([^""]+)""$")]
    public void WhenIQueryFieldCoverageDeliverablesFilteredByType(string type)
    {
        var where = new Dictionary<string, object?>
        {
            ["title"] = new Dictionary<string, object?> { ["contains"] = Current().Marker },
            ["type"] = new Dictionary<string, object?> { ["eq"] = type }
        };

        var result = Execute(DeliverablesConnectionDocument, new { where, first = 10 }, "FcDeliverables");
        NoErrors(result);

        _scenarioContext[ConnectionKey] = result.Field("deliverables").GetProperty("nodes").EnumerateArray()
            .Select(node => new ConnectionRow(
                node.GetProperty("id").GetString()!,
                node.GetProperty("title").GetString()!,
                node.GetProperty("type").GetString()!))
            .ToList();

        Store(result);
    }

    /// <summary>Asserts the persisted <c>type</c> of the deliverable created by the type step.</summary>
    [Then(@"^the field coverage deliverable should have type ""([^""]+)""$")]
    public void ThenTheFieldCoverageDeliverableShouldHaveType(string expected)
    {
        var created = _scenarioContext.TryGetValue<JsonElement>(CreatedDeliverableKey, out var element)
            ? element
            : throw new InvalidOperationException("No field coverage deliverable has been created.");

        created.GetProperty("type").GetString().Should().Be(expected);
    }

    /// <summary>Asserts how many nodes the type-filtered connection returned.</summary>
    [Then(@"^the field coverage deliverable connection should contain (\d+) nodes$")]
    public void ThenTheFieldCoverageDeliverableConnectionShouldContainNodes(int count)
        => Nodes().Should().HaveCount(count, "the connection payload is {0}", Last.RawBody);

    /// <summary>Asserts the type-filtered connection returned exactly the deliverable created for that type string.</summary>
    [Then(@"^the field coverage deliverable connection should contain only the deliverable of type ""([^""]+)""$")]
    public void ThenTheFieldCoverageDeliverableConnectionShouldContainOnlyThatDeliverable(string type)
    {
        var fixture = Current();
        var expectedId = fixture.DeliverablesByType[type];

        Nodes().Should().ContainSingle("the connection payload is {0}", Last.RawBody);
        Nodes()[0].Id.Should().Be(expectedId);
        Nodes()[0].Title.Should().Be($"{fixture.Marker} deliverable-{type}");
        Nodes()[0].Type.Should().Be(type.ToUpperInvariant(), "the server persists the parsed enum, not the raw string");
    }

    /// <summary>
    /// Asserts that a column holds the value that was sent for a different input field. This is how the
    /// <c>agentFeedback</c> / <c>executionPlan</c> crossing of <c>updateDeliverable</c> is pinned; the assertion
    /// turns red as soon as the API maps the two fields correctly.
    /// </summary>
    [Then(@"^the field coverage deliverable field ""([^""]+)"" should hold the value that was sent for ""([^""]+)""$")]
    public void ThenTheFieldCoverageDeliverableFieldShouldHoldTheValueSentForAnother(string field, string sentFor)
    {
        var expected = Current().Sent.TryGetValue(sentFor, out var value)
            ? value
            : throw new InvalidOperationException($"No value for '{sentFor}' has been sent by this scenario.");

        var actual = Last.Field("deliverable").TryGetProperty(field, out var property) && property.ValueKind != JsonValueKind.Null
            ? property.ToString()
            : throw new InvalidOperationException($"'deliverable.{field}' was not persisted. Payload: {Last.RawBody}");

        actual.Should().Be(expected, $"updateDeliverable is observed to write the value sent for '{sentFor}' into '{field}'");
    }

    // ------------------------------------------------------------------ helpers

    /// <summary>A node of the type-filtered deliverable connection.</summary>
    private sealed record ConnectionRow(string Id, string Title, string Type);

    /// <summary>
    /// Compares one field of the last query payload against the value that was sent for it. For a deliverable
    /// column listed in <see cref="CrossedDeliverableFields" /> the value of the input field the API really wrote
    /// it to is used, so the crossing of <c>agentFeedback</c> and <c>executionPlan</c> stays pinned.
    /// </summary>
    private void AssertField(string root, string field)
    {
        var (sent, sentFor) = ExpectedFor(root, field);

        var entity = Last.Field(root);
        var actual = entity.TryGetProperty(field, out var property) && property.ValueKind != JsonValueKind.Null
            ? property.ToString()
            : throw new InvalidOperationException($"'{root}.{field}' was not persisted. Payload: {Last.RawBody}");

        actual.Should().Be(sent, $"round-trip of '{root}.{field}' against the value sent for '{sentFor}'");
    }

    /// <summary>Resolves which sent value the API is observed to store in a column.</summary>
    private (string Value, string SentFor) ExpectedFor(string root, string field)
    {
        var sent = Current().Sent;
        var sentFor = root == "deliverable" && CrossedDeliverableFields.TryGetValue(field, out var crossed)
            ? crossed
            : field;

        return sent.TryGetValue(sentFor, out var value)
            ? (value, sentFor)
            : throw new InvalidOperationException($"No value for '{sentFor}' has been sent by this scenario.");
    }

    /// <summary>Records a sent value, replacing the values recorded by the previous write step.</summary>
    private void Remember(string field, string value)
    {
        var sent = Current().Sent;
        sent.Clear();
        sent[field] = value;
    }

    /// <summary>
    /// Expands the harness escapes used inside feature-file examples: <c>@MARKER</c> becomes the
    /// per-scenario marker, <c>\n</c> and <c>\t</c> become real control characters.
    /// </summary>
    private string Decode(string raw) => raw
        .Replace("@MARKER", Current().Marker, StringComparison.Ordinal)
        .Replace("\\n", "\n", StringComparison.Ordinal)
        .Replace("\\t", "\t", StringComparison.Ordinal);

    /// <summary>Reports whether an <c>UpdateAgentTaskInput</c> field is an <c>Int</c> rather than a <c>String</c>.</summary>
    private static bool IsIntegerField(string field) => field is "promptTokens" or "completionTokens" or "executionDurationInSeconds";

    /// <summary>Fails when the payload carries errors, using the raw body as the failure message.</summary>
    private static void NoErrors(GraphQLResult result)
        => result.HasErrors.Should().BeFalse("the payload must not carry errors: {0}", result.RawBody);

    /// <summary>Posts a GraphQL document and returns the parsed result.</summary>
    private GraphQLResult Execute(string query, object? variables, string operationName)
        => GraphQLRequest.Execute(_httpClient, query, variables, operationName);

    /// <summary>Stores a payload as the scenario's last response.</summary>
    private void Store(GraphQLResult result) => _scenarioContext[ResponseKey] = result;

    /// <summary>Reads the payload of the last request of the scenario.</summary>
    private GraphQLResult Last
        => _scenarioContext.TryGetValue<GraphQLResult>(ResponseKey, out var result) && result is not null
            ? result
            : throw new InvalidOperationException("No field coverage request has been executed.");

    /// <summary>Reads the nodes captured by the last type-filtered connection query.</summary>
    private List<ConnectionRow> Nodes()
        => _scenarioContext.TryGetValue<List<ConnectionRow>>(ConnectionKey, out var nodes) && nodes is not null
            ? nodes
            : throw new InvalidOperationException("No field coverage connection query has been executed.");

    /// <summary>Reads the fixture of the current scenario.</summary>
    private Fixture Current()
        => _scenarioContext.TryGetValue<Fixture>(FixtureKey, out var fixture) && fixture is not null
            ? fixture
            : throw new InvalidOperationException("No field coverage marker has been generated for this scenario.");

    /// <summary>Resolves the project of the scenario.</summary>
    private string ProjectId() => Current().ProjectId ?? throw new InvalidOperationException("No field coverage project has been created.");

    /// <summary>Resolves the deliverable of the scenario.</summary>
    private string DeliverableId() => Current().DeliverableId ?? throw new InvalidOperationException("No field coverage deliverable has been created.");

    /// <summary>Resolves the agent task of the scenario.</summary>
    private string AgentTaskId() => Current().AgentTaskId ?? throw new InvalidOperationException("No field coverage agent task has been created.");

    /// <summary>Resolves the large language model of the scenario.</summary>
    private string ModelId() => Current().ModelId ?? throw new InvalidOperationException("No field coverage large language model has been created.");
}
