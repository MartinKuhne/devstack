# Test Data Cleanup Implementation Plan

## Summary
Implement automated database cleanup for test data by marking all test records with `[TestData]` prefix and creating a cleanup routine that removes these records before/after test execution.

## Current State Analysis

### Test Entry Points
1. **Integration Tests** (`DevStack.Tests.Integration`):
   - `ProjectMutationTests.cs` - Uses in-memory SQLite, creates Project, Item entities
   - `FeatureMutationTests.cs` - Creates Item entities with Title fields
   - `TaskMutationTests.cs` - Creates AgentTask entities with Title fields
   - `EpicMutationTests.cs` - Creates Item entities with Title fields
   - `EpicQueryTests.cs` - Creates Item entities with Title fields
   - `QueryTests.cs` - Seeds Project, Item, AgentTask, ModelConfiguration, AuditEvent
   - `SchemaSnapshotTests.cs` - Seeds Project, Item, AgentTask

2. **SpecFlow Integration Tests** (`DevStack.Tests.Integration.GraphQL.Client`):
   - `SpecFlowHooks.cs` - BeforeScenario/AfterScenario hooks (no cleanup currently)
   - `CommonSteps.cs` - Creates Project, Feature, Task, Defect via GraphQL client

3. **Unit Tests** (`DevStack.Tests.Unit`):
   - `TaskStatusTransitionServiceTests.cs` - Creates AgentTask in memory (no persistence)
   - `ItemStatusTransitionServiceTests.cs` - Creates Item in memory (no persistence)
   - Other unit tests use in-memory objects only

### Entities with Title/Name Fields
- **Project**: `Name` field (max 200 chars)
- **Item** (Feature, Defect, Epic): `Title` field (max 300 chars)
- **Epic**: `Title` field (max 200 chars) - actually uses Item with Subtype=Epic
- **AgentTask**: `Title` field (max 300 chars)

### Test Bootstrap/Teardown Patterns
1. **xUnit Integration Tests**:
   - `IAsyncLifetime.InitializeAsync()`: Creates in-memory SQLite database
   - `IAsyncLifetime.DisposeAsync()`: Closes connection, disposes DbContext
   - Each test gets a fresh in-memory database (no cleanup needed between tests)

2. **SpecFlow Tests**:
   - `[BeforeScenario]`: Initializes GraphQL client
   - `[AfterScenario]`: Disposes service provider
   - No database cleanup currently

## Implementation Strategy

### Phase 1: Test Data Convention Enforcement
**Goal**: Ensure all persisted test data is marked with `[TestData]` prefix

1. **Update xUnit Integration Tests**:
   - Modify all `SeedDataAsync()` methods to prefix titles with `[TestData]`
   - Update direct entity creation in test methods
   - Affected files:
     - `ProjectMutationTests.cs` (Name field)
     - `FeatureMutationTests.cs` (Title field)
     - `TaskMutationTests.cs` (Title field)
     - `EpicMutationTests.cs` (Title field)
     - `EpicQueryTests.cs` (Title field)
     - `QueryTests.cs` (Name and Title fields)
     - `SchemaSnapshotTests.cs` (Name and Title fields)

2. **Update SpecFlow Steps**:
   - Modify `CommonSteps.cs` to prefix all test data creation with `[TestData]`
   - Affected methods:
     - `GivenAParentProjectExists()`
     - `GivenAParentFeatureExists()`
     - `GivenAProjectExists()`
     - `GivenAFeatureExists()`
     - `GivenATaskExists()`
     - `GivenADefectExists()`
     - All other "Given" methods that create entities

### Phase 2: Database Cleanup Service
**Goal**: Create reusable cleanup routine for test data

1. **Create `TestDataCleanupService`** in `DevStack.Infrastructure`:
   ```csharp
   public class TestDataCleanupService
   {
       public Task CleanupAsync(DevStackDbContext context, CancellationToken ct = default);
       private Task CleanupProjectsAsync(...);
       private Task CleanupItemsAsync(...);
       private Task CleanupTasksAsync(...);
       private Task CleanupEpicsAsync(...);
       private Task CleanupModelConfigurationsAsync(...);
       private Task CleanupWorkflowRunsAsync(...);
       private Task CleanupAuditEventsAsync(...);
   }
   ```

2. **Cleanup Logic**:
   - Delete records where `Name` or `Title` contains `[TestData]`
   - Respect referential integrity (delete in correct order):
     1. AuditEvents (references all entities)
     2. Tasks (references Items/Features)
     3. Items (Features, Defects, Epics) - cascade delete handles dependencies
     4. Projects (parent entity)
     5. ModelConfigurations
     6. WorkflowRuns

3. **Add extension method**:
   ```csharp
   public static class DevStackDbContextExtensions
   {
       public static Task CleanupTestDataAsync(this DevStackDbContext context, CancellationToken ct = default);
   }
   ```

### Phase 3: Wire Cleanup into Test Lifecycle

1. **For xUnit Integration Tests**:
   - Option A: Create base test class with cleanup in `InitializeAsync`
   - Option B: Use collection fixture with shared cleanup
   - Recommended: Create `IntegrationTestBase` class that all tests inherit from

2. **For SpecFlow Tests**:
   - Update `SpecFlowHooks.cs` to call cleanup in `[BeforeScenario]` and `[AfterScenario]`
   - Requires DI container setup for cleanup service

### Phase 4: Verification Tests

1. **Unit Test for Cleanup Service**:
   - Test that records with `[TestData]` are deleted
   - Test that records without marker are preserved
   - Test referential integrity (child records deleted before parents)

2. **Integration Test for Cleanup**:
   - Create test data with `[TestData]` marker
   - Run cleanup
   - Verify no tagged data remains

### Dependencies
- No new NuGet packages required
- Cleanup service goes in `DevStack.Infrastructure` project
- Test updates only affect test projects

### Risks & Considerations
1. **Breaking existing tests**: Ensure all tests are updated before merging
2. **Performance**: Cleanup should be fast (in-memory SQLite is already fast)
3. **Referential integrity**: Must delete in correct order to avoid FK violations
4. **SpecFlow integration**: May need to configure DI for cleanup service

### Success Criteria
- All test data creation includes `[TestData]` marker
- Cleanup runs automatically before and after tests
- No manual database cleanup required between test runs
- All existing tests pass with new cleanup logic
- Build and test quality gates pass

## Implementation Order
1. Create `TestDataCleanupService` and extension methods
2. Update xUnit integration tests to use `[TestData]` prefix
3. Update SpecFlow step definitions to use `[TestData]` prefix
4. Wire cleanup into xUnit test base class
5. Wire cleanup into SpecFlow hooks
6. Add verification tests for cleanup service
7. Run full build/test pipeline
