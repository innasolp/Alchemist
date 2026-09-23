using BackgroundTaskQueue;
using BackgroundTaskQueueService;
using Import.Service.Infrastructure;

namespace Alchemist.Product.Import.Background.Service;

internal class EventBackgroundTaskQueueHostedService([FromKeyedServices(ServiceKeys.EventBackgroundTaskQueue)] IBackgroundTaskQueue taskQueue,
    ILogger<EventBackgroundTaskQueueHostedService> logger) : BackgroundTaskQueuedHostedService(taskQueue, logger)
{
}