namespace DevStack.Mcp;

public class Resources
{
}

[McpServerResourceType]
public class ResourceType
{
    [McpServerResource, Description(Descriptions.Resources.ServerInfo)]
    public static string ServerInfo()
        => Descriptions.Resources.ServerInfoContent;
}
