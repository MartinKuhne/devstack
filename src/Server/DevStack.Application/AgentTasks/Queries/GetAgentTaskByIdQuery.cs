namespace DevStack.Application.AgentTasks.Queries;

public record GetAgentTaskByIdQuery(Guid Id);

public interface IGetAgentTaskByIdHandler : DevStack.Application.ICommandHandler<Domain.Entities.AgentTask, GetAgentTaskByIdQuery>
{
}
