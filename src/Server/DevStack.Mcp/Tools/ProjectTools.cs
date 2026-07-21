using DevStack.Application.Projects.Commands;
using DevStack.Mcp.Dto;

using ModelContextProtocol;

namespace DevStack.Mcp.Tools;

[McpServerToolType]
public class ProjectTools
{
    private readonly ILogger<ProjectTools> _logger;
    private readonly DevStackDbContext _dbContext;
    private readonly ICommandHandler<Guid, CreateProjectCommand> _createProjectHandler;

    public ProjectTools(ILogger<ProjectTools> logger, DevStackDbContext dbContext, ICommandHandler<Guid, CreateProjectCommand> createProjectHandler)
    {
        _logger = logger;
        _dbContext = dbContext;
        _createProjectHandler = createProjectHandler;
    }

    [McpServerTool(Name = "get_projects"), Description("Read all projects from DevStack. Returns project name, id, and repository. Usage hint: Call this first to get a list of available projects before performing other operations.")]
    public async Task<string> GetProjects(CancellationToken ct = default)
    {
        var projects = await _dbContext.Projects
            .Select(p => new ProjectDto(p.Id.ToString(), p.Name, null, p.Repository))
            .ToListAsync(ct);

        return ToolResponse.Success("Projects", new ProjectListResponse(projects));
    }

    [McpServerTool(Name = "get_project"), Description("Read a project by its ID. Returns project name and repository. Usage hint: Provide a valid project ID obtained from get_projects.")]
    public async Task<string> GetProjectById([Description("The project ID")][DefaultValue(null)] Guid? id, CancellationToken ct = default)
    {
        if (id == null)
        {
            throw new McpProtocolException("Project ID is required", McpErrorCode.InvalidParams);
        }

        var project = await _dbContext.Projects
            .Where(p => p.Id == id.Value)
            .Select(p => new ProjectDto(p.Id.ToString(), p.Name, null, p.Repository))
            .FirstOrDefaultAsync(ct);

        if (project == null)
        {
            throw new McpProtocolException($"Project with ID {id.Value} not found", McpErrorCode.InvalidParams);
        }

        return ToolResponse.Success("Project", project);
    }

    [McpServerTool(Name = "create_project"), Description("Create a new project in DevStack. Usage hint: Name and repository are required fields.")]
    public async Task<string> CreateProject(
        [Description("The project name")] string name,
        [Description("The repository URL")] string repository,
        [Description("The project description")][DefaultValue(null)] string? description,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new McpProtocolException("Project name is required", McpErrorCode.InvalidParams);
        }

        var id = await _createProjectHandler.Handle(
            new CreateProjectCommand(name, description, repository),
            ct);

        _logger.LogInformation("Created project with ID: {Id}", id);
        return ToolResponse.Success("Project Created",
            new CreateProjectResponse(id.ToString(), name, description, repository),
            "Use the returned ID for subsequent get_project, create_deliverable, or update operations.");
    }
}
