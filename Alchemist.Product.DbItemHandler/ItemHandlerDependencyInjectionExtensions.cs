using Alchemist.DataService.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Product.DbItemHandler;

public static class ItemHandlerDependencyInjectionExtensions
{  
    public static void AddShopCacheToServicesIfNeed(this IServiceCollection services)
    {
        var sd = services.FirstOrDefault(s => s.ServiceType == typeof(IShopCachedRepository));
        if (sd == null)
            services.AddSingleton<IShopCachedRepository>((serviceProvider) =>
            {
                var shopDataService = serviceProvider.GetRequiredService<IShopDataService>();
                return new ShopCachedRepository(shopDataService);
            });
    }
}