using System.Net.Http.Json;
using DevStack.Domain.Entities;
using DevStack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace DevStack.Tests.Integration.GraphQL.Client;

public class TestContainerSetup : IAsyncLifetime, IDisposable
{
    private readonly PostgreSqlContainer _postgreSqlContainer;
    private readonly HttpClient _httpClient;
    private DevStackDbContext? _dbContext;

    public TestContainerSetup()
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithUsername("test")
            .WithPassword("test")
            .WithDatabase("devstack_test")
            .Build();

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000/graphql")
        };
    }

    public string ConnectionString => _postgreSqlContainer.GetConnectionString();

    public DevStackDbContext DbContext => _dbContext ?? throw new InvalidOperationException("Database not initialized");

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        var options = new DbContextOptionsBuilder<DevStackDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        _dbContext = new DevStackDbContext(options);
        await _dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_dbContext is not null)
        {
            await _dbContext.DisposeAsync();
        }
        await _postgreSqlContainer.StopAsync();
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}

public class TestContainerFixture : IAsyncLifetime
{
    private DevStackDbContext? _dbContext;
    private static readonly Lazy<Task> _containerStart = new Lazy<Task>(async () =>
    {
        var container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithUsername("test")
            .WithPassword("test")
            .WithDatabase("devstack_test")
            .Build();
        await container.StartAsync();
        _sharedConnectionString = container.GetConnectionString();
        _startedContainer = container;
    });
    private static string? _sharedConnectionString;
    private static PostgreSqlContainer? _startedContainer;

    public TestContainerFixture()
    {
    }

    public string ConnectionString => _sharedConnectionString ?? throw new InvalidOperationException("Container not started");

    public DevStackDbContext DbContext => _dbContext ?? throw new InvalidOperationException("Database not initialized");

    public async Task InitializeAsync()
    {
        await _containerStart.Value;

        var options = new DbContextOptionsBuilder<DevStackDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        _dbContext = new DevStackDbContext(options);
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_dbContext is not null)
        {
            await _dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Projects\", \"Features\", \"Defects\", \"Tasks\", \"ModelConfigurations\", \"WorkflowRuns\", \"AuditEvents\" RESTART IDENTITY CASCADE");
            await _dbContext.DisposeAsync();
            _dbContext = null;
        }
    }
}
