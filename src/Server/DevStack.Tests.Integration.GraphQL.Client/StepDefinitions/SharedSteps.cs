using TechTalk.SpecFlow;

namespace DevStack.Tests.Integration.GraphQL.Client.Features;

[Binding]
public class SharedSteps
{
    [Given("the API is available")]
    public void GivenTheApiIsAvailable()
    {
        // No-op: API availability is assumed
    }

    [Given("a parent project exists")]
    public void GivenAParentProjectExists()
    {
        // No-op: Project setup handled in BeforeScenario
    }
}
