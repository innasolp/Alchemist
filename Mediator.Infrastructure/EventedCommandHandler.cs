using Mediator.Infrastructure.Events;
using MediatR;
namespace Mediator.Infrastructure;

public class EventedCommandHandler<T, TCommand, TEvent, TCommandHandler>(TCommandHandler handler, IPublisher eventPublisher, string eventName)
    : IRequestHandler<TCommand, T>
     where TCommand : Command<T>
    where TEvent : Event<T>, new()
    where TCommandHandler : ICommandHandler<TCommand, T>
{
    protected TCommandHandler Handler { get; } = handler;

    protected IPublisher EventPublisher { get; } = eventPublisher;

    public async Task<T> Handle(TCommand request, CancellationToken cancellationToken)
    {
        var result = await Handler.Handle(request, cancellationToken);
        await EventPublisher.Publish(new TEvent() { Entity = result , CreationDate = DateTime.UtcNow, EventName = eventName}, cancellationToken);
        return result;
    }
}

public class CreateEventedCommandHandler<T, TCreateCommandHandler>(TCreateCommandHandler handler, IPublisher eventPublisher, string eventName)
    : EventedCommandHandler<T, CreateCommand<T>, CreationEvent<T>, TCreateCommandHandler>(handler,eventPublisher, eventName)
    where TCreateCommandHandler : ICreateCommandHandler<CreateCommand<T>,T>
{}

public class CreateEventedCommandHandler<T>(ICreateCommandHandler<T> handler, IPublisher eventPublisher, string eventName)
    : CreateEventedCommandHandler<T, ICreateCommandHandler<T>>(handler,eventPublisher, eventName)    
{}

public class UpdateEventedCommandHandler<T, TUpdateCommandHandler>(TUpdateCommandHandler handler, IPublisher eventPublisher, string eventName)
    : EventedCommandHandler<T, UpdateCommand<T>, UpdateEvent<T>, TUpdateCommandHandler>(handler,eventPublisher, eventName)
    where TUpdateCommandHandler : IUpdateCommandHandler<UpdateCommand<T>,T>
{}

public class UpdateEventedCommandHandler<T>(IUpdateCommandHandler<T> handler, IPublisher eventPublisher, string eventName)
    : UpdateEventedCommandHandler<T, IUpdateCommandHandler<T>>(handler,eventPublisher, eventName)
{}