using System.Text.Json;

namespace DevStack.Mcp.Dto;

public static class ToolResponse
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public static string Success<T>(string title, T data, string? usageHint = null)
    {
        var json = JsonSerializer.Serialize(data, SerializerOptions);
        var response = $"## {title}\n\n```json\n{json}\n```";
        return usageHint is not null ? $"{response}\n\nUsage hint: {usageHint}" : response;
    }

    public static string Error(string message) =>
        JsonSerializer.Serialize(new { error = message }, SerializerOptions);
}
