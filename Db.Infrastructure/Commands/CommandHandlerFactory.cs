using Microsoft.Extensions.DependencyInjection;

namespace Db.Infrastructure.Commands;

internal class CommandHandlerFactory(IServiceScopeFactory serviceScopeFactory) : ICommandHandlerFactory
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public ICommandHandler<TCommand>? GetHandler<TCommand>() where TCommand : ICommand
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var handlerType = typeof(ICommandHandler<>).MakeGenericType(typeof(TCommand));

        return scope.ServiceProvider.GetService(handlerType) as ICommandHandler<TCommand>;
    }

    public ICommandHandler<TCommand, TResult>? GetHandler<TCommand, TResult>() where TCommand : ICommand
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(typeof(TCommand), typeof(TResult));

        return scope.ServiceProvider.GetService(handlerType) as ICommandHandler<TCommand, TResult>;
    }

    public object? GetHandler(Type commandType)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var handlerType = typeof(ICommandHandler<>).MakeGenericType(commandType);

        return scope.ServiceProvider.GetService(handlerType);
    }

    public object? GetHandler(Type commandType, Type resultType)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, resultType);

        return scope.ServiceProvider.GetService(handlerType);
    }
}