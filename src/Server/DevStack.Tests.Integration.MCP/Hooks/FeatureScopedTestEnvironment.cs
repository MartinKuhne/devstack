using DevStack.Tests.Integration.Shared;

namespace DevStack.Tests.Integration.MCP.Hooks;

/// <summary>
/// Holds a <see cref="DevStackTestEnv"/> that is built lazily for a single feature file and disposed with it.
/// </summary>
/// <remarks>
/// The assembly-level environment is created in <c>[BeforeTestRun]</c>; a second, differently configured
/// environment (feature flag off, API host) is expensive, so it is only built for the features that need it.
/// </remarks>
internal sealed class FeatureScopedTestEnvironment : IDisposable
{
    private readonly Func<DevStackTestEnv> _factory;
    private DevStackTestEnv? _environment;

    public FeatureScopedTestEnvironment(Func<DevStackTestEnv> factory)
    {
        _factory = factory;
    }

    /// <summary>Builds the environment on first access and returns the same instance afterwards.</summary>
    public DevStackTestEnv Current => _environment ??= _factory();

    /// <summary>Disposes and forgets the environment so that a later feature can rebuild it.</summary>
    public void Dispose()
    {
        _environment?.Dispose();
        _environment = null;
    }
}
