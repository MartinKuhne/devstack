using DevStack.Api.GraphQL;
using Microsoft.FeatureManagement;
using DevStack.Api.GraphQL.Types;
using DevStack.Domain.Services;
using DevStack.Infrastructure.Projects;
using DevStack.Infrastructure.ModelConfigurations;
using DevStack.Persistence;
using DevStack.Infrastructure.Services;
using DevStack.Api.Logging;
using DevStack.Api.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Wolverine;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

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

builder.Host.UseWolverine();

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
    .AddCheck("ready", () => HealthCheckResult.Healthy(), tags: ["ready"]);

builder.Services.AddRouting();

builder.Services.AddDbContext<DevStackDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddTransient<ICreateProjectHandler, CreateProjectHandler>();
builder.Services.AddTransient<IUpdateProjectHandler, UpdateProjectHandler>();
builder.Services.AddTransient<IDeleteProjectHandler, DeleteProjectHandler>();
builder.Services.AddTransient<IGetProjectByIdHandler, GetProjectByIdHandler>();
var limitFeatureStatusTransitions = builder.Configuration
    .GetSection("FeatureManagement")
    .GetValue<bool>("LimitFeatureStatusTransitions");
builder.Services.AddTransient<ItemStatusTransitionService>(_ => new ItemStatusTransitionService(limitFeatureStatusTransitions));
builder.Services.AddTransient<ICreateLargeLanguageModelHandler, CreateLargeLanguageModelHandler>();
builder.Services.AddTransient<IUpdateLargeLanguageModelHandler, UpdateLargeLanguageModelHandler>();
builder.Services.AddTransient<IDeleteLargeLanguageModelHandler, DeleteLargeLanguageModelHandler>();

var secretKey = builder.Configuration["DEVSTACK_SECRET_KEY"] 
    ?? Environment.GetEnvironmentVariable("DEVSTACK_SECRET_KEY") 
    ?? throw new InvalidOperationException("DEVSTACK_SECRET_KEY must be set");
builder.Services.AddSingleton<ISecretService>(new AesSecretService(secretKey));

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
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

builder.Services.AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddType<ProjectType>()
    .AddType<ItemType>()
    .AddType<LargeLanguageModelType>()
    .AddType<DashboardSummary>()
    .AddObjectType<ProjectConnection>()
    .AddObjectType<ItemConnection>()
    .AddObjectType<ProjectPageInfo>()
    .AddObjectType<ItemPageInfo>()
    .DisableIntrospection(false)
    .AddErrorFilter<GraphQLErrorFilter>();

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
    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        if (ex != null) return Serilog.Events.LogEventLevel.Error;
        if (httpContext.Response.StatusCode > 500) return Serilog.Events.LogEventLevel.Error;
        if (httpContext.Response.StatusCode > 400 && httpContext.Response.StatusCode < 500) return Serilog.Events.LogEventLevel.Warning;
        return Serilog.Events.LogEventLevel.Information;
    };
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? "");
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme ?? "");
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
        diagnosticContext.Set("ContentType", httpContext.Request.ContentType ?? "");
    };
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

public partial class Program { }