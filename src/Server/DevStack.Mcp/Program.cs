using DevStack.Mcp.Middlewares;
using DevStack.Mcp.Logging;
using DevStack.Infrastructure.Projects;
using DevStack.Infrastructure.Features;
using DevStack.Infrastructure.Defects;
using DevStack.Infrastructure.Tasks;
using DevStack.Infrastructure.Epics;
using DevStack.Domain.Services;
using DevStack.Infrastructure.WorkflowRuns;
using DevStack.Infrastructure.ModelConfigurations;
using DevStack.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.WithProperty("Application", "DevStack.Mcp")
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
builder.Services.AddTransient<ICreateDefectHandler, CreateDefectHandler>();
builder.Services.AddTransient<IUpdateDefectHandler, UpdateDefectHandler>();
builder.Services.AddTransient<ITransitionDefectStatusHandler, TransitionDefectStatusHandler>();
builder.Services.AddTransient<IDeleteDefectHandler, DeleteDefectHandler>();
var limitFeatureStatusTransitions = builder.Configuration
    .GetSection("FeatureManagement")
    .GetValue<bool>("LimitFeatureStatusTransitions");
builder.Services.AddTransient<ItemStatusTransitionService>(_ => new ItemStatusTransitionService(limitFeatureStatusTransitions));
builder.Services.AddTransient<ICreateTaskHandler, CreateTaskHandler>();
builder.Services.AddTransient<IUpdateTaskHandler, UpdateTaskHandler>();
builder.Services.AddTransient<ITransitionTaskStatusHandler, TransitionTaskStatusHandler>();
builder.Services.AddTransient<IDeleteTaskHandler, DeleteTaskHandler>();
builder.Services.AddTransient<ICreateLargeLanguageModelHandler, CreateLargeLanguageModelHandler>();
builder.Services.AddTransient<IUpdateLargeLanguageModelHandler, UpdateLargeLanguageModelHandler>();
builder.Services.AddTransient<IDeleteLargeLanguageModelHandler, DeleteLargeLanguageModelHandler>();
builder.Services.AddTransient<ICreateWorkflowRunHandler, CreateWorkflowRunHandler>();
builder.Services.AddTransient<IUpdateWorkflowRunHandler, UpdateWorkflowRunHandler>();
builder.Services.AddTransient<ICancelWorkflowRunHandler, CancelWorkflowRunHandler>();
builder.Services.AddTransient<ICreateEpicHandler, CreateEpicHandler>();
builder.Services.AddTransient<IUpdateEpicHandler, UpdateEpicHandler>();
builder.Services.AddScoped<DevStack.Mcp.DevStackTools>();

var secretKey = builder.Configuration["DEVSTACK_SECRET_KEY"] 
    ?? Environment.GetEnvironmentVariable("DEVSTACK_SECRET_KEY") 
    ?? throw new InvalidOperationException("DEVSTACK_SECRET_KEY must be set");
builder.Services.AddSingleton<DevStack.Infrastructure.Services.ISecretService>(new DevStack.Infrastructure.Services.AesSecretService(secretKey));

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

var useCustomMcpHandler = builder.Configuration
    .GetSection("FeatureManagement")
    .GetValue<bool>("UseCustomMcpHandler");

if (useCustomMcpHandler)
{
    builder.Services.AddScoped<DevStack.Mcp.IMcpMethodHandler, DevStack.Mcp.McpMethodHandler>();
    builder.Services.AddScoped<DevStack.Mcp.JsonRpcMcpEndpointHandler>();
}
else
{
    builder.Services.AddMcpServer()
        .WithHttpTransport(options => options.Stateless = true)
        .WithTools<DevStack.Mcp.DevStackTools>();
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

app.MapGet("/", () => Results.Ok(new { status = "ok", timestamp = DateTime.UtcNow }))
    .WithName("Ping");

if (useCustomMcpHandler)
{
    app.MapPost("/mcp", async (HttpContext context, DevStack.Mcp.JsonRpcMcpEndpointHandler handler) =>
        await handler.HandleMcpRequestAsync(context))
        .WithName("MCP_JsonRpc")
        .Produces(StatusCodes.Status200OK);

    app.MapGet("/mcp", DevStack.Mcp.JsonRpcMcpEndpointHandler.HandleSseStreamAsync)
        .WithName("MCP_SSE")
        .ExcludeFromDescription();
}
else
{
    app.MapMcp("/mcp");
}

app.Run();

public partial class Program { }