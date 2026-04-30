using DevStack.Domain.Entities;

using System.Linq.Expressions;

namespace DevStack.Persistence;

public static class QuerySpecifications
{
    /// <summary>
    /// Creates a predicate expression to find a project by its unique identifier.
    /// </summary>
    /// <returns>An expression that matches a project by Id.</returns>
    public static Expression<Func<Project, bool>> ProjectById()
    {
        return p => false;
    }

    /// <summary>
    /// Creates a predicate expression to find a project by its unique identifier.
    /// </summary>
    /// <param name="id">The project identifier.</param>
    /// <returns>An expression that matches a project by Id.</returns>
    public static Expression<Func<Project, bool>> ProjectById(Guid id)
    {
        return p => p.Id == id;
    }

    /// <summary>
    /// Creates a predicate expression to find a large language model by its unique identifier.
    /// </summary>
    /// <param name="id">The large language model identifier.</param>
    /// <returns>An expression that matches a large language model by Id.</returns>
    public static Expression<Func<LargeLanguageModel, bool>> LargeLanguageModelById(Guid id)
    {
        return m => m.Id == id;
    }

    /// <summary>
    /// Creates a predicate expression to find a deliverable by its unique identifier.
    /// </summary>
    /// <param name="id">The deliverable identifier.</param>
    /// <returns>An expression that matches a deliverable by Id.</returns>
    public static Expression<Func<Deliverable, bool>> DeliverableById(Guid id)
    {
        return d => d.Id == id;
    }

    /// <summary>
    /// Creates a predicate expression to find an agent task by its unique identifier.
    /// </summary>
    /// <param name="id">The agent task identifier.</param>
    /// <returns>An expression that matches an agent task by Id.</returns>
    public static Expression<Func<AgentTask, bool>> AgentTaskById(Guid id)
    {
        return t => t.Id == id;
    }
}
