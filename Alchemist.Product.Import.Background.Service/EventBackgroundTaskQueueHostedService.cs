using BackgroundTaskQueue;
using BackgroundTaskQueueService;

namespace Alchemist.Product.Import.Background.Service;

public class EventBackgroundTaskQueueHostedService([FromKeyedServices("EventBackgroundTaskQueue")] IBackgroundTaskQueue taskQueue,
    ILogger<EventBackgroundTaskQueueHostedService> logger) : BackgroundTaskQueuedHostedService(taskQueue, logger)
{
}