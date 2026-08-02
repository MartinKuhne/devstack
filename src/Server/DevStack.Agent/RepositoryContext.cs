using System;

namespace DevStack.Agent;

/// <summary>
/// Resolved context for the repository the agent is running under.
/// <see cref="Worktree"/> is the absolute path of the local checkout
/// (the OpenCode server's <c>worktree</c> field, or the
/// <c>--repositoryRoot</c> override). <see cref="CanonicalRemoteUrl"/>
/// is the normalized GitHub-style URL used to look up the matching
/// DevStack project (<c>https://github.com/owner/repo</c> for a GitHub
/// remote, or the raw remote URL for non-GitHub hosts). <see cref="GitHub"/>
/// is populated when the remote is a GitHub URL; the agent uses it to
/// enrich the listing with a live Octokit lookup.
/// </summary>
public sealed record RepositoryContext(
    string Worktree,
    string CanonicalRemoteUrl,
    GitHubRepositoryRef? GitHub);

/// <summary>
/// Parsed <c>owner/repo</c> pair extracted from a GitHub remote URL.
/// <c>null</c> for non-GitHub remotes.
/// </summary>
public sealed record GitHubRepositoryRef(string Owner, string Name)
{
    /// <summary>Returns the canonical <c>https://github.com/owner/name</c> URL.</summary>
    public Uri ToCanonicalUri() => new($"https://github.com/{Owner}/{Name}");
}
