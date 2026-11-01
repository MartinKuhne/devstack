using DevStack.Api.GraphQL;
using DevStack.Api.GraphQL.Types;
using DevStack.Api.HealthChecks;
using DevStack.Api.Logging;
using DevStack.Api.Middlewares;
using DevStack.Application.AgentTasks.Commands;
using DevStack.Application.LargeLanguageModels.Commands;
using DevStack.Application.Projects.Commands;
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

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.WithProperty("Application", "DevStack")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Destructure.With(new SensitiveDataDestructuringPolicy());
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
    .AddCheck("ready", () => HealthCheckResult.Healthy(), tags: ["ready"])
    .AddCheck<DatabaseHealthCheck>("db", tags: ["live", "ready"]);

builder.Services.AddRouting();

builder.Services.AddDbContext<DevStackDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.RegisterCommandHandlers();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource("Npgsql");

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

builder.Services.AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .DisableIntrospection(false)
    .AddErrorFilter<GraphQLErrorFilter>()
        .AddFiltering()
        .AddSorting()
        .ModifyCostOptions(options => options.MaxFieldCost = 10000);

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

// BUG-09 — this host registers the same MCP tools as DevStack.Mcp but without the argument-normalization filter,
// so the two hosts answer the same request differently.
// Category:        contract divergence between two deployments of the same tool surface
// Trigger path:    a client sends tools/call with an array where the tool declares a string, e.g.
//                  create_project(name: ["alpha","beta"])
//                  -> DevStack.Mcp/Program.cs:76-81 registers McpArgumentNormalizationFilter before the logging
//                     filter, so the array is coerced to "alpha\nbeta" and the call binds and SUCCEEDS
//                  -> this pipeline (:118-121) registers only McpToolLoggingFilter, so argument binding fails
//                  -> the MCP SDK returns a CallToolResult with IsError == true and the generic text
//                     "An error occurred invoking '<tool>'."
// Why this is a real bug: both hosts expose the identical tool list (ProjectTools, DeliverableTools, TaskTools,
//                  the same prompt and resources), so clients reasonably treat them as interchangeable, yet the
//                  same document produces success on one and a generic error on the other. The API host already
//                  references the project that contains the filter, so the omission has no technical cause.
// Why this is NOT a false positive: the difference was observed from a client when the integration suite drove
//                  both endpoints with the same array argument. The current integration gate intentionally runs
//                  MCP scenarios against this API-hosted endpoint only, so the standalone comparison is no longer
//                  part of the test coverage.
// Concrete consequence: an agent that works against one host and is repointed at the other starts failing calls
//                  it used to pass, with an error message that names nothing; and the generic message hides the
//                  real cause (an argument-shape mismatch) from the operator.
// Confidence:      Confirmed.
// Fix direction:   either add filters.AddCallToolFilter(McpArgumentNormalizationFilter.Create(...)) here so both
//                  hosts behave alike, or decide that coercion is wrong and remove it from the standalone host -
//                  then document which host is authoritative. Product-owner decision recorded as BUG-09.
// Historical test: DevStack.Tests.Integration.MCP previously pinned the cross-host difference; the comparison was
//                  removed when the integration suite moved to the API-hosted endpoint only.
mcpBuilder
    .WithPrompts<DeliverableWorkflowPrompt>(null)
    .WithResources<ResourceType>()
    .WithRequestFilters(filters =>
    {
        filters.AddCallToolFilter(McpToolLoggingFilter.Create());
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DevStackDbContext>();
    await db.Database.MigrateAsync();
}

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = DevStack.Api.Logging.LogLevelResolver.ResolveLevel;
    options.EnrichDiagnosticContext = DevStack.Api.Logging.RequestEnricher.EnrichDiagnosticContext;
});

app.UseRouting();

app.UseMiddleware<McpExceptionHandlingMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors("AllowAll");

app.MapGet("/", () => Results.Ok(new { status = "ok", timestamp = DateTime.UtcNow }))
    .WithName("Ping")
    .WithTags("Health");

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(
            new { status = report.Status.ToString(), timestamp = DateTime.UtcNow },
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase }));
    }
}).WithTags("Health");

app.MapHealthChecks("/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(
            new { status = report.Status.ToString(), timestamp = DateTime.UtcNow },
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase }));
    }
}).WithTags("Health");

app.MapGraphQL("/graphql");
app.MapMcp("/mcp");

app.Run();
