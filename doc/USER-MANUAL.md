# DevStack User Manual

## Overview

DevStack is a framework for driving AI coding agents through a continuous plan-execute-review loop. It manages a backlog of deliverables (features, defects, spikes, maintenance) and tasks, tracks their state, and orchestrates AI agents to work on them — all while letting you review and intervene at any step.

## Architecture

```
┌──────────────┐      ┌──────────────┐      ┌──────────────┐
│  Admin UI    │◄────►│  GraphQL API │◄────►│  PostgreSQL  │
│  (React)     │      │  (HotChoc.)  │      │              │
└──────────────┘      └──────┬───────┘      └──────────────┘
                             │
                    ┌────────▼────────┐
                    │   MCP Server    │
                    │  (HTTP SSE)     │
                    └────────┬────────┘
                             │
                    ┌────────▼────────┐
                    │  AI Coding      │
                    │  Agent / Tool   │
                    └─────────────────┘
```

### Data Model

- **Project** — A software repository. Identified by its remote URL.
- **Deliverable** — A unit of work within a project (Feature, Defect, Spike, Maintenance). Has a lifecycle: Draft → Design → Plan → Implement → Merge → Deploy → Test → Done (with Failed, NeedsReview, Rejected as error states).
- **AgentTask** — An atomic coding task within a deliverable. Each deliverable produces one or more agent tasks. Has statuses: Ready → InProgress → Done (with Failed, NeedsReview, Rejected).

## Getting Started

See [INSTALLATION.md](INSTALLATION.md) for setup instructions (Docker, environment, MCP configuration for various AI tools).

---

## Admin UI

The Admin UI is a React application for managing your DevStack data. Access it at http://localhost:8087.

### Dashboard (`/`)

Shows a summary of deliverables grouped by status (Draft, Design, Plan, Implement, etc.) with counts per status. Quick links to create projects and view in-progress work.

### Projects

**List** (`/projects`): Table of all projects with name, description, repository URL. Sortable and paginated.

**Create**: Click "New Project", fill in Name (required), Description, Repository URL.

**Edit**: Click the edit icon on a project row. Same fields as creation, pre-populated.

**Delete**: Click the trash icon. Confirmation required.

**Detail** (`/projects/:id`): Shows project info, list of its deliverables, and buttons to create deliverables or edit the project.

### Deliverables

**List** (`/deliverables`): Table of all deliverables across all projects. Filterable by type (Feature, Defect, Maintenance, Spike) and status. Searchable by title. Sortable and paginated.

**Create**: Click "New Deliverable" from a project detail page. Fill in:
- Type (dropdown: Feature, Defect, Maintenance, Spike)
- Title (required)
- Description, Acceptance Criteria, Design
- Initial Status (defaults to Draft)

**Edit**: On the deliverable detail page, click "Edit". Fields include all text blocks (Description, Design, Acceptance Criteria, Execution Plan, Security Impact, Performance Impact, Test Plan, Deployment Plan, Blocking).

**Change Status**: On the deliverable detail page, use the status dropdown. The system enforces valid transitions.

**Delete**: Confirmation required.

**Detail** (`/deliverables/:id`): Shows all deliverable fields rendered as Markdown, a status change control, and a sidebar listing agent tasks belonging to this deliverable.

### Agent Tasks

Agent tasks are managed within a deliverable's detail page (sidebar section).

**Create**: On a deliverable detail page, click "New" in the agent tasks sidebar. Fill in:
- Title (required)
- Deliverable ID (pre-filled, read-only)
- Complexity Rating (1-10, default 5)
- Description
- Depends On Task (optional)

**Edit**: On the agent task detail page, click "Edit". Fields: Title, Complexity Rating, Description, Result.

**Change Status**: On the agent task detail page, use the status dropdown or quick-action buttons:
- **Approve** (NeedsReview → Done)
- **Reject** (NeedsReview → Rejected, with optional feedback)
- **Retry** (Failed → Ready)

Allowed transitions:
- Ready → InProgress, NeedsReview
- InProgress → NeedsReview, Done, Failed
- NeedsReview → Done, Rejected
- Failed → Ready
- Done → Ready
- Rejected → Ready

**Detail** (`/agent-tasks/:id`): Tabbed view with:
- **Overview**: Description, Result, Complexity Rating, linked deliverable
- **Telemetry**: Agent name, Commit Hash, Prompt/Completion tokens, Execution Duration, Errors
- **Dependencies**: Linked predecessor tasks

### Large Language Models

Configure which LLMs DevStack can use for automation (the runner script reads these from GraphQL).

**List**: Grid of model cards showing Name, URL, Cost, Max Complexity (color-coded).

**Create**: Click "Add Model". Fields:
- Endpoint URL (required)
- Model Name (required)
- Alias (optional)
- Cost (0-100)
- API Key (required, password field with show/hide)
- Max Complexity (1-10)
- Max Concurrency (1-100, default 1)

**Edit/Delete**: Use icons on each model card.

---

## MCP Server

The MCP Server exposes DevStack's data and workflows to AI coding agents via the Model Context Protocol (HTTP SSE transport at `/mcp`).

### Tools

All tools return Markdown-formatted output with JSON data for LLM readability.

#### Project Tools

| Tool | Description | Key Parameters |
|------|-------------|----------------|
| `get_projects` | List all projects | — |
| `get_project` | Read project by ID | `id` (Guid) |
| `create_project` | Create a new project | `name` (required), `repository`, `description` |

#### Deliverable Tools

| Tool | Description | Key Parameters |
|------|-------------|----------------|
| `get_deliverable` | Read deliverable by ID | `id` (Guid) |
| `get_next_deliverable` | Find next deliverable in Implement status | `repositoryUrl` or `projectId` |
| `create_deliverable` | Create a new deliverable | `projectId` (required), `title` (required), `description`, `design`, `acceptanceCriteria`, `executionPlan`, `securityImpact`, `performanceImpact`, `testPlan`, `deploymentPlan` |
| `update_deliverable` | Update fields (only non-null applied) | `id`, any of the text fields |
| `update_deliverable_status` | Transition deliverable state | `id`, `targetStatus`, `actor` |

#### Agent Task Tools

| Tool | Description | Key Parameters |
|------|-------------|----------------|
| `get_task` | Read agent task by ID | `id` (Guid) |
| `get_next_task` | Find next task to work on (prioritizes partial-progress deliverables) | `repositoryUrl` or `projectId` |
| `create_task` | Create a new agent task in Ready state | `projectId` (required), `deliverableId` (required), `title` (required), `description` |
| `update_task` | Update task fields | `id`, `status`, `description`, `result`, `errors`, `commitHash`, `agent` |
| `update_task_status` | Transition task state | `id`, `targetStatus`, `actor` |

> **Note**: Agent task tools are behind a feature flag (`FeatureManagement:AgentTaskTools`). Set it to `true` in `appsettings.json` to enable them.

### Prompts

Pre-built prompt templates that guide the AI agent through DevStack workflows:

| Prompt | Purpose |
|--------|---------|
| `greeting` | Simple hello prompt |
| `help` | Lists available commands |
| `deliverable_workflow` | Guidance on picking the next deliverable and marking it done |

### Error Handling

- Invalid parameters: `McpProtocolException` with `InvalidParams` error code
- Unknown tool: `MethodNotFound`
- Internal error: code `-32603`
- Health endpoint (`GET /health`) returns 200 OK or 503 on DB failure



---

## Use Cases

### 1. Entering a Feature in the UI

1. Open http://localhost:8087
2. **Create a project** (if none exists): Navigate to Projects → New Project. Enter name and repository URL.
3. **Create a deliverable**: Navigate to the project → New Deliverable. Select type "Feature", enter a title and description. Set initial status to "Draft".
4. **Edit the deliverable**: Add Acceptance Criteria, Design notes, or other fields via the Edit button.
5. **Transition to Design**: Change the deliverable status to "Design" using the status dropdown. This signals the automation runner to begin work.

> If the automation runner is running (`devstack.ps1 run`), it picks up deliverables in Design status and runs the design prompt, which instructs the AI to create an architecture plan.

### 2. Asking AI to Plan a Feature

This happens automatically when the runner finds a Feature deliverable in PLAN status. The AI agent (via the `plan.prompt`) will:

1. Read the deliverable's Description and Design fields using `get_deliverable`
2. Analyze the architecture and create an implementation plan
3. Populate the deliverable's Security Impact, Performance Impact, Test Plan, and Deployment Plan using `update_deliverable`
4. Decompose the work into atomic coding tasks using `create_task` (one call per task)
5. Transition the deliverable to Implement using `update_deliverable_status`

You can also do this manually by asking your AI coding tool (connected to the MCP server) to plan: "Plan the feature [title] for project [name]." The AI will use the MCP tools to read the deliverable, create tasks, and set statuses.

### 3. Asking AI to Implement a Feature

When deliverables are in Implement status with Ready agent tasks, the runner executes `implement.prompt` for each task. The AI agent will:

1. Read the task description and associated deliverable using `get_task` / `get_deliverable`
2. Implement the described change in the codebase
3. Record the commit hash and result using `update_task`
4. Transition the task to Done using `update_task_status`

For manual invocation: "Implement task [id] for deliverable [id]." The AI will read the full context via MCP tools, implement the change, and mark the task complete.

### 4. Review and Merge Flow

After all agent tasks for a deliverable are DONE, the runner triggers `pull-request.prompt`, which:
1. Creates a PR summary from the completed work
2. Pushes the branch
3. Creates a pull request on the repository
4. Transitions the deliverable to Merge using `update_deliverable_status`

When a deliverable reaches Merge status, the `merge.prompt` runs and the AI:
1. Responds to PR review comments
2. Applies safe fixes and resolves threads
3. Merges once all comments are resolved and CI passes
4. Transitions to Test status

Review and approve completed tasks via the Admin UI — use the Approve/Reject buttons on agent tasks, and change deliverable statuses as needed.

### 5. Full Automated Workflow

```
You (UI) → Create Project → Create Deliverable (Feature, status: Draft)
You (UI) → Edit deliverable, add description → Change status to Design
Runner   → Picks up Design/Feature → Runs design prompt
AI       → Creates architecture plan, populates Design/Security/Performance/Test/Deployment fields
AI       → Transitions deliverable to Plan
Runner   → Picks up Plan/Feature → Runs plan prompt
AI       → Creates implementation plan, creates agent tasks
AI       → Transitions deliverable to Implement
Runner   → Picks up Implement deliverables → Runs implement.prompt per task
AI       → Implements each task, commits, updates status
Runner   → All tasks Done → Runs pull-request.prompt
AI       → Creates PR, transitions to Merge
Runner   → Picks up Merge → Runs merge.prompt
AI       → Resolves comments, merges PR, transitions to Test → Done
```

### 6. Adding Custom Prompts

Prompt files live in `scripts/prompts/` as `.prompt` files. Each prompt can reference deliverable fields via `{{Title}}`, `{{Description}}`, `{{AcceptanceCriteria}}`, `{{Design}}`, `{{DeliverableId}}`, `{{ProjectId}}` for deliverable contexts, and `{{Description}}`, `{{AgentTaskId}}` for task contexts.

The runner matches prompts to deliverables via the status transition tables in `devstack.ps1`. To add a new workflow phase, add a row to the transition table and create the corresponding prompt file.

---

## Architecture Notes

- **No authentication** in this iteration. The system is designed for local or trusted-network use.
- The GraphQL schema at `src/schema.graphql` is the contract — all components (UI, runner, MCP) use it.
- MCP tools use snake_case naming (`create_deliverable`, `update_task_status`) and include `[Description]` attributes for LLM tool selection.
- The runner uses `npx opencode run` to invoke AI agents. It selects the cheapest model meeting the complexity requirement.
- Agent tasks created via the UI start in Draft status; agent tasks created via MCP tools start in Ready status.
