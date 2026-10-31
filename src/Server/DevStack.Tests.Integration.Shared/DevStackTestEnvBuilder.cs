using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;

namespace DevStack.Tests.Integration.Shared;

public class DevStackTestEnvBuilder
{
    private string? _solutionDir;
    private DevStackTestEnvMode _mode = DevStackTestEnvMode.Api;
    private string _postgresUsername = "devstack";
    private string _postgresPassword = "dd9656af-e039-48ef-ae1d-bab2ef79a836";
    private string _postgresDatabase = "devstack";
    private string _secretKey = "af436d91-2723-4f6e-8ac3-553c91f12e12";
    private string _environmentName = "Production";
    private int _appPort = 8080;
    private string _healthCheckPath = "/health";
    private string? _customApiImageName;
    private string? _customMcpImageName;
    private readonly Dictionary<string, string> _extraEnvironmentVariables = new();

    public DevStackTestEnvBuilder WithSolutionDir(string solutionDir)
    {
        _solutionDir = solutionDir;
        return this;
    }

    public DevStackTestEnvBuilder WithMode(DevStackTestEnvMode mode)
    {
        _mode = mode;
        return this;
    }

    public DevStackTestEnvBuilder WithPostgresCredentials(string username, string password, string database)
    {
        _postgresUsername = username;
        _postgresPassword = password;
        _postgresDatabase = database;
        return this;
    }

    public DevStackTestEnvBuilder WithSecretKey(string secretKey)
    {
        _secretKey = secretKey;
        return this;
    }

    public DevStackTestEnvBuilder WithEnvironmentName(string environmentName)
    {
        _environmentName = environmentName;
        return this;
    }

    public DevStackTestEnvBuilder WithAppPort(int port)
    {
        _appPort = port;
        return this;
    }

    public DevStackTestEnvBuilder WithHealthCheckPath(string healthCheckPath)
    {
        _healthCheckPath = healthCheckPath;
        return this;
    }

    public DevStackTestEnvBuilder WithApiImageName(string imageName)
    {
        _customApiImageName = imageName;
        return this;
    }

    public DevStackTestEnvBuilder WithMcpImageName(string imageName)
    {
        _customMcpImageName = imageName;
        return this;
    }

    /// <summary>
    /// Adds an environment variable to the application container, e.g. to toggle feature flags.
    /// </summary>
    public DevStackTestEnvBuilder WithEnvironment(string name, string value)
    {
        _extraEnvironmentVariables[name] = value;
        return this;
    }

    public string? SolutionDir => _solutionDir;
    public DevStackTestEnvMode Mode => _mode;
    public string PostgresUsername => _postgresUsername;
    public string PostgresPassword => _postgresPassword;
    public string PostgresDatabase => _postgresDatabase;
    public string SecretKey => _secretKey;
    public string EnvironmentName => _environmentName;
    public int AppPort => _appPort;
    public string HealthCheckPath => _healthCheckPath;
    public IFutureDockerImage? AppImage => _appImage;
    public IReadOnlyDictionary<string, string> ExtraEnvironmentVariables => _extraEnvironmentVariables;

    private IFutureDockerImage? _appImage;

    internal void BuildAppImage(string solutionDir)
    {
        var dockerfilePath = Path.Combine(solutionDir,
            _mode == DevStackTestEnvMode.Api ? "DevStack.Api" : "DevStack.Mcp",
            "Dockerfile");

        Console.WriteLine($"[DevStackTestEnvBuilder] Dockerfile path: {Path.GetFullPath(dockerfilePath)}");
        Console.WriteLine($"[DevStackTestEnvBuilder] Docker build context: {Path.GetFullPath(solutionDir)}");

        if (!File.Exists(dockerfilePath))
        {
            throw new FileNotFoundException(
                $"Dockerfile not found at '{Path.GetFullPath(dockerfilePath)}'. " +
                $"Build context is '{Path.GetFullPath(solutionDir)}'. " +
                $"Check that the Dockerfile exists at the expected location and that .dockerignore is not excluding it.");
        }

        var imageName = ResolveImageName(ImageNameFromEnvironment());

        _appImage = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(solutionDir)
            .WithDockerfile(_mode == DevStackTestEnvMode.Api ? "DevStack.Api/Dockerfile" : "DevStack.Mcp/Dockerfile")
            .WithName(imageName)
            .WithImageBuildPolicy(_ => true)
            .Build();
    }

    /// <summary>
    /// Reads an image-name override from the environment so concurrent test runs
    /// do not rebuild and retag the same image.
    /// </summary>
    private string? ImageNameFromEnvironment()
    {
        var variable = _mode == DevStackTestEnvMode.Api
            ? "DEVSTACK_TEST_API_IMAGE"
            : "DEVSTACK_TEST_MCP_IMAGE";

        var value = Environment.GetEnvironmentVariable(variable);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    /// <summary>
    /// Resolves the effective image name: explicit builder name, then environment override, then default.
    /// </summary>
    private string ResolveImageName(string? environmentOverride)
        => _mode switch
        {
            DevStackTestEnvMode.Api => _customApiImageName ?? environmentOverride ?? "devstack-api:test",
            DevStackTestEnvMode.Mcp => _customMcpImageName ?? environmentOverride ?? "devstack-mcp:test",
            _ => throw new InvalidOperationException($"Unknown mode: {_mode}")
        };

    internal string GetAppUrl(int mappedPort)
    {
        return $"http://localhost:{mappedPort}";
    }

    public DevStackTestEnv Build()
    {
        return new DevStackTestEnv(this);
    }
}
