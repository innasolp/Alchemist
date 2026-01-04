using Alchemist.DataService.Interfaces;
using Microsoft.Extensions.DependencyInjection;

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
            var shopCachedRepository = serviceProvider.GetRequiredService<IShopCachedRepository>();
            return new ImportProductItemHandler(productDataService, shopDataService, eventName, shopCachedRepository);
        });
    }

    private static void AddShopCacheToServicesIfNeed(IServiceCollection services)
    {
        var sd = services.FirstOrDefault(s => s.ServiceType == typeof(IShopCachedRepository));
        if (sd == null)
            services.AddSingleton<IShopCachedRepository>((serviceProvider) =>
            {
                var shopDataService = serviceProvider.GetRequiredService<IShopDataService>();
                return new ShopCachedRepository(shopDataService);
            });
    }

    public static IServiceCollection AddCategoryItemHandler(this IServiceCollection services, string eventName)
    {
        AddShopCacheToServicesIfNeed(services);

        return services.AddSingleton<IImportItemHandler, ImportCategoryItemHandler>((serviceProvider) =>
        {
            var shopDataService = serviceProvider.GetRequiredService<IShopDataService>();
            var shopCachedRepository = serviceProvider.GetRequiredService<IShopCachedRepository>();
            return new ImportCategoryItemHandler(shopDataService, eventName, shopCachedRepository);
        });
    }
}