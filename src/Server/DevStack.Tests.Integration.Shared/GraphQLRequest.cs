using System.Text;
using System.Text.Json;

namespace DevStack.Tests.Integration.Shared;

/// <summary>
/// Executes GraphQL documents over HTTP against a test environment endpoint.
/// </summary>
public static class GraphQLRequest
{
    /// <summary>
    /// Posts a GraphQL document with optional variables and operation name.
    /// </summary>
    public static async Task<GraphQLResult> ExecuteAsync(
        HttpClient client,
        string query,
        object? variables = null,
        string? operationName = null,
        CancellationToken cancellationToken = default)
    {
        var payload = new Dictionary<string, object?>
        {
            ["query"] = query
        };

        if (variables is not null)
        {
            payload["variables"] = variables;
        }

        if (!string.IsNullOrEmpty(operationName))
        {
            payload["operationName"] = operationName;
        }

        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync(string.Empty, content, cancellationToken);
        return await GraphQLResult.FromResponseAsync(response, cancellationToken);
    }

    /// <summary>
    /// Synchronous variant used by the existing SpecFlow step definitions.
    /// </summary>
    public static GraphQLResult Execute(
        HttpClient client,
        string query,
        object? variables = null,
        string? operationName = null)
        => ExecuteAsync(client, query, variables, operationName).GetAwaiter().GetResult();
}
