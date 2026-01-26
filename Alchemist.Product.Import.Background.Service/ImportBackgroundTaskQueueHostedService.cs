using BackgroundTaskQueue;
using BackgroundTaskQueueService;

namespace Alchemist.Product.Import.Background.Service;

public class ImportBackgroundTaskQueueHostedService([FromKeyedServices("ImportBackgroundTaskQueue")] IBackgroundTaskQueue taskQueue,
    ILogger<ImportBackgroundTaskQueueHostedService> logger) : BackgroundTaskQueuedHostedService(taskQueue, logger)
{
}