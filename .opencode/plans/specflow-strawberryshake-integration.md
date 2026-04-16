# SpecFlow + StrawberryShake GraphQL Integration Plan

## Overview
Implement SpecFlow BDD framework with StrawberryShake-generated GraphQL client for integration tests in `DevStack.Tests.Integration.GraphQL.Client`. Create Gherkin feature files for all GraphQL queries and mutations, implement step definitions, and test against a deployed API instance at `http://localhost:8087`.

## Current State Analysis

### Existing Test Infrastructure
- **Testcontainers**: PostgreSQL container with shared instance (`TestContainerFixture`)
- **Current approach**: Direct database access via `DevStackDbContext` using xUnit
- **Target**: SpecFlow BDD tests with GraphQL client against running API

### GraphQL Schema Coverage
The API exposes the following entities via HotChocolate:
- **Projects**: CRUD operations
- **Features**: CRUD + status transitions
- **Defects**: CRUD + status transitions  
- **Tasks**: CRUD + status transitions
- **Epics**: CRUD operations
- **ModelConfigurations**: CRUD operations
- **WorkflowRuns**: CRUD + cancel operations
- **DashboardSummary**: Aggregation query

### Existing GraphQL Files
Admin UI already has `.graphql` files in `src/AdminUi/src/graphql/` that can serve as reference patterns.

## Implementation Plan

### Phase 1: Setup and Configuration (Task #168)

**Packages to Add:**
```xml
<PackageReference Include="SpecFlow.xUnit" Version="3.9.74" />
<PackageReference Include="SpecFlow.Tools.MsBuild.Generation" Version="3.9.74" />
<PackageReference Include="StrawberryShake.CodeGeneration" Version="12.22.7" />
<PackageReference Include="StrawberryShake.CodeGeneration.CSharp" Version="12.22.7" />
<PackageReference Include="StrawberryShake.Transport.Http" Version="15.1.14" />
```

**Configuration Files:**

1. **specflow.json** - SpecFlow configuration
   ```json
   {
     "language": {
       "unitTestProvider": "xUnit"
     },
     "stepAssemblies": [
       { "assembly": "SpecFlow.Tools.MsBuild.Generation" }
     ],
     "generator": {
       "allowDebugGeneratedFiles": true
     }
   }
   ```

2. **.graphqlrc.json** - GraphQL code generation configuration
   ```json
   {
     "schema": "http://localhost:8087/graphql",
     "documents": "**/*.graphql",
     "generators": [
       {
         "name": "csharp",
         "output": "Generated",
         "typeStyle": "record",
         "namespace": "DevStack.Tests.Integration.GraphQL.Client.Generated"
       }
     ]
   }
   ```

3. **Update .csproj** to enable code generation:
   ```xml
   <ItemGroup>
     <PackageReference Include="SpecFlow.xUnit" Version="3.9.74" />
     <PackageReference Include="SpecFlow.Tools.MsBuild.Generation" Version="3.9.74" />
     <PackageReference Include="StrawberryShake.CodeGeneration" Version="12.22.7" />
     <PackageReference Include="StrawberryShake.CodeGeneration.CSharp" Version="12.22.7" />
     <PackageReference Include="StrawberryShake.Transport.Http" Version="15.1.14" />
   </ItemGroup>
   
   <ItemGroup>
     <FeatureFiles Include="Features/**/*.feature" />
     <GraphQL Include="GraphQL/**/*.graphql" />
   </ItemGroup>
   ```

### Phase 2: Gherkin Feature Files (Tasks #169-175)

**Directory Structure:**
```
src/Server/DevStack.Tests.Integration.GraphQL.Client/
  Features/
    Projects.feature
    Features.feature
    Defects.feature
    Tasks.feature
    Epics.feature
    ModelConfigurations.feature
    WorkflowRuns.feature
    Dashboard.feature
    ErrorHandling.feature
  GraphQL/
    Queries/
      projects.graphql
      features.graphql
      ... (all queries)
    Mutations/
      createProject.graphql
      createFeature.graphql
      ... (all mutations)
```

**Example Feature File - Projects.feature:**
```gherkin
Feature: Project Management
  As a project manager
  I want to manage projects via GraphQL
  So that I can track my development work

  Scenario: Create a new project
    Given I have a valid project name "Test Project"
    When I create the project with description "Test description"
    Then the project should be created successfully
    And the project ID should not be empty
    And the project name should match "Test Project"

  Scenario Outline: Create project with validation
    Given I have a project name "<name>"
    When I attempt to create the project
    Then I should receive validation error "<error>"

    Examples:
      | name | error |
      |      | Name is required |
      | <repeat>500 characters<repeat> | Name must be 200 characters or less |

  Scenario: Get project by ID
    Given a project exists with ID <id>
    When I retrieve the project by ID
    Then the project should be found
    And the project fields should match

  Scenario: Get projects with pagination
    Given there are 10 projects in the database
    When I retrieve projects with first=5
    Then I should receive 5 projects
    And the pageInfo should indicate hasNextPage=true
```

### Phase 3: GraphQL Query/Mutation Files (Tasks #169-175)

**Directory Structure:**
```
GraphQL/
  Queries/
    projects.graphql
    features.graphql
    defects.graphql
    tasks.graphql
    epics.graphql
    modelConfigurations.graphql
    workflowRuns.graphql
    dashboard.graphql
  Mutations/
    createProject.graphql
    updateProject.graphql
    deleteProject.graphql
    createFeature.graphql
    updateFeature.graphql
    transitionFeatureStatus.graphql
    deleteFeature.graphql
    ... (similar for all entities)
```

**Query Examples:**

Projects query:
```graphql
query GetProjects($first: Int!, $skip: Int) {
  projects(first: $first, skip: $skip) {
    nodes {
      id
      name
      description
      architecture
      memory
      githubUrl
      createdAt
      updatedAt
    }
    pageInfo {
      hasNextPage
      hasPreviousPage
      totalCount
    }
  }
}
```

Feature mutation:
```graphql
mutation CreateFeature($input: CreateFeatureInput!) {
  createFeature(input: $input) {
    feature {
      id
      title
      status
    }
    errors
  }
}
```

### Phase 4: Client Wrapper (Task #176)

**DevStackGraphQLClient Class:**
```csharp
[Binding]
public class DevStackGraphQLClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _graphQLEndpoint = "http://localhost:8087/graphql";
    
    public DevStackGraphQLClient()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_graphQLEndpoint)
        };
    }
    
    // Query methods
    public async Task<Project?> GetProjectByIdAsync(Guid id) { ... }
    public async Task<GetProjectsResult> GetProjectsAsync(int first = 50, int? skip = null) { ... }
    
    // Mutation methods
    public async Task<Guid> CreateProjectAsync(CreateProjectInput input) { ... }
    public async Task<bool> UpdateProjectAsync(UpdateProjectInput input) { ... }
    
    // Error handling
    // Response mapping
    // IDisposable for cleanup
}
```

### Phase 5: Step Definitions (Tasks #177-183)

**Directory Structure:**
```
StepDefinitions/
  ProjectsStepDefinitions.cs
  FeaturesStepDefinitions.cs
  DefectsStepDefinitions.cs
  TasksStepDefinitions.cs
  EpicsStepDefinitions.cs
  ModelConfigurationsStepDefinitions.cs
  WorkflowRunsStepDefinitions.cs
  DashboardStepDefinitions.cs
  ErrorHandlingStepDefinitions.cs
```

**Example Step Definitions - ProjectsStepDefinitions.cs:**
```csharp
[Binding]
public class ProjectsStepDefinitions
{
    private readonly DevStackGraphQLClient _graphQLClient;
    private readonly TestContext _testContext;
    private Project? _createdProject;
    private List<Project> _retrievedProjects = new();
    private Exception? _exception;

    public ProjectsStepDefinitions(DevStackGraphQLClient graphQLClient, TestContext testContext)
    {
        _graphQLClient = graphQLClient;
        _testContext = testContext;
    }

    [Given(@"I have a valid project name ""(.*)""")]
    public async Task GivenIHaveAValidProjectName(string name)
    {
        _testContext.ProjectName = name;
    }

    [When(@"I create the project with description ""(.*)""")]
    public async Task WhenICreateTheProject(string description)
    {
        try
        {
            var input = new CreateProjectInput(
                _testContext.ProjectName,
                description,
                null, null, null);
            
            _createdProject = await _graphQLClient.CreateProjectAsync(input);
        }
        catch (Exception ex)
        {
            _exception = ex;
        }
    }

    [Then(@"the project should be created successfully")]
    public void ThenTheProjectShouldBeCreatedSuccessfully()
    {
        _createdProject.Should().NotBeNull();
        _exception.Should().BeNull();
    }

    [Then(@"the project ID should not be empty")]
    public void ThenTheProjectIDShouldNotBeEmpty()
    {
        _createdProject!.Id.Should().NotBe(Guid.Empty);
    }
}
```

**TestContext for sharing state between steps:**
```csharp
public class TestContext
{
    public string? ProjectName { get; set; }
    public string? FeatureTitle { get; set; }
    public Guid? CreatedId { get; set; }
    // ... other shared state
}
```

### Phase 6: Test Infrastructure (Hooks & Fixtures)

**GlobalHooks.cs:**
```csharp
[Binding]
public class GlobalHooks
{
    private readonly TestContainerFixture _fixture;
    private readonly TestContext _testContext;

    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        // Initialize static resources
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        _testContext.Clear();
        // Setup test data if needed
    }

    [AfterScenario]
    public async Task AfterScenario()
    {
        // Cleanup test data
        await CleanupTestDataAsync();
    }

    [AfterTestRun]
    public static void AfterTestRun()
    {
        // Cleanup static resources
    }
}
```

### Phase 7: Edge Cases and Error Handling (Task #185)

**ErrorHandling.feature:**
```gherkin
Feature: Error Handling
  As an API consumer
  I want to receive meaningful error messages
  So that I can handle failures appropriately

  Scenario: Create project with invalid ID format
    Given I have an invalid project ID "not-a-guid"
    When I attempt to retrieve the project
    Then I should receive "NOT_FOUND" error
    And the error message should indicate invalid format

  Scenario: Transition feature to invalid status
    Given a feature exists with status "Planning"
    When I attempt to transition to "Failed"
    Then I should receive validation error
    And the error should list valid transitions
```

## Dependencies

### NuGet Packages
- `SpecFlow.xUnit` v3.9.74
- `SpecFlow.Tools.MsBuild.Generation` v3.9.74
- `StrawberryShake.CodeGeneration` v12.22.7
- `StrawberryShake.CodeGeneration.CSharp` v12.22.7
- `StrawberryShake.Transport.Http` v15.1.14 (already present)

### External Dependencies
- API running on `http://localhost:8087`
- PostgreSQL database (via Testcontainers for cleanup)

## Testing Strategy

### Prerequisites
1. Start DevStack.Api on localhost:8087
   ```bash
   cd src/Server/DevStack.Api
   dotnet run --urls "http://localhost:8087"
   ```

2. Run SpecFlow tests:
   ```bash
   dotnet test src/Server/DevStack.Tests.Integration.GraphQL.Client
   ```

3. View SpecFlow test reports:
   ```bash
   dotnet specflow
   ```

### Test Organization
- One feature file per entity type
- Group by operation (Create, Read, Update, Delete, Transition)
- Separate feature for edge cases/errors
- Use SpecFlow test bindings for dependency injection

### Gherkin Best Practices
- Use business-readable language
- Focus on behavior, not implementation
- Use scenario outlines for data-driven tests
- Keep steps small and reusable
- Use tables for complex data

## Quality Gates

- [ ] All 211 existing tests covered by new SpecFlow BDD tests
- [ ] Code generation runs automatically on build
- [ ] No compiler warnings
- [ ] Tests pass when API is running on localhost:8087
- [ ] SpecFlow test reports generated
- [ ] Documentation complete

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| API not running | Tests fail | Clear documentation, skip tests if connection fails |
| Schema changes | Generated code breaks | Regenerate on schema update, CI check |
| Slow tests | Long feedback | Parallel test execution, efficient queries |
| Network issues | Flaky tests | Retry logic, proper timeouts |
| Steep learning curve | Team resistance | Provide examples, documentation, training |

## Task Breakdown Summary

| Task ID | Title | Est. Hours | Priority |
|---------|-------|------------|----------|
| 168 | Add SpecFlow and StrawberryShake packages and configure code generation | 2 | High |
| 169 | Create SpecFlow feature files for Projects | 1 | Medium |
| 170 | Create SpecFlow feature files for Features | 1 | Medium |
| 171 | Create SpecFlow feature files for Defects | 1 | Medium |
| 172 | Create SpecFlow feature files for Tasks | 1 | Medium |
| 173 | Create SpecFlow feature files for Epics | 1 | Medium |
| 174 | Create SpecFlow feature files for ModelConfigurations | 1 | Medium |
| 175 | Create SpecFlow feature files for WorkflowRuns | 1 | Medium |
| 176 | Create GraphQL client wrapper for SpecFlow step definitions | 2 | High |
| 177 | Implement SpecFlow step definitions for Projects | 2 | High |
| 178 | Implement SpecFlow step definitions for Features | 2 | High |
| 179 | Implement SpecFlow step definitions for Defects | 2 | High |
| 180 | Implement SpecFlow step definitions for Tasks | 2 | High |
| 181 | Implement SpecFlow step definitions for Epics | 2 | High |
| 182 | Implement SpecFlow step definitions for ModelConfigurations | 2 | High |
| 183 | Implement SpecFlow step definitions for WorkflowRuns | 2 | High |
| 184 | Create SpecFlow feature and step definitions for DashboardSummary | 1 | Medium |
| 185 | Create SpecFlow feature and step definitions for edge cases and errors | 2 | Medium |
| 186 | Add documentation and README for SpecFlow tests | 1 | Low |

**Total Estimated Hours: 30 hours**
**Breakdown into <20 min tasks: Each task is scoped to be completed in 10-20 minutes by an AI agent**

## Implementation Notes

### Code Generation
- Use `dotnet restore` to trigger initial code generation
- `.feature` files generate xUnit test classes automatically
- `.graphql` files generate StrawberryShake client code
- Generated code goes into `obj/Generated` directory

### BDD Approach
- Write feature files first (outside-in development)
- Implement step definitions to make tests pass
- Refactor steps for reusability
- Keep business language prominent

### Query Design
- Follow AdminUI patterns for consistency
- Include only necessary fields (avoid over-fetching)
- Use variables for all inputs
- Handle both success and error payloads

### Test Isolation
- Each scenario uses `TestContainerFixture` for database cleanup
- Database cleaned between scenarios
- GraphQL client creates fresh instances per scenario
- Use TestContext for sharing state between steps in same scenario
