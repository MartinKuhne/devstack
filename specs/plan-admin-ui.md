# DevStack Implementation Plan — Admin UI

Owner: Frontend / UI team
Stack: React 19, TypeScript, Vite, Tailwind CSS, shadcn/ui, Apollo Client (GraphQL)
Component: `src/AdminUi/`

---

## Design decisions

### Framework and tooling
- React 19 with functional components and hooks; no class components.
- Vite as the build tool (faster HMR, modern ESM output).
- TypeScript strict mode enabled.
- Tailwind CSS for utility styling; shadcn/ui for base components (Dialog, DropdownMenu, DataTable, Form, Toast, Badge, Card, Tabs, Skeleton, etc.).
- Apollo Client for GraphQL operations (caching, optimistic updates, error handling).
- React Router v6 for client-side routing.
- React Hook Form + Zod for form validation.
- `@tanstack/react-query` is an acceptable alternative to Apollo if the team prefers its caching model; document the choice in ADR-007.

### State management
- Apollo InMemoryCache as the primary data store for server state.
- React Context only for UI state that does not belong in Apollo (e.g., sidebar collapse, toast queue).
- No Redux or Zustand unless a specific need emerges.

### Component architecture
- Feature-based folder structure: `src/features/{entity}/components/`, `src/features/{entity}/hooks/`, `src/features/{entity}/graphql/`.
- Shared base components in `src/components/ui/` (shadcn/ui wrappers).
- Layout components in `src/layouts/` (AppShell, Sidebar, TopBar).
- Each GraphQL query/mutation lives in a `*.graphql` file co-located with its feature, consumed via `graphql-request` codegen to generate TypeScript types.

### Styling
- Tailwind `tailwind.config.ts` extending shadcn/ui design tokens.
- CSS variables for theming (light/dark mode via `class` strategy on the root element).
- No inline styles except dynamic values derived from props/state.
- Allman brace convention on TSX: opening/closing brace on their own line.

### GraphQL integration
- `graphql-codegen` configured to read `*.graphql` files and generate `gql` tagged templates + TypeScript input/output types.
- Apollo Link for error reporting to a generic error boundary.
- Optimistic updates for status changes using Apollo `update` callbacks after mutations.
- Polling via Apollo `pollInterval` for dashboard data in phase 1 (upgrade to subscriptions later).

### Error handling and loading
- Global error boundary wrapping routes with a fallback UI.
- Per-page skeleton loaders matching the shape of loaded content.
- Empty states with descriptive messages and CTAs.
- Toast notifications for mutation success/failure.
- GraphQL errors surfaced via toast with the error message from `extensions.code`.

---

## Phase 0 — Bootstrap the Admin UI

**Goal:** A buildable, runnable React app with routing, layout, GraphQL client, and design system set up.

### Step 0.1 — Create React app and configure tooling
- [ ] Initialize Vite React TypeScript project in `src/AdminUi/` via `npm create vite@latest . -- --template react-ts`.
- [ ] Install `tailwindcss`, `@tailwindcss/vite` (or postcss plugin depending on version), `tailwindcss-animate`.
- [ ] Initialize Tailwind with `npx tailwindcss init` and configure `content` paths for all `.tsx`, `.ts`, `.html` files.
- [ ] Install `typescript`, `vite`, and dev dependencies; configure `tsconfig.json` with strict mode, `jsxImportSource: "react"`, and path aliases (`@/` → `src/`).
- [ ] Configure `vite.config.ts` with path aliases and Tailwind plugin.
- [ ] Verify `npm run build` produces a production bundle with zero errors.

### Step 0.2 — Install and configure shadcn/ui
- [ ] Follow shadcn/ui CLI installation guide: `npx shadcn@latest init`.
- [ ] Configure `components.json` with the chosen Tailwind config path, CSS variable prefix (`--default`), and style preference.
- [ ] Add base components used throughout the app: `Button`, `Card`, `Badge`, `Dialog`, `DropdownMenu`, `Input`, `Label`, `Select`, `Separator`, `Sheet`, `Skeleton`, `Tabs`, `Table`, `Toast`, `Tooltip`, `Popover`, `Command`.
- [ ] Verify shadcn components render correctly with both light and dark class strategy.
- [ ] Add `clsx` and `tailwind-merge` utilities as project dependencies (shadcn defaults).

### Step 0.3 — Set up routing
- [ ] Install `react-router-dom` v6.
- [ ] Create `src/layouts/AppShell.tsx` with:
  - Fixed sidebar navigation.
  - Top bar with app name and dark mode toggle.
  - Main content area with `<Outlet />`.
- [ ] Create route tree in `src/routes.tsx` with the following routes:
  - `/` → redirect to `/dashboard`.
  - `/dashboard` → `DashboardPage`.
  - `/projects` → `ProjectListPage`.
  - `/projects/:id` → `ProjectDetailPage`.
  - `/projects/:id/features` → `FeatureListPage`.
  - `/projects/:id/features/:featureId` → `FeatureDetailPage`.
  - `/projects/:id/defects` → `DefectListPage`.
  - `/projects/:id/defects/:defectId` → `DefectDetailPage`.
  - `/projects/:id/tasks` → `TaskListPage`.
  - `/projects/:id/settings` → `ProjectSettingsPage`.
  - `/settings` → `GlobalSettingsPage`.
- [ ] Wrap the app in `<BrowserRouter>` and `<QueryClientProvider>` (Apollo Client `Provider`) in `main.tsx`.

### Step 0.4 — Configure Apollo Client
- [ ] Install `@apollo/client`, `graphql`.
- [ ] Create `src/apollo-client.ts` configuring `ApolloClient` with:
  - `HttpLink` pointing to `{API_BASE_URL}/graphql` (environment variable `VITE_API_URL`).
  - `InMemoryCache` with `typePolicies` for `Query` fields to enable cursor pagination.
  - `ApolloLink` error link that logs GraphQL errors to the console and optionally reports to an error service.
- [ ] Create `src/providers/ApolloProvider.tsx` wrapping the app with `<ApolloProvider client={client}>`.
- [ ] Create `src/providers/ToastProvider.tsx` using a toast library (e.g., `sonner` or `react-hot-toast`) as a global toast context.

### Step 0.5 — Add code generation for GraphQL
- [ ] Install `graphql-codegen` dev dependencies: `@graphql-codegen/cli`, `@graphql-codegen/typescript`, `@graphql-codegen/typescript-operations`, `@graphql-codegen/typescript-react-apollo`.
- [ ] Create `codegen.ts` config pointing to the running GraphQL API (or `schema.graphql` file once generated) and output to `src/generated/graphql.ts`.
- [ ] Create `src/graphql/queries/` folder with empty `.graphql` files (placeholder for now; to be filled in later phases).
- [ ] Run `graphql-codegen` and verify `src/generated/graphql.ts` is created with `CodegenDocument`, `GetDashboardSummaryQuery`, and any other types.
- [ ] Add `npm run codegen` script to `package.json`; run it whenever schema changes.
- [ ] Add a CI step that fails if `npm run codegen` produces diffs (schema drift check).

### Step 0.6 — Add styling and theming foundation
- [ ] Configure Tailwind dark mode via `class` strategy; add `darkMode: ["class"]` to `tailwind.config.ts`.
- [ ] Set up CSS variables in `src/index.css` for `--background`, `--foreground`, `--muted`, etc., matching shadcn/ui defaults.
- [ ] Add a dark mode toggle button in the top bar that toggles the `dark` class on `<html>` and persists preference to `localStorage`.
- [ ] Verify dark/light mode switches correctly for all shadcn components.

### Step 0.7 — Add CI and build configuration
- [ ] Add `eslint` with `@typescript-eslint` and `react-hooks` plugins; configure `eslint.config.ts` (flat config format).
- [ ] Add `prettier` with `.prettierrc` configured for 4-space indent, single quotes, trailing commas.
- [ ] Configure `lint-staged` to run `eslint --fix` and `prettier --write` on staged `.ts` and `.tsx` files via a Husky pre-commit hook.
- [ ] Add `.env.example` with `VITE_API_URL=http://localhost:5000/graphql`.
- [ ] Add `Dockerfile` and `.dockerignore` for multi-stage build (Vite build → nginx serve).
- [ ] Add `docker-compose.yml` entry or update root compose to include the Admin UI service.
- [ ] Verify `npm run lint`, `npm run typecheck`, and `npm run build` all pass.

### Step 0.8 — Create placeholder pages
- [ ] Create a minimal `DashboardPage`, `ProjectListPage`, `ProjectDetailPage`, `FeatureDetailPage`, `DefectDetailPage`, `TaskListPage` component in each route slot that renders a `<Card>` with the page title and a skeleton loader.
- [ ] Add a global error boundary component wrapping `<Routes>` that shows a friendly error card with a reload button on error.
- [ ] Confirm all routes navigate without 404 and the layout renders correctly.

**Complexity:** 5/10
**Dependencies:** None (parallel with GraphQL API Phase 0)
**Test impact:** Establishes lint, typecheck, and build pipelines.
**Risks:** Version drift between shadcn/ui CLI and manually installed Tailwind plugins.

---

## Phase 1 — Dashboard

**Goal:** Operators land on a dashboard summarizing project health and work item status.

### Step 1.1 — Define and implement the dashboard query
- [ ] Create `src/graphql/queries/dashboard.graphql`:
  ```graphql
  query GetDashboardSummary {
    dashboardSummary {
      projectsInFlight
      featuresInReview
      featuresFailed
      tasksInProgress
      tasksFailed
      recentAuditEvents(take: 10) {
        id
        entityType
        entityId
        eventType
        oldValue
        newValue
        actor
        occurredAt
      }
    }
  }
  ```
- [ ] Run `npm run codegen` to generate `GetDashboardSummaryQuery` types.
- [ ] Create `src/features/dashboard/hooks/useDashboardSummary.ts` wrapping the query with Apollo's `useQuery`.

### Step 1.2 — Build the dashboard page layout
- [ ] Create `src/features/dashboard/components/DashboardPage.tsx`.
- [ ] Add stat cards for each metric (projects in flight, features in review, features failed, tasks in progress, tasks failed) using `Card` + `Badge` for color coding (green/yellow/red).
- [ ] Add a `RecentActivity` table using `Table` shadcn component showing the last 10 audit events with columns: Entity, Event, Actor, Time.
- [ ] Add a skeleton loader that renders while data is loading (use Apollo `loading` state).
- [ ] Add an empty state if all metrics are zero.

### Step 1.3 — Add quick-action links
- [ ] Add a "New Project" button on the dashboard card that links to `/projects/new`.
- [ ] Add a "View Failed" link below the failed features/tasks badge that navigates to the filtered feature/task list.
- [ ] Add a refresh button that manually re-fetches the dashboard query.

### Step 1.4 — Add dashboard polling
- [ ] Configure Apollo `pollInterval: 30_000` (30 seconds) on the dashboard query.
- [ ] Add a visual indicator (subtle spinner in the page header) showing when a background refresh is in progress.

### Step 1.5 — Write tests
- [ ] Add `@testing-library/react`, `@testing-library/jest-dom`, `msw` (Mock Service Worker) for API mocking.
- [ ] Write a unit test for `DashboardPage` rendering the correct number of stat cards.
- [ ] Write a unit test for the activity table rendering audit events.
- [ ] Mock the GraphQL response via MSW and test the full render+loading flow.
- [ ] Confirm `npm run test` passes.

**Complexity:** 4/10
**Dependencies:** Phase 0 (Admin UI), Phase 2 (GraphQL API — dashboard query)
**Test impact:** Unit tests and MSW integration tests for dashboard components.
**Risks:** GraphQL schema may change; codegen must be re-run.

---

## Phase 2 — Project management

**Goal:** Operators can view, create, and edit projects including GitHub configuration and AI memory.

### Step 2.1 — Project list page
- [ ] Create `src/graphql/queries/projects.graphql`:
  ```graphql
  query GetProjects {
    projects {
      id
      name
      description
      githubUrl
      updatedAt
    }
  }
  ```
- [ ] Create `src/features/projects/hooks/useProjects.ts`.
- [ ] Build `src/features/projects/components/ProjectListPage.tsx`:
  - `Table` listing all projects with columns: Name, Description (truncated), GitHub URL, Last Updated.
  - Row click navigates to project detail.
  - "New Project" button opens a `Dialog`.
  - Empty state with a message and CTA.

### Step 2.2 — Project create dialog
- [ ] Create `src/graphql/mutations/createProject.graphql`.
- [ ] Create `src/features/projects/components/CreateProjectDialog.tsx`:
  - Form fields: Name (required), Description (textarea), Architecture (textarea/markdown), Memory (textarea/markdown with monospace font), GitHub URL (optional, validated as URL).
  - Use `react-hook-form` + `zod` schema for validation.
  - On submit, call `createProject` mutation.
  - On success, show toast, close dialog, and refetch project list.
  - On error, show error toast with message from `extensions.code`.

### Step 2.3 — Project detail page
- [ ] Create `src/graphql/queries/project.graphql`:
  ```graphql
  query GetProject($id: ID!) {
    project(id: $id) {
      id name description architecture memory githubUrl
      features { id title status updatedAt }
      defects { id title status updatedAt }
      modelConfigurations { id model alias maxComplexity }
      updatedAt createdAt
    }
  }
  ```
- [ ] Create `src/features/projects/hooks/useProject.ts`.
- [ ] Build `src/features/projects/components/ProjectDetailPage.tsx`:
  - Header with project name, edit button, GitHub link icon.
  - Tabbed layout (Tabs component) with tabs: Overview, Features, Defects, Models, Settings.
  - Overview tab: description, architecture notes rendered as markdown, memory rendered as markdown (monospace), project stats.
  - Features tab: filtered `FeatureList` component (see Phase 3).
  - Defects tab: filtered `DefectList` component (see Phase 4).
  - Models tab: model configuration cards (see Phase 5).
  - Settings tab: edit/delete project.

### Step 2.4 — Project edit dialog
- [ ] Create `src/graphql/mutations/updateProject.graphql`.
- [ ] Create `src/features/projects/components/EditProjectDialog.tsx`:
  - Pre-populate form with existing project data.
  - Handle optimistic concurrency error: if `CONCURRENCY_CONFLICT`, show a dialog asking the user to reload.
  - Handle `NOT_FOUND`: redirect to project list with an error toast.

### Step 2.5 — Markdown rendering
- [ ] Install `react-markdown`, `remark-gfm` (GitHub Flavored Markdown).
- [ ] Create `src/components/ui/MarkdownRenderer.tsx`:
  - Renders markdown with `react-markdown`.
  - Applies Tailwind prose styles (install `@tailwindcss/typography` plugin).
  - Code blocks get syntax highlighting via `react-syntax-highlighter`.
  - External links open in a new tab with `rel="noopener noreferrer"`.
- [ ] Use `MarkdownRenderer` in the project overview, feature detail, and task detail pages.

### Step 2.6 — GitHub URL and token fields
- [ ] For `ProjectSettingsPage`, add a section for GitHub configuration:
  - GitHub URL (display only, read from project).
  - GitHub Token (masked input — display `****` unless revealed; allow operator to set/clear).
  - Save triggers `updateProject` mutation with encrypted token stored server-side.

### Step 2.7 — Write tests
- [ ] Write unit tests for `CreateProjectDialog` form validation (zod schema).
- [ ] Write MSW integration test for project create flow.
- [ ] Write unit tests for `MarkdownRenderer` with a fixture markdown string.
- [ ] Write unit test for `EditProjectDialog` optimistic concurrency error handling.

**Complexity:** 6/10
**Dependencies:** Phase 1 (Admin UI), Phase 2 (GraphQL mutations for project)
**Test impact:** Form validation tests, MSW integration tests, markdown rendering tests.
**Risks:** Large markdown fields (memory, architecture) may cause performance issues in the editor; consider lazy loading.

---

## Phase 3 — Feature management

**Goal:** Operators can view, create, edit, and transition features through their lifecycle.

### Step 3.1 — Feature list component
- [ ] Create `src/graphql/queries/features.graphql`:
  ```graphql
  query GetFeatures($projectId: ID!, $status: [FeatureStatus!]) {
    features(projectId: $projectId, status: $status) {
      id title status description acceptanceCriteria
      plan openQuestions result errors
      securityImpact performanceImpact testPlan deploymentPlan
      tasks { id title status }
      createdAt updatedAt
    }
  }
  ```
- [ ] Create `src/features/features/hooks/useFeatures.ts`.
- [ ] Build `src/features/features/components/FeatureList.tsx`:
  - Toolbar: status filter dropdown (multi-select), search by title, "New Feature" button.
  - `Table` with columns: Title, Status (badge with color per status), Task count, Updated.
  - Row click navigates to feature detail.
  - Status badge colors: Planning=gray, Ready=blue, InProgress=yellow, InReview=purple, ReadyForTest=cyan, Testing=orange, Done=green, Failed=red, Rejected=slate.
- [ ] Use Apollo `useQuery` with filter state in URL search params for shareable URLs.

### Step 3.2 — Feature create dialog
- [ ] Create `src/graphql/mutations/createFeature.graphql`.
- [ ] Build `src/features/features/components/CreateFeatureDialog.tsx`:
  - Fields: Title (required), Description (textarea), Acceptance Criteria (textarea), Open Questions (textarea).
  - On submit, call `createFeature` mutation.
  - On success, show toast and navigate to the new feature detail page.

### Step 3.3 — Feature detail page
- [ ] Create `src/graphql/queries/feature.graphql`:
  ```graphql
  query GetFeature($id: ID!) {
    feature(id: $id) {
      id title status description acceptanceCriteria
      plan securityImpact performanceImpact testPlan deploymentPlan
      openQuestions result errors
      tasks { id title status deliverable acceptanceCriteria risks result complexityRating createdAt updatedAt }
      createdAt updatedAt
    }
  }
  ```
- [ ] Create `src/features/features/hooks/useFeature.ts`.
- [ ] Build `src/features/features/components/FeatureDetailPage.tsx`:
  - Header: title, status badge, "Edit" button.
  - Left column (2/3 width): Description, Acceptance Criteria, Plan, Security Impact, Performance Impact, Test Plan, Deployment Plan, Open Questions, Result, Errors — all rendered as markdown.
  - Right column (1/3 width): Status transition panel (see Step 3.4), Task summary list.
  - Tabs at the bottom for Task board view.

### Step 3.4 — Status transition component
- [ ] Create `src/features/features/components/StatusTransitionPanel.tsx`:
  - Displays current status.
  - Dropdown showing only valid target statuses (derived from `FeatureStatusTransitionService` rules — expose valid targets via a new GraphQL query if not already).
  - "Transition" button calls `transitionFeatureStatus` mutation with actor = "operator".
  - On success, refetch feature and show toast.
  - On `FEATURE_VALIDATION_ERROR`, show inline error explaining why the transition is not allowed.
  - Display audit trail: last 5 status change events from `auditEvents`.

### Step 3.5 — Feature edit dialog
- [ ] Create `src/graphql/mutations/updateFeature.graphql`.
- [ ] Build `src/features/features/components/EditFeatureDialog.tsx`:
  - Full form for all editable fields.
  - Long-form fields use `react-hook-form` with a `Textarea` component; no rich text editor in phase 1.
  - Save triggers `updateFeature` mutation.

### Step 3.6 — Task board view within feature
- [ ] Create `src/features/tasks/components/TaskBoard.tsx`:
  - Kanban-style board grouped by `TaskStatus` using shadcn `Tabs` or a custom column layout.
  - Each column shows task cards with title, complexity badge, and deliverable preview.
  - Click a card to open `TaskDetailDrawer`.
  - "New Task" button in each column (or global) opens `CreateTaskDialog` pre-linked to the current feature.

### Step 3.7 — Write tests
- [ ] Write unit tests for status badge color logic.
- [ ] Write unit tests for `StatusTransitionPanel` — valid/invalid transitions.
- [ ] Write MSW integration tests for feature create and status transition flows.
- [ ] Write unit test for `TaskBoard` rendering the correct columns.

**Complexity:** 7/10
**Dependencies:** Phase 2 (Project), Phase 2 (GraphQL mutations)
**Test impact:** Status transition logic tests, MSW integration tests.
**Risks:** Status transition validation lives in the backend; the UI must reflect the backend rules correctly; add a test that confirms invalid transitions are blocked in the UI.

---

## Phase 4 — Defect management

**Goal:** Operators can view, create, edit, and transition defects; defects may optionally link to a parent feature.

### Step 4.1 — Defect list component
- [ ] Create `src/graphql/queries/defects.graphql`.
- [ ] Build `src/features/defects/components/DefectList.tsx`:
  - Toolbar: status filter, "New Defect" button.
  - Table with columns: Title, Status, Parent Feature (link or "—"), Severity (badge), Updated.
  - Row click navigates to defect detail.

### Step 4.2 — Defect create dialog
- [ ] Create `src/graphql/mutations/createDefect.graphql` with optional `parentFeatureId` input.
- [ ] Build `src/features/defects/components/CreateDefectDialog.tsx`:
  - Fields: Title, Description, Acceptance Criteria, Parent Feature (searchable select — use shadcn `Command`/`Popover` combo to search projects/features), Severity (select: Low, Medium, High, Critical).

### Step 4.3 — Defect detail page
- [ ] Create `src/graphql/queries/defect.graphql`.
- [ ] Build `src/features/defects/components/DefectDetailPage.tsx`:
  - Layout mirrors `FeatureDetailPage` (two-column with markdown sections).
  - Left column: Description, Acceptance Criteria, Plan, Result, Errors, Security Impact, Performance Impact.
  - Right column: Status transition panel (reusing `StatusTransitionPanel` component), Parent Feature link, Severity badge.

### Step 4.4 — Defect edit dialog
- [ ] Create `src/graphql/mutations/updateDefect.graphql`.
- [ ] Build `src/features/defects/components/EditDefectDialog.tsx`.

### Step 4.5 — Write tests
- [ ] Write MSW integration tests for defect create with and without parent feature.
- [ ] Write unit test for severity badge color logic.

**Complexity:** 5/10
**Dependencies:** Phase 3 (Feature — patterns reused)
**Test impact:** Reuses feature test patterns; new defect-specific tests.

---

## Phase 5 — Task management

**Goal:** Operators can create tasks, edit task details, and transition task status from the board.

### Step 5.1 — Task detail drawer
- [ ] Create `src/graphql/queries/task.graphql`.
- [ ] Build `src/features/tasks/components/TaskDetailDrawer.tsx`:
  - Sheet (shadcn `Sheet`) sliding in from the right.
  - Shows all task fields rendered as markdown.
  - Status transition dropdown.
  - "Edit" button opens `EditTaskDialog`.
  - Displays complexity rating as a colored badge.

### Step 5.2 — Task create dialog
- [ ] Create `src/graphql/mutations/createTask.graphql`.
- [ ] Build `src/features/tasks/components/CreateTaskDialog.tsx`:
  - Fields: Title (required), Deliverable (textarea), Acceptance Criteria (textarea), Risks (textarea), Required Follow-ups (textarea), Complexity Rating (select 1-10).
  - Hidden `featureId` field passed as a prop.

### Step 5.3 — Task edit dialog
- [ ] Create `src/graphql/mutations/updateTask.graphql`.
- [ ] Build `src/features/tasks/components/EditTaskDialog.tsx`:
  - Pre-populated form.
  - Optimistic concurrency handling.

### Step 5.4 — Complexity badge
- [ ] Create `src/components/ui/ComplexityBadge.tsx`:
  - Color scale: 1-3=green, 4-6=yellow, 7-8=orange, 9-10=red.
  - Displays the number.

### Step 5.5 — Write tests
- [ ] Write unit tests for `ComplexityBadge` color logic.
- [ ] Write MSW integration tests for task create and edit flows.

**Complexity:** 5/10
**Dependencies:** Phase 3 (Task board already built in feature phase)
**Test impact:** Task-specific mutation tests.

---

## Phase 6 — Model configuration

**Goal:** Operators can manage model endpoints and complexity routing per project.

### Step 6.1 — Model configuration list
- [ ] Create `src/graphql/queries/modelConfigurations.graphql`.
- [ ] Create `src/features/models/components/ModelConfigurationList.tsx`:
  - Card grid showing model name, alias, max complexity, URL.
  - "Add Model" button.

### Step 6.2 — Model configuration dialog
- [ ] Create `src/graphql/mutations/createModelConfiguration.graphql`.
- [ ] Build `src/features/models/components/ModelConfigurationDialog.tsx`:
  - Fields: Model URL (required, validated as URL), Model Name (required), Alias (required), API Key (password input, masked), Max Complexity (number input 1-10).
  - On save, API encrypts the key server-side.

### Step 6.3 — Write tests
- [ ] Write form validation tests for URL and complexity constraints.

**Complexity:** 3/10
**Dependencies:** Phase 2 (patterns)
**Test impact:** Form validation tests.

---

## Phase 7 — Global settings and polish

**Goal:** Global settings page and UX polish.

### Step 7.1 — Global settings page
- [ ] Create `src/features/settings/components/GlobalSettingsPage.tsx`:
  - Default model configuration (which model to use for planning, coding, etc.).
  - Polling interval configuration (how often the UI polls for updates).
  - Theme preference.

### Step 7.2 — Responsive design pass
- [ ] Ensure sidebar collapses to a hamburger menu on mobile.
- [ ] Ensure tables scroll horizontally on small screens.
- [ ] Ensure dialogs and sheets are full-width on mobile.

### Step 7.3 — Keyboard navigation and accessibility
- [ ] Add `aria-label` to icon-only buttons.
- [ ] Ensure all interactive elements are reachable via Tab key.
- [ ] Add skip-to-content link.
- [ ] Run `axe-core` in CI to catch accessibility violations.

### Step 7.4 — Performance: lazy loading
- [ ] Implement React.lazy + Suspense for route-level code splitting.
- [ ] Verify the initial bundle size with `npm run build && npx vite-bundle-visualizer`.
- [ ] Lazy load the markdown editor component only when a long-form field is in edit mode.

### Step 7.5 — Final quality gates
- [ ] Run `npm run lint` — zero errors.
- [ ] Run `npm run typecheck` — zero errors.
- [ ] Run `npm run build` — zero errors, no warnings.
- [ ] Run all unit and integration tests.
- [ ] Run `docker compose up --build` and smoke test all major flows.
- [ ] Add Playwright E2E smoke tests for: create project, create feature, transition feature status.

**Complexity:** 4/10
**Dependencies:** Phases 1–6
**Test impact:** Accessibility tests, E2E smoke tests.

---

## Deliverables checklist

- [ ] Phase 0: App builds, routes resolve, Apollo client configured, shadcn components render
- [ ] Phase 1: Dashboard shows metrics and audit events with polling
- [ ] Phase 2: Project CRUD with markdown rendering
- [ ] Phase 3: Feature CRUD, status transitions, task board
- [ ] Phase 4: Defect CRUD with parent feature linking
- [ ] Phase 5: Task CRUD with complexity badges and drawer
- [ ] Phase 6: Model configuration management
- [ ] Phase 7: Responsive, accessible, E2E smoke tests pass
