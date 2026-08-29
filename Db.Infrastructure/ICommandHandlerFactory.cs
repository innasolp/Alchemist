namespace Db.Infrastructure;

public interface ICommandHandlerFactory
{
    ICommandHandler<TCommand>? GetHandler<TCommand>() where TCommand :  ICommand;

    ICommandHandler<TCommand, TResult>? GetHandler<TCommand, TResult>() where TCommand :  ICommand;

    object? GetHandler(Type commandType);

    object? GetHandler(Type commandType, Type resultType);
}