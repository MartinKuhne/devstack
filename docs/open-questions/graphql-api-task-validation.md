# Open Questions - GraphQL API Implementation

## Task 12: Implement task GraphQL mutations and task status transitions

### Open Question: TaskStatusTransitionService exists but is not used

**Issue:** `TaskStatusTransitionService` exists in `DevStack.Domain.Services` but `TransitionTaskStatusHandler` does not use it for validation. Instead, it allows any status transition without validation.

**Current behavior:** The handler in `src/Server/DevStack.Infrastructure/Tasks/TaskHandlers.cs:131-168` directly sets the status without calling any validation service.

**Expected behavior per spec:** Route status transitions through `TaskStatusTransitionService` for validation.

**Options:**
1. Wire `TaskStatusTransitionService` into `TransitionTaskStatusHandler` constructor
2. Create a proper task transition service integration similar to `FeatureStatusTransitionService`
3. Accept current implementation if task status transitions are intentionally unvalidated

**Impact:** Invalid task transitions (e.g., from Planning directly to Done) are not prevented at the API layer.
