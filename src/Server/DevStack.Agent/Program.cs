using DevStack.Agent;
using DevStack.Agent.GraphQL;
using DevStack.OpenCode.DependencyInjection;
using DevStack.OpenCode.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Events;

// Serilog is the application's primary logger. The static `Log.Logger` is
// configured first so anything that runs before the host builds (including
// configuration-load failures) goes through the same pipeline. `AddSerilog`
// then bridges Microsoft.Extensions.Logging into Serilog and removes the
// default MEL console provider so the two formatters don't fight.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "DevStack.Agent")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Logging.ClearProviders();
    builder.Services.AddSerilog();

    // Configuration sources (in order of increasing precedence):
    //   1. appsettings.json shipped next to the binary (AppContext.BaseDirectory)
    //   2. OpenCode__BaseUrl / OpenCode__UserAgent / … environment variables
    //   3. Command-line overrides via --opencode:BaseUrl=... syntax
    var appsettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
    builder.Configuration
        .AddJsonFile(appsettingsPath, optional: true, reloadOnChange: false)
        .AddEnvironmentVariables()
        .AddCommandLine(args);

    // Register the OpenCode SDK against the host's IConfiguration. The
    // IHostApplicationBuilder overload binds options from builder.Configuration
    // automatically, so OpenCode:BaseUrl / OpenCode:UserAgent / … in
    // appsettings.json flow through to the SDK.
    builder.AddOpenCode();

    // Register the StrawberryShake-generated DevStack GraphQL client. The
    // base URL is read from the DevStack:GraphQL:BaseUrl config key (or the
    // --devstack:graphql:base-url command-line switch) and defaults to the
    // local DevStack.Api on :8087. The AddDevStackClient() extension is
    // produced by StrawberryShake.Server's MSBuild codegen.
    var graphQLBaseUrl = ResolveGraphQLBaseUrl(builder.Configuration, args);
    builder.Services
        .AddDevStackClient()
        .ConfigureHttpClient(client => client.BaseAddress = new Uri(graphQLBaseUrl));

    // Register the CLI itself.
    builder.Services.AddSingleton<OpenCodeAgent>();
    builder.Services.AddSingleton<DevStackProjectClient>();
    builder.Services.AddSingleton<RepositoryLocator>();
    builder.Services.AddSingleton<RepositoryContextResolver>();
    builder.Services.AddSingleton<PlanDeliverableLister>();
    builder.Services.AddSingleton<PlanExecutor>();

    using var host = builder.Build();

    // Pretty banner so the run is easy to spot in a log.
    Console.WriteLine("DevStack.Agent — OpenCode hello-prompt driver");
    var options = host.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<DevStack.OpenCode.Options.OpenCodeOptions>>().Value;
    Console.WriteLine($"  baseUrl:   {options.BaseUrl}");
    Console.WriteLine($"  userAgent: {options.UserAgent}");
    Console.WriteLine($"  graphQL:   {graphQLBaseUrl}");
    Console.WriteLine();

    // Side-quests: --list-projects / --get-project <id> short-circuit the
    // OpenCode prompt flow so the StrawberryShake client can be smoke-tested
    // without a running OpenCode server.
    if (HasFlag(args, "--list-projects"))
    {
        var first = ParseIntFlag(args, "--list-projects-first", 50);
        return await RunListProjectsAsync(host.Services, first);
    }

    if (HasFlag(args, "--get-project"))
    {
        var raw = ParseFlag(args, "--get-project");
        if (string.IsNullOrWhiteSpace(raw) || !Guid.TryParse(raw, out var id))
        {
            Console.Error.WriteLine("--get-project requires a UUID argument, e.g. --get-project 00000000-0000-0000-0000-000000000000");
            return 2;
        }
        return await RunGetProjectAsync(host.Services, id);
    }

    if (HasFlag(args, "--show-plan"))
    {
        var repositoryRoot = ParseFlag(args, "--repositoryRoot");
        return await RunShowPlanAsync(host.Services, repositoryRoot);
    }

    if (HasFlag(args, "--run-plan"))
    {
        var repositoryRoot = ParseFlag(args, "--repositoryRoot");
        var promptPath = ResolvePlanPromptPath(builder.Configuration, args);
        var requestedModel = ParseModel(args, host.Services.GetRequiredService<ILogger<OpenCodeAgent>>());
        return await RunRunPlanAsync(host.Services, repositoryRoot, promptPath, requestedModel);
    }

    var prompt = ParsePrompt(args, host.Services.GetRequiredService<ILogger<OpenCodeAgent>>());
    var model = ParseModel(args, host.Services.GetRequiredService<ILogger<OpenCodeAgent>>());
    var title = ParseFlag(args, "--title");

    var agent = host.Services.GetRequiredService<OpenCodeAgent>();
    var sessionId = await agent.RunAsync(prompt, model, title);

    Console.WriteLine();
    Console.WriteLine($"Done. sessionId={sessionId}");
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "DevStack.Agent terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

static string ParsePrompt(string[] argv, ILogger<OpenCodeAgent> logger)
{
    if (argv.Length == 0)
    {
        logger.LogInformation("No prompt provided on the command line, defaulting to \"Hello\".");
        return "Hello";
    }

    if (argv[0].StartsWith("--", StringComparison.Ordinal))
    {
        logger.LogInformation("No prompt provided before flags, defaulting to \"Hello\".");
        return "Hello";
    }

    return argv[0];
}

static ModelRef? ParseModel(string[] argv, ILogger<OpenCodeAgent> logger)
{
    var spec = ParseFlag(argv, "--model");
    if (string.IsNullOrWhiteSpace(spec))
    {
        return null;
    }

    var slash = spec.IndexOf('/');
    if (slash <= 0 || slash == spec.Length - 1)
    {
        logger.LogWarning("--model value '{Spec}' is missing the required 'provider/model' shape; ignoring.", spec);
        return null;
    }

    return new ModelRef { ProviderId = spec[..slash], ModelId = spec[(slash + 1)..] };
}

static string? ParseFlag(string[] argv, string name)
{
    for (var i = 0; i < argv.Length - 1; i++)
    {
        if (string.Equals(argv[i], name, StringComparison.Ordinal))
        {
            return argv[i + 1];
        }
    }

    return null;
}

/// <summary>
/// Resolves the plan prompt template path, honouring (highest
/// precedence first): <c>--plan-prompt &lt;path&gt;</c>, the
/// <c>DevStack__Plan__PromptPath</c> environment variable, the
/// <c>DevStack:Plan:PromptPath</c> appsettings key, and finally the
/// repo-relative default <c>scripts/prompts/plan.prompt</c>. Relative
/// paths are resolved against the worktree by
/// <see cref="PlanExecutor.ResolvePromptPath"/> at execution time.
/// </summary>
static string ResolvePlanPromptPath(IConfiguration configuration, string[] argv)
{
    const string Key = "DevStack:Plan:PromptPath";
    const string CliKey = "--plan-prompt";

    var fromCli = ParseFlag(argv, CliKey);
    if (!string.IsNullOrWhiteSpace(fromCli))
    {
        return fromCli;
    }

    var fromConfig = configuration[Key];
    return string.IsNullOrWhiteSpace(fromConfig) ? "scripts/prompts/plan.prompt" : fromConfig;
}

static int ParseIntFlag(string[] argv, string name, int defaultValue)
{
    var raw = ParseFlag(argv, name);
    if (string.IsNullOrWhiteSpace(raw))
    {
        return defaultValue;
    }
    return int.TryParse(raw, out var parsed) ? parsed : defaultValue;
}

static bool HasFlag(string[] argv, string name)
{
    foreach (var a in argv)
    {
        if (string.Equals(a, name, StringComparison.Ordinal))
        {
            return true;
        }
    }
    return false;
}

/// <summary>
/// Resolves the GraphQL endpoint, honouring (highest precedence first):
/// the command line (<c>--devstack:graphql:base-url</c>), the environment
/// (<c>DevStack__GraphQL__BaseUrl</c>), and <c>appsettings.json</c>.
/// Falls back to the local DevStack.Api on :8087 when nothing is set.
/// </summary>
static string ResolveGraphQLBaseUrl(IConfiguration configuration, string[] argv)
{
    const string Key = "DevStack:GraphQL:BaseUrl";
    const string CliKey = "--devstack:graphql:base-url";

    var fromCli = ParseFlag(argv, CliKey);
    if (!string.IsNullOrWhiteSpace(fromCli))
    {
        return fromCli;
    }

    var fromConfig = configuration[Key];
    return string.IsNullOrWhiteSpace(fromConfig) ? "http://localhost:8087/graphql" : fromConfig;
}

static async Task<int> RunListProjectsAsync(IServiceProvider services, int first)
{
    var lister = services.GetRequiredService<DevStackProjectClient>();
    var projects = await lister.ListProjectsAsync(first);

    Console.WriteLine();
    if (projects.Count == 0)
    {
        Console.WriteLine("No projects returned by the DevStack GraphQL API.");
        return 0;
    }

    Console.WriteLine($"DevStack projects ({projects.Count}):");
    Console.WriteLine();
    foreach (var p in projects)
    {
        Console.WriteLine($"  {p.Id}  {p.Name}");
        Console.WriteLine($"      repo:     {p.Repository}");
        if (!string.IsNullOrWhiteSpace(p.Description))
        {
            Console.WriteLine($"      describe: {p.Description}");
        }
    }
    return 0;
}

static async Task<int> RunGetProjectAsync(IServiceProvider services, Guid id)
{
    var lister = services.GetRequiredService<DevStackProjectClient>();
    var project = await lister.GetProjectByIdAsync(id);

    Console.WriteLine();
    if (project is null)
    {
        Console.WriteLine($"Project {id} not found.");
        return 0;
    }

    Console.WriteLine($"Project {project.Id}: {project.Name}");
    Console.WriteLine($"  repo:      {project.Repository}");
    if (!string.IsNullOrWhiteSpace(project.Description))
    {
        Console.WriteLine($"  describe:  {project.Description}");
    }
    return 0;
}

static async Task<int> RunShowPlanAsync(IServiceProvider services, string? repositoryRoot)
{
    if (!TryResolvePlanContext(services, repositoryRoot, out var context, out var report, out var exitCode))
    {
        return exitCode;
    }

    Console.WriteLine();
    Console.WriteLine($"Repository: {context.Worktree}");
    Console.WriteLine($"  remote:   {context.CanonicalRemoteUrl}");
    if (context.GitHub is { } gh)
    {
        Console.WriteLine($"  github:   {gh.Owner}/{gh.Name}");
    }
    Console.WriteLine();
    Console.WriteLine($"DevStack project: {report.Project.Name} ({report.Project.Id})");
    Console.WriteLine($"PLAN deliverables ({report.PlanDeliverables.Count}):");
    Console.WriteLine();
    if (report.PlanDeliverables.Count == 0)
    {
        Console.WriteLine("  (none)");
        return 0;
    }

    Console.WriteLine($"  {"TYPE",-10}  {"ID",-36}  {"STATUS",-6}  TITLE");
    foreach (var d in report.PlanDeliverables)
    {
        Console.WriteLine($"  {d.Type,-10}  {d.Id,-36}  {d.Status,-6}  {d.Title}");
    }
    return 0;
}

static async Task<int> RunRunPlanAsync(IServiceProvider services, string? repositoryRoot, string promptPath, DevStack.OpenCode.Models.ModelRef? model)
{
    if (!TryResolvePlanContext(services, repositoryRoot, out var context, out var report, out var exitCode))
    {
        return exitCode;
    }

    Console.WriteLine();
    Console.WriteLine($"Repository: {context.Worktree}");
    Console.WriteLine($"  remote:   {context.CanonicalRemoteUrl}");
    if (context.GitHub is { } gh)
    {
        Console.WriteLine($"  github:   {gh.Owner}/{gh.Name}");
    }
    Console.WriteLine();
    Console.WriteLine($"DevStack project: {report.Project.Name} ({report.Project.Id})");
    Console.WriteLine($"Prompt template: {promptPath} (resolved against the worktree if relative)");
    Console.WriteLine($"Executing plan for {report.PlanDeliverables.Count} deliverable(s)…");

    if (report.PlanDeliverables.Count == 0)
    {
        Console.WriteLine();
        Console.WriteLine("  (nothing to plan)");
        return 0;
    }

    var executor = services.GetRequiredService<PlanExecutor>();
    PlanRunSummary summary;
    try
    {
        summary = await executor.ExecuteAsync(report, context, promptPath, model: model);
    }
    catch (FileNotFoundException ex)
    {
        Console.Error.WriteLine($"error: {ex.Message}");
        return 2;
    }

    Console.WriteLine();
    Console.WriteLine($"Plan summary: {summary.Processed.Count} succeeded, {summary.Failures.Count} failed.");
    return summary.AllSucceeded ? 0 : 3;
}

/// <summary>
/// Shared "resolve the worktree, parse the git remote, find the
/// DevStack project, and list PLAN deliverables" helper for
/// <c>--show-plan</c> and <c>--run-plan</c>. Returns <c>true</c> on
/// success and writes the exit code to <paramref name="exitCode"/>
/// on failure (always prints a friendly <c>error: …</c> on stderr
/// for failures, matching the existing CLI conventions).
/// </summary>
static bool TryResolvePlanContext(
    IServiceProvider services,
    string? repositoryRoot,
    out RepositoryContext context,
    out PlanDeliverableReport report,
    out int exitCode)
{
    context = null!;
    report = null!;
    exitCode = 0;

    var resolver = services.GetRequiredService<RepositoryContextResolver>();
    var lister = services.GetRequiredService<PlanDeliverableLister>();
    var openCode = services.GetService<DevStack.OpenCode.Client.IOpenCodeClient>();

    // Build a locator that uses the OpenCode SDK when it is
    // available, or a no-SDK locator when it isn't (so --show-plan
    // and --run-plan both work without an OpenCode server as long
    // as --repositoryRoot is supplied).
    var locator = openCode is null
        ? new RepositoryLocator(null, services.GetRequiredService<ILogger<RepositoryLocator>>())
        : new RepositoryLocator(openCode, services.GetRequiredService<ILogger<RepositoryLocator>>());

    try
    {
        var worktree = locator.LocateAsync(repositoryRoot).GetAwaiter().GetResult();
        context = resolver.ResolveAsync(worktree).GetAwaiter().GetResult();
    }
    catch (Exception ex) when (ex is InvalidOperationException or DirectoryNotFoundException or LibGit2Sharp.NotFoundException or LibGit2Sharp.RepositoryNotFoundException)
    {
        Console.Error.WriteLine($"error: {ex.Message}");
        exitCode = 2;
        return false;
    }

    try
    {
        report = lister.ListAsync(context).GetAwaiter().GetResult();
    }
    catch (InvalidOperationException ex)
    {
        // "No DevStack project is registered for repository '...'" —
        // surface as a friendly error rather than a fatal stack trace.
        Console.Error.WriteLine($"error: {ex.Message}");
        exitCode = 2;
        return false;
    }

    return true;
}
