using Alchemist.DataService.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Alchemist.Product.DbItemHandler;

public static class ItemHandlerDependencyInjectionExtensions
{
    public static IServiceCollection AddProductItemHandler(this IServiceCollection services, string eventName)
    {
        AddShopCacheToServicesIfNeed(services);

        return services.AddSingleton<IImportItemHandler, ImportProductItemHandler>((serviceProvider) =>
        {
            var productDataService = serviceProvider.GetRequiredService<IProductDataService>();
            var shopDataService = serviceProvider.GetRequiredService<IShopDataService>();
            var shopCache = serviceProvider.GetRequiredService<IShopCache>();
            return new ImportProductItemHandler(productDataService, shopDataService, eventName, shopCache);
        });
    }

    private static void AddShopCacheToServicesIfNeed(IServiceCollection services)
    {
        var sd = services.FirstOrDefault(s => s.ServiceType == typeof(IShopCache));
        if (sd == null)
            services.AddSingleton<IShopCache>((serviceProvider) =>
            {
                var shopDataService = serviceProvider.GetRequiredService<IShopDataService>();
                return new ShopCache(shopDataService);
            });
    }

    public static IServiceCollection AddCategoryItemHandler(this IServiceCollection services, string eventName)
    {
        AddShopCacheToServicesIfNeed(services);

        return services.AddSingleton<IImportItemHandler, ImportCategoryItemHandler>((serviceProvider) =>
        {
            var shopDataService = serviceProvider.GetRequiredService<IShopDataService>();
            var shopCache = serviceProvider.GetRequiredService<IShopCache>();
            return new ImportCategoryItemHandler(shopDataService, eventName, shopCache);
        });
    }
}