using BackgroundTaskQueue;
using Message.Interfaces;
using Microsoft.Extensions.Logging;

namespace Db.Infrastructure.Messages;

internal class CallbackBackgroundMessageHandler<T, TEvent> 
    : IEventHandler<T, TEvent>, ICallback<string>, IDisposable
    where TEvent : IIdentifiedEvent<T>
{
    private readonly IAcknowlegefulMessageSender _messageSender;

    private readonly IBackgroundTaskQueue _backgroundTaskQueue;

    public CallbackBackgroundMessageHandler(IAcknowlegefulMessageSender messageSender, IBackgroundTaskQueue backgroundTaskQueue)
    {
        _messageSender = messageSender;
        _backgroundTaskQueue = backgroundTaskQueue;

        _messageSender.AcknowlegeCallbackAsync += AcknowlegeCallbackAsync;
    }

    private async Task AcknowlegeCallbackAsync(object sender, AcknowlegeEventArgs eventArgs)
    {
        _callback?.Invoke(this, new EventArgs<string>(eventArgs.RequestId));
    }

    event EventHandler<EventArgs<string>>? _callback;
    event EventHandler<EventArgs<string>> ICallback<string>.Callback
    {
        add
        {
            _callback += value;
        }

        remove
        {
            _callback -= value;
        }
    }

    private async Task SendMessageAsync(T @event, string eventName, string eventId, ILogger logger, CancellationToken cancellationToken)
    {
        try
        {
            if (!_messageSender.IsConnected)
                await _messageSender.Start();

            await _messageSender.Send(eventName, eventId, @event, cancellationToken);

            LogNotificationInfo(logger, eventName);
        }
        catch (Exception ex)
        {
            LogNotificationError(logger, eventName, ex);
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
           new ValueTask(SendMessageAsync(notification.Entity, notification.EventName, notification.Id, logger, token)), cancellationToken);
    }

    public void Dispose()
    {
        _messageSender.AcknowlegeCallbackAsync -= AcknowlegeCallbackAsync;
    }
}