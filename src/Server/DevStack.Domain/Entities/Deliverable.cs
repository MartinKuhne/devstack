using DevStack.Domain.Services;

namespace DevStack.Domain.Entities;

public class Deliverable
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    [Required]
    public Guid ProjectId { get; private set; }

    [ForeignKey(nameof(ProjectId))]
    public virtual Project? Project { get; set; }

    [Required]
    public DeliverableType Type { get; private set; }

    [Required]
    [StringLength(200)]
    public string Title { get; private set; } = string.Empty;

    [Required]
    public DeliverableStatus Status { get; private set; }

    public string? Description { get; private set; }

    public string? Design { get; private set; }

    public string? AcceptanceCriteria { get; private set; }

    public string? ExecutionPlan { get; private set; }

    public string? AgentFeedback { get; private set; }

    public string? SecurityImpact { get; private set; }

    public string? PerformanceImpact { get; private set; }

    public string? TestPlan { get; private set; }

    public string? DeploymentPlan { get; private set; }

    public string? Blocking { get; private set; }

    public virtual ICollection<AgentTask> AgentTasks { get; set; } = new List<AgentTask>();

    public Deliverable()
    {
    }

    public Deliverable(
        Guid projectId,
        DeliverableType type,
        string title,
        DeliverableStatus status = DeliverableStatus.Draft,
        string? description = null,
        string? design = null,
        string? acceptanceCriteria = null,
        string? executionPlan = null,
        string? securityImpact = null,
        string? performanceImpact = null,
        string? testPlan = null,
        string? deploymentPlan = null,
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required", nameof(title));
        if (title.Length > 200)
            throw new ArgumentException("Title must be 200 characters or less", nameof(title));

        Id = id.HasValue && id.Value != Guid.Empty ? id.Value : Guid.NewGuid();
        ProjectId = projectId;
        Type = type;
        Title = title;
        Status = status;
        Description = description;
        Design = design;
        AcceptanceCriteria = acceptanceCriteria;
        ExecutionPlan = executionPlan;
        SecurityImpact = securityImpact;
        PerformanceImpact = performanceImpact;
        TestPlan = testPlan;
        DeploymentPlan = deploymentPlan;
    }

    public void UpdateMetadata(
        string? title = null,
        string? description = null,
        string? acceptanceCriteria = null,
        string? executionPlan = null,
        string? agentFeedback = null,
        string? securityImpact = null,
        string? performanceImpact = null,
        string? testPlan = null,
        string? deploymentPlan = null,
        string? blocking = null,
        string? design = null)
    {
        if (title is not null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be empty", nameof(title));
            if (title.Length > 200)
                throw new ArgumentException("Title must be 200 characters or less", nameof(title));
            Title = title;
        }

        if (description is not null) Description = description;
        if (acceptanceCriteria is not null) AcceptanceCriteria = acceptanceCriteria;
        if (executionPlan is not null) ExecutionPlan = executionPlan;
        if (agentFeedback is not null) AgentFeedback = agentFeedback;
        if (securityImpact is not null) SecurityImpact = securityImpact;
        if (performanceImpact is not null) PerformanceImpact = performanceImpact;
        if (testPlan is not null) TestPlan = testPlan;
        if (deploymentPlan is not null) DeploymentPlan = deploymentPlan;
        if (blocking is not null) Blocking = blocking;
        if (design is not null) Design = design;
    }

    // DRIFT-01 — the status machine is not applied in the write path: this assigns unconditionally.
    // Category:        invariant-violation (documented rules are not enforced)
    // Trigger path:    updateDeliverableStatus / update_deliverable_status
    //                  -> UpdateDeliverableStatusHandler calls deliverable.TransitionStatus(command.TargetStatus)
    //                  -> this method assigns Status with no guard, and SaveChangesAsync commits it
    // Why this is a real bug: StatusTransitionService.CanTransition (StatusTransitionService.cs:9) encodes the
    //                  intended rules and is covered by the unit suite, but a repository-wide search finds no
    //                  caller in the write path - only in DevStack.Tests.Unit. The rules therefore exist solely
    //                  for the tests, and an illegal transition such as Done -> Draft succeeds: Done has no
    //                  outgoing entries at all, so only Done -> Done is allowed.
    // Why this is NOT a false positive: the unit tests prove the rules were meant to gate transitions (they
    //                  assert CanTransition returns false for exactly these cases), and three shipped integration
    //                  scenarios perform transitions that AllowedTransitions rejects and assert that they
    //                  SUCCEED: Implement -> Done (AllowedTransitions offers only Merge, Plan and Failed from
    //                  Implement), Implement -> NeedsReview (NeedsReview is reachable only from Test) and
    //                  NeedsReview -> Rejected (Rejected is offered only from Draft, Design and Plan). The tool
    //                  description even advertises the guard ("The state machine enforces valid
    //                  transitions", Descriptions.DeliverableTools.UpdateDeliverableStatus), so the contract is
    //                  claimed but not delivered.
    // Concrete consequence: deliverables can be moved backwards or resurrected from terminal states with no
    //                  audit trail, and any client that relies on the advertised guard silently gets bad data.
    // Confidence:      Confirmed.
    // Fix direction:   guard the assignment - if (!StatusTransitionService.CanTransition(Status, targetStatus))
    //                  throw new InvalidOperationException(...) - or delete StatusTransitionService and stop
    //                  advertising the guard. Product-owner decision recorded as DRIFT-01.
    // Pinned by:       DevStack.Tests.Integration.GraphQL Features/DeliverableMutations.feature scenarios
    //                  "Transition deliverable status from Implement to Done", "Transition deliverable status to
    //                  NeedsReview" and "Transition deliverable status to Rejected", plus
    //                  Features/DeliverableAutoCompletion.feature. Those scenarios turn red the moment a guard
    //                  is added - reconcile them with the chosen rule set then.
    public void TransitionStatus(DeliverableStatus targetStatus)
    {
        Status = targetStatus;
    }
}
