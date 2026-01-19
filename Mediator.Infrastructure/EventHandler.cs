using MediatR;
namespace Mediator.Infrastructure;

public abstract class EventHandler<TEvent> : INotificationHandler<TEvent>
    where TEvent : IEvent
{
    public abstract Task Handle(TEvent notification, CancellationToken cancellationToken = default);
}