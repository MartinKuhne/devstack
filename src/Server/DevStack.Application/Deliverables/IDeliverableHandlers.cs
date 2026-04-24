using DevStack.Application.Deliverables.Commands;

namespace DevStack.Application.Deliverables;

public interface ICreateDeliverableHandler : ICommandHandler<Guid, CreateDeliverableCommand>
{
}

public interface IUpdateDeliverableHandler : ICommandHandler<UpdateDeliverableCommand>
{
}

public interface IUpdateDeliverableStatusHandler : ICommandHandler<UpdateDeliverableStatusCommand>
{
}

public interface IDeleteDeliverableHandler : ICommandHandler<DeleteDeliverableCommand>
{
}
