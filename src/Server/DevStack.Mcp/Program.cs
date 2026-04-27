using DevStack.Application;
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
        .WithToolsFromAssembly()
        .WithPrompts<Prompts>()
        .WithResources<ResourceType>();

    builder.Services.AddDbContext<DevStackDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        options.UseNpgsql(connectionString);
    });

    builder.Services.RegisterCommandHandlers();

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
