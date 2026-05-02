using BackgroundTaskQueue;
using BackgroundTaskQueueService;

namespace Shop.API;

internal class ShopBackgroundTaskQueuedHostedService(IBackgroundTaskQueue taskQueue, ILogger<ShopBackgroundTaskQueuedHostedService> logger) 
    : BackgroundTaskQueuedHostedService(taskQueue, logger)
{
}
