using System;

namespace DevStack.Agent;

/// <summary>
/// Consumer-shaped view of a DevStack deliverable. Decouples the CLI from
/// the StrawberryShake-generated GraphQL types so callers don't take a
/// direct dependency on the schema's wire shape (or its enum casing).
/// </summary>
public sealed record DeliverableSummary(
    Guid Id,
    Guid ProjectId,
    string Type,
    string Title,
    string Status,
    string? Description);
