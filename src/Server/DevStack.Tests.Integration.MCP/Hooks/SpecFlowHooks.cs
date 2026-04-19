using TechTalk.SpecFlow;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Net.Http.Headers;
using DevStack.Tests.Integration.MCP.Client;
using Testcontainers.PostgreSql;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Npgsql;
using DevStack.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevStack.Tests.Integration.MCP.Hooks;

[Binding]
public sealed class SpecFlowHooks
{
    private readonly ScenarioContext _scenarioContext;
    private IMcpJsonRpcClient? _mcpClient;
    private HttpClient? _httpClient;
    private static PostgreSqlContainer? _postgresContainer;
    private static IContainer? _mcpContainer;
    private static readonly object _lock = new();
    private static int _containerRefCount = 0;
    private static Guid _seededProjectId = Guid.Empty;

    public SpecFlowHooks(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        lock (_lock)
        {
            if (_containerRefCount > 0)
            {
                _containerRefCount++;
                return;
            }

            _containerRefCount = 1;
            InitializePostgresContainer();
            InitializeMcpContainer();
        }
    }

    private static void InitializePostgresContainer()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .WithDatabase("devstack")
            .WithUsername("devstack")
            .WithPassword("devstack_password_123")
            .Build();

        _postgresContainer.StartAsync().Wait(TimeSpan.FromSeconds(60));

        var connectionString = _postgresContainer.GetConnectionString();
        SeedDatabase(connectionString);
    }

    private static void InitializeMcpContainer()
    {
        var testProjectDir = AppContext.BaseDirectory;
        for (var i = 0; i < 10 && testProjectDir.Contains("DevStack.Tests.Integration.MCP"); i++)
        {
            testProjectDir = Path.GetDirectoryName(testProjectDir) ?? Directory.GetParent(testProjectDir)?.FullName ?? "";
        }

        var serverDir = Path.Combine(testProjectDir, "Server");
        var mcpDockerfilePath = Path.Combine(serverDir, "DevStack.Mcp", "Dockerfile");
        var dockerBuildContext = serverDir;

        if (!File.Exists(mcpDockerfilePath))
        {
            throw new FileNotFoundException(
                $"MCP Dockerfile not found at {mcpDockerfilePath}. Ensure the project structure is correct.");
        }

        var mcpImage = new ImageFromDockerfileBuilder()
            .WithName("devstack-mcp:test")
            .WithContextDirectory(dockerBuildContext)
            .WithDockerfile("DevStack.Mcp/Dockerfile")
            .Build();

        var mcpContainer = new ContainerBuilder()
            .WithImage(mcpImage)
            .WithPortBinding(8080)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithEnvironment("ConnectionStrings__DefaultConnection", _postgresContainer!.GetConnectionString())
            .WithEnvironment("DEVSTACK_SECRET_KEY", "test-secret-key-for-mcp-integration-tests")
            .Build();

        mcpContainer.StartAsync().Wait(TimeSpan.FromSeconds(120));
        _mcpContainer = mcpContainer;
    }

    private static void SeedDatabase(string connectionString)
    {
        var options = new DbContextOptionsBuilder<DevStackDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        using var context = new DevStackDbContext(options);
        context.Database.Migrate();

        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        using var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM \"Projects\"", connection);
        var count = (long)checkCmd.ExecuteScalar()!;
        if (count > 0)
        {
            using var getCmd = new NpgsqlCommand("SELECT \"Id\" FROM \"Projects\" LIMIT 1", connection);
            _seededProjectId = Guid.Parse(getCmd.ExecuteScalar()!.ToString()!);
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
        seedCmd.ExecuteNonQuery();
        _seededProjectId = projectId;
    }

    public static Guid GetSeededProjectId()
    {
        return _seededProjectId;
    }

    [AfterTestRun]
    public static void AfterTestRun()
    {
        lock (_lock)
        {
            _containerRefCount--;
            if (_containerRefCount > 0)
            {
                return;
            }

            if (_mcpContainer != null)
            {
                _mcpContainer.StopAsync().Wait(TimeSpan.FromSeconds(30));
                _mcpContainer.DisposeAsync().GetAwaiter().GetResult();
                _mcpContainer = null;
            }

            if (_postgresContainer != null)
            {
                _postgresContainer.StopAsync().Wait(TimeSpan.FromSeconds(30));
                _postgresContainer.DisposeAsync().GetAwaiter().GetResult();
                _postgresContainer = null;
            }
        }
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        if (_postgresContainer == null)
        {
            throw new InvalidOperationException("PostgreSQL container not initialized. Check BeforeTestRun.");
        }

        var connectionString = _postgresContainer.GetConnectionString();

        ushort? mcpPort = null;
        if (_mcpContainer is not null)
        {
            mcpPort = _mcpContainer.GetMappedPublicPort(8080);
        }

        var port = mcpPort ?? 8887;

        var httpClient = new HttpClient
        {
            BaseAddress = new Uri($"http://localhost:{port}")
        };
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        var services = new ServiceCollection();
        services.AddSingleton(httpClient);
        services.AddSingleton<IMcpJsonRpcClient>(sp =>
            new McpJsonRpcClient(httpClient, $"http://localhost:{port}/mcp"));

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
                var response = client.GetAsync($"http://localhost:{port}/mcp").Result;
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
}
