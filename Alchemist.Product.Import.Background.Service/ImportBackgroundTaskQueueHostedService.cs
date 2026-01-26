using BackgroundTaskQueue;
using BackgroundTaskQueueService;

namespace Alchemist.Product.Import.Background.Service;

public class ImportBackgroundTaskQueueHostedService([FromKeyedServices("ImportBackgroundTaskQueue")] IBackgroundTaskQueue taskQueue,
    ILogger<EventBackgroundTaskQueueHostedService> logger) : BackgroundTaskQueuedHostedService(taskQueue, logger)
{
}