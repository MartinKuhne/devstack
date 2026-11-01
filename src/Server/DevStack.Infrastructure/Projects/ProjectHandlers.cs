using DevStack.Application.Projects.Commands;
using DevStack.Application.Projects.Queries;

namespace DevStack.Infrastructure.Projects;

public class UpdateProjectHandler : ICommandHandler<UpdateProjectCommand>
{
    private readonly DevStackDbContext _dbContext;

    public UpdateProjectHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects.FindAsync([request.Id], cancellationToken);
        if (project == null)
            throw new InvalidOperationException($"Project with ID {request.Id} not found.");

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            if (request.Name.Length > 200)
                throw new ArgumentException("Name must be 200 characters or less", nameof(request.Name));

            project.Name = request.Name;
        }

        if (request.Description is not null) project.Description = request.Description;
        if (request.Repository is not null) project.Repository = request.Repository;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class DeleteProjectHandler : ICommandHandler<DeleteProjectCommand>
{
    private readonly DevStackDbContext _dbContext;

    public DeleteProjectHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _dbContext.Projects.FindAsync([request.Id], cancellationToken);
        if (project == null)
            throw new InvalidOperationException($"Project with ID {request.Id} not found.");

        // DRIFT-13 — deleting a project silently destroys its whole subtree, with no confirmation surface.
        // Category:        data-loss (cascade scope vs. the mutation's apparent contract)
        // Trigger path:    mutation { deleteProject(id: P) }
        //                  -> this line marks the Project for deletion; no deliverable or task rows are loaded
        //                  -> EF cascades: DeliverableConfiguration declares Deliverable -> Project Cascade and
        //                     AgentTaskConfiguration.cs:58-61 declares AgentTask -> Project Cascade, so
        //                     SaveChangesAsync deletes every deliverable and every task underneath in one statement
        //                  -> the mutation returns true and the client has no way to learn how many rows went with it
        // Why this is a real bug: deleteProject is exposed as a single-row delete (it takes an id and returns a
        //                  boolean), but its blast radius is the entire project tree. A UI client that offers
        //                  "delete project" without a confirmation step destroys all recorded deliverable and task
        //                  history in one call, and the return value cannot distinguish "one row gone" from
        //                  "everything gone".
        // Why this is NOT a false positive: the cascade is not incidental - it is declared explicitly in both EF
        //                  configurations, and the integration suite verifies it by deleting a project that has
        //                  deliverables and tasks and then confirming every descendant row is gone. The same pattern
        //                  applies one level down: DeleteDeliverableHandler (DeliverableHandlers.cs:116) removes its
        //                  tasks the same way. Only AgentTask -> DependsOnAgentTask is SetNull
        //                  (AgentTaskConfiguration.cs:53-56), which shows the delete behaviours were chosen per
        //                  relationship rather than inherited by default.
        // Concrete consequence: one mistaken deleteProject from any client (the endpoint is anonymous) irreversibly
        //                  removes every deliverable and task recorded for that project.
        // Confidence:      Confirmed.
        // Fix direction:   decide the intended contract: keep the cascade and return a count of removed rows so the
        //                  caller can confirm, or switch to Restrict and require the tree to be emptied first.
        //                  Product-owner decision recorded as DRIFT-13.
        // Pinned by:       DevStack.Tests.Integration.GraphQL Features/GraphQLReferentialIntegrity.feature
        //                  (delete project cascades to deliverables and tasks; delete deliverable cascades to tasks).
        _dbContext.Projects.Remove(project);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public class GetProjectByIdHandler
{
    private readonly DevStackDbContext _dbContext;

    public GetProjectByIdHandler(DevStackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Project?> Handle(GetProjectByIdQuery query, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Projects.FindAsync([query.Id], cancellationToken);
    }
}
