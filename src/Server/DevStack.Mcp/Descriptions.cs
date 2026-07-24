namespace DevStack.Mcp;

static class Descriptions
{
    internal static class Resources
    {
        /// <summary>
        /// Static resource providing server metadata for agent discovery and
        /// capability advertisement.
        /// </summary>
        internal const string ServerInfo = "DevStack MCP Server — version, name, and description resource";

        /// <summary>
        /// Server identity content returned to the agent when the resource is
        /// accessed.
        /// </summary>
        internal const string ServerInfoContent = "DevStack MCP Server v1.0.0.0 — Provides access to DevStack application features including project management, deliverable tracking, and agent task orchestration.";
    }

    internal static class Prompts
    {
        /// <summary>
        /// End-to-end guidance for agents on the deliverable lifecycle workflow
        /// from picking work to marking it complete.
        /// </summary>
        internal const string DeliverableWorkflow = "DevStack deliverable workflow — pick the next deliverable, implement it, and mark it complete";

        /// <summary>
        /// Detailed multi-step workflow content shown to agents when they request
        /// the deliverable_workflow prompt.
        /// </summary>
        internal const string DeliverableWorkflowContent = """
            # DevStack Deliverable Workflow

            To work on deliverables in DevStack, follow this workflow:

            ## 1. Get the next deliverable

            Call `get_next_deliverable` with either a `repositoryUrl` (e.g. `"https://github.com/my-org/my-repo"`) or a `projectId` (a UUID). This returns the highest-priority deliverable in **Implement** status, ordered by creation order.

            ## 2. Create agent tasks

            Break the deliverable down into tasks using `create_task`. Provide:
            - `projectId` and `deliverableId` from the deliverable
            - `title` — short, actionable task name
            - `description` — detailed instructions the agent will follow

            ## 3. Work on tasks

            Use `get_next_task` (with the same `repositoryUrl` or `projectId`) to find the next task. The system prioritises deliverables that already have partial progress. Update the task as you work:

            - `update_task` — save the `result`, `errors`, `commitHash`, and `agent` name
            - `update_task_status` — transition the task through InProgress, Done, Failed, Rejected, or NeedsReview

            ## 4. Update the deliverable

            After all tasks are done, fill in the deliverable's structured fields:
            - `design` — architecture and design decisions
            - `acceptanceCriteria` — how to verify correctness
            - `executionPlan` — step-by-step implementation order
            - `performanceImpact` — performance analysis (see field description)
            - `securityImpact` — security considerations
            - `testPlan` — testing strategy
            - `deploymentPlan` — rollout strategy
            - `agentFeedback` — retrospective notes from the agent
            - `blocking` — any blockers encountered

            ## 5. Mark the deliverable complete

            Call `update_deliverable_status` with:
            - `id` — the deliverable's UUID
            - `targetStatus` — set to `"Done"`
            - `actor` — your agent name

            ## Example

            ```
            get_next_deliverable(repositoryUrl: "https://github.com/example/my-project")
            create_task(projectId: "<guid>", deliverableId: "<guid>", title: "Implement login", description: "Build OAuth2 login flow")
            get_next_task(repositoryUrl: "https://github.com/example/my-project")
            update_task(id: "<guid>", result: "Login flow complete", commitHash: "abc123", agent: "my-agent")
            update_task_status(id: "<guid>", targetStatus: "Done", actor: "my-agent")
            update_deliverable(id: "<guid>", performanceImpact: "...", securityImpact: "...")
            update_deliverable_status(id: "<guid>", targetStatus: "Done", actor: "my-agent")
            ```
            """;
    }

    internal static class ProjectTools
    {
        internal const string GetProjects = """
            ## get_projects

            Read all projects from DevStack.

            - **Returns**: List of project `id`, `name`, and `repository` URL.
            - **Best Practice**: Call this first to discover available projects before performing any other operations.

            **Output fields per project**:
            | Field | Type | Description |
            |-------|------|-------------|
            | `id` | UUID | Unique project identifier |
            | `name` | String | Human-readable project name |
            | `repository` | String | Git repository URL |
            """;

        internal const string GetProject = """
            ## get_project

            Read a single project by its ID.

            - **Parameter**: `id` (UUID) — Obtained from `get_projects`.
            - **Returns**: Project details including `name`, `description`, and `repository`.
            - **Error**: Returns an error if the project does not exist.
            """;

        internal const string CreateProject = """
            ## create_project

            Create a new project in DevStack.

            - **Required fields**: `name`, `repository`.
            - **Optional**: `description`.
            - **Returns**: The newly created project's `id` (UUID).

            **Name requirements**:
            - Should be concise and descriptive (e.g. `"My Project"`).
            - Used for display and identification throughout the system.

            **Repository requirements**:
            - Must be a valid URL pointing to a Git repository (e.g. `"https://github.com/my-org/my-repo.git"`).
            - Should use HTTPS scheme for universal compatibility.

            **Description guidelines**:
            - Provide a brief summary of the project's purpose (1-3 sentences).
            - Include information about the tech stack, domain, or any relevant context that helps agents understand the project's scope.
            """;

        internal const string Id = """
            ## id

            The unique identifier of a project.

            **Field**: `id` (UUID)

            **Source**: Obtained from `create_project` or `get_projects`.
            """;

        internal const string Name = """
            ## name

            The human-readable name of the project.

            **Field**: `name` (String)

            **Requirements**:
            - 1–200 characters.
            - Should uniquely identify the project within your organisation.
            - Avoid special characters that may cause issues in URLs or shell commands.
            - Examples: `"DevStack"`, `"My Web Application"`, `"Data Pipeline v2"`
            """;

        internal const string Repository = """
            ## repository

            The Git repository URL associated with the project.

            **Field**: `repository` (String)

            **Requirements**:
            - Must be a valid, absolute URL (HTTPS is preferred over SSH for CI/CD compatibility).
            - Should point to the primary source code repository.
            - Format: `https://hostname/owner/repo[.git]`
            - Example: `"https://github.com/my-org/my-repo"`

            **Validation**: The system does not verify reachability; provide a correct URL so that other tools can use it for lookups.
            """;

        internal const string Description = """
            ## description

            A brief summary of the project's purpose and scope.

            **Field**: `description` (String, optional)

            **Requirements**:
            - 0–2000 characters.
            - Should convey the project's overall goal, tech stack, and domain.
            - Helps agents understand the context when making implementation decisions.
            - Can include markdown formatting if needed.

            **When to provide**: Always include a description so that agents working on deliverables have sufficient context to make appropriate technical decisions.
            """;

        internal const string CreateUsageHint = "Use the returned ID for subsequent get_project, create_deliverable, or update operations. Store the ID for use throughout the agent's session.";
    }

    internal static class DeliverableTools
    {
        internal const string GetDeliverable = """
            ## get_deliverable

            Read a deliverable by its ID.

            - **Parameter**: `id` (UUID) — The deliverable's unique identifier.
            - **Returns**: All deliverable fields including `title`, `description`, `acceptanceCriteria`, `design`, `securityImpact`, `performanceImpact`, `testPlan`, `deploymentPlan`, `agentFeedback`, `blocking`, and `status`.

            **Best Practice**: After creating or updating a deliverable, call `get_deliverable` to verify the changes were persisted correctly.
            """;

        internal const string GetNextDeliverable = """
            ## get_next_deliverable

            Find the next deliverable ready for implementation.

            - **Parameters**: Either `repositoryUrl` (String) or `projectId` (UUID) — exactly one must be provided.
            - **Algorithm**: Returns the oldest deliverable in **Implement** status for the identified project, ordered by creation order (ascending by `id`).
            - **Returns**: Full deliverable details including all structured fields.

            **Error cases**:
            - Neither parameter provided → error: "Either repositoryUrl or projectId must be provided"
            - Project not found → error: "Project not found"
            - No deliverables in Implement status → error: "No deliverable found in Implement status for this project"
            """;

        internal const string CreateDeliverable = """
            ## create_deliverable

            Create a new deliverable (Feature) in DevStack.

            - **Required fields**: `projectId`, `title`.
            - **Optional fields**: `description`, `design`, `acceptanceCriteria`, `executionPlan`, `securityImpact`, `performanceImpact`, `testPlan`, `deploymentPlan`.
            - **Default type**: Feature.
            - **Initial status**: Draft.
            - **Returns**: The newly created deliverable's `id` (UUID).

            **Best Practice**: Fill in as many optional fields as you can at creation time to provide full context to agents working on this deliverable.
            """;

        internal const string UpdateDeliverable = """
            ## update_deliverable

            Modify an existing deliverable in DevStack.

            - **Patching behaviour**: Only non-null fields are updated. Null fields are ignored.
            - **Parameter**: `id` (UUID, required) — The deliverable to update.
            - **Optional fields**: `description`, `design`, `acceptanceCriteria`, `executionPlan`, `securityImpact`, `performanceImpact`, `testPlan`, `deploymentPlan`, `agentFeedback`, `blocking`.

            **Best Practice**: After implementation, fill in all structured fields (performance, security, test plan, deployment plan) with thorough analysis to create a complete record.
            """;

        internal const string UpdateDeliverableStatus = """
            ## update_deliverable_status

            Change the state of a deliverable in DevStack.

            - **State machine**: Valid transitions are enforced server-side. See the `DeliverableStatus` enum for available values.
            - **Parameters**:
              - `id` (UUID, required) — The deliverable to transition.
              - `targetStatus` (DeliverableStatus, required) — The desired state.
              - `actor` (String, required) — The agent or user performing the transition.

            **Typical flow**: Draft → Design → Plan → Implement → Merge → Deploy → Test → Done
            **Terminal states**: Done, Failed, Rejected

            **Actor requirement**: Always provide a meaningful `actor` string (e.g., your agent name) for audit trail purposes.
            """;

        internal const string Id = """
            ## id

            The unique identifier of a deliverable.

            **Field**: `id` (UUID)

            **Source**: Obtained from `create_deliverable` or `get_deliverable`.
            """;

        internal const string ProjectId = """
            ## projectId

            The unique identifier of the parent project.

            **Field**: `projectId` (UUID)

            **Requirements**:
            - Must reference an existing project (obtainable via `get_projects`).
            """;

        internal const string RepositoryUrl = """
            ## repositoryUrl

            The Git repository URL used to identify a project.

            **Field**: `repositoryUrl` (String)

            **Requirements**:
            - Must match a project's `repository` field exactly (as provided during `create_project`).
            - Format: `https://hostname/owner/repo` (HTTPS preferred).
            - Either `repositoryUrl` or `projectId` must be provided (not both, not neither).
            - Example: `"https://github.com/my-org/my-repo"`
            """;

        internal const string Title = """
            ## title

            The name / short description of the deliverable.

            **Field**: `title` (String)

            **Requirements**:
            - 1–200 characters.
            - Should be a concise, actionable summary of the feature or defect.
            - Use imperative mood where possible (e.g. "Add user authentication", not "Adding user authentication").
            - Examples: `"Implement OAuth2 login"`, `"Fix payment gateway timeout"`, `"Add export-to-CSV feature"`
            """;

        internal const string Description = """
            ## description

            A detailed explanation of the deliverable.

            **Field**: `description` (String, optional)

            **Requirements**:
            - 0–5000 characters.
            - Should describe the feature/defect in enough detail that an implementation agent can understand scope, context, and dependencies.
            - For features: what problem it solves, who the users are, how they interact with it.
            - For defects: steps to reproduce, expected vs actual behaviour, environment details.
            - Can include markdown formatting.
            """;

        internal const string Design = """
            ## design

            The design document for the deliverable.

            **Field**: `design` (String, optional)

            **Requirements**:
            - Provide architecture decisions, system design, component diagrams (ASCII or Mermaid), data flow, API contracts, and data model changes.
            - Include trade-offs considered and the rationale for the chosen approach.
            - Reference relevant patterns (e.g. CQRS, Event Sourcing, Repository Pattern).

            **When to fill**: Complete before implementation begins. The design informs all downstream work (execution plan, test plan, security review).
            """;

        internal const string AcceptanceCriteria = """
            ## acceptanceCriteria

            The acceptance criteria defining when the deliverable is complete.

            **Field**: `acceptanceCriteria` (String, optional)

            **Requirements**:
            - Write as a checklist of verifiable, testable conditions.
            - Each criterion should be pass/fail — no ambiguity.
            - Cover happy path, error cases, and edge cases.
            - Use Gherkin-style (Given/When/Then) or plain bullet lists.

            **Example**:
            ```
            - [ ] User can log in with valid credentials
            - [ ] User sees error message with invalid credentials
            - [ ] Session expires after 30 minutes of inactivity
            - [ ] Rate limiting: max 5 login attempts per minute
            ```
            """;

        internal const string ExecutionPlan = """
            ## executionPlan

            The step-by-step plan for implementing the deliverable.

            **Field**: `executionPlan` (String, optional)

            **Requirements**:
            - List the implementation steps in order, referencing files, modules, or services that need to be changed.
            - Include dependencies between steps.
            - Estimate effort or complexity per step if possible.
            - Reference the design document where relevant.

            **Example**:
            ```
            1. Add User entity to the domain model (src/Domain/Entities/User.cs)
            2. Create migration for Users table
            3. Implement IUserRepository interface and Postgres implementation
            4. Add Register endpoint in AuthController
            5. Add Login endpoint with JWT token generation
            6. Write unit tests for AuthService
            7. Write integration tests for Auth endpoints
            ```
            """;

        internal const string SecurityImpact = """
            ## securityImpact

            Security impact assessment for the deliverable.

            **Field**: `securityImpact` (String, optional)

            **Requirements**: Analyse the following security concerns and document your findings:
            - **Authentication/Authorization**: Does this change affect who can access what? Are proper permission checks in place?
            - **Input Validation**: Are all inputs sanitised? Could injection attacks (SQL, XSS, command injection) occur?
            - **Data Protection**: Is sensitive data encrypted at rest and in transit? Are secrets handled properly (not logged, not hardcoded)?
            - **OWASP Top 10**: Review against OWASP Top 10 Web Application Security Risks. Which risks apply and how are they mitigated?
            - **Dependencies**: Are any new libraries introduced? Do they have known vulnerabilities?
            - **Audit Trail**: Are security-relevant events logged?
            - **Rate Limiting / Throttling**: Is abuse prevention in place?
            """;

        internal const string PerformanceImpact = """
            ## performanceImpact

            Performance impact assessment for the deliverable.

            **Field**: `performanceImpact` (String, optional)

            **Requirements**: Analyse the following performance dimensions and document your findings:
            - **Latency**: Expected response times for the new functionality. Measure p50, p95, p99 if possible.
            - **Throughput**: How many requests/operations per second can the system handle?
            - **Resource Usage**: CPU, memory, disk I/O, and network bandwidth impact.
            - **Database**: New queries, indexes needed, N+1 problems, connection pool usage, query plan analysis.
            - **Caching Strategy**: What data should be cached? What cache invalidation strategy is used?
            - **Concurrency**: Thread safety, lock contention, race conditions.
            - **Scaling**: Horizontal/vertical scaling implications. Does this change affect scalability?
            - **Bottlenecks**: Identify any potential performance bottlenecks and how they are addressed.
            - **Baseline Comparison**: "Before vs after" measurements to quantify the impact.
            """;

        internal const string TestPlan = """
            ## testPlan

            The test plan for verifying the deliverable works correctly.

            **Field**: `testPlan` (String, optional)

            **Requirements**: Describe the testing strategy covering:
            - **Unit Tests**: What individual components/modules are tested in isolation? Target coverage? Framework used.
            - **Integration Tests**: How components interact with real dependencies (database, external APIs, message queues).
            - **End-to-End Tests**: Full user journey tests that exercise the complete system.
            - **Edge Cases**: Boundary conditions, empty states, error states, concurrent access.
            - **Performance Tests**: Load tests, stress tests, soak tests if applicable.
            - **Security Tests**: Penetration testing, SAST/DAST scans, dependency scanning.
            - **Test Data**: What test data is needed? How is it set up and torn down?
            - **Automation**: Which tests are automated in CI/CD? Which are manual?
            """;

        internal const string DeploymentPlan = """
            ## deploymentPlan

            The deployment plan for rolling out the deliverable to production.

            **Field**: `deploymentPlan` (String, optional)

            **Requirements**: Document the deployment strategy covering:
            - **Rollout Strategy**: Blue/green, canary, feature flags, or direct deployment.
            - **Migration Steps**: Database migrations, data backfill, schema changes (backward compatible?).
            - **Dependencies**: External services, API versioning, configuration changes.
            - **Rollback Plan**: How to revert if something goes wrong. Include specific steps and commands.
            - **Monitoring**: What metrics, alerts, and dashboards should be monitored during and after deployment.
            - **Smoke Tests**: Quick verification steps to run after deployment to confirm the system is healthy.
            - **Communication**: Who needs to be notified before/after deployment? Any downtime expected?
            - **Timeline**: When will the deployment occur? Is there a maintenance window?
            """;

        internal const string AgentFeedback = """
            ## agentFeedback

            Feedback and retrospective notes from the agent after completing work on the deliverable.

            **Field**: `agentFeedback` (String, optional)

            **Requirements**:
            - Summarise what went well, what was challenging, and any lessons learned.
            - Note any assumptions that proved incorrect.
            - Suggest improvements for future deliverables of similar scope.
            - If the deliverable was rejected or failed, explain why and what would need to change.

            **Best Practice**: Always fill this in after completing a deliverable so that future agents benefit from your experience.
            """;

        internal const string Blocking = """
            ## blocking

            Issues or dependencies that are blocking progress on the deliverable.

            **Field**: `blocking` (String, optional)

            **Requirements**:
            - Clearly describe each blocker, its impact, and what is needed to resolve it.
            - Include links to related issues, PRs, or external dependencies.
            - Suggest workarounds if available.
            - Update this field as blockers are resolved or new ones arise.

            **Format**:
            ```
            - [BLOCKER] Awaiting API key from DevOps team — impacts integration tests
            - [RISK] Database migration may cause downtime — evaluate zero-downtime approach
            ```
            """;

        internal const string TargetStatus = """
            ## targetStatus

            The desired state to transition the deliverable to.

            **Field**: `targetStatus` (DeliverableStatus enum)

            **Available values**:
            | Status | Description |
            |--------|-------------|
            | `Draft` | Initial state, not yet ready for work |
            | `Design` | Design phase — architecture and design decisions |
            | `Plan` | Planning phase — execution plan, acceptance criteria |
            | `Implement` | Implementation in progress |
            | `Merge` | Code merged, pending verification |
            | `Deploy` | Deployed to target environment |
            | `Test` | Testing and validation phase |
            | `Done` | Completed successfully (terminal) |
            | `Failed` | Failed to complete (terminal) |
            | `Rejected` | Rejected (terminal) |
            | `NeedsReview` | Requires human review |

            **Requirements**: The transition must be valid per the server-side state machine. Invalid transitions return an error.
            """;

        internal const string Actor = """
            ## actor

            The agent or user performing an action or state transition.

            **Field**: `actor` (String)

            **Requirements**:
            - A meaningful identifier that can be used for audit trail purposes.
            - Examples: `"my-agent-v1"`, `"code-reviewer-bot"`, `"john.doe@company.com"`
            - Should be consistent across all operations performed by the same entity so that actions can be attributed correctly.
            - Avoid generic values like `"agent"` or `"unknown"`.
            """;

        internal const string CreateUsageHint = "Use the returned ID for subsequent get_deliverable, update_deliverable, or update_deliverable_status calls. Store the ID for use throughout the agent's session.";
        internal const string UpdateUsageHint = "Call get_deliverable after updating to verify the changes were persisted correctly. Only non-null fields are updated.";
    }

    internal static class TaskTools
    {
        internal const string GetTask = """
            ## get_task

            Read an agent task by its ID.

            - **Parameter**: `id` (UUID) — The task's unique identifier.
            - **Returns**: All task fields including `title`, `status`, `description`, `result`, `errors`, `commitHash`, and `agent`.

            **Best Practice**: After creating or updating a task, call `get_task` to verify the changes were persisted correctly.
            """;

        internal const string GetNextTask = """
            ## get_next_task

            Find the next agent task to work on for a project.

            - **Parameters**: Either `repositoryUrl` (String) or `projectId` (UUID) — exactly one must be provided.
            - **Algorithm**:
              1. Find the project by `repositoryUrl` or `projectId`.
              2. Find all deliverables in **Implement** status for that project.
              3. For each deliverable, count completed tasks vs pending tasks.
              4. Prioritise deliverables with **partial progress** (some tasks done, some pending) over those with none started.
              5. Within the selected deliverable, return tasks in priority order: Ready first, then InProgress.
            - **Returns**: The next task to work on, or a message indicating all tasks are complete.

            **Error cases**:
            - Neither parameter provided → error
            - Project not found → error: "Project not found"
            - No deliverables in Implement status → no pending tasks message
            """;

        internal const string CreateTask = """
            ## create_task

            Create a new agent task in DevStack.

            - **Required fields**: `projectId`, `deliverableId`, `title`.
            - **Optional**: `description`.
            - **Default complexity**: 5 (on a 1–10 scale).
            - **Initial status**: Ready.
            - **Returns**: The newly created task's `id` (UUID) and initial status.

            **Best Practice**:
            - Break deliverables into small, focused tasks that can be completed in a single session (15–30 minutes).
            - Provide a descriptive `description` with specific implementation instructions.
            - Reference the deliverable's design and execution plan in the task description.
            """;

        internal const string UpdateTask = """
            ## update_task

            Modify an existing agent task in DevStack.

            - **Patching behaviour**: Only non-null fields are updated. Null fields are ignored.
            - **Parameter**: `id` (UUID, required) — The task to update.
            - **Optional fields**: `status`, `description`, `result`, `errors`, `commitHash`, `agent`.

            **Best Practice**:
            - Always save the `result` with a summary of what was accomplished.
            - Save `errors` with any issues encountered (even if resolved).
            - Record the `commitHash` for traceability.
            - After updating, call `get_task` to verify the changes.
            """;

        internal const string UpdateTaskStatus = """
            ## update_task_status

            Change the state of an agent task in DevStack.

            - **State machine**: Valid transitions are enforced server-side.
            - **Parameters**:
              - `id` (UUID, required) — The task to transition.
              - `targetStatus` (AgentTaskStatus, required) — The desired state.
              - `actor` (String, required) — The agent or user performing the transition.

            **Available status values**:
            | Status | Description |
            |--------|-------------|
            | `Ready` | Task is ready to be worked on |
            | `InProgress` | Agent is actively working on the task |
            | `Done` | Task completed successfully |
            | `Failed` | Task failed |
            | `Rejected` | Task was rejected |
            | `NeedsReview` | Task requires human review |

            **Actor requirement**: Always provide a meaningful `actor` string (e.g., your agent name) for audit trail purposes.
            """;

        internal const string Id = """
            ## id

            The unique identifier of an agent task.

            **Field**: `id` (UUID)

            **Source**: Obtained from `create_task` or `get_task`.
            """;

        internal const string ProjectId = """
            ## projectId

            The unique identifier of the parent project for this task.

            **Field**: `projectId` (UUID)

            **Requirements**:
            - Must reference an existing project.
            - Typically obtained from the deliverable or from `get_projects`.
            """;

        internal const string RepositoryUrl = """
            ## repositoryUrl

            The Git repository URL used to identify a project for task lookups.

            **Field**: `repositoryUrl` (String)

            **Requirements**:
            - Must match a project's `repository` field exactly.
            - Format: `https://hostname/owner/repo` (HTTPS preferred).
            - Either `repositoryUrl` or `projectId` must be provided.
            - Example: `"https://github.com/my-org/my-repo"`
            """;

        internal const string DeliverableId = """
            ## deliverableId

            The unique identifier of the parent deliverable/feature.

            **Field**: `deliverableId` (UUID)

            **Requirements**:
            - Must reference an existing deliverable.
            - Typically obtained from the deliverable creation response or `get_next_deliverable`.
            """;

        internal const string Title = """
            ## title

            The name / short description of the agent task.

            **Field**: `title` (String)

            **Requirements**:
            - 1–200 characters.
            - Should be a concise, actionable summary of what the agent needs to do.
            - Use imperative mood (e.g. "Add validation to login form", "Write unit tests for AuthService").
            - Should be specific enough that a single agent can complete the task without further clarification.

            **Examples**:
            - `"Implement password hashing in UserService"`
            - `"Add input validation to the registration endpoint"`
            - `"Write integration tests for the payment webhook"`
            """;

        internal const string Description = """
            ## description

            Detailed instructions and context for the agent task.

            **Field**: `description` (String, optional)

            **Requirements**:
            - 0–5000 characters.
            - Provide specific implementation instructions, referencing files, classes, or methods where relevant.
            - Include acceptance criteria for the task so the agent can self-verify.
            - Reference the deliverable's design, acceptance criteria, or execution plan.
            - Can include code snippets, file paths, and markdown formatting.

            **Example**:
            ```md
            Implement password hashing in `src/Services/UserService.cs`:

            1. Add BCrypt.Net NuGet package dependency
            2. Replace plain-text password storage with BCrypt hash
            3. Update the `VerifyPassword` method to use BCrypt.Verify()
            4. Ensure existing passwords are re-hashed on next login

            Acceptance: All password-related unit tests pass.
            ```
            """;

        internal const string Status = """
            ## status

            The updated status of the agent task.

            **Field**: `status` (AgentTaskStatus enum)

            **Available values**:
            | Value | Description |
            |-------|-------------|
            | `Ready` | Task is ready to be worked on |
            | `InProgress` | Agent is actively working on the task |
            | `Done` | Task completed successfully |
            | `Failed` | Task failed |
            | `Rejected` | Task was rejected |
            | `NeedsReview` | Task requires human review |

            **Best Practice**: Use `update_task_status` for state transitions rather than setting `status` directly on `update_task`, as the state machine enforces valid transitions.
            """;

        internal const string Result = """
            ## result

            The outcome or deliverable produced by the agent task.

            **Field**: `result` (String, optional)

            **Requirements**:
            - Summarise what was accomplished, including key decisions made and any unexpected findings.
            - Reference specific files changed, commits made, or outputs produced.
            - If the task involved code changes, include the commit hash in the `commitHash` field and briefly describe the changes here.

            **Best Practice**: Always fill this in when completing a task, even if the task failed. It provides context for human reviewers and future agents.
            """;

        internal const string Errors = """
            ## errors

            Errors, warnings, or issues encountered during task execution.

            **Field**: `errors` (String, optional)

            **Requirements**:
            - Include full error messages and stack traces where relevant.
            - Describe what was attempted, what went wrong, and what might fix it.
            - If the error was resolved during the task, note the resolution.
            - If the error caused the task to fail (status = Failed), explain clearly why.

            **Format**:
            ```
            ERROR: System.InvalidOperationException: Connection refused at ...
            Resolution: Updated connection string to use the correct port.
            ```
            """;

        internal const string CommitHash = """
            ## commitHash

            The Git commit hash associated with the task's implementation.

            **Field**: `commitHash` (String, optional)

            **Requirements**:
            - Must be a valid Git commit hash (SHA-1, 40 hex characters, or short form of 7+ characters).
            - Should point to the commit that contains the implementation for this task.
            - Enables traceability from task → code change.

            **Examples**:
            - `"a1b2c3d4e5f6789012345678abcdef1234567890"` (full hash)
            - `"a1b2c3d"` (short hash, 7 characters minimum)
            """;

        internal const string Agent = """
            ## agent

            The name or identifier of the agent that worked on or is assigned to the task.

            **Field**: `agent` (String, optional)

            **Requirements**:
            - A meaningful identifier for attribution and audit trail purposes.
            - Should be consistent across all operations performed by the same agent.
            - Examples: `"my-coding-agent-v2"`, `"code-reviewer-bot"`, `"implementer-alpha"`

            **Best Practice**: Set this when creating or claiming a task so that other agents know who is working on it.
            """;

        internal const string TargetStatus = """
            ## targetStatus

            The desired state to transition the agent task to.

            **Field**: `targetStatus` (AgentTaskStatus enum)

            **Available values**:
            | Value | Description |
            |-------|-------------|
            | `Ready` | Task is ready to be worked on |
            | `InProgress` | Agent is actively working on the task |
            | `Done` | Task completed successfully |
            | `Failed` | Task failed |
            | `Rejected` | Task was rejected |
            | `NeedsReview` | Task requires human review |

            **Requirements**: The transition must be valid per the server-side state machine. Invalid transitions return an error.
            """;

        internal const string Actor = """
            ## actor

            The agent or user performing an action or state transition.

            **Field**: `actor` (String)

            **Requirements**:
            - A meaningful identifier for audit trail purposes.
            - Examples: `"my-agent-v1"`, `"code-reviewer-bot"`, `"john.doe@company.com"`
            - Should be consistent across all operations performed by the same entity.
            - Avoid generic values like `"agent"` or `"unknown"`.
            """;

        internal const string CreateUsageHint = "Use the returned ID for subsequent get_task, update_task, or update_task_status calls. Store the ID for use throughout the agent's session.";
        internal const string UpdateUsageHint = "Call get_task after updating to verify the changes were persisted correctly. Only non-null fields are updated.";
    }
}
