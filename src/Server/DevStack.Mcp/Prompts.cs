namespace DevStack.Mcp;

[McpServerPromptType]
public class Prompts
{
    [McpServerPrompt, Description("A simple greeting prompt")]
    public static string Greeting()
        => "Hello! How can I help you with DevStack today?";

    [McpServerPrompt, Description("Get help with DevStack commands")]
    public static string Help()
        => "Available commands: get_deliverable, create_deliverable, update_deliverable, get_project, get_task, etc.";
}
