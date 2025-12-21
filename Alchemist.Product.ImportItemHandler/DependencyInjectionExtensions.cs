using Alchemist.BackgroundTaskQueue;
using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Products.Interfaces;
using Message.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Product.ImportItemHandler;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddImportProductMessageSender(this IServiceCollection services, Func<IServiceCollection, object, IServiceCollection> action)
    {
        return action(services, ServiceKeys.ImportProductMessageSenderKey);
    }

    public static IServiceCollection AddImportCategoryMessageSender(this IServiceCollection services, Func<IServiceCollection, object, IServiceCollection> action)
    {
        return action(services, ServiceKeys.ImportCategoryMessageSenderKey);
    }

    public static IServiceCollection AddProductQueueItemHandler(this IServiceCollection services, object messageSenderKey, string methodName)
    {
        return services.AddSingleton<IProductItemHandler>((serviceProvider) =>
        {
            var messageSender = serviceProvider.GetRequiredKeyedService<IMessageSender>(messageSenderKey);
            var taskQueue = serviceProvider.GetRequiredService<IBackgroundTaskQueue>();
            return new ProductQueueItemHandler(taskQueue, messageSender, methodName);
        });
    }

    public static IServiceCollection AddCategoryQueueItemHandler(this IServiceCollection services, object messageSenderKey, string methodName)
    {
        return services.AddSingleton<ICategoryItemHandler>((serviceProvider) =>
        {
            var messageSender = serviceProvider.GetRequiredKeyedService<IMessageSender>(messageSenderKey);
            var taskQueue = serviceProvider.GetRequiredService<IBackgroundTaskQueue>();
            return new CategoryQueueItemHandler(taskQueue, messageSender, methodName);
        });
    }
}