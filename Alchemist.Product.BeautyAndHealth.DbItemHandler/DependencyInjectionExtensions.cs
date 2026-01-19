using Alchemist.Product.DbItemHandler;
using Alchemist.Product.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Shop.Interfaces;

namespace Alchemist.Product.BeautyAndHealth.DbItemHandler;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddBeautyAndHealthProductItemHandler(this IServiceCollection services, string eventName)
    {
        services.AddShopCacheToServicesIfNeed();

        return services.AddSingleton<IImportItemHandler, BeautyAndHealthProductItemHandler>((serviceProvider) =>
        {
            var productDataService = serviceProvider.GetRequiredService<IProductDataService>();
            var shopDataService = serviceProvider.GetRequiredService<IShopDataService>();
            var shopCachedRepository = serviceProvider.GetRequiredService<IShopCachedRepository>();
            return new BeautyAndHealthProductItemHandler(productDataService, shopDataService, eventName, shopCachedRepository);
        });
    }
}