namespace DevStack.Application;

public interface ICommandHandler<TReturn, TCommand>
{
    Task<TReturn> Handle(TCommand command, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<TCommand>
{
    Task Handle(TCommand command, CancellationToken cancellationToken = default);
}
