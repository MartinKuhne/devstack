namespace DevStack.OpenCode.Models;

/// <summary>
/// Discriminated union for a named reference, which may be a git repository,
/// a local directory, or a plain string shorthand.
/// </summary>
[JsonConverter(typeof(ReferenceConfigConverter))]
public sealed record ReferenceConfig
{
    private ReferenceConfig(ReferenceKind kind, object payload)
    {
        Kind = kind;
        Payload = payload;
    }

    /// <summary>Discriminator describing how <see cref="Payload"/> should be interpreted.</summary>
    public ReferenceKind Kind { get; }

    /// <summary>Underlying value.</summary>
    public object Payload { get; }

    /// <summary>Builds a git reference.</summary>
    public static ReferenceConfig FromGit(ReferenceGitConfig git) => new(ReferenceKind.Git, git);

    /// <summary>Builds a local reference.</summary>
    public static ReferenceConfig FromLocal(ReferenceLocalConfig local) => new(ReferenceKind.Local, local);

    /// <summary>Builds a string shorthand reference.</summary>
    public static ReferenceConfig FromString(string shorthand) => new(ReferenceKind.String, shorthand);

    /// <summary>Returns the underlying git config when <see cref="Kind"/> is <see cref="ReferenceKind.Git"/>.</summary>
    public ReferenceGitConfig? Git => Kind == ReferenceKind.Git ? (ReferenceGitConfig)Payload : null;

    /// <summary>Returns the underlying local config when <see cref="Kind"/> is <see cref="ReferenceKind.Local"/>.</summary>
    public ReferenceLocalConfig? Local => Kind == ReferenceKind.Local ? (ReferenceLocalConfig)Payload : null;

    /// <summary>Returns the underlying string shorthand when <see cref="Kind"/> is <see cref="ReferenceKind.String"/>.</summary>
    public string? Shorthand => Kind == ReferenceKind.String ? (string)Payload : null;
}
