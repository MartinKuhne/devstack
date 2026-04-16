# StrawberryShake GraphQL Client Integration Plan

## Overview
Implement StrawberryShake-generated GraphQL client for integration tests in `DevStack.Tests.Integration.GraphQL.Client` to test all GraphQL queries and mutations against a deployed API instance at `http://localhost:8087`.

## Current State Analysis

### Existing Test Infrastructure
- **Testcontainers**: PostgreSQL container with shared instance (`TestContainerFixture`)
- **Current approach**: Direct database access via `DevStackDbContext`
- **Target**: GraphQL client-based testing against running API

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
<PackageReference Include="StrawberryShake.CodeGeneration" Version="12.22.7" />
<PackageReference Include="StrawberryShake.CodeGeneration.CSharp" Version="12.22.7" />
<PackageReference Include="StrawberryShake.Transport.Http" Version="15.1.14" />
```

**Configuration Files:**
1. `.graphqlrc.json` - Configuration for code generation
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

2. Update `.csproj` to enable code generation:
   ```xml
   <ItemGroup>
     <GraphQL Include="GraphQL/**/*.graphql" />
   </ItemGroup>
   ```

### Phase 2: GraphQL Query/Mutation Files (Tasks #169-175)

**Directory Structure:**
```
src/Server/DevStack.Tests.Integration.GraphQL.Client/
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

### Phase 3: Client Wrapper (Task #176)

**DevStackGraphQLClient Class:**
```csharp
public class DevStackGraphQLClient : IDisposable
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _graphQLEndpoint;
    
    public DevStackGraphQLClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _graphQLEndpoint = "http://localhost:8087/graphql";
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

### Phase 4: Test Refactoring (Tasks #177-183)

**Migration Strategy:**
1. Keep `TestContainerFixture` for database cleanup between tests
2. Replace direct DB access with GraphQL client calls
3. Verify data through GraphQL responses
4. Use FluentAssertions for assertions

**Example Test:**
```csharp
[Fact]
public async Task CreateProject_ShouldReturnProjectId()
{
    // Arrange
    var client = new DevStackGraphQLClient(_httpClientFactory);
    var input = new CreateProjectInput("Test Project", "Description", null, null, null);
    
    // Act
    var result = await client.CreateProjectAsync(input);
    
    // Assert
    result.Should().NotBe(Guid.Empty);
}
```

### Phase 5: Edge Cases and Error Handling (Task #185)

**Test Scenarios:**
- Invalid UUID format
- Non-existent entity IDs
- Validation errors (name too long, missing required fields)
- Invalid status transitions (e.g., Failed → Planning)
- Concurrent modification conflicts
- Null/empty input handling

## Dependencies

### NuGet Packages
- `StrawberryShake.CodeGeneration` v12.22.7
- `StrawberryShake.CodeGeneration.CSharp` v12.22.7
- `StrawberryShake.Transport.Http` v15.1.14 (already present)

### External Dependencies
- API running on `http://localhost:8087`
- PostgreSQL database (via Testcontainers)

## Testing Strategy

### Prerequisites
1. Start DevStack.Api on localhost:8087
   ```bash
   cd src/Server/DevStack.Api
   dotnet run --urls "http://localhost:8087"
   ```

2. Run tests:
   ```bash
   dotnet test src/Server/DevStack.Tests.Integration.GraphQL.Client
   ```

### Test Organization
- One test class per entity type
- Group by operation (Create, Read, Update, Delete, Transition)
- Separate class for edge cases/errors
- Use xUnit test collections for isolation

## Quality Gates

- [ ] All 211 existing tests covered by new GraphQL-based tests
- [ ] Code generation runs automatically on build
- [ ] No compiler warnings
- [ ] Tests pass when API is running on localhost:8087
- [ ] Documentation complete

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| API not running | Tests fail | Clear documentation, skip tests if connection fails |
| Schema changes | Generated code breaks | Regenerate on schema update, CI check |
| Slow tests | Long feedback | Parallel test execution, efficient queries |
| Network issues | Flaky tests | Retry logic, proper timeouts |

## Task Breakdown Summary

| Task ID | Title | Est. Hours | Priority |
|---------|-------|------------|----------|
| 168 | Add StrawberryShake packages and configure code generation | 2 | High |
| 169 | Create GraphQL query files for Projects | 1 | Medium |
| 170 | Create GraphQL query files for Features | 1 | Medium |
| 171 | Create GraphQL query files for Defects | 1 | Medium |
| 172 | Create GraphQL query files for Tasks | 1 | Medium |
| 173 | Create GraphQL query files for Epics | 1 | Medium |
| 174 | Create GraphQL query files for ModelConfigurations | 1 | Medium |
| 175 | Create GraphQL query files for WorkflowRuns | 1 | Medium |
| 176 | Create GraphQL client wrapper service for tests | 2 | High |
| 177 | Refactor Project tests to use StrawberryShake client | 2 | High |
| 178 | Refactor Feature tests to use StrawberryShake client | 2 | High |
| 179 | Refactor Defect tests to use StrawberryShake client | 2 | High |
| 180 | Refactor Task tests to use StrawberryShake client | 2 | High |
| 181 | Refactor Epic tests to use StrawberryShake client | 2 | High |
| 182 | Refactor ModelConfiguration tests to use StrawberryShake client | 2 | High |
| 183 | Refactor WorkflowRun tests to use StrawberryShake client | 2 | High |
| 184 | Create DashboardSummary query tests | 1 | Medium |
| 185 | Create edge case and error handling tests | 2 | Medium |
| 186 | Add documentation and README for running tests | 1 | Low |

**Total Estimated Hours: 30 hours**
**Breakdown into <20 min tasks: Each task is scoped to be completed in 10-20 minutes by an AI agent**

## Implementation Notes

### Code Generation
- Use `dotnet restore` to trigger initial code generation
- `.graphql` files automatically regenerate on build
- Generated code goes into `obj/Generated` directory

### Query Design
- Follow AdminUI patterns for consistency
- Include only necessary fields (avoid over-fetching)
- Use variables for all inputs
- Handle both success and error payloads

### Test Isolation
- Each test class uses `IClassFixture<TestContainerFixture>`
- Database cleaned between test collections
- GraphQL client creates fresh instances per test
