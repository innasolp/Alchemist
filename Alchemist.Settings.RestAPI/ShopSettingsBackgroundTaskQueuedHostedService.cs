using BackgroundTaskQueue;
using BackgroundTaskQueueService;

namespace Alchemist.Settings.RestAPI;

internal class ShopSettingsBackgroundTaskQueuedHostedService(IBackgroundTaskQueue taskQueue, ILogger<ShopSettingsBackgroundTaskQueuedHostedService> logger) 
    : BackgroundTaskQueuedHostedService(taskQueue, logger)
{
}
