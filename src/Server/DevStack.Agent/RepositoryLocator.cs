using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using DevStack.OpenCode.Client;

using Microsoft.Extensions.Logging;

namespace DevStack.Agent;

/// <summary>
/// Resolves the absolute filesystem path of the git repository the
/// agent is operating on. The primary path asks the OpenCode SDK for
/// <c>project/current</c> and uses the server's reported
/// <c>worktree</c>; if the server is unreachable, doesn't return a
/// project, or doesn't expose a worktree, the caller can pass
/// <c>--repositoryRoot</c> and the locator falls back to that path
/// verbatim. The <c>--repositoryRoot</c> override always wins — useful
/// when running without an OpenCode server (e.g. the
/// <c>--show-plan</c> smoke test).
/// </summary>
public sealed class RepositoryLocator
{
    private readonly IOpenCodeClient? _openCode;
    private readonly ILogger<RepositoryLocator> _logger;

    /// <summary>
    /// Builds the locator. The OpenCode client is optional; pass
    /// <c>null</c> to skip the SDK lookup and require a
    /// <c>--repositoryRoot</c> override.
    /// </summary>
    public RepositoryLocator(IOpenCodeClient? openCode, ILogger<RepositoryLocator> logger)
    {
        _openCode = openCode;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Resolves the worktree path. Throws when neither the SDK nor
    /// the override can produce one — the caller should surface a
    /// clear "no repository found" error rather than silently
    /// continuing.
    /// </summary>
    public async Task<string> LocateAsync(string? repositoryRootOverride, CancellationToken cancellationToken = default)
    {
        // 1. Explicit override always wins — lets callers force a
        //    specific path even when the OpenCode server is running.
        if (!string.IsNullOrWhiteSpace(repositoryRootOverride))
        {
            var resolved = Path.GetFullPath(repositoryRootOverride);
            if (!Directory.Exists(resolved))
            {
                throw new DirectoryNotFoundException(
                    $"--repositoryRoot '{resolved}' does not exist or is not a directory.");
            }
            _logger.LogInformation("Using --repositoryRoot override as the worktree: {Worktree}", resolved);
            return resolved;
        }

        // 2. Ask the OpenCode SDK which worktree the server thinks it's
        //    running under. The server is a long-lived process; its
        //    worktree is the authoritative "where am I?" answer.
        if (_openCode is not null)
        {
            try
            {
                var project = await _openCode.Project.GetCurrentAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(project.Worktree))
                {
                    _logger.LogInformation(
                        "OpenCode SDK reported worktree '{Worktree}' (vcs={Vcs}, vcsDir={VcsDir}).",
                        project.Worktree,
                        project.Vcs ?? "<unknown>",
                        project.VcsDir ?? "<unknown>");
                    return project.Worktree;
                }
                _logger.LogWarning(
                    "OpenCode SDK returned a project with an empty worktree; falling back to --repositoryRoot.");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex,
                    "Failed to fetch the current project from the OpenCode SDK at {BaseUrl}; " +
                    "fall back to --repositoryRoot to point at a local checkout.",
                    _openCode.BaseUrl);
            }
        }

        throw new InvalidOperationException(
            "Could not determine the git repository the agent is running under. " +
            "Either start the OpenCode SDK (so it can report its worktree) or pass --repositoryRoot <path>.");
    }
}
