using MediatR;
using Message.Interfaces;

namespace Mediator.Messages;

public class MessageEventHandler<TEvent, T>(IMessageSender messageSender) : INotificationHandler<TEvent>
    where TEvent : Event<T>
{
    protected IMessageSender MessageSender { get; } = messageSender;

    public virtual async Task Handle(TEvent @event, CancellationToken cancellationToken = default)
    {
        if (!MessageSender.IsConnected)
            await MessageSender.Start(cancellationToken);

        await MessageSender.Send(@event.Entity, @event.EventName, cancellationToken);
    }
}