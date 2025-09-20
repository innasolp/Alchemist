using Alchemist.DataService.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Product.Import.Model.Shop;

public static class ShopFacadeDependencyInjection
{
    public static IServiceCollection AddShopFacade(this IServiceCollection services, IShopModelFactory shopModelFactory)
    {
        return services.AddSingleton<IShopFacade>((serviceProvider) =>
        {
            var shopDataService = serviceProvider.GetRequiredService<IShopDataService>();
            return new ShopFacade(shopModelFactory, shopDataService);
        });
    }
}
