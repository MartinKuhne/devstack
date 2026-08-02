using System;

namespace DevStack.Agent;

/// <summary>
/// Consumer-shaped view of a DevStack project. Decouples the CLI from the
/// StrawberryShake-generated GraphQL types so callers don't take a direct
/// dependency on the schema's wire shape.
/// </summary>
public sealed record ProjectSummary(
    Guid Id,
    string Name,
    string? Description,
    string Repository);
