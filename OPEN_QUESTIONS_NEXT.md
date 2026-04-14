# Open Questions for Frontend Tasks

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
