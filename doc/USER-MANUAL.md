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

---

## Appendix A: MCP Tool Reference

This appendix provides the complete reference for all MCP tool calls, including parameters, return values, and examples.

All responses follow a standard Markdown format:
- **Success**: `## Title\n\n```json\n{...}\n```\n\nUsage hint: ...`
- **Error**: `{"error": "message"}`

The JSON block contains the response data. Below are examples showing the JSON content for each tool.

---

### A.1 Project Tools

#### `get_projects`

Retrieves all projects.

**Parameters**: None

**Response Example**:
```json
{
  "projects": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "MyProject",
      "description": "A sample project",
      "repository": "https://github.com/user/repo"
    }
  ]
}
```

**Usage Hint**: Use this tool first to get a list of available projects before performing other operations.

---

#### `get_project`

Retrieves a specific project by ID.

**Parameters**:

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `id` | Guid | Yes | The project ID |

**Response Example**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "MyProject",
  "description": "A sample project",
  "repository": "https://github.com/user/repo"
}
```

**Usage Hint**: Provide a valid project ID obtained from `get_projects`.

---

#### `create_project`

Creates a new project.

**Parameters**:

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `name` | string | Yes | The project name |
| `repository` | string | Yes | The repository URL |
| `description` | string | No | The project description |

**Response Example**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "MyProject",
  "description": "A sample project",
  "repository": "https://github.com/user/repo"
}
```

**Usage Hint**: `name` and `repository` are required fields.

---

### A.2 Deliverable Tools

#### `get_deliverable`

Reads a deliverable by its ID. Returns all fields including title, description, acceptance criteria, and status.

**Parameters**:

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `id` | Guid | Yes | The deliverable ID |

**Response Example**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "projectId": "550e8400-e29b-41d4-a716-446655440000",
  "title": "Implement login feature",
  "description": "Add OAuth2 login",
  "design": "Use IdentityServer4",
  "acceptanceCriteria": "Users can log in with Google",
  "executionPlan": "1. Add middleware 2. Configure callbacks",
  "securityImpact": "OAuth2 tokens stored securely",
  "performanceImpact": "Minimal latency addition",
  "testPlan": "Unit tests for token validation",
  "deploymentPlan": "Deploy to staging first",
  "agentFeedback": null,
  "blocking": null
}
```

**Usage Hint**: Provide a valid deliverable ID.

---

#### `get_next_deliverable`

Find the next deliverable in the specified status for a project.

**Parameters**:

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `projectId` | Guid | Yes | The project ID |
| `status` | DeliverableStatus | Yes | The deliverable status to filter by |

**Valid Status Values**: `Draft`, `Design`, `Plan`, `Implement`, `Merge`, `Deploy`, `Test`, `Done`, `Failed`, `NeedsReview`, `Rejected`

**Response Example**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001"
}
```

**Usage Hint**: Provide the ProjectId and Status parameters. Do NOT use this tool if the user has already provided a deliverable ID.

---

#### `create_deliverable`

Creates a new deliverable (Feature) in the project. New deliverables are created in `Draft` status.

**Parameters**:

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `projectId` | Guid | Yes | The project ID |
| `title` | string | Yes | The deliverable title |
| `description` | string | No | A high level summary of what is being changed, how, and why. Use markdown format. |
| `design` | string | No | The feature-level software architecture design. Include module boundaries, contracts, architecture patterns, principles (SOLID, DRY, etc.), and fit with existing architecture. Use markdown format. |
| `acceptanceCriteria` | string | No | Objective, verifyable criteria signaling completion. Use markdown format. |
| `executionPlan` | string | No | Summary of what needs to be changed and how. Use markdown format. |
| `securityImpact` | string | No | Security impact assessment: authentication, authorization, input validation, injection risks, secrets, data protection, logging, dependencies, DoS, threat surface. Use markdown format. |
| `performanceImpact` | string | No | Performance impact assessment: latency, throughput, scalability, resource utilization, database, caching, concurrency, hot paths, batch operations, external calls, efficiency. Use markdown format. |
| `testPlan` | string | No | Test plan: unit tests, integration tests, regression tests, negative tests, data-driven tests, concurrency tests, performance tests, security tests, coverage gaps, test data, CI/CD considerations. Use markdown format. |
| `deploymentPlan` | string | No | The deployment plan. |

**Response Example**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "projectId": "550e8400-e29b-41d4-a716-446655440000",
  "type": "Feature",
  "status": "Draft"
}
```

**Usage Hint**: `projectId` and `title` are required fields. The deliverable is created as a Feature in Draft status.

---

#### `update_deliverable`

Modifies an existing deliverable. Only non-null fields are updated.

**Parameters**:

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `id` | Guid | Yes | The deliverable ID |
| `description` | string | No | A high level summary of what is being changed, how, and why. Use markdown format. |
| `design` | string | No | The feature-level software architecture design. Use markdown format. |
| `acceptanceCriteria` | string | No | Objective, verifyable criteria signaling completion. Use markdown format. |
| `executionPlan` | string | No | Summary of what needs to be changed and how. Use markdown format. |
| `securityImpact` | string | No | Security impact assessment. Use markdown format. |
| `performanceImpact` | string | No | Performance impact assessment. Use markdown format. |
| `testPlan` | string | No | Test plan. Use markdown format. |
| `deploymentPlan` | string | No | The deployment plan. |
| `agentFeedback` | string | No | The updated agent feedback. |
| `blocking` | string | No | The updated blocking issues. |

**Response Example**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "updated": true
}
```

**Usage Hint**: Provide the deliverable ID and only the fields you want to change.

---

#### `update_deliverable_status`

Changes the state of a deliverable. Valid transitions are enforced by the state machine.

**Parameters**:

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `id` | Guid | Yes | The deliverable ID |
| `targetStatus` | DeliverableStatus | Yes | The target status |
| `actor` | string | Yes | The actor performing the transition |

**Valid Status Values**: `Draft`, `Design`, `Plan`, `Implement`, `Merge`, `Deploy`, `Test`, `Done`, `Failed`, `NeedsReview`, `Rejected`

**Valid Transitions**:
- `Draft` → `Design`, `Rejected`
- `Design` → `Plan`, `Rejected`
- `Plan` → `Implement`, `Rejected`
- `Implement` → `Merge`, `NeedsReview`, `Failed`, `Rejected`
- `Merge` → `Deploy`, `Failed`, `Rejected`
- `Deploy` → `Test`, `Failed`, `Rejected`
- `Test` → `Done`, `Failed`, `Rejected`
- `NeedsReview` → `Implement`, `Rejected`
- `Failed` → `Implement`, `Rejected`
- `Rejected` → (terminal)
- `Done` → (terminal)

**Response Example**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "status": "Design",
  "actor": "opencode-agent"
}
```

**Usage Hint**: Provide valid target status such as InProgress, Done, Failed, Rejected, or NeedsReview.

---

### A.3 Task Tools

#### `get_task`

Reads an agent task by its ID. Returns all fields including title, status, description, result, and errors.

**Parameters**:

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `id` | Guid | Yes | The agent task ID |

**Response Example**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "projectId": "550e8400-e29b-41d4-a716-446655440000",
  "title": "Implement login validation",
  "status": "Done",
  "description": "Add input validation for login form",
  "result": "Implemented validation logic in LoginForm.cs",
  "errors": null,
  "commitHash": "abc123def456",
  "agent": "qwen3.7-plus"
}
```

**Usage Hint**: Provide a valid task ID obtained from `create_task` or other operations.

---

#### `get_next_task`

Find the next task to work on for a project. Looks at deliverables in `Implement` status and prioritizes those with partial progress.

**Parameters**:

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `repositoryUrl` | string | No | The repository URL |
| `projectId` | Guid | No | The project ID |

**Note**: At least one of `repositoryUrl` or `projectId` must be provided.

**Response Example**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "projectId": "550e8400-e29b-41d4-a716-446655440000",
  "title": "Implement login validation",
  "status": "Ready",
  "description": "Add input validation for login form",
  "result": null,
  "errors": null,
  "commitHash": null,
  "agent": null
}
```

**Usage Hint**: Provide either a repository URL or project ID. Do NOT use this tool if the user has already provided a deliverable ID or task ID.

---

#### `create_task`

Creates a new agent task. New tasks are created in `Ready` status.

**Parameters**:

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `deliverableId` | Guid | Yes | The deliverable/feature ID |
| `title` | string | Yes | The task title |
| `description` | string | No | A complete incremental change plan. Include: Objective, Affected code (files, types, functions), Why (reason for change), How (implementation strategy), Risks and dependencies, Test impact. Be concrete and reference real names. Use markdown format. |

**Response Example**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "status": "Ready"
}
```

**Usage Hint**: `deliverableId` must reference an existing deliverable. `title` is required. ProjectId is inferred from the deliverable.

---

#### `update_task`

Modifies an existing agent task. Only non-null fields are updated.

**Parameters**:

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `id` | Guid | Yes | The agent task ID |
| `status` | AgentTaskStatus | No | The updated status |
| `description` | string | No | A complete incremental change plan. Use markdown format. |
| `result` | string | No | An overview of what was accomplished. Use markdown format. |
| `errors` | string | No | If the task failed, list any errors or impediments. Omit if the task succeeded. |
| `commitHash` | string | No | The commit hash. Only provide this if the task was completed. |
| `agent` | string | No | The name of the coding agent and model (if known) that completed the task. |

**Valid Status Values**: `Ready`, `InProgress`, `Done`, `Failed`, `NeedsReview`, `Rejected`

**Response Example**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "updated": true
}
```

**Usage Hint**: Provide the task ID and only the fields you want to change.

---

#### `update_task_status`

Changes the state of an agent task. Valid transitions are enforced by the state machine.

**Parameters**:

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `id` | Guid | Yes | The agent task ID |
| `targetStatus` | AgentTaskStatus | Yes | The target status |
| `actor` | string | Yes | The actor performing the transition |

**Valid Status Values**: `Ready`, `InProgress`, `Done`, `Failed`, `NeedsReview`, `Rejected`

**Valid Transitions**:
- `Ready` → `InProgress`, `Rejected`
- `InProgress` → `Done`, `Failed`, `NeedsReview`, `Rejected`
- `NeedsReview` → `InProgress`, `Rejected`
- `Failed` → `InProgress`, `Rejected`
- `Rejected` → (terminal)
- `Done` → (terminal)

**Response Example**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "status": "InProgress",
  "actor": "opencode-agent"
}
```

**Usage Hint**: Provide valid target status such as InProgress, Done, Failed, Rejected, or NeedsReview.

---

### A.4 Prompts

#### `deliverable_workflow`

The only prompt available in the MCP server. It provides comprehensive instructions for working with the deliverable workflow system.

**Usage**: When calling `prompts/get`, use `name: "deliverable_workflow"` with optional arguments:
- `projectId`: The project ID to work with
- `mode`: The workflow mode (e.g., "full", "implement")

The prompt includes instructions for:
- Project discovery and setup
- Deliverable creation and progression through statuses
- Task creation and execution
- Quality gates and verification
- Code conventions and best practices
