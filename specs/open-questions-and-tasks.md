# Open Questions and Remaining Tasks

## Phase 2 - GraphQL API

### Step 2.7 — Defect Status Transition
- **Issue:** The `FeatureStatusTransitionService.Transition()` method expects a `Feature` parameter, but Defect is a separate type (though both inherit from WorkItem).
- **Question:** Should we:
  1. Modify `FeatureStatusTransitionService` to accept `WorkItem` base class?
  2. Create a separate `DefectStatusTransitionService`?
  3. Create a generic `WorkItemStatusTransitionService<T>`?
- **Recommendation:** Option 1 - modify to accept `WorkItem` since the logic is identical for both types.

### Step 2.8 — Task Status Transitions
- **Issue:** Similar to Defect, Task has its own `TaskStatus` enum which is different from `FeatureStatus`.
- **Question:** Should we create a separate `TaskStatusTransitionService`?
- **Recommendation:** Yes - create separate service for Task since the status transitions are different.

### Step 2.9 — ModelConfiguration Mutations
- **Issue:** Need to implement encryption/decryption for API keys using `ISecretService`.
- **Status:** Handler infrastructure ready, encryption integration needed.

### Step 2.10 — WorkflowRun Mutations
- **Issue:** Need to implement create/update/cancel operations.
- **Status:** Basic handler structure ready.

### Step 2.11 — DataLoader Implementation
- **Issue:** Need to add DataLoader for relationship loading to prevent N+1 queries.
- **Status:** Not yet implemented.

### Step 2.12 — OpenTelemetry Integration
- **Issue:** Need to add OpenTelemetry tracing for GraphQL operations.
- **Status:** Not yet implemented.

### Step 2.13 — Integration Tests
- **Issue:** Need integration tests for GraphQL mutations using Testcontainers.
- **Status:** Not yet implemented.

### Step 2.5-2.6 — Project and Feature Payloads
- **Issue:** The mutation methods return payloads with just the Id set. Need to fetch the full entities after creation.
- **Question:** Should mutations return the full entity or just the Id?
- **Recommendation:** Return full entity by fetching it after creation.

## Phase 3 — Hardening and Operate

### Step 3.1 — Structured Logging with Serilog
- **Issue:** Need to replace ILogger with Serilog for structured JSON logging.
- **Status:** Not yet implemented.

### Step 3.2 — Problem Details and Error Mapping
- **Issue:** Need to map domain exceptions to ProblemDetails and GraphQL error extensions.
- **Status:** Not yet implemented.

### Step 3.3-3.4 — Performance Optimizations
- **Issue:** Need to add response caching and database indexes.
- **Status:** Not yet implemented.

### Step 3.5 — API Documentation
- **Issue:** Need to add schema descriptions and generate SDL file.
- **Status:** Not yet implemented.
