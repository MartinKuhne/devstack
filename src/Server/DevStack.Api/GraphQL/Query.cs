using DevStack.Domain.Entities;
using DevStack.Domain.Enums;
using DevStack.Persistence;
using HotChocolate.Types;
using HotChocolate.Data;

namespace DevStack.Api.GraphQL.Types;

public class Query
{
    public Project? GetProject([Service] DevStackDbContext dbContext, Guid id)
    {
        return dbContext.Projects.Find(id);
    }

    [UsePaging(MaxPageSize = 100)]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Project> GetProjects(
        [Service] DevStackDbContext dbContext)
    {
        return dbContext.Projects.AsQueryable();
    }

    public LargeLanguageModel? GetLargeLanguageModel([Service] DevStackDbContext dbContext, Guid id)
    {
        return dbContext.LargeLanguageModels.Find(id);
    }

    public IQueryable<LargeLanguageModel> GetLargeLanguageModels(
        [Service] DevStackDbContext dbContext)
    {
        return dbContext.LargeLanguageModels.AsQueryable();
    }

    public Deliverable? GetDeliverable([Service] DevStackDbContext dbContext, Guid id)
    {
        return dbContext.Deliverables.Find(id);
    }

    [UsePaging(MaxPageSize = 100)]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Deliverable> GetDeliverables(
        [Service] DevStackDbContext dbContext)
    {
        return dbContext.Deliverables.AsQueryable();
    }

    public AgentTask? GetAgentTask([Service] DevStackDbContext dbContext, Guid id)
    {
        return dbContext.AgentTasks.Find(id);
    }

    [UsePaging(MaxPageSize = 100)]
    [UseFiltering]
    [UseSorting]
    public IQueryable<AgentTask> GetAgentTasks(
        [Service] DevStackDbContext dbContext)
    {
        return dbContext.AgentTasks.AsQueryable();
    }
}
