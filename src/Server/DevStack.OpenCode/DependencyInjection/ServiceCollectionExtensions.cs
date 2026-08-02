using DevStack.OpenCode.Client;
using DevStack.OpenCode.Options;
using DevStack.OpenCode.Store;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DevStack.OpenCode.DependencyInjection;

/// <summary>
/// DI registration helpers for the OpenCode SDK. The recommended entry
/// points are <c>services.AddOpenCode(...)</c> and
/// <c>builder.AddOpenCode(...)</c>, which return a chainable
/// <see cref="OpenCodeBuilder"/>.
/// </summary>
public static class OpenCodeServiceCollectionExtensions
{
    /// <summary>
    /// Registers the OpenCode SDK with default options.
    /// </summary>
    public static OpenCodeBuilder AddOpenCode(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return AddOpenCodeCore(services, configure: null, configuration: null);
    }

    /// <summary>
    /// Registers the OpenCode SDK and configures its options via the
    /// supplied <paramref name="configure"/> delegate.
    /// </summary>
    public static OpenCodeBuilder AddOpenCode(this IServiceCollection services, Action<OpenCodeOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);
        return AddOpenCodeCore(services, configure, configuration: null);
    }

    /// <summary>
    /// Registers the OpenCode SDK and binds its options to the
    /// <c>OpenCode</c> section of <paramref name="configuration"/>.
    /// </summary>
    public static OpenCodeBuilder AddOpenCode(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        return AddOpenCodeCore(services, configure: null, configuration);
    }

    /// <summary>
    /// Registers the OpenCode SDK and binds its options from
    /// <paramref name="configuration"/> with an additional
    /// <paramref name="configure"/> override.
    /// </summary>
    public static OpenCodeBuilder AddOpenCode(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<OpenCodeOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(configure);
        return AddOpenCodeCore(services, configure, configuration);
    }

    private static OpenCodeBuilder AddOpenCodeCore(
        IServiceCollection services,
        Action<OpenCodeOptions>? configure,
        IConfiguration? configuration)
    {
        var optionsBuilder = services
            .AddOptions<OpenCodeOptions>()
            .Validate(o => o.BaseUrl is not null, "OpenCode BaseUrl must not be null.")
            .Validate(o => !string.IsNullOrEmpty(o.SchemaPath), "OpenCode SchemaPath must not be empty.")
            .Validate(o => o.HttpTimeout > TimeSpan.Zero, "OpenCode HttpTimeout must be positive.");

        if (configuration is not null)
        {
            optionsBuilder.Bind(configuration.GetSection(OpenCodeOptions.SectionName));
        }

        if (configure is not null)
        {
            optionsBuilder.Configure(configure);
        }

        services.TryAddSingleton<IOpenCodeConfigStore, OpenCodeConfigStore>();

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

        return new OpenCodeBuilder(services);
    }
}

/// <summary>
/// Host-builder registration helpers for the OpenCode SDK.
/// </summary>
public static class OpenCodeHostBuilderExtensions
{
    /// <summary>
    /// Registers the OpenCode SDK on the supplied host application
    /// builder, binding options from <see cref="IHostApplicationBuilder.Configuration"/>.
    /// </summary>
    public static OpenCodeBuilder AddOpenCode(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Services.AddOpenCode(builder.Configuration);
    }

    /// <summary>
    /// Registers the OpenCode SDK and applies an additional options
    /// configuration delegate on top of the host's configuration.
    /// </summary>
    public static OpenCodeBuilder AddOpenCode(this IHostApplicationBuilder builder, Action<OpenCodeOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);
        return builder.Services.AddOpenCode(builder.Configuration, configure);
    }
}

/// <summary>
/// Legacy aliases for the <c>AddOpenCodeSdk(...)</c> entry points. Use the
/// <c>AddOpenCode(...)</c> extensions on
/// <see cref="OpenCodeServiceCollectionExtensions"/> /
/// <see cref="OpenCodeHostBuilderExtensions"/> for new code.
/// </summary>
public static class OpenCodeLegacyServiceCollectionExtensions
{
    /// <summary>Legacy alias for <see cref="OpenCodeServiceCollectionExtensions.AddOpenCode(IServiceCollection)"/>.</summary>
    [Obsolete("Use AddOpenCode instead. This alias will be removed in a future release.")]
    public static OpenCodeBuilder AddOpenCodeSdk(this IServiceCollection services) => services.AddOpenCode();

    /// <summary>Legacy alias for <see cref="OpenCodeServiceCollectionExtensions.AddOpenCode(IServiceCollection, Action{OpenCodeOptions})"/>.</summary>
    [Obsolete("Use AddOpenCode instead. This alias will be removed in a future release.")]
    public static OpenCodeBuilder AddOpenCodeSdk(this IServiceCollection services, Action<OpenCodeOptions> configure) =>
        services.AddOpenCode(configure);

    /// <summary>Legacy alias for <see cref="OpenCodeServiceCollectionExtensions.AddOpenCode(IServiceCollection, IConfiguration)"/>.</summary>
    [Obsolete("Use AddOpenCode instead. This alias will be removed in a future release.")]
    public static OpenCodeBuilder AddOpenCodeSdk(this IServiceCollection services, IConfiguration configuration) =>
        services.AddOpenCode(configuration);

    /// <summary>Legacy alias for <see cref="OpenCodeServiceCollectionExtensions.AddOpenCode(IServiceCollection, IConfiguration, Action{OpenCodeOptions})"/>.</summary>
    [Obsolete("Use AddOpenCode instead. This alias will be removed in a future release.")]
    public static OpenCodeBuilder AddOpenCodeSdk(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<OpenCodeOptions> configure) =>
        services.AddOpenCode(configuration, configure);
}
