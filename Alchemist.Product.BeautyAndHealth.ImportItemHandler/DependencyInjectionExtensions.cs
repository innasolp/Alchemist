using Alchemist.BackgroundTaskQueue;
using Alchemist.Import.Factory.Products;
using Message.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Product.BeautyAndHealth.ImportItemHandler;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddImportProductMessageSender(this IServiceCollection services, Func<IServiceCollection, object, IServiceCollection> action)
    {
        return action(services, ServiceKeys.ImportProductMessageSenderKey);
    }

    public static IServiceCollection AddProductQueueItemHandlerFactory(this IServiceCollection services, object messageSenderKey, string methodName)
    {
        return services.AddSingleton<IProductItemHandlerFactory>((serviceProvider) =>
        {
            var messageSender = serviceProvider.GetRequiredKeyedService<IMessageSender>(messageSenderKey);
            var taskQueue = serviceProvider.GetRequiredService<IBackgroundTaskQueue>();
            return new BeautyAndHealthProductItemHandlerFactory(taskQueue, messageSender, methodName);
        });
    }
}