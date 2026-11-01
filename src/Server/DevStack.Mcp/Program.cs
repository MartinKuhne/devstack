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
using DevStack.Mcp.Tools;

using ModelContextProtocol.Server;

using Npgsql;

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

    builder.Services.AddFeatureManagement();

    var mcpBuilder = builder.Services.AddMcpServer()
        .WithHttpTransport(options =>
        {
            options.Stateless = true;
        });

    mcpBuilder.WithTools<ProjectTools>(null);
    mcpBuilder.WithTools<DeliverableTools>(null);

    if (builder.Configuration.GetValue<bool>("FeatureManagement:AgentTaskTools"))
    {
        mcpBuilder.WithTools<TaskTools>(null);
    }

    mcpBuilder
        .WithPrompts<DeliverableWorkflowPrompt>(null)
        .WithResources<ResourceType>()
        .WithRequestFilters(filters =>
        {
            filters.AddCallToolFilter(McpArgumentNormalizationFilter.Create());
            filters.AddCallToolFilter(McpToolLoggingFilter.Create());
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
                .AddSource("Npgsql")
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

    // BUG-05 — this middleware never sees a tool failure, so tool errors are missing from error logging.
    // Category:        observability / error-contract
    // Trigger path:    tools/call for a tool whose method throws (e.g. get_task with an unknown id)
    //                  -> the MCP SDK's tool-invocation pipeline catches the exception INSIDE the request
    //                     handler and converts it: McpProtocolException becomes a JSON-RPC error and any other
    //                     exception becomes a CallToolResult with IsError = true and the text
    //                     "An error occurred invoking '<tool>'."
    //                  -> the HTTP response is therefore a 200 with a well-formed JSON-RPC body
    //                  -> _next(context) returns normally, so neither catch block in
    //                     McpExceptionHandlingMiddleware (:24 McpProtocolException, :41 Exception) runs
    // Why this is a real bug: the middleware is the only component that logs exceptions with the request
    //                  method/path correlation (McpExceptionHandlingMiddleware.cs:26 and :49), so the failures
    //                  an operator most needs to see - a tool throwing, an argument that cannot bind - never
    //                  reach it. The catch-all at :41 effectively only fires for faults raised by the transport
    //                  or by middleware itself, which makes the component look like it works while it is blind
    //                  to the real failure population.
    // Why this is NOT a false positive: the observed client outcomes are the proof - every tool error in the
    //                  integration suite arrives as an HTTP 200 with IsError set, i.e. nothing propagates to the
    //                  ASP.NET pipeline. The generic message the client receives is generated by the SDK, not by
    //                  this middleware, which is why it does not name the exception. Note that the MCP host also
    //                  registers app.UseExceptionHandler() on the NEXT line, which would swallow the same
    //                  exceptions again if they ever did escape.
    // Concrete consequence: tool failures are invisible in the error log; an operator investigating "every tool
    //                  call fails" sees no corresponding entries and cannot correlate a failure with a request.
    // Confidence:      Confirmed (the SDK conversion is the documented and observed behaviour).
    // Fix direction:   log at the place the failures actually surface - a call-tool filter that inspects the
    //                  result's IsError and logs it - and keep this middleware only for transport-level faults,
    //                  or delete it so the guarantee it implies is not falsely advertised.
    // Pinned by:       not observable from an MCP client, so no test pins it. Recorded in
    //                  doc/planning/integration-test-coverage-ledger.md §6 as BUG-05; the observable outcomes it
    //                  explains are pinned by DevStack.Tests.Integration.MCP Features/ToolErrorPaths.feature.
    app.UseMiddleware<McpExceptionHandlingMiddleware>();
    app.UseExceptionHandler();

    app.MapMcp("/mcp");

    // BUG-08 — this host never runs EF migrations, and /health cannot tell an unmigrated database from a good one.
    // Category:        deployment / error-contract (unactionable failure)
    // Trigger path:    start DevStack.Mcp against a Postgres instance whose schema has never been migrated
    //                  -> Program.cs has no Database.MigrateAsync() anywhere; the API host does
    //                     (DevStack.Api/Program.cs:125-129), so the two hosts have different bootstrap contracts
    //                  -> the host starts and /health below runs dbContext.Database.CanConnectAsync, which only
    //                     opens a connection and does not touch any table -> 200 { status: "healthy" }
    //                  -> every DB-backed tool then throws (42P01 relation does not exist), the MCP SDK converts
    //                     it into a CallToolResult with IsError = true and the text
    //                     "An error occurred invoking '<tool>'."
    // Why this is a real bug: the readiness probe reports healthy while the service cannot serve a single
    //                  meaningful request, so an orchestrator keeps the instance in rotation, and the generic
    //                  tool error gives the operator no hint that the schema is missing - it is indistinguishable
    //                  from a bug in the tool.
    // Why this is NOT a false positive: CanConnectAsync is a connectivity check only; it succeeds against an
    //                  empty database with no DevStack tables. This was observed when the feature-flag-off
    //                  integration environment used the standalone host and had to migrate explicitly. The
    //                  current MCP integration suite uses the API host, which migrates on startup.
    // Concrete consequence: a deployment that points the standalone MCP host at a fresh database comes up
    //                  "healthy" and fails every call; the operator sees tool errors, not a schema problem.
    // Confidence:      Confirmed.
    // Fix direction:   either run MigrateAsync at startup like the API host (same scope pattern), or make the
    //                  probe actually verify the schema - e.g. query one mapped table - and return 503 when it is
    //                  missing. Decide which host owns migrations so they are not run twice from two services.
    // Historical test evidence: the feature-flag-off environment previously migrated explicitly before starting
    //                  the standalone host. The current API-only integration suite no longer covers this host.
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
