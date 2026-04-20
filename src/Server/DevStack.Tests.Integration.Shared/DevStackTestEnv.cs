using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;
using Testcontainers.PostgreSql;

namespace DevStack.Tests.Integration.Shared;

public enum DevStackTestEnvMode
{
    Api,
    Mcp
}

public sealed class DevStackTestEnv : IDisposable
{
    private readonly INetwork _network;
    private readonly PostgreSqlContainer _postgresContainer;
    private readonly IContainer _appContainer;
    private readonly IFutureDockerImage _appImage;
    private bool _disposed;

    internal DevStackTestEnv(DevStackTestEnvBuilder builder)
    {
        string solutionDir = builder.SolutionDir
            ?? DevStackTestEnvFactory.FindSolutionDirectory();

        _network = new NetworkBuilder()
            .WithName($"devstack-test-{Guid.NewGuid():N}")
            .Build();

        _network.CreateAsync().GetAwaiter().GetResult();

        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .WithUsername(builder.PostgresUsername)
            .WithPassword(builder.PostgresPassword)
            .WithDatabase(builder.PostgresDatabase)
            .WithNetwork(_network)
            .WithNetworkAliases("postgres")
            .Build();

        _postgresContainer.StartAsync().GetAwaiter().GetResult();

        string postgresConnectionString = $"Host=postgres;Port=5432;Database={builder.PostgresDatabase};Username={builder.PostgresUsername};Password={builder.PostgresPassword}";

        builder.BuildAppImage(solutionDir);
        _appImage = builder.AppImage
            ?? throw new InvalidOperationException("App image was not built.");

        _appImage.CreateAsync().GetAwaiter().GetResult();

        _appContainer = new ContainerBuilder()
            .WithImage(_appImage)
            .WithNetwork(_network)
            .WithPortBinding((ushort)builder.AppPort, true)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.EnvironmentName)
            .WithEnvironment("ConnectionStrings__DefaultConnection", postgresConnectionString)
            .WithEnvironment("DEVSTACK_SECRET_KEY", builder.SecretKey)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(
                    f => f.ForPort((ushort)builder.AppPort).ForPath(builder.HealthCheckPath)))
            .Build();

        _appContainer.StartAsync().GetAwaiter().GetResult();

        AppPort = builder.AppPort;
        PostgresConnectionString = _postgresContainer.GetConnectionString();
        PostgresConnectionStringForHost = _postgresContainer.GetConnectionString();
        SecretKey = builder.SecretKey;
        Mode = builder.Mode;

        int mappedPort = _appContainer.GetMappedPublicPort((ushort)builder.AppPort);
        AppUrl = builder.GetAppUrl(mappedPort);
    }

    public int AppPort { get; }
    public string PostgresConnectionString { get; }
    public string PostgresConnectionStringForHost { get; }
    public string AppUrl { get; }
    public string SecretKey { get; }
    public DevStackTestEnvMode Mode { get; }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            _appContainer.DisposeAsync().GetAwaiter().GetResult();
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
            _appImage.DisposeAsync().GetAwaiter().GetResult();
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
}
