namespace DevStack.Mcp;

[McpServerPromptType]
public class GreetingPrompt
{
    [McpServerPrompt(Name = "greeting"), Description("A simple greeting prompt")]
    public static ChatMessage Greeting(
        [Description("The name of the user to greet")] string name = "")
    {
        if (string.IsNullOrWhiteSpace(name))
            return new(ChatRole.User, "Hello! How can I help you with DevStack today?");

        return new(ChatRole.User, $"Hello, {name}! How can I help you with DevStack today?");
    }
}

[McpServerPromptType]
public class HelpPrompt
{
    [McpServerPrompt(Name = "help"), Description("Get help with DevStack commands")]
    public static ChatMessage Help(
        [Description("The specific command to get help for")] string command = "")
    {
        if (string.IsNullOrWhiteSpace(command))
            return new(ChatRole.User, "Available commands: get_deliverable, create_deliverable, update_deliverable, get_project, get_task, etc.");

        return new(ChatRole.User, $"Help for '{command}': Use the {command} tool to interact with DevStack. Check the DevStack documentation for detailed usage information.");
    }
}

[McpServerPromptType]
public class DeliverableWorkflowPrompt
{
    [McpServerPrompt(Name = "deliverable_workflow"), Description("Guidance on how to pick the next deliverable to work on and mark it complete")]
    public static ChatMessage DeliverableWorkflow()
    {
        return new(ChatRole.User, """
            To work on deliverables in DevStack, follow this workflow:

            1. **Get the next deliverable** — Use the `get_next_deliverable` tool, passing either a `repositoryUrl` or a `projectId`. It returns the next deliverable in "Implement" status, ordered by creation order.

            2. **Update the deliverable to Done** — After the deliverable is complete, use the `update_deliverable_status` tool with the deliverable's ID, set `targetStatus` to "Done", and provide an `actor` string (e.g., your name or agent name).

            Example:
            ```
            get_next_deliverable(repositoryUrl: "https://github.com/example/my-project")
            update_deliverable_status(id: "<deliverable-id>", targetStatus: "Done", actor: "my-agent")
            ```
            """);
    }
}
