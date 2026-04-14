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

    public IQueryable<Project> GetProjects([Service] DevStackDbContext dbContext)
    {
        return dbContext.Projects;
    }

    public FeatureConnection GetFeatures(
        [Service] DevStackDbContext dbContext,
        Guid? projectId = null,
        List<FeatureStatus>? status = null,
        DateTime? createdAfter = null,
        DateTime? createdBefore = null,
        int first = 50,
        int? skip = null)
    {
        var query = dbContext.Features.AsQueryable();
        if (projectId.HasValue)
        {
            query = query.Where(f => f.ProjectId == projectId.Value);
        }
        if (status is not null && status.Count > 0)
        {
            query = query.Where(f => status.Contains(f.Status));
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

        return new FeatureConnection
        {
            Nodes = nodes,
            PageInfo = new FeaturePageInfo
            {
                HasNextPage = (skip ?? 0) + nodes.Count < totalCount,
                HasPreviousPage = skip > 0,
                TotalCount = totalCount
            },
            TotalCount = totalCount
        };
    }

    public Feature? GetFeatureById([Service] DevStackDbContext dbContext, Guid id)
    {
        return dbContext.Features.Find(id);
    }

    public IQueryable<Defect> GetDefects([Service] DevStackDbContext dbContext, Guid? projectId = null)
    {
        var query = dbContext.Defects.AsQueryable();
        if (projectId.HasValue)
        {
            query = query.Where(d => d.ProjectId == projectId.Value);
        }
        return query;
    }

    public Defect? GetDefectById([Service] DevStackDbContext dbContext, Guid id)
    {
        return dbContext.Defects.Find(id);
    }

    public TaskConnection GetTasks(
        [Service] DevStackDbContext dbContext,
        Guid? featureId = null,
        List<global::DevStack.Domain.Enums.TaskStatus>? status = null,
        DateTime? createdAfter = null,
        DateTime? createdBefore = null,
        int first = 50,
        int? skip = null)
    {
        var query = dbContext.Tasks.AsQueryable();
        if (featureId.HasValue)
        {
            query = query.Where(t => t.FeatureId == featureId.Value);
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

    public IQueryable<ModelConfiguration> GetModelConfigurations([Service] DevStackDbContext dbContext, Guid projectId)
    {
        return dbContext.ModelConfigurations.Where(mc => mc.ProjectId == projectId);
    }

    public IQueryable<AuditEvent> GetAuditEvents([Service] DevStackDbContext dbContext, Guid entityId, int take = 50)
    {
        return dbContext.AuditEvents
            .Where(a => a.EntityId == entityId)
            .OrderByDescending(a => a.OccurredAt)
            .Take(take);
    }

    public List<FeatureStatus> GetValidStatusTransitions([Service] DevStackDbContext dbContext, Guid featureId)
    {
        var feature = dbContext.Features.Find(featureId);
        if (feature == null)
            return new List<FeatureStatus>();

        var service = new FeatureStatusTransitionService();
        var workItem = new Feature
        {
            Id = feature.Id,
            Status = feature.Status,
            Result = feature.Result,
            Errors = feature.Errors,
            OpenQuestions = feature.OpenQuestions
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
            FeaturesInReview = dbContext.Features.Count(f => f.Status == FeatureStatus.InReview),
            FeaturesFailed = dbContext.Features.Count(f => f.Status == FeatureStatus.Failed),
            TasksInProgress = dbContext.Tasks.Count(t => t.Status == global::DevStack.Domain.Enums.TaskStatus.Code),
            TasksFailed = dbContext.Tasks.Count(t => t.Status == global::DevStack.Domain.Enums.TaskStatus.Failed),
            RecentAuditEvents = dbContext.AuditEvents
                .OrderByDescending(a => a.OccurredAt)
                .Take(10)
                .ToList()
        };
    }
}
