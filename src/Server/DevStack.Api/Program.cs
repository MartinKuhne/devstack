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
        .AddSorting();

builder.Services.AddFeatureManagement();

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

app.Run();
