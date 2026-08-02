using DevStack.OpenCode.Client;
using DevStack.OpenCode.Options;
using DevStack.OpenCode.Store;

using Microsoft.Extensions.Configuration;

namespace DevStack.OpenCode.DependencyInjection;

/// <summary>
/// DI registration helpers for the OpenCode SDK.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the OpenCode SDK with the default options.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Optional options configurator.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOpenCodeSdk(
        this IServiceCollection services,
        Action<OpenCodeOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var optionsBuilder = services
            .AddOptions<OpenCodeOptions>()
            .Validate(o => o.BaseUrl is not null, "OpenCode BaseUrl must not be null.")
            .Validate(o => !string.IsNullOrEmpty(o.SchemaPath), "OpenCode SchemaPath must not be empty.")
            .Validate(o => o.HttpTimeout > TimeSpan.Zero, "OpenCode HttpTimeout must be positive.");

        if (configure is not null)
        {
            optionsBuilder.Configure(configure);
        }

        services.AddHttpClient<IOpenCodeClient, OpenCodeClient>((sp, http) =>
        {
            var opts = sp.GetRequiredService<IOptions<OpenCodeOptions>>().Value;
            http.BaseAddress = opts.BaseUrl;
            http.Timeout = opts.HttpTimeout;
            if (!string.IsNullOrEmpty(opts.UserAgent) && http.DefaultRequestHeaders.UserAgent.Count == 0)
            {
                http.DefaultRequestHeaders.Add("User-Agent", opts.UserAgent);
            }
        });

        services.AddSingleton<IOpenCodeConfigStore, OpenCodeConfigStore>();

        return services;
    }

    /// <summary>
    /// Registers the OpenCode SDK and binds its options to the
    /// <c>OpenCode</c> configuration section.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Application configuration root.</param>
    /// <param name="configure">Optional options configurator run after binding.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOpenCodeSdk(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<OpenCodeOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOpenCodeSdk(opts =>
        {
            configuration.GetSection(OpenCodeOptions.SectionName).Bind(opts);
            configure?.Invoke(opts);
        });

        return services;
    }
}
