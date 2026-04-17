using System.Net.Http.Json;
using System.Text.Json.Nodes;
using DevStack.Infrastructure.Persistence;
using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace DevStack.Tests.Integration.GraphQL.Client;

[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
{
}

public class IntegrationTestFixture : IAsyncLifetime
{
    private DevStackDbContext? _dbContext;
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<DevStackDbContext> _options;
    public readonly HttpClient HttpClient;
    public readonly string GraphQlUrl;

    public IntegrationTestFixture()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<DevStackDbContext>()
            .UseSqlite(_connection)
            .Options;
        
        _dbContext = new DevStackDbContext(_options);
        HttpClient = new HttpClient();
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();
        
        GraphQlUrl = configuration.GetValue<string>("GraphQL:Url") ?? "http://localhost:8087/graphql";
    }

    public DevStackDbContext CreateDbContext()
    {
        return new DevStackDbContext(_options);
    }

    public async Task InitializeAsync()
    {
        await _dbContext!.Database.EnsureDeletedAsync();
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        if (_dbContext is not null)
        {
            await _dbContext.DisposeAsync();
        }
        _connection.Close();
        HttpClient.Dispose();
    }

    private async Task<JsonNode?> SendMutationAsync(string query, object? variables = null)
    {
        var response = await HttpClient.PostAsJsonAsync("GraphQlUrl", new { query, variables });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonNode.Parse(json)?["data"];
    }

    public async Task<Guid> CreateTestProjectAsync(string name, string? description = null)
    {
        var data = await SendMutationAsync("""
            mutation CreateProject($input: CreateProjectInput!) {
              createProject(input: $input) {
                project { id }
                errors
              }
            }
            """,
            new { input = new { name, description } });
        
        data!["createProject"]!["errors"]!.AsArray().Should().BeEmpty();
        return Guid.Parse(data["createProject"]!["project"]!["id"]!.GetValue<string>());
    }

    public async Task<Guid> CreateTestFeatureAsync(Guid projectId, string title, string? description = null)
    {
        var data = await SendMutationAsync("""
            mutation CreateFeature($input: CreateFeatureInput!) {
              createFeature(input: $input) {
                item { id }
                errors
              }
            }
            """,
            new { input = new { projectId, title, description } });
        
        data!["createFeature"]!["errors"]!.AsArray().Should().BeEmpty();
        return Guid.Parse(data["createFeature"]!["item"]!["id"]!.GetValue<string>());
    }

    public async Task<Guid> CreateTestDefectAsync(Guid projectId, Guid? parentFeatureId, string title, Domain.Enums.Severity? severity = null)
    {
        var data = await SendMutationAsync("""
            mutation CreateDefect($input: CreateDefectInput!) {
              createDefect(input: $input) {
                item { id }
                errors
              }
            }
            """,
            new { 
                input = new 
                { 
                    projectId, 
                    parentFeatureId, 
                    title, 
                    severity = severity?.ToString().ToUpper() 
                } 
            });
        
        data!["createDefect"]!["errors"]!.AsArray().Should().BeEmpty();
        return Guid.Parse(data["createDefect"]!["item"]!["id"]!.GetValue<string>());
    }

    public async Task<Guid> CreateTestTaskAsync(Guid projectId, Guid itemId, string title, int complexityRating)
    {
        var data = await SendMutationAsync("""
            mutation CreateTask($input: CreateTaskInput!) {
              createTask(input: $input) {
                task { id }
                errors
              }
            }
            """,
            new { input = new { projectId, itemId, title, complexityRating } });
        
        data!["createTask"]!["errors"]!.AsArray().Should().BeEmpty();
        return Guid.Parse(data["createTask"]!["task"]!["id"]!.GetValue<string>());
    }
}
