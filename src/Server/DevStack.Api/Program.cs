using DevStack.Api.GraphQL;
using Microsoft.FeatureManagement;
using DevStack.Api.GraphQL.Types;
using DevStack.Domain.Services;
using DevStack.Infrastructure.Projects;
using DevStack.Infrastructure.Features;
using DevStack.Infrastructure.Defects;
using DevStack.Infrastructure.Tasks;
using DevStack.Infrastructure.Epics;
using DevStack.Infrastructure.ModelConfigurations;
using DevStack.Infrastructure.WorkflowRuns;
using DevStack.Infrastructure.Persistence;
using DevStack.Infrastructure.Services;
using DevStack.Api.Logging;
using DevStack.Api.Middlewares;
using DevStack.Api.Mcp;
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
        .Destructure.With(new SensitiveDataDestructuringPolicy())
        .WriteTo.Console();
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
builder.Services.AddTransient<ICreateFeatureHandler, CreateFeatureHandler>();
builder.Services.AddTransient<IUpdateFeatureHandler, UpdateFeatureHandler>();
builder.Services.AddTransient<ITransitionFeatureStatusHandler, TransitionFeatureStatusHandler>();
builder.Services.AddTransient<IDeleteFeatureHandler, DeleteFeatureHandler>();
builder.Services.AddTransient<FeatureStatusTransitionService>();
builder.Services.AddTransient<ICreateDefectHandler, CreateDefectHandler>();
builder.Services.AddTransient<IUpdateDefectHandler, UpdateDefectHandler>();
builder.Services.AddTransient<ITransitionDefectStatusHandler, TransitionDefectStatusHandler>();
builder.Services.AddTransient<IDeleteDefectHandler, DeleteDefectHandler>();
builder.Services.AddTransient<ICreateTaskHandler, CreateTaskHandler>();
builder.Services.AddTransient<IUpdateTaskHandler, UpdateTaskHandler>();
builder.Services.AddTransient<ITransitionTaskStatusHandler, TransitionTaskStatusHandler>();
builder.Services.AddTransient<IDeleteTaskHandler, DeleteTaskHandler>();
builder.Services.AddTransient<ICreateModelConfigurationHandler, CreateModelConfigurationHandler>();
builder.Services.AddTransient<IUpdateModelConfigurationHandler, UpdateModelConfigurationHandler>();
builder.Services.AddTransient<IDeleteModelConfigurationHandler, DeleteModelConfigurationHandler>();
builder.Services.AddTransient<ICreateWorkflowRunHandler, CreateWorkflowRunHandler>();
builder.Services.AddTransient<IUpdateWorkflowRunHandler, UpdateWorkflowRunHandler>();
builder.Services.AddTransient<ICancelWorkflowRunHandler, CancelWorkflowRunHandler>();
builder.Services.AddTransient<ICreateEpicHandler, CreateEpicHandler>();
builder.Services.AddTransient<IUpdateEpicHandler, UpdateEpicHandler>();
builder.Services.AddScoped<DevStackTools>();

var secretKey = builder.Configuration["DEVSTACK_SECRET_KEY"] 
    ?? Environment.GetEnvironmentVariable("DEVSTACK_SECRET_KEY") 
    ?? throw new InvalidOperationException("DEVSTACK_SECRET_KEY must be set");
builder.Services.AddSingleton<ISecretService>(new AesSecretService(secretKey));

var isDevelopment = builder.Environment.IsDevelopment();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();
        
        if (isDevelopment)
        {
            tracing.AddConsoleExporter();
        }
        
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
        
        if (isDevelopment)
        {
            metrics.AddConsoleExporter();
        }
        
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
    .AddType<FeatureType>()
    .AddType<DefectType>()
    .AddType<TaskType>()
    .AddType<ModelConfigurationType>()
    .AddType<WorkflowRunType>()
    .AddType<AuditEventType>()
    .AddType<DashboardSummary>()
    .AddObjectType<ProjectConnection>()
    .AddObjectType<FeatureConnection>()
    .AddObjectType<DefectConnection>()
    .AddObjectType<TaskConnection>()
    .AddObjectType<EpicConnection>()
    .AddObjectType<ProjectPageInfo>()
    .AddObjectType<FeaturePageInfo>()
    .AddObjectType<DefectPageInfo>()
    .AddObjectType<TaskPageInfo>()
    .AddObjectType<EpicPageInfo>()
    .DisableIntrospection(false)
    .AddErrorFilter<GraphQLErrorFilter>();

builder.Services.AddFeatureManagement();

var useCustomMcpHandler = builder.Configuration
    .GetSection("FeatureManagement")
    .GetValue<bool>("UseCustomMcpHandler");

if (useCustomMcpHandler)
{
    builder.Services.AddScoped<IMcpMethodHandler, McpMethodHandler>();
    builder.Services.AddScoped<JsonRpcMcpEndpointHandler>();
}
else
{
    builder.Services.AddMcpServer()
        .WithTools<DevStackTools>();
}



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

if (useCustomMcpHandler)
{
    app.MapPost("/mcp", async (HttpContext context, JsonRpcMcpEndpointHandler handler) =>
        await handler.HandleMcpRequestAsync(context))
        .WithName("MCP_JsonRpc")
        .Produces(StatusCodes.Status200OK);

    app.MapGet("/mcp", JsonRpcMcpEndpointHandler.HandleSseStreamAsync)
        .WithName("MCP_SSE")
        .ExcludeFromDescription();
}
else
{
    app.MapMcp("/mcp");
}

app.MapGraphQL("/graphql");

app.Run();

public partial class Program { }
