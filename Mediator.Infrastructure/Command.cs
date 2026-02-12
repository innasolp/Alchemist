using MediatR;

namespace Mediator.Infrastructure;

public abstract class Command<T>(T entity) : IRequest<T>
{   
    public T Entity { get; } = entity;
}

public class CreateCommand<T>(T entity) : Command<T>(entity)
{
}

public class UpdateCommand<T>(T entity) : Command<T>(entity)
{
}