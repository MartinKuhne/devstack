using TechTalk.SpecFlow;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using DevStack.Tests.Integration.MCP.Client;
using Testcontainers.PostgreSql;
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
    private static Process? _mcpProcess;
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

            if (_mcpProcess != null && !_mcpProcess.HasExited)
            {
                _mcpProcess.Kill(true);
                _mcpProcess.WaitForExit(TimeSpan.FromSeconds(10));
                _mcpProcess.Dispose();
                _mcpProcess = null;
            }

            if (_postgresContainer != null)
            {
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

        var services = new ServiceCollection();

  var port = 8887;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri($"http://localhost:{port}")
        };
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        services.AddSingleton(_httpClient);
        services.AddSingleton<IMcpJsonRpcClient>(sp =>
            new McpJsonRpcClient(_httpClient, $"http://localhost:{port}/mcp"));

        var provider = services.BuildServiceProvider();
        _mcpClient = provider.GetRequiredService<IMcpJsonRpcClient>();

        _scenarioContext["McpClient"] = _mcpClient;
        _scenarioContext["HttpClient"] = _httpClient;
        _scenarioContext["ConnectionString"] = connectionString;
        _scenarioContext["McpPort"] = port;
        _scenarioContext["ProjectId"] = _seededProjectId.ToString();
        Console.WriteLine($"[MCP] Seeded ProjectId: {_seededProjectId}");

        StartMcpServer(connectionString, port);
    }

    [AfterScenario]
    public void AfterScenario()
    {
        _httpClient?.Dispose();
        _httpClient = null;
        _mcpClient = null;
    }

    private static void StartMcpServer(string connectionString, int port)
    {
        lock (_lock)
        {
            bool processIsRunning = false;
            try
            {
                if (_mcpProcess != null && !_mcpProcess.HasExited)
                {
                    processIsRunning = true;
                }
            }
            catch (InvalidOperationException)
            {
                // Process object is stale, reset it
                _mcpProcess?.Dispose();
                _mcpProcess = null;
            }

            if (processIsRunning)
            {
                return;
            }

           var mcpProjectPath = Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..", "..", "Server", "DevStack.Mcp",
                "DevStack.Mcp.csproj");

            mcpProjectPath = Path.GetFullPath(mcpProjectPath);

            var startInfo = new ProcessStartInfo
            {
                FileName = "C:\\Program Files\\dotnet\\dotnet.exe",
                Arguments = $"run --no-build --project \"{mcpProjectPath}\" -- --urls http://localhost:{port}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = Path.GetDirectoryName(mcpProjectPath),
                Environment =
                {
                    ["ASPNETCORE_ENVIRONMENT"] = "Development",
                    ["ConnectionStrings__DefaultConnection"] = connectionString,
                    ["DEVSTACK_SECRET_KEY"] = "test-secret-key-for-mcp-integration-tests"
                }
            };
            Console.WriteLine($"[MCP] Connection string: {connectionString}");

            _mcpProcess = new Process { StartInfo = startInfo };
            _mcpProcess.OutputDataReceived += (s, e) => Console.WriteLine($"[MCP OUT] {e.Data}");
            _mcpProcess.ErrorDataReceived += (s, e) => Console.WriteLine($"[MCP ERR] {e.Data}");
            _mcpProcess.EnableRaisingEvents = true;
            _mcpProcess.Exited += (s, e) => Console.WriteLine($"[MCP EXIT] Process exited with code {_mcpProcess.ExitCode}");
            _mcpProcess.Start();

            Console.WriteLine($"[MCP] Process started with PID: {_mcpProcess.Id}");

           try
            {
                WaitForMcpServerReady(port, TimeSpan.FromSeconds(60));
                Thread.Sleep(2000);
            }
            catch (TimeoutException)
            {
                Console.WriteLine($"[MCP] Server failed to start. Checking process status...");
                if (_mcpProcess != null && !_mcpProcess.HasExited)
                {
                    _mcpProcess.Kill(true);
                    _mcpProcess.WaitForExit(TimeSpan.FromSeconds(10));
                }
                _mcpProcess?.Dispose();
                _mcpProcess = null;
                throw;
            }
        }
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

    private static int GetAvailablePort()
    {
        using var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public static IMcpJsonRpcClient GetMcpClient(ScenarioContext context)
    {
        return context.TryGetValue<IMcpJsonRpcClient>("McpClient", out var client)
            ? client
            : throw new InvalidOperationException("MCP client not initialized. Ensure BeforeScenario hook has run.");
    }
}
