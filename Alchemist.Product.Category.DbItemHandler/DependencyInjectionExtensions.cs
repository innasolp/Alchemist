using Alchemist.Product.DbItemHandler;
using Microsoft.Extensions.DependencyInjection;
using Shop.Interfaces;

namespace Alchemist.Product.Category.DbItemHandler;

public static class DependencyInjectionExtensions
{ 
    public static IServiceCollection AddCategoryItemHandler(this IServiceCollection services, string eventName)
    {
        services.AddShopCacheToServicesIfNeed();

        return services.AddSingleton<IImportItemHandler, ImportCategoryItemHandler>((serviceProvider) =>
        {
            var shopDataService = serviceProvider.GetRequiredService<IShopDataService>();
            var shopCachedRepository = serviceProvider.GetRequiredService<IShopCachedRepository>();
            return new ImportCategoryItemHandler(shopDataService, eventName, shopCachedRepository);
        });
    }
}