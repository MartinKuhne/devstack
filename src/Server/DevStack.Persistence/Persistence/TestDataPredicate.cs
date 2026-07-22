using System.Linq.Expressions;

using DevStack.Domain.Entities;

namespace DevStack.Persistence;

public static class TestDataPredicate
{
    private const string TestMarker = "[DeleteAfterTest]";

    /// <summary>
    /// Creates a predicate expression to identify test data projects.
    /// </summary>
    /// <returns>An expression that matches projects containing the test data marker in their name.</returns>
    public static Expression<Func<Project, bool>> Project()
    {
        return p => p.Name.Contains(TestMarker);
    }

    /// <summary>
    /// Creates a predicate expression to identify test data large language models.
    /// </summary>
    /// <returns>An expression that matches large language models containing the test data marker in their URL.</returns>
    public static Expression<Func<LargeLanguageModel, bool>> LargeLanguageModel()
    {
        return m => m.Url.Contains(TestMarker);
    }

    /// <summary>
    /// Creates a predicate expression to identify test data deliverables.
    /// </summary>
    /// <returns>An expression that matches deliverables containing the test data marker in their title.</returns>
    public static Expression<Func<Deliverable, bool>> Deliverable()
    {
        return d => d.Title.Contains(TestMarker);
    }

    /// <summary>
    /// Creates a predicate expression to identify test data agent tasks.
    /// </summary>
    /// <returns>An expression that matches agent tasks containing the test data marker in their title.</returns>
    public static Expression<Func<AgentTask, bool>> AgentTask()
    {
        return t => t.Title.Contains(TestMarker);
    }
}
