using MediatR;

namespace Mediator.Infrastructure.Command;

public abstract class Command<T>(T entity) : IRequest<T>
{   
    public T Entity { get; } = entity;
}
