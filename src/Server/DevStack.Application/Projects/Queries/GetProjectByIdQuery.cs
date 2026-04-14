using DevStack.Domain.Entities;

namespace DevStack.Application.Projects.Queries;

public record GetProjectByIdQuery(Guid Id);

public interface IGetProjectByIdHandler
{
    Task<Project?> Handle(GetProjectByIdQuery query, CancellationToken cancellationToken = default);
}
