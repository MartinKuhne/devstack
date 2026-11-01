using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;

namespace DevStack.Tests.Integration.Shared;

public class DevStackTestEnvBuilder
{
    private string? _solutionDir;
    private string _postgresUsername = "devstack";
    private string _postgresPassword = "dd9656af-e039-48ef-ae1d-bab2ef79a836";
    private string _postgresDatabase = "devstack";
    private string _secretKey = "af436d91-2723-4f6e-8ac3-553c91f12e12";
    private string _environmentName = "Production";
    private int _appPort = 8080;
    private string _healthCheckPath = "/health";
    private string? _customApiImageName;
    private readonly Dictionary<string, string> _extraEnvironmentVariables = new();

    public DevStackTestEnvBuilder WithSolutionDir(string solutionDir)
    {
        _solutionDir = solutionDir;
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

    /// <summary>
    /// Adds an environment variable to the application container, e.g. to toggle feature flags.
    /// </summary>
    public DevStackTestEnvBuilder WithEnvironment(string name, string value)
    {
        _extraEnvironmentVariables[name] = value;
        return this;
    }

    public string? SolutionDir => _solutionDir;
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
        var dockerfilePath = Path.Combine(solutionDir, "DevStack.Api", "Dockerfile");

        Console.WriteLine($"[DevStackTestEnvBuilder] Dockerfile path: {Path.GetFullPath(dockerfilePath)}");
        Console.WriteLine($"[DevStackTestEnvBuilder] Docker build context: {Path.GetFullPath(solutionDir)}");

        if (!File.Exists(dockerfilePath))
        {
            throw new FileNotFoundException(
                $"Dockerfile not found at '{Path.GetFullPath(dockerfilePath)}'. " +
                $"Build context is '{Path.GetFullPath(solutionDir)}'. " +
                $"Check that the Dockerfile exists at the expected location and that .dockerignore is not excluding it.");
        }

        var environmentImageName = ImageNameFromEnvironment();
        var imageName = environmentImageName ?? _customApiImageName ?? "devstack-api:test";
        var reusePrebuiltImage = environmentImageName is not null;

        _appImage = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(solutionDir)
            .WithDockerfile("DevStack.Api/Dockerfile")
            .WithName(imageName)
            // CI supplies immutable prebuilt tags; local runs rebuild to avoid reusing stale source images.
            .WithImageBuildPolicy(cachedImage => !reusePrebuiltImage || cachedImage is null)
            .WithCleanUp(!reusePrebuiltImage)
            .Build();
    }

    /// <summary>
    /// Reads an image-name override from the environment so concurrent test runs
    /// do not rebuild and retag the same image.
    /// </summary>
    private string? ImageNameFromEnvironment()
    {
        var value = Environment.GetEnvironmentVariable("DEVSTACK_TEST_API_IMAGE");
        return string.IsNullOrWhiteSpace(value) ? null : value;
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
