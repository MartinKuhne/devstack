using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LibGit2Sharp;

using Microsoft.Extensions.Logging;

using Octokit;

// Both LibGit2Sharp and Octokit expose `Repository` and `Credentials`
// types, so alias the local-git ones to keep the resolver readable
// and avoid an ambiguity at the call site.
using GitRepository = LibGit2Sharp.Repository;
using GitCredentials = LibGit2Sharp.Credentials;

namespace DevStack.Agent;

/// <summary>
/// Resolves a <see cref="RepositoryContext"/> from a local worktree
/// path. Uses <c>LibGit2Sharp</c> to open the repo, read the
/// <c>origin</c> remote URL, and normalize it to a canonical
/// <c>https://host/owner/repo</c> form. When the remote is on
/// GitHub, also calls <c>Octokit</c> to verify the repository is
/// reachable on the GitHub side (best-effort: failure is logged and
/// the listing continues with the locally-known owner/name). The
/// returned context is what <see cref="PlanDeliverableLister"/> uses
/// to find the matching DevStack project.
/// </summary>
public sealed class RepositoryContextResolver
{
    /// <summary>Default remote name the resolver inspects first.</summary>
    public const string DefaultRemoteName = "origin";

    private readonly ILogger<RepositoryContextResolver> _logger;
    private readonly IGitHubClientFactory _gitHubClientFactory;

    /// <summary>
    /// Builds the resolver. The GitHub client factory is injectable
    /// so tests can supply a fake; production callers pass
    /// <see cref="GitHubClientFactory.Default"/> (or a custom
    /// <c>ProductHeaderValue</c>).
    /// </summary>
    public RepositoryContextResolver(
        ILogger<RepositoryContextResolver> logger,
        IGitHubClientFactory? gitHubClientFactory = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gitHubClientFactory = gitHubClientFactory ?? IGitHubClientFactory.Default;
    }

    /// <summary>
    /// Opens the worktree as a git repository, reads the
    /// <paramref name="remoteName"/> remote, and returns the
    /// resolved context. Throws when the directory is not a git
    /// repository or the named remote is missing.
    /// </summary>
    public async Task<RepositoryContext> ResolveAsync(
        string worktreePath,
        string remoteName = DefaultRemoteName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(worktreePath);

        using var repo = new GitRepository(worktreePath);
        var remote = repo.Network.Remotes[remoteName]
            ?? throw new InvalidOperationException(
                $"Git repository at '{worktreePath}' has no '{remoteName}' remote. " +
                $"Add one with: git remote add {remoteName} <url>");

        var rawUrl = remote.Url;
        _logger.LogInformation("Git remote '{Remote}' of '{Worktree}' resolves to {Url}.", remoteName, worktreePath, rawUrl);

        var canonical = GitRemoteUrlNormalizer.Normalize(rawUrl);
        var gitHub = GitRemoteUrlNormalizer.TryParseGitHub(rawUrl);

        if (gitHub is not null)
        {
            _logger.LogInformation("Parsed GitHub remote as owner={Owner}, name={Name}.", gitHub.Owner, gitHub.Name);
            await TryVerifyWithOctokitAsync(gitHub, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            _logger.LogInformation(
                "Remote URL is not a GitHub URL; Octokit verification skipped. DevStack lookup will use '{Canonical}' verbatim.",
                canonical);
        }

        return new RepositoryContext(worktreePath, canonical, gitHub);
    }

    private async Task TryVerifyWithOctokitAsync(GitHubRepositoryRef gitHub, CancellationToken cancellationToken)
    {
        try
        {
            var client = _gitHubClientFactory.Create();
            var octokitRepo = await client.Repository.Get(gitHub.Owner, gitHub.Name).ConfigureAwait(false);
            _logger.LogInformation(
                "Octokit verified GitHub repository {Owner}/{Name}: defaultBranch={DefaultBranch}, stars={Stars}, private={Private}.",
                octokitRepo.Owner.Login, octokitRepo.Name, octokitRepo.DefaultBranch, octokitRepo.StargazersCount, octokitRepo.Private);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Best-effort: the DevStack side still has the canonical
            // URL to look up, so a flaky GitHub verify should not
            // sink the whole --show-plan flow.
            _logger.LogWarning(ex,
                "Octokit could not verify GitHub repository {Owner}/{Name}; continuing with the locally-known owner/name.",
                gitHub.Owner, gitHub.Name);
        }
    }
}

/// <summary>
/// Pure functions for turning a raw <c>git remote get-url</c> string
/// into a canonical URL and (when possible) a GitHub
/// <c>owner/name</c> pair. Kept separate from the resolver so it's
/// trivially unit-testable.
/// </summary>
public static class GitRemoteUrlNormalizer
{
    /// <summary>
    /// Returns the URL in <c>https://host/owner/repo[.git]</c> form
    /// when it can be parsed, or the original string otherwise (so
    /// non-GitHub hosts fall through to the DevStack lookup
    /// untouched). The <c>.git</c> suffix is preserved verbatim —
    /// the DevStack <c>Project.repository</c> field is convention
    /// rather than enforced, and existing projects tend to store
    /// the full clone URL (e.g. <c>…/personal-productivity-ai.git</c>);
    /// stripping it would silently miss the match.
    /// </summary>
    public static string Normalize(string remoteUrl)
    {
        if (string.IsNullOrWhiteSpace(remoteUrl))
        {
            throw new ArgumentException("remoteUrl must be non-empty", nameof(remoteUrl));
        }

        var parsed = TryParseGitHub(remoteUrl);
        if (parsed is not null)
        {
            // Re-attach the original tail so we keep any ".git" suffix
            // and the host casing from the input.
            return BuildCanonicalHttps(parsed, remoteUrl);
        }

        return remoteUrl.TrimEnd('/');
    }

    /// <summary>
    /// Returns the <c>owner/name</c> pair when
    /// <paramref name="remoteUrl"/> looks like a GitHub URL, or
    /// <c>null</c> for non-GitHub hosts. The parser handles both SSH
    /// (<c>git@github.com:owner/name.git</c>) and HTTPS
    /// (<c>https://github.com/owner/name[.git]</c>) forms, with or
    /// without the <c>.git</c> suffix. Other hosts return
    /// <c>null</c> so the caller can fall through to the verbatim
    /// DevStack lookup.
    /// </summary>
    public static GitHubRepositoryRef? TryParseGitHub(string remoteUrl)
    {
        if (string.IsNullOrWhiteSpace(remoteUrl))
        {
            return null;
        }

        var trimmed = remoteUrl.Trim();

        // SSH form: git@github.com:owner/name[.git]
        const string SshPrefix = "git@github.com:";
        if (trimmed.StartsWith(SshPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return ParseOwnerRepo(trimmed[SshPrefix.Length..]);
        }

        // HTTPS / HTTP form: https://github.com/owner/name[.git]
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
            && uri.Host.EndsWith("github.com", StringComparison.OrdinalIgnoreCase))
        {
            // Uri.LocalPath is "/owner/name" (or "/owner/name.git").
            var segments = uri.LocalPath.TrimStart('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length >= 2)
            {
                return new GitHubRepositoryRef(segments[0], StripDotGit(segments[1]));
            }
        }

        return null;
    }

    private static GitHubRepositoryRef? ParseOwnerRepo(string tail)
    {
        var segments = tail.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2)
        {
            return null;
        }
        return new GitHubRepositoryRef(segments[0], StripDotGit(segments[1]));
    }

    private static string StripDotGit(string name) =>
        name.EndsWith(".git", StringComparison.OrdinalIgnoreCase)
            ? name[..^4]
            : name;

    private static string BuildCanonicalHttps(GitHubRepositoryRef gh, string originalUrl)
    {
        // Reconstruct from the original to preserve ".git" / casing
        // without re-parsing. The full path component (including any
        // ".git") comes from the segment after the host.
        var trimmed = originalUrl.Trim();
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
            && uri.Host.EndsWith("github.com", StringComparison.OrdinalIgnoreCase))
        {
            var segments = uri.LocalPath.TrimStart('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length >= 2)
            {
                return $"https://github.com/{segments[0]}/{segments[1]}";
            }
        }
        // SSH form: just the original tail.
        return $"https://github.com/{gh.Owner}/{gh.Name}{(trimmed.EndsWith(".git", StringComparison.OrdinalIgnoreCase) ? ".git" : string.Empty)}";
    }
}

/// <summary>
/// Factory for <see cref="IGitHubClient"/>. Wraps a
/// <c>ProductHeaderValue</c> (Octokit requires every call to be
/// tagged with one) and an optional <c>GITHUB_TOKEN</c> from the
/// environment so authenticated requests get a 5000/hour budget
/// instead of the 60/hour anonymous budget.
/// </summary>
public interface IGitHubClientFactory
{
    /// <summary>Builds a fresh <see cref="IGitHubClient"/>.</summary>
    IGitHubClient Create();

    /// <summary>Default factory; honours <c>GITHUB_TOKEN</c> when set.</summary>
    static readonly IGitHubClientFactory Default = new EnvironmentGitHubClientFactory();
}

internal sealed class EnvironmentGitHubClientFactory : IGitHubClientFactory
{
    private static readonly ProductHeaderValue Product = new("DevStack.Agent", "1.0");

    public IGitHubClient Create()
    {
        var client = new GitHubClient(Product);
        var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if (!string.IsNullOrWhiteSpace(token))
        {
            // Octokit 14 exposes IConnection.Credentials as
            // Octokit.Credentials; setting it on the client is the
            // documented way to add an auth header to every call.
            client.Credentials = new Octokit.Credentials(token);
        }
        return client;
    }
}
