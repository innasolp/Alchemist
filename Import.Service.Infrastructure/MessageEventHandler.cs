using MediatR;
using Message.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Import.Service.Commands;

internal class MessageEventHandler<TEvent, T>([FromKeyedServices(ServiceKeys.EventMessageSenderKey)] IMessageSender messageSender)
    : INotificationHandler<TEvent>
    where TEvent : Event<T>
{
    private readonly IMessageSender _messageSender = messageSender;

    public async Task Handle(TEvent notification, CancellationToken cancellationToken)
    {
        await SendServiceMessageAsync(notification.EventName, notification.Message, cancellationToken);
    }

    protected async Task SendServiceMessageAsync(string eventName, T message, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_messageSender.IsConnected)
                await _messageSender.Start(cancellationToken);

            await _messageSender.Send(message, eventName, cancellationToken);
        }
        catch (Exception e)
        {
            throw new Exception($"Sending service message {message} for event {eventName} failed.", e);
        }
    }
}