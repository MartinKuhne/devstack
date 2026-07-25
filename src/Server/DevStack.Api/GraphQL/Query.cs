using DevStack.Persistence;

using HotChocolate.Data;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;

namespace DevStack.Api.GraphQL.Types;

public class Query
{
    public Project? GetProject([Service] DevStackDbContext dbContext, Guid id)
    {
        var predicate = QuerySpecifications.ProjectById(id);
        return dbContext.Projects.FirstOrDefault(predicate);
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
        var predicate = QuerySpecifications.LargeLanguageModelById(id);
        return dbContext.LargeLanguageModels.FirstOrDefault(predicate);
    }

    [UsePaging(MaxPageSize = 100)]
    [UseFiltering]
    [UseSorting]
    public IQueryable<LargeLanguageModel> GetLargeLanguageModels(
        [Service] DevStackDbContext dbContext)
    {
        return dbContext.LargeLanguageModels.AsQueryable();
    }

    public Deliverable? GetDeliverable([Service] DevStackDbContext dbContext, Guid id)
    {
        return dbContext.Deliverables
            .Include(d => d.AgentTasks)
            .FirstOrDefault(d => d.Id == id);
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
        var predicate = QuerySpecifications.AgentTaskById(id);
        return dbContext.AgentTasks.FirstOrDefault(predicate);
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
