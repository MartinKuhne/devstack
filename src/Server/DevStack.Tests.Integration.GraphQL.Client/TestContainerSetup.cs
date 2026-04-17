using DevStack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace DevStack.Tests.Integration.GraphQL.Client;

public class TestContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer;
    private WebApplicationFactory<Program>? _factory;
    private HttpClient? _httpClient;

    public TestContainerFixture()
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithUsername("test")
            .WithPassword("test")
            .WithDatabase("devstack_test")
            .Build();
    }

    public string ConnectionString => _postgreSqlContainer.GetConnectionString();

    public HttpClient HttpClient => _httpClient ?? throw new InvalidOperationException("Fixture not initialized");

    public DevStackDbContext CreateDbContext() => new DevStackDbContext(
        new DbContextOptionsBuilder<DevStackDbContext>()
            .UseNpgsql(ConnectionString)
            .Options);

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:DefaultConnection", ConnectionString);
                builder.UseSetting("DEVSTACK_SECRET_KEY", "test-secret-key-for-testing!");
            });

        _httpClient = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _httpClient?.Dispose();

        if (_factory != null)
        {
            try
            {
                await using var ctx = CreateDbContext();
                await ctx.Database.ExecuteSqlRawAsync(
                    "TRUNCATE TABLE \"Projects\", \"Features\", \"Defects\", \"Tasks\", \"ModelConfigurations\", \"WorkflowRuns\", \"AuditEvents\" RESTART IDENTITY CASCADE");
            }
            catch { }

            _factory.Dispose();
        }

        await _postgreSqlContainer.StopAsync();
    }
}
