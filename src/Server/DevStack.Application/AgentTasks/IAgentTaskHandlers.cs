using DevStack.Application;
using DevStack.Application.AgentTasks.Commands;

namespace DevStack.Application.AgentTasks;

public interface ICreateAgentTaskHandler : ICommandHandler<Guid, CreateAgentTaskCommand>
{
}

public interface IUpdateAgentTaskHandler : ICommandHandler<UpdateAgentTaskCommand>
{
}

public interface IUpdateAgentTaskStatusHandler : ICommandHandler<UpdateAgentTaskStatusCommand>
{
}

public interface IDeleteAgentTaskHandler : ICommandHandler<DeleteAgentTaskCommand>
{
}
