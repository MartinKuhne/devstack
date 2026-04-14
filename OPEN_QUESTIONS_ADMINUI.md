# Open Questions

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

## General

### Docker Compose Integration

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

## Next Steps

1. Install and configure Playwright for E2E smoke tests
2. Complete shadcn/ui component library setup
3. Add more feature-specific components (project forms, feature lists, etc.)
4. Implement GraphQL queries and mutations
5. Set up Apollo Client caching and data management
6. Add unit tests for components
7. Configure docker-compose.yml for Admin UI
8. Add environment configuration for API endpoints
