using DevStack.Domain.Entities;

using Microsoft.EntityFrameworkCore;

using System.Linq.Expressions;

namespace DevStack.Persistence;

public static class DevStackDbContextExtensions
{
    public static async Task CleanupTestDataAsync(
        this DevStackDbContext context,
        CancellationToken cancellationToken = default)
    {
        await CleanupAsync(context, context.Deliverables, TestDataPredicate.Deliverable(), cancellationToken);
        await CleanupAsync(context, context.AgentTasks, TestDataPredicate.AgentTask(), cancellationToken);
        await CleanupAsync(context, context.Projects, TestDataPredicate.Project(), cancellationToken);
        await CleanupAsync(context, context.LargeLanguageModels, TestDataPredicate.LargeLanguageModel(), cancellationToken);
    }

    private static async Task CleanupAsync<TEntity>(
        DevStackDbContext context,
        DbSet<TEntity> dbSet,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        var testItems = await dbSet.Where(predicate).ToListAsync(cancellationToken);

        if (testItems.Any())
        {
            dbSet.RemoveRange(testItems);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
