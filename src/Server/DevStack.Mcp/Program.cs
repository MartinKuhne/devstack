using DevStack.Application.AgentTasks;
using DevStack.Application.AgentTasks.Queries;
using DevStack.Application.Deliverables;
using DevStack.Application.Deliverables.Commands;
using DevStack.Application.Deliverables.Queries;
using DevStack.Application.LargeLanguageModels.Commands;
using DevStack.Application.Projects.Commands;
using DevStack.Application.Projects.Queries;
using DevStack.Infrastructure;
using DevStack.Infrastructure.AgentTasks;
using DevStack.Infrastructure.Deliverables;
using DevStack.Infrastructure.ModelConfigurations;
using DevStack.Infrastructure.Projects;
using DevStack.Mcp;
using DevStack.Mcp.Logging;

using ModelContextProtocol.Server;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.WithProperty("Application", "DevStack.Mcp")
    .Enrich.FromLogContext()
    .Enrich.WithCorrelationId()
    .WriteTo.Console()
    .WriteTo.File(
        new Serilog.Formatting.Json.JsonFormatter(),
        "logs/devstack-mcp-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

try
{
    Log.Information("Starting DevStack MCP Server");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((hostContext, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(hostContext.Configuration)
            .ReadFrom.Services(services)
            .WriteTo.Console()
            .WriteTo.File(
                new Serilog.Formatting.Json.JsonFormatter(),
                "logs/devstack-mcp-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .Enrich.WithProperty("Application", "DevStack.Mcp")
            .Enrich.FromLogContext()
            .Enrich.WithCorrelationId();
    });

    builder.Services.AddMcpServer()
        .WithHttpTransport(options =>
        {
            options.Stateless = true;
        })
        .WithToolsFromAssembly()
        .WithPrompts<GreetingPrompt>()
        .WithPrompts<HelpPrompt>()
        .WithResources<ResourceType>()
        .WithRequestFilters(filters =>
        {
            filters.AddCallToolFilter(McpToolLoggingFilter.Create(builder.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>()));
        });

    builder.Services.AddDbContext<DevStackDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        options.UseNpgsql(connectionString);
    });

    builder.Services.RegisterCommandHandlers();

    builder.Services.AddOpenTelemetry()
        .WithTracing(tracing =>
        {
            tracing
                .AddSource("DevStack.Mcp")
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation();

            var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
            if (!string.IsNullOrEmpty(otlpEndpoint))
            {
                tracing.AddOtlpExporter();
            }
        })
        .WithMetrics(metrics =>
        {
            metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation();

            var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
            if (!string.IsNullOrEmpty(otlpEndpoint))
            {
                metrics.AddOtlpExporter();
            }
        });

    builder.Services.AddProblemDetails();

    var app = builder.Build();

    app.UseExceptionHandler();

    app.MapMcp("/mcp");

    app.MapGet("/health", async (DevStackDbContext dbContext, CancellationToken ct) =>
    {
        try
        {
            await dbContext.Database.CanConnectAsync(ct);
            return Results.Ok(new { status = "healthy", database = "connected" });
        }
        catch
        {
            return Results.StatusCode(503);
        }
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "DevStack MCP Server terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
