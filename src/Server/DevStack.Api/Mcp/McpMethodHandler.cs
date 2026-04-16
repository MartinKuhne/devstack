using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using DevStack.Domain.Enums;
using ModelContextProtocol.Server;

namespace DevStack.Api.Mcp;

public interface IMcpMethodHandler
{
    Task<object?> HandleAsync(string method, JsonElement? parameters);
}

public class McpMethodHandler : IMcpMethodHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<McpMethodHandler> _logger;
    private readonly Dictionary<string, ToolInfo> _tools;
    private readonly Lazy<DevStackTools> _devStackTools;

    private record ToolInfo(MethodInfo Method, string Description, JsonObject Schema);

    public McpMethodHandler(IServiceProvider serviceProvider, ILogger<McpMethodHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _tools = DiscoverTools();
        _devStackTools = new Lazy<DevStackTools>(() => _serviceProvider.GetRequiredService<DevStackTools>());
    }

    private Dictionary<string, ToolInfo> DiscoverTools()
    {
        var tools = new Dictionary<string, ToolInfo>();
        var type = typeof(DevStackTools);
        
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            var attrs = method.GetCustomAttributes(true);
            var toolAttr = attrs.OfType<McpServerToolAttribute>().FirstOrDefault();
            var descAttr = attrs.OfType<DescriptionAttribute>().FirstOrDefault();
            
            if (toolAttr == null) continue;

            var name = method.Name.ToLowerInvariant();
            var description = descAttr?.Description ?? method.Name;
            var schema = BuildInputSchema(method);
            
            tools[name] = new ToolInfo(method, description, schema);
        }
        
        _logger.LogInformation("Discovered {Count} MCP tools", tools.Count);
        return tools;
    }

    private JsonObject BuildInputSchema(MethodInfo method)
    {
        var properties = new Dictionary<string, JsonNode?>();
        var required = new List<string>();
        
        foreach (var param in method.GetParameters())
        {
            if (param.ParameterType == typeof(CancellationToken)) continue;
            
            var paramType = "string";
            var format = (string?)null;
            var description = param.Name;
            
            if (param.ParameterType == typeof(Guid) || param.ParameterType == typeof(Guid?))
            {
                paramType = "string";
                format = "uuid";
            }
            else if (param.ParameterType == typeof(int) || param.ParameterType == typeof(int?))
            {
                paramType = "integer";
            }
            else if (param.ParameterType == typeof(bool) || param.ParameterType == typeof(bool?))
            {
                paramType = "boolean";
            }
            else if (param.ParameterType == typeof(List<FeatureStatus>))
            {
                paramType = "array";
            }
            else if (param.ParameterType == typeof(List<DevStack.Domain.Enums.TaskStatus>))
            {
                paramType = "array";
            }
            
            var propObj = new JsonObject { ["type"] = paramType };
            if (format != null) propObj["format"] = format;
            if (description != null) propObj["description"] = description;
            properties[param.Name!] = propObj;
            
            if (param.HasDefaultValue == false)
            {
                required.Add(param.Name!);
            }
        }
        
        var schema = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject(properties)
        };
        
        if (required.Count > 0)
        {
            schema["required"] = new JsonArray(required.Select(r => JsonValue.Create(r)).ToArray());
        }
        
        return schema;
    }

    public async Task<object?> HandleAsync(string method, JsonElement? parameters)
    {
        _logger.LogInformation("Handling MCP method: {Method}", method);

        return method switch
        {
            "initialize" => HandleInitialize(),
            "tools/list" => HandleListTools(),
            "tools/call" => await HandleCallToolAsync(parameters),
            "resources/list" => new { resources = Array.Empty<object>() },
            "resources/read" => throw new JsonRpcException(-32601, "Not implemented"),
            "prompts/list" => new { prompts = Array.Empty<object>() },
            "prompts/get" => throw new JsonRpcException(-32601, "Not implemented"),
            "completion/complete" => throw new JsonRpcException(-32601, "Not implemented"),
            _ => throw new JsonRpcException(-32601, $"Method '{method}' not found")
        };
    }

    private object HandleInitialize() => new
    {
        protocolVersion = "2025-03-26",
        capabilities = new { tools = new { listChanged = true } },
        serverInfo = new { name = "DevStack MCP Server", version = "1.0.0" }
    };

    private object HandleListTools()
    {
        var tools = _tools.Select(kv => new
        {
            name = kv.Key,
            description = kv.Value.Description,
            inputSchema = kv.Value.Schema
        }).ToArray();
        
        return new { tools };
    }

    private async Task<object?> HandleCallToolAsync(JsonElement? parameters)
    {
        if (!parameters.HasValue)
            throw new JsonRpcException(-32602, "Missing parameters");

        var toolName = parameters.Value.TryGetProperty("name", out var nameElem)
            ? nameElem.GetString()?.ToLowerInvariant()
            : null;

        if (string.IsNullOrEmpty(toolName))
            throw new JsonRpcException(-32602, "Missing 'name' parameter");

        if (!_tools.TryGetValue(toolName, out var toolInfo))
            throw new JsonRpcException(-32601, $"Tool '{toolName}' not found");

        JsonElement? argsElem = null;
        if (parameters.Value.TryGetProperty("arguments", out var a))
            argsElem = a;

        _logger.LogInformation("Calling tool: {ToolName}", toolName);

        var result = await InvokeToolAsync(toolInfo.Method, argsElem);
        return new { success = true, result };
    }

    private async Task<object?> InvokeToolAsync(MethodInfo method, JsonElement? arguments)
    {
        var parameters = method.GetParameters();
        var args = new List<object?>();
        
        foreach (var param in parameters)
        {
            if (param.ParameterType == typeof(CancellationToken))
            {
                args.Add(CancellationToken.None);
                continue;
            }
            
            object? value = null;
            if (arguments.HasValue && arguments.Value.TryGetProperty(param.Name!, out var elem))
            {
                value = ConvertJsonValue(elem, param.ParameterType);
            }
            
            if (value == null && param.HasDefaultValue)
            {
                value = param.DefaultValue;
            }
            
            args.Add(value);
        }

        var tool = _devStackTools.Value;
        var result = method.Invoke(tool, args.ToArray());
        
        if (result is Task<string> taskResult)
            return await taskResult;
        if (result is Task taskAwaitable)
        {
            await taskAwaitable;
            return null;
        }
        
        // Convert entity results to simple objects
        return ConvertResult(result);
    }

    private object? ConvertResult(object? result)
    {
        if (result == null) return null;
        
        // Handle collections
        if (result is System.Collections.IEnumerable enumerable and not string)
        {
            var list = new List<object?>();
            foreach (var item in enumerable)
            {
                list.Add(ConvertEntity(item));
            }
            return list;
        }
        
        return ConvertEntity(result);
    }

    private object? ConvertEntity(object? entity)
    {
        if (entity == null) return null;
        
        var type = entity.GetType();
        var props = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var dict = new Dictionary<string, object?>();
        
        foreach (var prop in props)
        {
            try
            {
                var value = prop.GetValue(entity);
                if (value is DevStack.Domain.Entities.Entity ent)
                {
                    dict[prop.Name] = ent.Id.ToString();
                }
                else if (value is System.Collections.IEnumerable coll and not string)
                {
                    dict[prop.Name] = coll.Cast<object>().Take(100).ToArray();
                }
                else
                {
                    dict[prop.Name] = value;
                }
            }
            catch
            {
                dict[prop.Name] = null;
            }
        }
        
        return dict;
    }

    private object? ConvertJsonValue(JsonElement elem, Type targetType)
    {
        if (targetType == typeof(string))
            return elem.GetString();
        if (targetType == typeof(Guid) || targetType == typeof(Guid?))
        {
            if (Guid.TryParse(elem.GetString(), out var guid))
                return guid;
            return null;
        }
        if (targetType == typeof(int) || targetType == typeof(int?))
            return elem.GetInt32();
        if (targetType == typeof(bool) || targetType == typeof(bool?))
            return elem.GetBoolean();
        if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
            return elem.GetDateTime();
        
        return elem.GetString();
    }
}

public class JsonRpcException : Exception
{
    public int Code { get; }
    public JsonRpcException(int code, string message) : base(message) => Code = code;
}
