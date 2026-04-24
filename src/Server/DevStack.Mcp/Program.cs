using DevStack.Application;
using DevStack.Application.AgentTasks;
using DevStack.Application.Deliverables;
using DevStack.Application.Deliverables.Commands;
using DevStack.Application.Deliverables.Queries;
using DevStack.Application.Projects.Commands;
using DevStack.Application.Projects.Queries;
using DevStack.Domain.Services;
using DevStack.Infrastructure.AgentTasks;
using DevStack.Infrastructure.Deliverables;
using DevStack.Infrastructure.ModelConfigurations;
using DevStack.Infrastructure.Projects;
using DevStack.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ModelContextProtocol.Server;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting DevStack MCP Server");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog();

    builder.Services.AddMcpServer()
        .WithHttpTransport(options =>
        {
            options.Stateless = true;
        })
        .WithToolsFromAssembly();

    builder.Services.AddDbContext<DevStackDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        options.UseNpgsql(connectionString);
    });

    builder.Services.AddScoped<ICommandHandler<Guid, CreateProjectCommand>, CreateProjectHandler>();
    builder.Services.AddScoped<ICommandHandler<UpdateProjectCommand>, UpdateProjectHandler>();
    builder.Services.AddScoped<ICommandHandler<DeleteProjectCommand>, DeleteProjectHandler>();
    builder.Services.AddScoped<IGetProjectByIdHandler, GetProjectByIdHandler>();
    builder.Services.AddScoped<ICreateLargeLanguageModelHandler, CreateLargeLanguageModelHandler>();
    builder.Services.AddScoped<IUpdateLargeLanguageModelHandler, UpdateLargeLanguageModelHandler>();
    builder.Services.AddScoped<IDeleteLargeLanguageModelHandler, DeleteLargeLanguageModelHandler>();
    builder.Services.AddScoped<ICommandHandler<Guid, CreateDeliverableCommand>, CreateDeliverableHandler>();
    builder.Services.AddScoped<ICommandHandler<UpdateDeliverableCommand>, UpdateDeliverableHandler>();
    builder.Services.AddScoped<ICommandHandler<UpdateDeliverableStatusCommand>, UpdateDeliverableStatusHandler>();
    builder.Services.AddScoped<ICommandHandler<DeleteDeliverableCommand>, DeleteDeliverableHandler>();
    builder.Services.AddScoped<IGetDeliverableByIdHandler, GetDeliverableByIdHandler>();
    builder.Services.AddScoped<DeliverableStatusTransitionService>();
    builder.Services.AddScoped<ICreateAgentTaskHandler, CreateAgentTaskHandler>();
    builder.Services.AddScoped<IUpdateAgentTaskHandler, UpdateAgentTaskHandler>();
    builder.Services.AddScoped<IUpdateAgentTaskStatusHandler, UpdateAgentTaskStatusHandler>();
    builder.Services.AddScoped<IDeleteAgentTaskHandler, DeleteAgentTaskHandler>();
    builder.Services.AddScoped<IGetAgentTaskByIdHandler, GetAgentTaskByIdHandler>();
    builder.Services.AddScoped<AgentTaskStatusTransitionService>();

    var app = builder.Build();

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
