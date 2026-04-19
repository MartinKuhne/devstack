using DevStack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevStack.Persistence;

public static class DevStackDbContextExtensions
{
    private const string TestDataMarker = "[TestData]";

    public static async Task CleanupTestDataAsync(
        this DevStackDbContext context,
        CancellationToken cancellationToken = default)
    {
        await CleanupDeliverablesAsync(context, cancellationToken);
        await CleanupAgentTasksAsync(context, cancellationToken);
        await CleanupProjectsAsync(context, cancellationToken);
        await CleanupLargeLanguageModelsAsync(context, cancellationToken);
    }

    private static async Task CleanupLargeLanguageModelsAsync(
        DevStackDbContext context,
        CancellationToken cancellationToken)
    {
        var models = await context.LargeLanguageModels
            .Where(m => m.Url.Contains(TestDataMarker))
            .ToListAsync(cancellationToken);

        if (models.Any())
        {
            context.LargeLanguageModels.RemoveRange(models);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task CleanupDeliverablesAsync(
        DevStackDbContext context,
        CancellationToken cancellationToken)
    {
        var deliverables = await context.Deliverables
            .Where(d => d.Title.Contains(TestDataMarker))
            .ToListAsync(cancellationToken);

        if (deliverables.Any())
        {
            context.Deliverables.RemoveRange(deliverables);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task CleanupAgentTasksAsync(
        DevStackDbContext context,
        CancellationToken cancellationToken)
    {
        var tasks = await context.AgentTasks
            .Where(t => t.Title.Contains(TestDataMarker))
            .ToListAsync(cancellationToken);

        if (tasks.Any())
        {
            context.AgentTasks.RemoveRange(tasks);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task CleanupProjectsAsync(
        DevStackDbContext context,
        CancellationToken cancellationToken)
    {
        var projects = await context.Projects
            .Where(p => p.Name.Contains(TestDataMarker))
            .ToListAsync(cancellationToken);

        if (projects.Any())
        {
            context.Projects.RemoveRange(projects);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
