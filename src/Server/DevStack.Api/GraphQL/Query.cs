using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Domain.Services;
using DevStack.Infrastructure.Persistence;
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
        int? skip = null)
    {
        var query = dbContext.Projects.AsQueryable();
        var totalCount = query.Count();
        query = query.OrderBy(p => p.CreatedAt);
        if (skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }
        var nodes = query.Take(first).ToList();

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

    public ItemConnection GetItems(
        [Service] DevStackDbContext dbContext,
        Guid? projectId = null,
        Guid? epicId = null,
        List<FeatureStatus>? status = null,
        List<ItemSubtype>? subtype = null,
        DateTime? createdAfter = null,
        DateTime? createdBefore = null,
        int first = 50,
        int? skip = null)
    {
        var query = dbContext.Items.AsQueryable();
        if (projectId.HasValue)
        {
            query = query.Where(f => f.ProjectId == projectId.Value);
        }
        if (epicId.HasValue)
        {
            query = query.Where(f => f.EpicId == epicId.Value);
        }
        if (status is not null && status.Count > 0)
        {
            query = query.Where(f => status.Contains(f.Status));
        }
        if (subtype is not null && subtype.Count > 0)
        {
            query = query.Where(f => subtype.Contains(f.Subtype));
        }
        if (createdAfter.HasValue)
        {
            query = query.Where(f => f.CreatedAt >= createdAfter.Value);
        }
        if (createdBefore.HasValue)
        {
            query = query.Where(f => f.CreatedAt <= createdBefore.Value);
        }

        var totalCount = query.Count();
        query = query.OrderBy(f => f.CreatedAt);
        if (skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }
        var nodes = query.Take(first).ToList();

        return new ItemConnection
        {
            Nodes = nodes,
            PageInfo = new ItemPageInfo
            {
                HasNextPage = (skip ?? 0) + nodes.Count < totalCount,
                HasPreviousPage = skip > 0,
                TotalCount = totalCount
            },
            TotalCount = totalCount
        };
    }

[Obsolete("Use GetItemById instead")]
    public Item? GetFeatureById([Service] DevStackDbContext dbContext, Guid id)
    {
        var item = dbContext.Items.Find(id);
        if (item == null || item.Subtype != Domain.Enums.ItemSubtype.Feature)
            return null;
        return item;
    }

    [Obsolete("Use GetItems instead")]
    public ItemConnection GetFeatures(
        [Service] DevStackDbContext dbContext,
        Guid? projectId = null,
        List<FeatureStatus>? status = null,
        int first = 50,
        int? skip = null)
    {
        return GetItems(dbContext, projectId, null, status, [Domain.Enums.ItemSubtype.Feature], null, null, first, skip);
    }

    [Obsolete("Use GetItems with subtype filter instead")]
    public ItemConnection GetDefects(
        [Service] DevStackDbContext dbContext,
        Guid? projectId = null,
        List<FeatureStatus>? status = null,
        int first = 50,
        int? skip = null)
    {
        var query = dbContext.Items.Where(i => i.Subtype == Domain.Enums.ItemSubtype.Defect);
        if (projectId.HasValue)
        {
            query = query.Where(i => i.ProjectId == projectId.Value);
        }
        if (status is not null && status.Count > 0)
        {
            query = query.Where(i => status.Contains(i.Status));
        }

        var totalCount = query.Count();
        query = query.OrderBy(i => i.CreatedAt);
        if (skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }
        var nodes = query.Take(first).ToList();

        return new ItemConnection
        {
            Nodes = nodes,
            PageInfo = new ItemPageInfo
            {
                HasNextPage = (skip ?? 0) + nodes.Count < totalCount,
                HasPreviousPage = skip > 0,
                TotalCount = totalCount
            },
            TotalCount = totalCount
        };
    }

    [Obsolete("Use GetItemById instead")]
    public Item? GetDefectById([Service] DevStackDbContext dbContext, Guid id)
    {
        var item = dbContext.Items.Find(id);
        if (item == null || item.Subtype != Domain.Enums.ItemSubtype.Defect)
            return null;
        return item;
    }

public TaskConnection GetTasks(
        [Service] DevStackDbContext dbContext,
        Guid? itemId = null,
        List<global::DevStack.Domain.Enums.TaskStatus>? status = null,
        DateTime? createdAfter = null,
        DateTime? createdBefore = null,
        int first = 50,
        int? skip = null)
    {
        var query = dbContext.Tasks.AsQueryable();
        if (itemId.HasValue)
        {
            query = query.Where(t => t.ItemId == itemId.Value);
        }
        if (status is not null && status.Count > 0)
        {
            query = query.Where(t => status.Contains(t.Status));
        }
        if (createdAfter.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= createdAfter.Value);
        }
        if (createdBefore.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= createdBefore.Value);
        }

        var totalCount = query.Count();
        query = query.OrderBy(t => t.CreatedAt);
        if (skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }
        var nodes = query.Take(first).ToList();

        return new TaskConnection
        {
            Nodes = nodes,
            PageInfo = new TaskPageInfo
            {
                HasNextPage = (skip ?? 0) + nodes.Count < totalCount,
                HasPreviousPage = skip > 0,
                TotalCount = totalCount
            },
            TotalCount = totalCount
        };
    }

    public global::DevStack.Domain.Entities.AgentTask? GetTaskById([Service] DevStackDbContext dbContext, Guid id)
    {
        return dbContext.Tasks.Find(id);
    }

    public IQueryable<ModelConfiguration> GetModelConfigurations([Service] DevStackDbContext dbContext)
    {
        return dbContext.ModelConfigurations;
    }

    public IQueryable<AuditEvent> GetAuditEvents([Service] DevStackDbContext dbContext, Guid entityId, int take = 50)
    {
        return dbContext.AuditEvents
            .Where(a => a.EntityId == entityId)
            .OrderByDescending(a => a.OccurredAt)
            .Take(take);
    }

    public EpicConnection GetEpics(
        [Service] DevStackDbContext dbContext,
        string? title = null,
        int first = 50,
        int? skip = null)
    {
        var query = dbContext.Epics.AsQueryable();
        if (!string.IsNullOrWhiteSpace(title))
        {
            query = query.Where(e => e.Title.Contains(title));
        }

        var totalCount = query.Count();
        query = query.OrderBy(e => e.CreatedAt);
        if (skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }
        var nodes = query.Take(first).ToList();

        return new EpicConnection
        {
            Nodes = nodes,
            PageInfo = new EpicPageInfo
            {
                HasNextPage = (skip ?? 0) + nodes.Count < totalCount,
                HasPreviousPage = skip > 0,
                TotalCount = totalCount
            },
            TotalCount = totalCount
        };
    }

    public Epic? GetEpicById([Service] DevStackDbContext dbContext, Guid id)
    {
        return dbContext.Epics.Find(id);
    }

    public List<FeatureStatus> GetValidStatusTransitions([Service] DevStackDbContext dbContext, Guid itemId)
    {
        var item = dbContext.Items.Find(itemId);
        if (item == null)
            return new List<FeatureStatus>();

        var service = new ItemStatusTransitionService();
        var workItem = new Item
        {
            Id = item.Id,
            Subtype = item.Subtype,
            Status = item.Status,
            Result = item.Result,
            Errors = item.Errors,
            OpenQuestions = item.OpenQuestions
        };

        var validTargets = new List<FeatureStatus>();
        foreach (var targetStatus in Enum.GetValues<FeatureStatus>())
        {
            var result = service.Transition(workItem, targetStatus, "query-validation");
            if (result.IsSuccess)
            {
                validTargets.Add(targetStatus);
            }
        }

        return validTargets;
    }

    public DashboardSummary GetDashboardSummary([Service] DevStackDbContext dbContext)
    {
        return new DashboardSummary
        {
            ProjectsInFlight = dbContext.Projects.Count(),
            FeaturesInReview = dbContext.Items.Count(f => f.Status == FeatureStatus.InReview),
            FeaturesFailed = dbContext.Items.Count(f => f.Status == FeatureStatus.Failed),
            TasksInProgress = dbContext.Tasks.Count(t => t.Status == global::DevStack.Domain.Enums.TaskStatus.Code),
            TasksFailed = dbContext.Tasks.Count(t => t.Status == global::DevStack.Domain.Enums.TaskStatus.Failed),
            RecentAuditEvents = dbContext.AuditEvents
                .OrderByDescending(a => a.OccurredAt)
                .Take(10)
                .ToList()
        };
    }
}
