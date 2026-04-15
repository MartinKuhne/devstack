# Open Questions

## Dead Letter Queue Implementation

1. **Optional Chaining Consistency**: Should we be using optional chaining more consistently in the worker.ts file when accessing job.data properties? Currently we have a mix of direct access and optional chaining with fallbacks.

2. **Error Handling Specificity**: Is the current error handling in the dead letter queue processor sufficient, or should we add more specific handling for different types of failures?

3. **Defect Creation Implementation**: The current implementation logs that it would create a defect but doesn't actually implement the defect creation. What is the expected timeline for implementing the actual defect creation functionality?

4. **Testing Coverage**: Do we have sufficient test coverage for edge cases in the dead letter queue functionality, particularly around malformed job data?

## General Observations

5. **Logging Consistency**: Are we consistent in our logging approach across different modules? Should we standardize on certain log fields or formats?

6. **Type Safety**: Are there additional TypeScript improvements we could make to increase type safety in the queues and worker implementations?

7. **Performance Impact**: What is the performance impact of the dead letter queue checking on each job failure? Is this negligible or should we consider optimizations?

---

## Epic Feature - Remaining Tasks

### Task 143: Implement Epic CRUD in Admin UI

**Status:** Blocked - Requires frontend GraphQL schema updates

**Issues:**
1. Frontend GraphQL schema (`src/AdminUi/src/graphql/schema.graphql`) is missing Epic types and mutations
2. Frontend schema is broken - missing `Task` type definition (lines 9, 10, 134-136 reference undefined type)
3. Backend has complete Epic API:
   - `EpicType.cs` - GraphQL type definition
   - `EpicQueryTests.cs` - Query tests (GetEpics, GetEpicById, filtering, pagination)
   - `EpicMutationTests.cs` - Mutation tests (CreateEpic, UpdateEpic, DeleteEpic)
   - `Epic.cs` - Domain entity with Title, Description, Features collection

**Required Steps:**
1. Fix frontend schema by adding missing `Task` type definition
2. Add Epic types to frontend schema:
   - `Epic` type with id, title, description, createdAt, updatedAt, features
   - `EpicConnection` for pagination
   - `CreateEpicInput`, `UpdateEpicInput`, `DeleteEpicInput`
   - Epic mutations: `createEpic`, `updateEpic`, `deleteEpic`
   - Epic queries: `epics`, `epicById`
3. Regenerate GraphQL types with `npm run codegen`
4. Create Epic pages:
   - `EpicListPage.tsx`
   - `EpicDetailPage.tsx`
   - `EpicCreateForm.tsx`
   - `EpicEditForm.tsx`
5. Add Epic navigation to AppShell
6. Update Feature pages to include Epic selection/assignment

**Complexity:** 8/10 (requires schema changes, codegen, multiple new pages)

### Task 146: Update existing Feature and Task tests for Epic hierarchy

**Status:** Blocked - Requires schema and API alignment

**Issues:**
1. `Feature` entity has `EpicId` field but GraphQL `Feature` type doesn't expose it
2. `AgentTask` entity does NOT have `EpicId` - correct design (Task -> Feature -> Epic)
3. GraphQL schema doesn't include `epicId` in `Feature` type or inputs
4. Frontend schema is broken and needs fixing first

**Required Steps:**
1. Add `epicId` field to GraphQL `Feature` type
2. Add `epicId` to `CreateFeatureInput` and `UpdateFeatureInput`
3. Update backend GraphQL `FeatureType` to resolve `epicId` and `epic` navigation
4. Regenerate frontend GraphQL types
5. Update existing Feature mutation tests to include EpicId scenarios
6. Add tests for Epic-Feature relationship
7. Verify all tests pass

**Complexity:** 6/10 (test updates + schema changes)

**Current State:**
- All existing tests pass (89 unit, 65 integration, 57 GraphQL integration)
- Epic entity and relationships are properly configured
- EpicId is nullable for backward compatibility

---

## Summary

Both tasks are blocked by the same root cause: **frontend GraphQL schema is incomplete and broken**. 

**Recommended Order:**
1. Fix missing `Task` type in frontend schema
2. Add Epic types and mutations to frontend schema
3. Add `epicId` to Feature GraphQL type
4. Regenerate GraphQL types
5. Complete Task #146 (test updates)
6. Complete Task #143 (Admin UI implementation)

**Dependencies:**
- Backend Epic API: ✅ Complete
- Frontend schema: ❌ Missing Task type, missing Epic types
- Frontend UI: ❌ Not implemented
- Tests: ❌ Not updated

---

## Admin UI E2E Playwright Tests

### Question 1: Generic Delete Entity Mutation for Test Cleanup
**Status:** Blocked  
**Related Tasks:** 
- Task #129: Create end-to-end workflow tests
- Task #131: Add test data cleanup mechanism
- Task #134: Execute Playwright E2E tests and fix issues

**Issue:** The `TestDataManager.ts` fixture references a `deleteEntity` mutation that doesn't exist in the GraphQL API.

**Current State:**
- Individual delete mutations exist for each entity type (`deleteProject`, `deleteFeature`, `deleteDefect`, `deleteTask`, `deleteModelConfiguration`, `deleteEpic`)
- Test cleanup code expects a generic `deleteEntity(id: ID!, type: EntityType!)` mutation
- No `EntityType` enum or generic delete mutation exists in `Mutation.cs`

**Options:**
1. **Add generic delete mutation** - Create a unified `deleteEntity` mutation with an `EntityType` enum that routes to the appropriate handler
2. **Update test fixtures** - Modify `TestDataManager.ts` to use individual delete mutations for each entity type
3. **Database cleanup strategy** - Use direct database deletion in tests instead of GraphQL mutations

**Recommendation:** Option 2 (update test fixtures) is the fastest path forward. Option 1 provides a cleaner API but requires more implementation work.

### Question 2: API Availability for E2E Tests
**Status:** Unclear  
**Related Tasks:** All E2E test tasks

**Issue:** E2E tests require a running API instance with PostgreSQL. The test configuration and CI pipeline integration need clarification.

**Open Items:**
- Should tests run against `docker compose up` local environment?
- Should tests spin up containers as part of the test run?
- What is the CI pipeline strategy for E2E tests?

### Question 3: Defect Status Transitions
**Status:** Unclear  
**Related Files:** `defect.spec.ts`

**Issue:** Defect tests reference status transitions but the API uses `FeatureStatus` for both features and defects. Need to verify if defects have their own status lifecycle or share the feature lifecycle.

**Open Items:**
- Should defects have independent status values?
- Are the same transition rules applied to defects as features?