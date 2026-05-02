using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Import.Settings.Product;
using BackgroundTaskQueue;
using Import.Settings.Interfaces;
using Message.Interfaces;

namespace Alchemist.Product.BeautyAndHealth.ImportItemHandler;

internal class BeautyAndHealthProductItemHandlerFactory(IBackgroundTaskQueue backgroundTaskQueue, IMessageSender messageSender, string methodName)
    : IProductItemHandlerFactory
{
    public IProductItemHandler Create(IImportSettings importSettings)
    {
        var productJsonSettingsService = importSettings.GetRequiredService("ProductJsonSettings");
        var productJsonSettings = productJsonSettingsService.GetServiceValue<JsonSettings>();

        return new ProductQueueItemHandler(backgroundTaskQueue,messageSender, methodName, productJsonSettings);
    }
}