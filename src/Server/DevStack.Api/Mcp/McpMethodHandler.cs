using System.Text.Json;

namespace DevStack.Api.Mcp;

/// <summary>
/// Handles JSON-RPC 2.0 MCP method calls
/// </summary>
public interface IMcpMethodHandler
{
    Task<object?> HandleAsync(string method, JsonElement? parameters);
}

/// <summary>
/// MCP method handler implementation
/// </summary>
public class McpMethodHandler : IMcpMethodHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<McpMethodHandler> _logger;

    public McpMethodHandler(IServiceProvider serviceProvider, ILogger<McpMethodHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<object?> HandleAsync(string method, JsonElement? parameters)
    {
        _logger.LogInformation("Handling MCP method: {Method}", method);

        return method switch
        {
            "initialize" => await HandleInitializeAsync(parameters),
            "tools/list" => await HandleListToolsAsync(parameters),
            "tools/call" => await HandleCallToolAsync(parameters),
            "resources/list" => await HandleListResourcesAsync(parameters),
            "resources/read" => await HandleReadResourceAsync(parameters),
            "prompts/list" => await HandleListPromptsAsync(parameters),
            "prompts/get" => await HandleGetPromptAsync(parameters),
            "completion/complete" => await HandleCompleteAsync(parameters),
            _ => throw new JsonRpcException(
                JsonRpcErrorCode.MethodNotFound,
                $"Method '{method}' not found")
        };
    }

    private Task<object?> HandleInitializeAsync(JsonElement? parameters)
    {
        // Initialize response with server info and capabilities
        return Task.FromResult<object?>(new
        {
            protocolVersion = "2024-11-05",
            capabilities = new
            {
                tools = new { listChanged = true }
            },
            serverInfo = new
            {
                name = "DevStack MCP Server",
                version = "1.0.0"
            }
        });
    }

    private Task<object?> HandleListToolsAsync(JsonElement? parameters)
    {
        // Return list of available tools
        return Task.FromResult<object?>(new
        {
            tools = new object[]
            {
                new
                {
                    name = "create_project",
                    description = "Create a new project",
                    inputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            name = new { type = "string", description = "Project name" },
                            description = new { type = "string", description = "Project description" }
                        },
                        required = new[] { "name" }
                    }
                },
                new
                {
                    name = "create_feature",
                    description = "Create a new feature",
                    inputSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            projectId = new { type = "string", format = "uuid", description = "Project ID" },
                            title = new { type = "string", description = "Feature title" }
                        },
                        required = new[] { "projectId", "title" }
                    }
                }
            }
        });
    }

    private async Task<object?> HandleCallToolAsync(JsonElement? parameters)
    {
        if (!parameters.HasValue)
            throw new JsonRpcException(JsonRpcErrorCode.InvalidParams, "Missing parameters");

        var toolName = parameters.Value.TryGetProperty("name", out var nameElem)
            ? nameElem.GetString()
            : null;

        if (string.IsNullOrEmpty(toolName))
            throw new JsonRpcException(JsonRpcErrorCode.InvalidParams, "Missing 'name' parameter");

        // Get tool arguments if provided
        JsonElement? toolArgs = null;
        if (parameters.Value.TryGetProperty("arguments", out var argsElem))
        {
            toolArgs = argsElem;
        }

        _logger.LogInformation("Calling tool: {ToolName}", toolName);

        // Dispatch to specific tool handlers
        return toolName switch
        {
            "create_project" => await HandleCreateProjectAsync(toolArgs),
            "create_feature" => await HandleCreateFeatureAsync(toolArgs),
            _ => throw new JsonRpcException(
                JsonRpcErrorCode.MethodNotFound,
                $"Tool '{toolName}' not found")
        };
    }

    private async Task<object?> HandleCreateProjectAsync(JsonElement? arguments)
    {
        if (!arguments.HasValue)
            throw new JsonRpcException(JsonRpcErrorCode.InvalidParams, "Missing arguments");

        var name = arguments.Value.TryGetProperty("name", out var nameElem)
            ? nameElem.GetString()
            : null;

        if (string.IsNullOrEmpty(name))
            throw new JsonRpcException(JsonRpcErrorCode.InvalidParams, "Missing 'name' argument");

        var description = arguments.Value.TryGetProperty("description", out var descElem)
            ? descElem.GetString()
            : null;

        var architecture = arguments.Value.TryGetProperty("architecture", out var archElem)
            ? archElem.GetString()
            : null;

        var memory = arguments.Value.TryGetProperty("memory", out var memElem)
            ? memElem.GetString()
            : null;

        var githubUrl = arguments.Value.TryGetProperty("githubUrl", out var ghElem)
            ? ghElem.GetString()
            : null;

        // Get the handler from DI and execute
        var handler = _serviceProvider.GetRequiredService<Infrastructure.Projects.ICreateProjectHandler>();
        var projectId = await handler.Handle(
            new Infrastructure.Projects.CreateProjectCommand(name, description, architecture, memory, githubUrl),
            CancellationToken.None);

        return new
        {
            success = true,
            projectId = projectId,
            message = $"Project '{name}' created successfully"
        };
    }

    private async Task<object?> HandleCreateFeatureAsync(JsonElement? arguments)
    {
        if (!arguments.HasValue)
            throw new JsonRpcException(JsonRpcErrorCode.InvalidParams, "Missing arguments");

        var projectIdStr = arguments.Value.TryGetProperty("projectId", out var projElem)
            ? projElem.GetString()
            : null;

        var title = arguments.Value.TryGetProperty("title", out var titleElem)
            ? titleElem.GetString()
            : null;

        if (string.IsNullOrEmpty(projectIdStr) || !Guid.TryParse(projectIdStr, out var projectId))
            throw new JsonRpcException(JsonRpcErrorCode.InvalidParams, "Invalid 'projectId'");

        if (string.IsNullOrEmpty(title))
            throw new JsonRpcException(JsonRpcErrorCode.InvalidParams, "Missing 'title'");

        var description = arguments.Value.TryGetProperty("description", out var descElem)
            ? descElem.GetString()
            : null;

        var acceptanceCriteria = arguments.Value.TryGetProperty("acceptanceCriteria", out var accElem)
            ? accElem.GetString()
            : null;

        var plan = arguments.Value.TryGetProperty("plan", out var planElem)
            ? planElem.GetString()
            : null;

        var securityImpact = arguments.Value.TryGetProperty("securityImpact", out var secElem)
            ? secElem.GetString()
            : null;

        var performanceImpact = arguments.Value.TryGetProperty("performanceImpact", out var perfElem)
            ? perfElem.GetString()
            : null;

        var testPlan = arguments.Value.TryGetProperty("testPlan", out var testElem)
            ? testElem.GetString()
            : null;

        var deploymentPlan = arguments.Value.TryGetProperty("deploymentPlan", out var deployElem)
            ? deployElem.GetString()
            : null;

        var openQuestions = arguments.Value.TryGetProperty("openQuestions", out var questionsElem)
            ? questionsElem.GetString()
            : null;

        // Get the handler from DI and execute
        var handler = _serviceProvider.GetRequiredService<Infrastructure.Features.ICreateFeatureHandler>();
        var featureId = await handler.Handle(
            new Infrastructure.Features.CreateFeatureCommand(
                projectId,
                title,
                description,
                acceptanceCriteria,
                plan,
                securityImpact,
                performanceImpact,
                testPlan,
                deploymentPlan,
                openQuestions,
                null),
            CancellationToken.None);

        return new
        {
            success = true,
            featureId = featureId,
            message = $"Feature '{title}' created successfully"
        };
    }

    private Task<object?> HandleListResourcesAsync(JsonElement? parameters) =>
        Task.FromResult<object?>(new { resources = Array.Empty<object>() });

    private Task<object?> HandleReadResourceAsync(JsonElement? parameters) =>
        throw new JsonRpcException(JsonRpcErrorCode.MethodNotFound, "Resource reading not implemented");

    private Task<object?> HandleListPromptsAsync(JsonElement? parameters) =>
        Task.FromResult<object?>(new { prompts = Array.Empty<object>() });

    private Task<object?> HandleGetPromptAsync(JsonElement? parameters) =>
        throw new JsonRpcException(JsonRpcErrorCode.MethodNotFound, "Prompt retrieval not implemented");

    private Task<object?> HandleCompleteAsync(JsonElement? parameters) =>
        throw new JsonRpcException(JsonRpcErrorCode.MethodNotFound, "Completion not implemented");
}

/// <summary>
/// Exception for JSON-RPC errors
/// </summary>
public class JsonRpcException : Exception
{
    public int Code { get; }

    public JsonRpcException(int code, string message) : base(message)
    {
        Code = code;
    }
}
