using TechTalk.SpecFlow;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Net.Http.Headers;
using DevStack.Tests.Integration.MCP.Client;
using Npgsql;
using DevStack.Persistence;
using DevStack.Tests.Integration.Shared;

namespace DevStack.Tests.Integration.MCP.Hooks;

[Binding]
public sealed class SpecFlowHooks
{
    private readonly ScenarioContext _scenarioContext;
    private IMcpJsonRpcClient? _mcpClient;
    private HttpClient? _httpClient;
    private static DevStackTestEnv? _env;
    private static Guid _seededProjectId = Guid.Empty;

    public SpecFlowHooks(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [BeforeTestRun]
    public static async Task BeforeTestRunAsync()
    {
        _env = DevStackTestEnvFactory.CreateBuilder()
            .WithMode(DevStackTestEnvMode.Mcp)
            .WithMcpImageName("devstack-mcp:test")
            .WithEnvironmentName("Development")
            .WithSecretKey("test-secret-key-for-mcp-integration-tests")
            .Build();

        await SeedDatabaseAsync(_env.PostgresConnectionString);
    }

    private static async Task SeedDatabaseAsync(string connectionString)
    {
        var options = new DbContextOptionsBuilder<DevStackDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        using var context = new DevStackDbContext(options);
        await context.Database.MigrateAsync();

        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        using var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM \"Projects\"", connection);
        var count = (long)(await checkCmd.ExecuteScalarAsync())!;
        if (count > 0)
        {
            using var getCmd = new NpgsqlCommand("SELECT \"Id\" FROM \"Projects\" LIMIT 1", connection);
            _seededProjectId = Guid.Parse((await getCmd.ExecuteScalarAsync())!.ToString()!);
            return;
        }

        var projectId = Guid.NewGuid();
        using var seedCmd = new NpgsqlCommand(
            "INSERT INTO \"Projects\" (\"Id\", \"Name\", \"Description\", \"Repository\") VALUES (@id, @name, @desc, @repo)",
            connection);
        seedCmd.Parameters.AddWithValue("id", projectId);
        seedCmd.Parameters.AddWithValue("name", "[TestData] MCP Test Project");
        seedCmd.Parameters.AddWithValue("desc", "A test project for MCP integration tests");
        seedCmd.Parameters.AddWithValue("repo", "https://github.com/test/mcp-project");
        await seedCmd.ExecuteNonQueryAsync();
        _seededProjectId = projectId;
    }

    public static Guid GetSeededProjectId()
    {
        return _seededProjectId;
    }

    [AfterTestRun]
    public static void AfterTestRunAsync()
    {
        _env?.Dispose();
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        if (_env is null)
        {
            throw new InvalidOperationException("DevStackTestEnv not initialized. Check BeforeTestRun.");
        }

        var port = _env.AppPort;
        var connectionString = _env.PostgresConnectionString;
        var appUrl = _env.AppUrl;

        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(appUrl)
        };
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        var services = new ServiceCollection();
        services.AddSingleton(httpClient);
        services.AddSingleton<IMcpJsonRpcClient>(sp =>
            new McpJsonRpcClient(httpClient, $"{appUrl}/mcp"));

        var provider = services.BuildServiceProvider();
        var mcpClient = provider.GetRequiredService<IMcpJsonRpcClient>();

        _httpClient = httpClient;
        _mcpClient = mcpClient;

        _scenarioContext["McpClient"] = _mcpClient;
        _scenarioContext["HttpClient"] = _httpClient;
        _scenarioContext["ConnectionString"] = connectionString;
        _scenarioContext["McpPort"] = port;
        _scenarioContext["ProjectId"] = _seededProjectId.ToString();
        Console.WriteLine($"[MCP] Seeded ProjectId: {_seededProjectId}");
        Console.WriteLine($"[MCP] Port: {port}");

        WaitForMcpServerReady(port, TimeSpan.FromSeconds(90));
    }

    [AfterScenario]
    public void AfterScenario()
    {
        _httpClient?.Dispose();
        _httpClient = null;
        _mcpClient = null;
    }

    private static void WaitForMcpServerReady(int port, TimeSpan timeout)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        using var client = new HttpClient();

        while (stopwatch.Elapsed < timeout)
        {
            try
            {
                var response = client.GetAsync($"http://localhost:{port}/health").Result;
                if (response.StatusCode == System.Net.HttpStatusCode.OK ||
                    response.StatusCode == System.Net.HttpStatusCode.BadGateway ||
                    response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                    response.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed)
                {
                    return;
                }
            }
            catch
            {
                // Server not ready yet
            }

            Thread.Sleep(500);
        }

        throw new TimeoutException($"MCP server did not become ready within {timeout.TotalSeconds} seconds on port {port}");
    }

    public static IMcpJsonRpcClient GetMcpClient(ScenarioContext context)
    {
        return context.TryGetValue<IMcpJsonRpcClient>("McpClient", out var client)
            ? client
            : throw new InvalidOperationException("MCP client not initialized. Ensure BeforeScenario hook has run.");
    }

    public static DevStackTestEnv GetTestEnvironment()
    {
        return _env ?? throw new InvalidOperationException("Test environment not initialized. Check BeforeTestRun.");
    }
}
