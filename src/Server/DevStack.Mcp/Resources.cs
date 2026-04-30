namespace DevStack.Mcp;

public class Resources
{
}

[McpServerResourceType]
public class ResourceType
{
    [McpServerResource, Description("Server information resource")]
    public static string ServerInfo()
        => "DevStack MCP Server v1.0.0.0 - Provides access to DevStack application features";
}
