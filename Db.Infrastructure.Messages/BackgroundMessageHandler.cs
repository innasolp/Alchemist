using BackgroundTaskQueue;
using Message.Interfaces;
using Microsoft.Extensions.Logging;

namespace Db.Infrastructure.Messages;

internal class BackgroundMessageHandler<T, TEvent>(IMessageSender messageSender, IBackgroundTaskQueue backgroundTaskQueue)
    : IEventHandler<T, TEvent>
    where TEvent: IEvent<T>
{
    private readonly IMessageSender _messageSender = messageSender;

    private readonly IBackgroundTaskQueue _backgroundTaskQueue = backgroundTaskQueue;

    private async Task SendMessageAsync(T @event, string eventName, ILogger logger, CancellationToken cancellationToken)
    {
        try
        {
            if (!_messageSender.IsConnected)
                await _messageSender.Start(cancellationToken);

            await _messageSender.Send(@event, eventName, cancellationToken);

            BackgroundMessageHandler<T, TEvent>.LogNotificationInfo(logger, eventName);
        }
        catch (Exception ex)
        {
            BackgroundMessageHandler<T, TEvent>.LogNotificationError(logger, eventName, ex);
        }
    }

    private static void LogNotificationError(ILogger logger, string notificationName, Exception ex) => 
        logger.LogError(ex, "Error publishing notification: {NotificationName}", notificationName);

    private static void LogNotificationInfo(ILogger logger, string notificationName)
    {
        logger.LogInformation("Successfully published notification: {NotificationName}", notificationName);
    }

    public async Task Handle(TEvent notification, CancellationToken cancellationToken)
    {
        await _backgroundTaskQueue.QueueBackgroundWorkItemAsync((token, logger) =>
           new ValueTask(SendMessageAsync(notification.Entity, notification.EventName, logger, token)), cancellationToken);
    }
}