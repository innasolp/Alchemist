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

    public static IServiceCollection AddProductItemHandler(this IServiceCollection services, object messageSenderKey, string methodName)
    {
        services.AddSingleton<ProductItemProcessor>();
        return services.AddSingleton<IProductItemHandler>((serviceProvider) =>
        {
            var messageSender = serviceProvider.GetRequiredKeyedService<IMessageSender>(messageSenderKey);
            var productItemProcessor = serviceProvider.GetRequiredService<ProductItemProcessor>();
            return new ProductItemHandler(messageSender, methodName, productItemProcessor);
        });
    }

    public static IServiceCollection AddCategoryItemHandler(this IServiceCollection services, object messageSenderKey, string methodName)
    {
        services.AddSingleton<CategoryItemProcessor>();
        return services.AddSingleton<ICategoryItemHandler>((serviceProvider) =>
        {
            var messageSender = serviceProvider.GetRequiredKeyedService<IMessageSender>(messageSenderKey);
            var categoryItemProcessor = serviceProvider.GetRequiredService<CategoryItemProcessor>();
            return new CategoryItemHandler(messageSender, methodName, categoryItemProcessor);
        });
    }
}