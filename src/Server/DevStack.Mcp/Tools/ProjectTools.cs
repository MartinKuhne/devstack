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

    [McpServerTool(Name = "get_projects"), Description(Descriptions.ProjectTools.GetProjects)]
    public async Task<string> GetProjects(CancellationToken ct = default)
    {
        var projects = await _dbContext.Projects
            .Select(p => new ProjectDto(p.Id.ToString(), p.Name, null, p.Repository))
            .ToListAsync(ct);

        return ToolResponse.Success("Projects", new ProjectListResponse(projects));
    }

    [McpServerTool(Name = "get_project"), Description(Descriptions.ProjectTools.GetProject)]
    public async Task<string> GetProjectById([Description(Descriptions.ProjectTools.Id)][DefaultValue(null)] Guid? id, CancellationToken ct = default)
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

        return ToolResponse.Success("Project", new GetProjectResponse(project.Id, project.Name, project.Description, project.Repository));
    }

    [McpServerTool(Name = "create_project"), Description(Descriptions.ProjectTools.CreateProject)]
    public async Task<string> CreateProject(
        [Description(Descriptions.ProjectTools.Name)] string name,
        [Description(Descriptions.ProjectTools.Repository)] string repository,
        [Description(Descriptions.ProjectTools.Description)][DefaultValue(null)] string? description,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new McpProtocolException("Project name is required", McpErrorCode.InvalidParams);
        }

        if (string.IsNullOrWhiteSpace(repository))
        {
            throw new McpProtocolException("Repository is required", McpErrorCode.InvalidParams);
        }

        var id = await _createProjectHandler.Handle(
            new CreateProjectCommand(name, description, repository),
            ct);

        _logger.LogInformation("Created project with ID: {Id}", id);
        return ToolResponse.Success("Project Created",
            new CreateProjectResponse(id.ToString(), name, description, repository),
            Descriptions.ProjectTools.CreateUsageHint);
    }
}
