namespace DevStack.Mcp;

static class Descriptions
{
    internal static class Resources
    {
        internal const string ServerInfo = "Server information resource";
        internal const string ServerInfoContent = "DevStack MCP Server v1.0.0.0 - Provides access to DevStack application features";
    }

    internal static class Prompts
    {
        internal const string DeliverableWorkflow = "Guidance on how to pick the next deliverable to work on and mark it complete";
        internal const string DeliverableWorkflowContent = """
            To work on deliverables in DevStack, follow this workflow:

            1. **Get the next deliverable** — Use the `get_next_deliverable` tool, passing either a `repositoryUrl` or a `projectId`. It returns the next deliverable in "Implement" status, ordered by creation order.

            2. **Update the deliverable to Done** — After the deliverable is complete, use the `update_deliverable_status` tool with the deliverable's ID, set `targetStatus` to "Done", and provide an `actor` string (e.g., your name or agent name).

            Example:
            ```
            get_next_deliverable(repositoryUrl: "https://github.com/example/my-project")
            update_deliverable_status(id: "<deliverable-id>", targetStatus: "Done", actor: "my-agent")
            ```
            """;
    }

    internal static class ProjectTools
    {
        internal const string GetProjects = "Read all projects from DevStack. Returns project name, id, and repository. Usage hint: Call this first to get a list of available projects before performing other operations.";
        internal const string GetProject = "Read a project by its ID. Returns project name and repository. Usage hint: Provide a valid project ID obtained from get_projects.";
        internal const string CreateProject = "Create a new project in DevStack. Usage hint: Name and repository are required fields.";

        internal const string Id = "The project ID";
        internal const string Name = "The project name";
        internal const string Repository = "The repository URL";
        internal const string Description = "The project description";

        internal const string CreateUsageHint = "Use the returned ID for subsequent get_project, create_deliverable, or update operations.";
    }

    internal static class DeliverableTools
    {
        internal const string GetDeliverable = "Read a deliverable by its ID. Returns all fields including title, description, acceptance criteria, and status. Usage hint: Provide a valid deliverable ID.";
        internal const string GetNextDeliverable = "Find the next deliverable in Implement status for a project. Provide either a repository URL or a project ID.";
        internal const string CreateDeliverable = "Create a new deliverable (Feature) in DevStack. New deliverables are created in Ready state. Usage hint: ProjectId must reference an existing project. Title and description are required fields.";
        internal const string UpdateDeliverable = "Modify an existing deliverable in DevStack. Only non-null fields are updated. Usage hint: Provide the deliverable ID and only the fields you want to change.";
        internal const string UpdateDeliverableStatus = "Change the state of a deliverable in DevStack. Valid transitions are enforced by the state machine. Usage hint: Provide valid target status such as InProgress, Done, Failed, Rejected, or NeedsReview.";

        internal const string Id = "The deliverable ID";
        internal const string ProjectId = "The project ID";
        internal const string RepositoryUrl = "The repository URL";
        internal const string Title = "The deliverable title";
        internal const string Description = "A high level summary of what is being changed, how, and why. Use markdown format.";
        internal const string Design = """
            The feature-level software architecture design for the deliverable. Consider each item; Omit aspects when not applicable.
            - Module boundaries: which layers or modules are involved and where the new code lives, and which bounded context it belongs to
            - Contracts: the interfaces, data transfer objects, or data shapes that other components depend on, and whether contracts are being added, changed, or kept stable
            - Architecture patterns: the patterns in play (e.g., layered/N-tier, clean architecture, DDD aggregates and repositories, CQRS, dependency injection, event-driven) and how they apply to this change
            - Principles and best practices: demonstrate separation of concerns, encapsulation, dependency inversion (depend on abstractions, not implementations), explicit dependencies, single responsibility, DRY, persistence ignorance, and mutable global state avoidance
            - Fit with existing architecture: how the change extends or aligns with the current system, and any integration points, dependencies, or migration concerns
            Keep it concise and specific to the deliverable. Use markdown format.
            """;
        internal const string AcceptanceCriteria = "Provide objective, verifyable criteria signaling the completion of this deliverable. Use markdown format.";
        internal const string ExecutionPlan = "Provide a summary of what needs to be changed and how. Use markdown format.";
        internal const string SecurityImpact = """
            The security impact assessment for the deliverable. Consider each item; Omit aspects when not applicable.
            - Authentication: changes to how users or services are verified (e.g., credentials, tokens, multi-factor)
            - Authorization: changes to access control, permissions, roles, or privilege boundaries
            - Input validation: handling of untrusted input at trust boundaries (parsing, deserialization, length/charset limits)
            - Injection risks: SQL, command, path, LDAP, XSS, or other injection vectors introduced or mitigated
            - Secrets and keys: introduction, storage, rotation, or exposure of passwords, API keys, certificates, or connection strings
            - Data protection: confidentiality, integrity, and encryption at rest and in transit for sensitive data
            - Logging and observability: whether sensitive data may be logged, and auditability of security-relevant actions
            - Dependencies: new or updated libraries and their trust, licensing, and vulnerability posture
            - Denial of service: resource exhaustion, rate limiting, backpressure, or amplification risks
            - Threat surface: new endpoints, ports, protocols, or exposed surfaces, and any residual risks
            Keep it concise and actionable. Use markdown format.
            """;
        internal const string PerformanceImpact = """
            The performance impact assessment for the deliverable. Consider each item; Omit aspects when not applicable.
            - Latency: changes to response times for user-facing or service-to-service paths, including new round trips
            - Throughput: effect on requests, messages, or transactions per second under expected and peak load
            - Scalability: whether the change affects horizontal or vertical scaling, statefulness, or bottlenecks that limit scale
            - Resource utilization: CPU, memory, disk, network, or connection pool consumption introduced or reduced
            - Database and storage: new queries, indexes, locks, round trips, N+1 patterns, or storage growth
            - Caching: cacheability, invalidation, stale-data risk, and fit with existing cache layers
            - Concurrency and parallelism: locking, contention, async/await correctness, deadlocks, or race conditions
            - Hot paths: impact on the most frequently executed code paths and whether work is moved into or out of them
            - Batch and bulk operations: effect of processing one item at a time versus batching, and large-result-set handling
            - External calls: new or changed calls to other services, APIs, or downstream systems and their latency/retry behavior
            - Efficiency of algorithms: time and space complexity of new algorithms or data transformations
            Keep it concise and actionable. Use markdown format.
            """;
        internal const string TestPlan = """
            The test plan for the deliverable. Consider each item; Omit aspects when not applicable.
            - Unit tests: new tests for changed logic, covering happy paths, edge cases, boundary values, and error paths; name existing tests that must be updated or are no longer required
            - Integration tests: interactions across modules, services, or external systems, including contract and schema validation
            - Regression tests: scenarios to verify that existing behavior is preserved, particularly for refactors or shared code paths
            - Negative and failure-path tests: invalid input, deserialization failures, timeouts, exceptions, and partial-failure handling
            - Data-driven and parameterized tests: variation over inputs, data shapes, locales, or configurations
            - Concurrency and race condition tests: parallel execution, async timing, locks, and idempotency
            - Performance and load tests: latency, throughput, memory, and resource use under expected and peak load
            - Security tests: authentication, authorization, input validation, injection, and secrets handling, where applicable
            - Test coverage and gaps: areas not covered by automated tests and the mitigation (manual verification, documented limitation)
            - Test data and fixtures: setup, seeding, teardown, and isolation from other tests or environments
            - CI/CD considerations: tests liable to be flaky, order-dependent, or environment-specific, and any required infrastructure
            Keep it concise and actionable.  Use markdown format.
            """;
        internal const string DeploymentPlan = "The deployment plan";
        internal const string AgentFeedback = "The updated agent feedback";
        internal const string Blocking = "The updated blocking issues";
        internal const string TargetStatus = "The target status";
        internal const string Actor = "The actor performing the transition";

        internal const string CreateUsageHint = "Use the returned ID for subsequent get_deliverable, update_deliverable, or update_deliverable_status calls.";
        internal const string UpdateUsageHint = "Use get_deliverable to verify the changes.";
    }

    internal static class TaskTools
    {
        internal const string GetTask = "Read an agent task by its ID. Returns all fields including title, status, description, result, and errors. Usage hint: Provide a valid task ID obtained from create_task or other operations.";
        internal const string GetNextTask = "Find the next task to work on for a project. Looks at deliverables in Implement status and prioritizes those with partial progress. Provide either a repository URL or project ID.";
        internal const string CreateTask = "Create a new agent task in DevStack. New tasks are created in Ready state. Usage hint: Both ProjectId and DeliverableId must reference existing entities.";
        internal const string UpdateTask = "Modify an existing agent task in DevStack. Only non-null fields are updated. Usage hint: Provide the task ID and only the fields you want to change.";
        internal const string UpdateTaskStatus = "Change the state of an agent task in DevStack. Valid transitions are enforced by the state machine. Usage hint: Provide valid target status such as InProgress, Done, Failed, Rejected, or NeedsReview.";

        internal const string Id = "The agent task ID";
        internal const string ProjectId = "The project ID";
        internal const string RepositoryUrl = "The repository URL";
        internal const string DeliverableId = "The deliverable/feature ID";
        internal const string Title = "The task title";
        internal const string Description = """
            A complete incremental change plan for the task. Include:
            - Objective: the specific goal of this change, stated as an outcome
            - Affected code: the files, types, and functions that will be modified or added (with paths)
            - Why: the reason for the change and what problem it solves
            - How: the implementation strategy and approach (step-by-step if useful)
            - Risks and dependencies: assumptions, breaking changes, or related work
            - Test impact: which tests must be added, updated, or are no longer required
            Be concrete and reference real names; keep it actionable for an executing agent. Use markdown format.
            """;
        internal const string Status = "The updated status";
        internal const string Result = "An overview of what was accomplished. Only provide this information once the task is completed. Use markdown format.";
        internal const string Errors = "If the task failed, list any errors or impediments here. Omit if the task succeeded";
        internal const string CommitHash = "The commit hash. Only provide this information if the task was completed. (omit if changes were not committed)";
        internal const string Agent = "The name of the coding agent and the model (if known) that completed the task";
        internal const string TargetStatus = "The target status";
        internal const string Actor = "The actor performing the transition";

        internal const string CreateUsageHint = "Use the returned ID for subsequent get_task, update_task, or update_task_status calls.";
        internal const string UpdateUsageHint = "Use get_task to verify the changes.";
    }
}
