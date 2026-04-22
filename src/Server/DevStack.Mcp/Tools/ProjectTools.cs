using DevStack.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace DevStack.Mcp.Tools;

[McpServerToolType]
public class ProjectTools
{
    private readonly ILogger<ProjectTools> _logger;
    private readonly DevStackDbContext _dbContext;

    public ProjectTools(ILogger<ProjectTools> logger, DevStackDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [McpServerTool(Name = "get_projects"), Description("Read all projects from DevStack. Returns project name, id, and repository. Usage hint: Call this first to get a list of available projects before performing other operations.")]
    public async Task<string> GetProjects(CancellationToken ct = default)
    {
        var projects = await _dbContext.Projects.Select(p => new { p.Name, p.Id, p.Repository }).ToListAsync(ct);
        return FormatMarkdownTable(projects, "Projects");
    }

    [McpServerTool(Name = "get_project"), Description("Read a project by its ID. Returns project name and repository. Usage hint: Provide a valid project ID obtained from get_projects.")]
    public async Task<string> GetProjectById([Description("The project ID")][DefaultValue(null)] Guid? id, CancellationToken ct = default)
    {
        if (id == null)
        {
            throw new ArgumentException("Project ID is required");
        }

        var project = await _dbContext.Projects.Where(p => p.Id == id.Value).Select(p => new { p.Name, p.Id, p.Repository }).FirstOrDefaultAsync(ct);
        if (project == null)
        {
            throw new KeyNotFoundException($"Project with ID {id.Value} not found");
        }

        return FormatMarkdownTable(new[] { project }, "Project");
    }

    private string FormatMarkdownTable<T>(IEnumerable<T> items, string title)
    {
        var json = JsonSerializer.Serialize(items);
        return $"## {title}\n\n```json\n{json}\n```\n\n";
    }
}
