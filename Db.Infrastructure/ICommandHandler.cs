namespace Db.Infrastructure;

public interface ICommandHandler
{
    Task Handle(object command, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<TCommand> 
    where TCommand : ICommand
{
    Task Handle(TCommand command, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<TCommand, TResult>
    where TCommand : ICommand
{
    Task<TResult> Handle(TCommand command, CancellationToken cancellationToken = default);
}