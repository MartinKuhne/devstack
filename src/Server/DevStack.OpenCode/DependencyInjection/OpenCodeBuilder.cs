using DevStack.OpenCode.Client;
using DevStack.OpenCode.Store;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DevStack.OpenCode.DependencyInjection;

/// <summary>
/// Fluent builder returned by <c>AddOpenCode(...)</c>. Use it to attach
/// additional services or override registrations in a chainable manner,
/// mirroring the <see cref="IHttpClientBuilder"/> pattern from
/// <c>Microsoft.Extensions.Http</c>.
/// </summary>
public sealed class OpenCodeBuilder
{
    /// <summary>The service collection that the OpenCode SDK is registered against.</summary>
    public IServiceCollection Services { get; }

    /// <summary>Creates a new builder over the given service collection.</summary>
    public OpenCodeBuilder(IServiceCollection services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }

    /// <summary>
    /// Replaces the default <see cref="IOpenCodeClient"/> registration with a
    /// custom <typeparamref name="TClient"/>. Useful for testing or for
    /// plugging in a different HTTP pipeline.
    /// </summary>
    public OpenCodeBuilder WithClient<TClient>()
        where TClient : class, IOpenCodeClient
    {
        Services.RemoveAll<IOpenCodeClient>();
        Services.AddSingleton<IOpenCodeClient, TClient>();
        return this;
    }

    /// <summary>
    /// Replaces the default <see cref="IOpenCodeConfigStore"/> registration
    /// with a custom <typeparamref name="TStore"/>.
    /// </summary>
    public OpenCodeBuilder WithConfigStore<TStore>()
        where TStore : class, IOpenCodeConfigStore
    {
        Services.RemoveAll<IOpenCodeConfigStore>();
        Services.AddSingleton<IOpenCodeConfigStore, TStore>();
        return this;
    }
}
