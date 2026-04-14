# Open Questions for Pending Tasks

## GitHub Token Encryption and UI

**Task:** Add project GitHub configuration fields to the settings page (Task #41)

**Question:** Should the GitHub token be exposed through the GraphQL API?

**Context:**
- The backend `Project` entity has a `GithubToken_Encrypted` field that is encrypted at rest
- Currently, the GraphQL schema does not expose this field for security reasons
- The task requires showing a masked token input that can be set/cleared

**Options:**
1. Add a separate mutation specifically for updating the GitHub token (recommended)
2. Add the field to the Project type but mark it as sensitive
3. Handle token updates through a dedicated API endpoint outside GraphQL

**Recommendation:** Option 1 - Create a dedicated mutation like `updateProjectGitHubToken` that:
- Only accepts the encrypted token value
- Does not return the token value in the response
- Requires additional authentication/authorization checks
- Logs access attempts for audit purposes

**Next Steps:**
- Decide on the approach
- Update backend mutation handlers
- Update GraphQL schema
- Regenerate frontend types
- Implement the UI component

---

## Filesystem Skill Dependencies

**Task:** Implement the filesystem skill with workspace containment checks (Task #86)

**Question:** What is the dependency resolution for the worker skills?

**Context:**
- Task 86 is blocked by Task 80 (graceful worker shutdown)
- Multiple skills are blocked in a chain: filesystem → coder workflow → architect workflow

**Next Steps:**
- Complete Task 80 (graceful shutdown) first
- Then proceed with filesystem skill implementation
- Ensure proper workspace path validation and containment checks

---

## Agent Process Skills

**Tasks:** Git skill (83), Pull-request skill (84), Feature/Task/Defect skills (85)

**Question:** What authentication mechanism should be used for GitHub/Gitea operations?

**Context:**
- Skills need to perform git operations and PR management
- Multiple providers (GitHub, Gitea) need to be supported
- Token management and rotation strategy unclear

**Next Steps:**
- Define authentication strategy
- Implement provider abstraction
- Add credential management

---

## GraphQL API Implementation

### Task 12: Implement task GraphQL mutations and task status transitions

#### Open Question: TaskStatusTransitionService exists but is not used

**Issue:** `TaskStatusTransitionService` exists in `DevStack.Domain.Services` but `TransitionTaskStatusHandler` does not use it for validation. Instead, it allows any status transition without validation.

**Current behavior:** The handler in `src/Server/DevStack.Infrastructure/Tasks/TaskHandlers.cs:131-168` directly sets the status without calling any validation service.

**Expected behavior per spec:** Route status transitions through `TaskStatusTransitionService` for validation.

**Options:**
1. Wire `TaskStatusTransitionService` into `TransitionTaskStatusHandler` constructor
2. Create a proper task transition service integration similar to `FeatureStatusTransitionService`
3. Accept current implementation if task status transitions are intentionally unvalidated

**Impact:** Invalid task transitions (e.g., from Planning directly to Done) are not prevented at the API layer.
