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

    private IFutureDockerImage? _appImage;

    internal void BuildAppImage(string solutionDir)
    {
        var dockerfilePath = Path.Combine(solutionDir,
            _mode == DevStackTestEnvMode.Api ? "DevStack.Api" : "DevStack.Mcp",
            "Dockerfile");

        if (!File.Exists(dockerfilePath))
        {
            throw new FileNotFoundException($"Dockerfile not found at {dockerfilePath}");
        }

        var imageName = _mode switch
        {
            DevStackTestEnvMode.Api => _customApiImageName ?? "devstack-api:test",
            DevStackTestEnvMode.Mcp => _customMcpImageName ?? "devstack-mcp:test",
            _ => throw new InvalidOperationException($"Unknown mode: {_mode}")
        };

        _appImage = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(solutionDir)
            .WithDockerfile(Path.Combine(_mode == DevStackTestEnvMode.Api ? "DevStack.Api" : "DevStack.Mcp", "Dockerfile"))
            .WithName(imageName)
            .WithImageBuildPolicy(_ => true)
            .Build();
    }

    internal string GetAppUrl(int mappedPort)
    {
        return $"http://localhost:{mappedPort}";
    }

    public DevStackTestEnv Build()
    {
        return new DevStackTestEnv(this);
    }
}
