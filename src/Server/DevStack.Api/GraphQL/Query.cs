using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Persistence;
using HotChocolate.Types;

namespace DevStack.Api.GraphQL.Types;

public class Query
{
    public Project? GetProjectById([Service] DevStackDbContext dbContext, Guid id)
    {
        return dbContext.Projects.Find(id);
    }

    public EntityConnection<Project> GetProjects(
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

        return new EntityConnection<Project>
        {
            Nodes = nodes,
            PageInfo = new PageInfo
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

    public EntityConnection<LargeLanguageModel> GetLargeLanguageModels(
        [Service] DevStackDbContext dbContext,
        int first = 50,
        int? skip = null,
        string? search = null,
        string? orderBy = "id")
    {
        var query = dbContext.LargeLanguageModels.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(m => m.Model.Contains(search) || m.ModelAlias.Contains(search) || m.Url.Contains(search));
        }

        int totalCount = query.Count();

        IQueryable<LargeLanguageModel> orderedQuery = (orderBy ?? "id").ToLower() switch
        {
            "model" => query.OrderBy(m => m.Model),
            "url" => query.OrderBy(m => m.Url),
            _ => query.OrderBy(m => m.Id)
        };

        if (skip.HasValue)
        {
            orderedQuery = orderedQuery.Skip(skip.Value);
        }
        var nodes = orderedQuery.Take(first).ToList();

        return new EntityConnection<LargeLanguageModel>
        {
            Nodes = nodes,
            PageInfo = new PageInfo
            {
                HasNextPage = (skip ?? 0) + nodes.Count < totalCount,
                HasPreviousPage = skip > 0,
                TotalCount = totalCount
            },
            TotalCount = totalCount
        };
    }

    public List<Deliverable> GetDeliverablesByProjectId(
        [Service] DevStackDbContext dbContext,
        Guid projectId,
        string? status = null,
        string? type = null,
        int? skip = null,
        int first = 50,
        string? orderBy = "id")
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

        IQueryable<Deliverable> orderedQuery = (orderBy ?? "id").ToLower() switch
        {
            "title" => query.OrderBy(d => d.Title),
            "status" => query.OrderBy(d => d.Status),
            "type" => query.OrderBy(d => d.Type),
            _ => query.OrderBy(d => d.Id)
        };

        if (skip.HasValue)
        {
            orderedQuery = orderedQuery.Skip(skip.Value);
        }

        return orderedQuery.Take(first).ToList();
    }

    public EntityConnection<Deliverable> GetDeliverables(
        [Service] DevStackDbContext dbContext,
        string? status = null,
        string? type = null,
        int? skip = null,
        int first = 50,
        string? orderBy = "id")
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

        int totalCount = query.Count();

        IQueryable<Deliverable> orderedQuery = (orderBy ?? "id").ToLower() switch
        {
            "title" => query.OrderBy(d => d.Title),
            "status" => query.OrderBy(d => d.Status),
            "type" => query.OrderBy(d => d.Type),
            _ => query.OrderBy(d => d.Id)
        };

        if (skip.HasValue)
        {
            orderedQuery = orderedQuery.Skip(skip.Value);
        }
        var nodes = orderedQuery.Take(first).ToList();

        return new EntityConnection<Deliverable>
        {
            Nodes = nodes,
            PageInfo = new PageInfo
            {
                HasNextPage = (skip ?? 0) + nodes.Count < totalCount,
                HasPreviousPage = skip > 0,
                TotalCount = totalCount
            },
            TotalCount = totalCount
        };
    }

    public List<AgentTask> GetAgentTasksByDeliverableId(
        [Service] DevStackDbContext dbContext,
        Guid deliverableId,
        string? status = null,
        int? skip = null,
        int first = 50,
        string? orderBy = "id")
    {
        var query = dbContext.AgentTasks
            .Where(t => t.DeliverableId == deliverableId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(t => t.Status.ToString() == status);
        }

        IQueryable<AgentTask> orderedQuery = (orderBy ?? "id").ToLower() switch
        {
            "title" => query.OrderBy(t => t.Title),
            "status" => query.OrderBy(t => t.Status),
            "complexity" => query.OrderBy(t => t.ComplexityRating),
            _ => query.OrderBy(t => t.Id)
        };

        if (skip.HasValue)
        {
            orderedQuery = orderedQuery.Skip(skip.Value);
        }

        return orderedQuery.Take(first).ToList();
    }

    public Deliverable? GetDeliverableById([Service] DevStackDbContext dbContext, Guid id)
    {
        return dbContext.Deliverables.Find(id);
    }

    public AgentTask? GetAgentTaskById([Service] DevStackDbContext dbContext, Guid id)
    {
        return dbContext.AgentTasks.Find(id);
    }

    public EntityConnection<AgentTask> GetAgentTasks(
        [Service] DevStackDbContext dbContext,
        Guid? deliverableId = null,
        string? status = null,
        int? skip = null,
        int first = 50,
        string? orderBy = "id")
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

        int totalCount = query.Count();

        IQueryable<AgentTask> orderedQuery = (orderBy ?? "id").ToLower() switch
        {
            "title" => query.OrderBy(t => t.Title),
            "status" => query.OrderBy(t => t.Status),
            "complexity" => query.OrderBy(t => t.ComplexityRating),
            _ => query.OrderBy(t => t.Id)
        };

        if (skip.HasValue)
        {
            orderedQuery = orderedQuery.Skip(skip.Value);
        }
        var nodes = orderedQuery.Take(first).ToList();

        return new EntityConnection<AgentTask>
        {
            Nodes = nodes,
            PageInfo = new PageInfo
            {
                HasNextPage = (skip ?? 0) + nodes.Count < totalCount,
                HasPreviousPage = skip > 0,
                TotalCount = totalCount
            },
            TotalCount = totalCount
        };
    }
}
