using DevStack.Domain.Entities;
using DevStack.Persistence;
using HotChocolate.Types;

namespace DevStack.Api.GraphQL.Types;

public class Query
{
    public Project? GetProjectById([Service] DevStackDbContext dbContext, Guid id)
    {
        return dbContext.Projects.Find(id);
    }

    public ProjectConnection GetProjects(
        [Service] DevStackDbContext dbContext,
        int first = 50,
        int? skip = null,
        string? search = null,
        string? orderBy = "id")
    {
        var query = dbContext.Projects.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
        }

        int totalCount = query.Count();

        IQueryable<Project> orderedQuery = (orderBy ?? "id").ToLower() switch
        {
            "name" => query.OrderBy(p => p.Name!),
            "repository" => query.OrderBy(p => p.Repository!),
            _ => query.OrderBy(p => p.Id)
        };

        if (skip.HasValue)
        {
            orderedQuery = orderedQuery.Skip(skip.Value);
        }
        var nodes = orderedQuery.Take(first).ToList();

        return new ProjectConnection
        {
            Nodes = nodes,
            PageInfo = new ProjectPageInfo
            {
                HasNextPage = (skip ?? 0) + nodes.Count < totalCount,
                HasPreviousPage = skip > 0,
                TotalCount = totalCount
            },
            TotalCount = totalCount
        };
    }

    public LargeLanguageModel? GetLargeLanguageModelById([Service] DevStackDbContext dbContext, Guid id)
    {
        return dbContext.LargeLanguageModels.Find(id);
    }

    public List<LargeLanguageModel> GetLargeLanguageModels(
        [Service] DevStackDbContext dbContext,
        string? search = null)
    {
        var query = dbContext.LargeLanguageModels.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(m => m.Model.Contains(search) || m.ModelAlias.Contains(search));
        }

        return query.OrderBy(m => m.Id).ToList();
    }

    public List<Deliverable> GetDeliverablesByProjectId(
        [Service] DevStackDbContext dbContext,
        Guid projectId,
        string? status = null,
        string? type = null)
    {
        var query = dbContext.Deliverables
            .Where(d => d.ProjectId == projectId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(d => d.Status.ToString() == status);
        }

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(d => d.Type.ToString() == type);
        }

        return query.OrderBy(d => d.Id).ToList();
    }

    public List<AgentTask> GetAgentTasksByDeliverableId(
        [Service] DevStackDbContext dbContext,
        Guid deliverableId,
        string? status = null)
    {
        var query = dbContext.AgentTasks
            .Where(t => t.DeliverableId == deliverableId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(t => t.Status.ToString() == status);
        }

        return query.OrderBy(t => t.Id).ToList();
    }

    public Deliverable? GetDeliverableById([Service] DevStackDbContext dbContext, Guid id)
    {
        return dbContext.Deliverables.Find(id);
    }

    public List<Deliverable> GetDeliverables(
        [Service] DevStackDbContext dbContext,
        string? status = null,
        string? type = null)
    {
        var query = dbContext.Deliverables.AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(d => d.Status.ToString() == status);
        }

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(d => d.Type.ToString() == type);
        }

        return query.OrderBy(d => d.Id).ToList();
    }

    public AgentTask? GetAgentTaskById([Service] DevStackDbContext dbContext, Guid id)
    {
        return dbContext.AgentTasks.Find(id);
    }

    public List<AgentTask> GetAgentTasks(
        [Service] DevStackDbContext dbContext,
        Guid? deliverableId = null,
        string? status = null)
    {
        var query = dbContext.AgentTasks.AsQueryable();

        if (deliverableId.HasValue)
        {
            query = query.Where(t => t.DeliverableId == deliverableId.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(t => t.Status.ToString() == status);
        }

        return query.OrderBy(t => t.Id).ToList();
    }
}
