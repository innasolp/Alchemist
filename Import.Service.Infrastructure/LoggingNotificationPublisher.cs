using Mediator.Messages;
using MediatR;
using MediatR.NotificationPublishers;
using Microsoft.Extensions.Logging;

namespace Import.Service.Commands;

internal class LoggingNotificationPublisher(ILogger logger) : INotificationPublisher
{
    private readonly ILogger _logger = logger;
    
    private readonly INotificationPublisher _defaultPublisher = new ForeachAwaitPublisher();

    public async Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification, CancellationToken cancellationToken = default)
    {
        var notificationName = notification.GetType().Name;
        
        try
        {
            await _defaultPublisher.Publish(handlerExecutors, notification, cancellationToken).ConfigureAwait(false);

            if (notification is Event<ServiceMessage> @event)
                _logger.LogInformation($"Successfully published event {@event.EventName} for service {@event.Entity.Name} {@event.Entity.Guid}.");
            else 
                _logger.LogInformation("Successfully published notification: {NotificationName}", notificationName);
        }
        catch (Exception ex)
        {
            if (notification is Event<ServiceMessage> @event)
                _logger.LogError(ex, $"Notification event {@event.EventName} for service {@event.Entity.Name} {@event.Entity.Guid} failed.");
            else
                _logger.LogError(ex, "Error publishing notification: {NotificationName}", notificationName);            
        }
    }
}