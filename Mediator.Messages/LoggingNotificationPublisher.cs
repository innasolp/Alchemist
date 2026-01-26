using MediatR;
using Microsoft.Extensions.Logging;

namespace Mediator.Messages;

public class LoggingNotificationPublisher<TDefaultPublisher>(ILogger logger) : INotificationPublisher
    where TDefaultPublisher : class, INotificationPublisher, new()
{
    private readonly ILogger _logger = logger;
    
    private readonly INotificationPublisher _defaultPublisher = new TDefaultPublisher();

    public async Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification, CancellationToken cancellationToken = default)
    {
        var notificationName = notification.GetType().Name;
        
        try
        {
            await _defaultPublisher.Publish(handlerExecutors, notification, cancellationToken).ConfigureAwait(false);

            if (notification is IEventMessage eventMessage)
            {
                var (messageFormat, args) = eventMessage.GetSuccessEventMessage();
                _logger.LogInformation(messageFormat, args);
            }
            else
                _logger.LogInformation("Successfully published notification: {NotificationName}", notificationName);
        }
        catch (Exception ex)
        {
            if (notification is IEventMessage eventMessage)
            {
                var (messageFormat, args) = eventMessage.GetFailedMessage();
                _logger.LogError(ex, messageFormat, args);
            }
            else
                _logger.LogError(ex, "Error publishing notification: {NotificationName}", notificationName);            
        }
    }
}