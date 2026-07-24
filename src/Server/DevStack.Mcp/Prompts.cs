namespace DevStack.Mcp;

[McpServerPromptType]
public class DeliverableWorkflowPrompt
{
    [McpServerPrompt(Name = "deliverable_workflow"), Description(Descriptions.Prompts.DeliverableWorkflow)]
    public static ChatMessage DeliverableWorkflow()
    {
        return new(ChatRole.User, Descriptions.Prompts.DeliverableWorkflowContent);
    }
}
