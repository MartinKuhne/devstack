using DevStack.Domain.Entities;

namespace DevStack.Application.Deliverables.Queries;

public record GetDeliverableByIdQuery(Guid Id);

public interface IGetDeliverableByIdHandler
{
    Task<Deliverable?> Handle(GetDeliverableByIdQuery query, CancellationToken cancellationToken = default);
}
