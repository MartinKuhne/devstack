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
}
