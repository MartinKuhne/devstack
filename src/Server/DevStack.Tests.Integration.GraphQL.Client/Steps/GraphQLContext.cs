using System.Net.Http.Json;
using System.Text.Json.Nodes;
using DevStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.GraphQL.Client.Steps;

[Binding]
public class GraphQLContext
{
    private readonly TestContainerFixture _fixture;
    private JsonNode? _lastResponse;

    [Given("the API is available")]
    public void GivenTheApiIsAvailable()
    {
        if (_fixture.HttpClient == null)
            throw new InvalidOperationException("API is not available");
    }

    public GraphQLContext(TestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task<JsonNode?> SendMutationAsync(string queryName, object? variables = null)
    {
        var query = await File.ReadAllTextAsync($"GraphQL/Mutations/{queryName}.graphql");
        var response = await _fixture.HttpClient.PostAsJsonAsync("/graphql", new { query, variables });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        _lastResponse = JsonNode.Parse(json)?["data"];
        return _lastResponse;
    }

    public async Task<JsonNode?> SendQueryAsync(string queryName, object? variables = null)
    {
        var query = await File.ReadAllTextAsync($"GraphQL/Queries/{queryName}.graphql");
        var response = await _fixture.HttpClient.PostAsJsonAsync("/graphql", new { query, variables });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        _lastResponse = JsonNode.Parse(json)?["data"];
        return _lastResponse;
    }

    public JsonNode? LastResponse => _lastResponse;

    public DevStackDbContext CreateDbContext() => _fixture.CreateDbContext();

    public HttpClient HttpClient => _fixture.HttpClient;
}