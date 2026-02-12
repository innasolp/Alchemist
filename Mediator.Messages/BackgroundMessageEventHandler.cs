using BackgroundTaskQueue;
using Mediator.Infrastructure;
using MediatR;
using Message.Interfaces;
using Microsoft.Extensions.Logging;

namespace Mediator.Messages;

public class BackgroundMessageEventHandler<TEvent, T>(IBackgroundTaskQueue backgroundTaskQueue, IMessageSender messageSender) 
    : MessageEventHandler<TEvent, T>(messageSender)
    where TEvent : Event<T>
{
    private readonly IBackgroundTaskQueue _backgroundTaskQueue = backgroundTaskQueue;

    public override async Task Handle(TEvent @event, CancellationToken cancellationToken = default)
    {
        var notificationName = @event.GetType().Name;

        await _backgroundTaskQueue.QueueBackgroundWorkItemAsync((token, logger) => 
            new ValueTask(HandleNotification(@event, logger, token, notificationName)), cancellationToken);        
    }

    private async Task HandleNotification(TEvent @event, ILogger logger, CancellationToken token, string notificationName)
    {
        try
        {
            await base.Handle(@event, token);

            LogNotificationInfo(logger, @event, notificationName);
        }
        catch (Exception ex)
        {
            LogNotificationError(logger, @event, notificationName, ex);
        }
    }

    private void LogNotificationError(ILogger logger, INotification notification, string notificationName, Exception ex)
    {
        if (notification is IEventMessage eventMessage)
        {
            var (messageFormat, args) = eventMessage.GetFailedMessage();
            logger.LogError(ex, messageFormat, args);
        }
        else
            logger.LogError(ex, "Error publishing notification: {NotificationName}", notificationName);
    }

    private void LogNotificationInfo(ILogger logger, INotification notification, string notificationName)
    {
        if (notification is IEventMessage eventMessage)
        {
            var (messageFormat, args) = eventMessage.GetSuccessEventMessage();
            logger.LogInformation(messageFormat, args);
        }
        else
            logger.LogInformation("Successfully published notification: {NotificationName}", notificationName);
    }
}