using BackgroundTaskQueue;
using BackgroundTaskQueueService;

namespace Alchemist.Product.Import.Background.Service;

internal class ImportBackgroundTaskQueueHostedService([FromKeyedServices("ImportBackgroundTaskQueue")] IBackgroundTaskQueue taskQueue,
    ILogger<ImportBackgroundTaskQueueHostedService> logger) : BackgroundTaskQueuedHostedService(taskQueue, logger)
{
}