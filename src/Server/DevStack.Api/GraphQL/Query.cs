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
        int? skip = null)
    {
        var query = dbContext.Projects.AsQueryable();
        var totalCount = query.Count();
        query = query.OrderBy(p => p.Id);
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

    public LargeLanguageModel? GetLargeLanguageModelById([Service] DevStackDbContext dbContext, Guid id)
    {
        return dbContext.LargeLanguageModels.Find(id);
    }

    public List<LargeLanguageModel> GetLargeLanguageModels([Service] DevStackDbContext dbContext)
    {
        return dbContext.LargeLanguageModels.ToList();
    }

    public List<LargeLanguageModel> GetLargeLanguageModelsByProjectId([Service] DevStackDbContext dbContext, Guid projectId)
    {
        return dbContext.LargeLanguageModels
            .Where(m => m.ProjectId == projectId)
            .OrderBy(m => m.Id)
            .ToList();
    }

    public List<Deliverable> GetDeliverablesByProjectId([Service] DevStackDbContext dbContext, Guid projectId)
    {
        return dbContext.Deliverables
            .Where(d => d.ProjectId == projectId)
            .OrderBy(d => d.Id)
            .ToList();
    }

    public List<AgentTask> GetAgentTasksByDeliverableId([Service] DevStackDbContext dbContext, Guid deliverableId)
    {
        return dbContext.AgentTasks
            .Where(t => t.DeliverableId == deliverableId)
            .OrderBy(t => t.Id)
            .ToList();
    }

    public Deliverable? GetDeliverableById([Service] DevStackDbContext dbContext, Guid id)
    {
        return dbContext.Deliverables.Find(id);
    }

    public List<Deliverable> GetDeliverables([Service] DevStackDbContext dbContext)
    {
        return dbContext.Deliverables.OrderBy(d => d.Id).ToList();
    }

    public AgentTask? GetAgentTaskById([Service] DevStackDbContext dbContext, Guid id)
    {
        return dbContext.AgentTasks.Find(id);
    }

    public List<AgentTask> GetAgentTasks(
        [Service] DevStackDbContext dbContext,
        Guid? itemId = null)
    {
        var query = dbContext.AgentTasks.AsQueryable();
        if (itemId.HasValue)
        {
            query = query.Where(t => t.ProjectId == itemId.Value);
        }
        return query.OrderBy(t => t.Id).ToList();
    }
}
