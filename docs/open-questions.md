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

---

## General Open Questions

### Task 28 - Frontend Tooling (partial)
- [ ] Add lint-staged and Husky pre-commit automation for staged TS/TSX files
- [ ] Verify docker compose works end-to-end with the new admin-ui service

### Task 30 - Dashboard Hook (blocked by)
- Codegen automatically generates types - verify GitHub Actions runs codegen

### Verify build passes in CI/CD pipeline
- [ ] Verify build passes in CI/CD pipeline

---

## Phase 1

### Step 1.7 — Implement EF Core infrastructure

**Question:** Should `Severity` on Defect be a required enum field added to the entity, or a tag-style value object?

**Recommendation:** Adding it as a nullable enum field to Defect in phase 1.

**Status:** Already implemented as nullable enum field.

---

**Question:** Should `Project.Memory` be lazily loaded from a separate blob storage in the future, or kept as text?

**Recommendation:** Text in phase 1.

**Status:** Already implemented as text field.

---

## Phase 2

### Step 2.12 — Add OpenTelemetry to API

**Question:** Should long-form fields accept raw markdown strings or structured JSON arrays for acceptance criteria?

**Recommendation:** Markdown text in phase 1, structured arrays in phase 2 if multi-item editing is needed.

**Status:** Open - not yet implemented.

---

**Question:** Should audit events be queryable through GraphQL or only stored for diagnostics?

**Recommendation:** Queryable via GraphQL with pagination.

**Status:** Open - not yet implemented.

---

## EF Core Version Conflicts

**Issue:** The solution had version conflicts between EF Core 8.0.10 (used in Infrastructure) and 10.0.5 (used in Api). This was causing build warnings.

**Recommendation:** Align all projects to use EF Core 8.0.10 for compatibility with Npgsql.EntityFrameworkCore.PostgreSQL 8.0.10.

**Status:** Resolved - updated DevStack.Api.csproj to use EF Core 8.0.10.

---

## Frontend Architecture

### Question: What type of frontend should be built for the Admin UI?

**Context:** The repository currently contains only a .NET backend API. There is no frontend code (no React/TypeScript files, no package.json, no admin-ui directory).

**Options:**
1. Create a new frontend project in this repository (e.g., `src/AdminUi/` or `src/frontend/`)
2. Create the frontend as a separate repository
3. Use a different frontend framework (Blazor, etc.) within the existing .NET project
4. Mixed approach (React for complex UI, Blazor for admin sections)

**Current state:** A new React frontend project has been initialized in `src/AdminUi/`.

**Recommendation:** React + TypeScript SPA consumed by the existing GraphQL API.

**Status:** Partially resolved - foundation is in place. Need to complete implementation.

---

## Phase 0 - Admin UI Foundation

### Step 0.6 — Add shadcn/ui base components

**Question:** Should shadcn/ui components be installed via the CLI (interactive) or manually configured?

**Context:** shadcn/ui initialization requires interactive input to select component library (Radix vs Base). This is problematic for automated workflows.

**Options:**
1. Use shadcn CLI with interactive mode
2. Manually configure all shadcn components and dependencies
3. Use a different component library (e.g., Mantine, Chakra UI)

**Current state:** Manual configuration is being used with Radix UI components.

**Recommendation:** Continue with manual Radix UI setup for better automation and control.

**Status:** In progress.

---

## Phase 7 - Quality Gates

### Step 7.4 — Add Playwright E2E smoke tests

**Question:** How should Playwright be integrated for smoke testing?

**Context:** Playwright requires browser automation setup and test infrastructure.

**Options:**
1. Use Playwright with Jest for test runner
2. Use Playwright's native test runner
3. Use Cypress instead of Playwright

**Current state:** Playwright not yet installed.

**Recommendation:** Use Playwright's native test runner for better TypeScript support and parallelization.

**Status:** Blocked - need to install and configure Playwright.

---

## Docker Compose Integration

**Question:** How should the Admin UI be integrated into the docker-compose.yml?

**Context:** The Admin UI needs to be served alongside the API and PostgreSQL.

**Options:**
1. Build Admin UI and serve via nginx
2. Serve Admin UI via the .NET API (static files)
3. Run Admin UI in a separate container with a lightweight server

**Current state:** docker-compose.yml needs to be updated.

**Recommendation:** Serve Admin UI via the .NET API as static files for simplicity.

**Status:** Open - needs implementation.

---

## Phase 3

### Step 3.4 — Status transition component

**Question:** This repository contains only a .NET backend API. There is no frontend code (no React/TypeScript files, no package.json, no admin-ui directory). The task requires creating React components like `StatusTransitionPanel.tsx` and `EditFeatureDialog.tsx`.

**Options:**
1. Create a new frontend project in this repository (e.g., `src/frontend/` or `admin-ui/`)
2. Create the frontend as a separate repository
3. Use a different frontend framework (Blazor, etc.) within the existing .NET project

**Recommendation:** Create a new React frontend project in `src/frontend/` or `admin-ui/` as a separate project that consumes the GraphQL API.

**Status:** Blocked until frontend architecture decision is made.

---

## Priority 1 - Frontend Architecture Decision

**Task IDs:** 68, 22-28, 36-67, 69-71, 73, 77, 82, 88, 94, 105

**Question:** The repository currently contains only a .NET backend API with no frontend code (no React/TypeScript files, no package.json, no admin-ui directory).

**Options:**
1. Create a new React frontend project in this repository (e.g., `src/AdminUi/` or `src/frontend/`)
2. Create the frontend as a separate repository
3. Use a different frontend framework within the existing .NET project (e.g., Blazor WebAssembly or Blazor Server)

**Current state:** No frontend code exists. All projects are .NET backend-only.

**Recommendation:** Create a new React + TypeScript frontend project in `src/AdminUi/` as a separate project that consumes the existing GraphQL API.

**Status:** BLOCKED - Requires user decision on frontend architecture before any frontend tasks can be started.

Decision: Create a frontend project under src/AdminUi/

---

## Priority 2 - Missing Backend Features for Frontend Tasks

**Tasks:** 11, 13, 14, 22-28, 36-67

**Issues:**
- Defect mutations need integration tests
- Model configuration mutations (create/update/delete) need to be added to Mutation.cs
- Workflow run mutations (create/update/cancel) need to be added to Mutation.cs

**Status:** These are backend tasks that are actually DOABLE and can be completed.

Decision: Implement all queries and mutations needed

---

## Priority 3 - Frontend-Only Tasks

**Task IDs:** 65, 66 (Responsive design and accessibility pass)

**Issues:** These tasks require a frontend to be present first. Once the frontend is bootstrapped, these can be completed.

**Status:** BLOCKED - Wait for frontend bootstrap (Task 22).

---

## Summary

### Tasks That Can Be Completed NOW (Backend Only):
- **Task 11:** Implement defect GraphQL mutations with parent-feature linking (handlers exist, need integration tests)
- **Task 13:** Implement model configuration GraphQL mutations with encrypted API key handling (handlers exist, need GraphQL mutations)
- **Task 14:** Implement workflow run GraphQL mutations for worker coordination (handlers exist, need GraphQL mutations)
- **Task 19:** Map API and GraphQL errors to structured problem details

### Tasks That Are BLOCKED (Frontend Required):
- **Task 68:** Run final frontend quality gates and add Playwright smoke coverage
- **Tasks 22-28:** Admin UI Bootstrap foundation (Phases 0)
- **Tasks 36-67:** Admin UI Phases 1-7
- **Tasks 65-66:** Responsive design and accessibility pass
- **Tasks 69-94, 105:** Agent Process worker tasks

### Recommendation:
1. Complete the backend tasks (11, 13, 14, 19) that are currently unblocked
2. Once frontend architecture decision is made, bootstrap the Admin UI project
3. Then proceed with frontend tasks

---

## Next Steps

1. Install and configure Playwright for E2E smoke tests
2. Complete shadcn/ui component library setup
3. Add more feature-specific components (project forms, feature lists, etc.)
4. Implement GraphQL queries and mutations
5. Set up Apollo Client caching and data management
6. Add unit tests for components
7. Configure docker-compose.yml for Admin UI
8. Add environment configuration for API endpoints
