using System.Diagnostics;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Testcontainers.PostgreSql;

namespace DevStack.Tests.Integration.GraphQL.Client;

public sealed class TestcontainersModule : IDisposable
{
    private static readonly Lazy<TestcontainersModule> _lazyInstance = new(() => new TestcontainersModule(), LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly object _buildLock = new();

    private readonly INetwork _network;
    private readonly PostgreSqlContainer _postgresContainer;
    private readonly IContainer _apiContainer;
    private bool _disposed;

    private TestcontainersModule()
    {
        string solutionDir = GetSolutionDirectory();

        _network = new NetworkBuilder()
            .WithName($"devstack-test-{Guid.NewGuid():N}")
            .Build();

        _network.CreateAsync().GetAwaiter().GetResult();

        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .WithUsername("devstack")
            .WithPassword("dd9656af-e039-48ef-ae1d-bab2ef79a836")
            .WithDatabase("devstack")
            .WithNetwork(_network)
            .WithNetworkAliases("postgres")
            .Build();

        _postgresContainer.StartAsync().GetAwaiter().GetResult();

        string postgresNetworkConnectionString = $"Host=postgres;Port=5432;Database=devstack;Username=devstack;Password=dd9656af-e039-48ef-ae1d-bab2ef79a836";
        SecretKey = "af436d91-2723-4f6e-8ac3-553c91f12e12";

        string imageName = BuildApiImage(solutionDir);

        _apiContainer = new ContainerBuilder()
            .WithImage(imageName)
            .WithNetwork(_network)
            .WithPortBinding(8080, true)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Production")
            .WithEnvironment("ConnectionStrings__DefaultConnection", postgresNetworkConnectionString)
            .WithEnvironment("DEVSTACK_SECRET_KEY", SecretKey)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(
                    f => f.ForPort(8080).ForPath("/health")))
            .Build();

        _apiContainer.StartAsync().GetAwaiter().GetResult();

        int graphqlPort = _apiContainer.GetMappedPublicPort(8080);
        GraphQlUrl = $"http://localhost:{graphqlPort}/graphql";
        PostgreSQLConnectionStringForHost = _postgresContainer.GetConnectionString();
    }

    public string PostgreSQLConnectionStringForHost { get; }
    public string GraphQlUrl { get; }
    public string SecretKey { get; }

    public static TestcontainersModule Instance => _lazyInstance.Value;

    public static void DisposeInstance()
    {
        if (_lazyInstance.IsValueCreated)
        {
            _lazyInstance.Value.Dispose();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            _apiContainer.DisposeAsync().GetAwaiter().GetResult();
        }
        catch
        {
        }

        try
        {
            _postgresContainer.DisposeAsync().GetAwaiter().GetResult();
        }
        catch
        {
        }

        try
        {
            _network.DeleteAsync().GetAwaiter().GetResult();
        }
        catch
        {
        }
    }

    private static string BuildApiImage(string solutionDir)
    {
        lock (_buildLock)
        {
            string? existingImage = GetExistingApiTestImage();
            if (!string.IsNullOrEmpty(existingImage))
            {
                return existingImage;
            }

            string imageTag = GetUniqueImageTag();
            string imageName = $"devstack-api-test:{imageTag}";

            string dockerfilePath = Path.Combine(solutionDir, "DevStack.Api", "Dockerfile");
            if (!File.Exists(dockerfilePath))
            {
                throw new FileNotFoundException($"Dockerfile not found at {dockerfilePath}");
            }

            BuildDockerImage(solutionDir, dockerfilePath, imageName);

            return imageName;
        }
    }

    private static void BuildDockerImage(string contextDir, string dockerfilePath, string imageName)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"build -f \"{dockerfilePath}\" -t \"{imageName}\" \"{contextDir}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start docker process.");

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Docker build failed with exit code {process.ExitCode}.\nSTDOUT: {stdout}\nSTDERR: {stderr}");
        }
    }

    private static string? GetExistingApiTestImage()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = "images --format \"{{.Repository}}:{{.Tag}}\" --filter \"reference=devstack-api-test:*\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = Process.Start(startInfo);
        if (process is null)
        {
            return null;
        }

        process.WaitForExit();
        var output = process.StandardOutput.ReadToEnd();

        if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(output))
        {
            return null;
        }

        var lines = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        return lines.Length > 0 ? lines[0].Trim() : null;
    }

    private static string GetUniqueImageTag()
    {
        return $"build-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}"[..12];
    }

    private static string GetSolutionDirectory()
    {
        string? currentDir = AppDomain.CurrentDomain.BaseDirectory;

        while (!string.IsNullOrEmpty(currentDir))
        {
            if (Directory.GetFiles(currentDir, "DevStack.slnx").Any() ||
                Directory.GetFiles(currentDir, "*.sln").Any())
            {
                return currentDir;
            }

            string? parentDir = Path.GetDirectoryName(currentDir);
            if (parentDir is null || parentDir == currentDir)
            {
                break;
            }

            currentDir = parentDir;
        }

        string testProjectDir = Path.GetDirectoryName(typeof(TestcontainersModule).Assembly.Location)
            ?? throw new InvalidOperationException("Could not determine assembly directory.");

        return Path.GetFullPath(Path.Combine(testProjectDir, "..", "..", "..", ".."));
    }
}
