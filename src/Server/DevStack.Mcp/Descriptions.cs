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
        internal const string Description = "The deliverable description";
        internal const string Design = "The design document";
        internal const string AcceptanceCriteria = "The acceptance criteria";
        internal const string ExecutionPlan = "The execution plan";
        internal const string SecurityImpact = "The security impact assessment";
        internal const string PerformanceImpact = "The performance impact assessment";
        internal const string TestPlan = "The test plan";
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
        internal const string Description = "The task description";
        internal const string Status = "The updated status";
        internal const string Result = "The result";
        internal const string Errors = "The errors";
        internal const string CommitHash = "The commit hash";
        internal const string Agent = "The agent";
        internal const string TargetStatus = "The target status";
        internal const string Actor = "The actor performing the transition";

        internal const string CreateUsageHint = "Use the returned ID for subsequent get_task, update_task, or update_task_status calls.";
        internal const string UpdateUsageHint = "Use get_task to verify the changes.";
    }
}
