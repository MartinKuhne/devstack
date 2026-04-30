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
