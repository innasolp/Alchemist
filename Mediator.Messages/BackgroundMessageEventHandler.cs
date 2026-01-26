using BackgroundTaskQueue;
using MediatR;
using Message.Interfaces;
using Microsoft.Extensions.Logging;

namespace Mediator.Messages;

public class BackgroundMessageEventHandler<TEvent, T>(ILogger logger, IBackgroundTaskQueue backgroundTaskQueue, IMessageSender messageSender) 
    : MessageEventHandler<TEvent, T>(messageSender)
    where TEvent : Event<T>
{
    private readonly ILogger _logger = logger;

    private readonly IBackgroundTaskQueue _backgroundTaskQueue = backgroundTaskQueue;

    public override async Task Handle(TEvent @event, CancellationToken cancellationToken = default)
    {
        var notificationName = @event.GetType().Name;

        await _backgroundTaskQueue.QueueBackgroundWorkItemAsync((token, logger) => 
            new ValueTask(HandleNotification(@event, token, notificationName)), cancellationToken);        
    }

    private async Task HandleNotification(TEvent @event, CancellationToken token, string notificationName)
    {
        try
        {
            await base.Handle(@event, token);

            LogNotificationInfo(@event, notificationName);
        }
        catch (Exception ex)
        {
            LogNotificationError(@event, notificationName, ex);
        }
    }

    private void LogNotificationError(INotification notification, string notificationName, Exception ex)
    {
        if (notification is IEventMessage eventMessage)
        {
            var (messageFormat, args) = eventMessage.GetFailedMessage();
            _logger.LogError(ex, messageFormat, args);
        }
        else
            _logger.LogError(ex, "Error publishing notification: {NotificationName}", notificationName);
    }

    private void LogNotificationInfo(INotification notification, string notificationName)
    {
        if (notification is IEventMessage eventMessage)
        {
            var (messageFormat, args) = eventMessage.GetSuccessEventMessage();
            _logger.LogInformation(messageFormat, args);
        }
        else
            _logger.LogInformation("Successfully published notification: {NotificationName}", notificationName);
    }
}