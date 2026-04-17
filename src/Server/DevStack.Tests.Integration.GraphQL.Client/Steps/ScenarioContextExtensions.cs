using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.GraphQL.Client.Steps;

public static class ScenarioContextExtensions
{
    public static string? GetString(this ScenarioContext context, string key)
    {
        return context.TryGetValue<string>(key, out var value) ? value : null;
    }

    public static T? Get<T>(this ScenarioContext context, string key)
    {
        return context.TryGetValue<T>(key, out var value) ? value : default;
    }

    public static void Set(this ScenarioContext context, string key, object value)
    {
        context[key] = value;
    }
}