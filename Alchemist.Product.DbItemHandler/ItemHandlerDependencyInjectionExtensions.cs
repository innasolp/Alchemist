using Alchemist.DataService.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Product.DbItemHandler;

public static class ItemHandlerDependencyInjectionExtensions
{
    public static IServiceCollection AddProductItemHandler(this IServiceCollection services, string eventName)
    {
        return services.AddSingleton<IImportItemHandler, ImportProductItemHandler>((serviceProvider) =>
        {
            var productDataService = serviceProvider.GetRequiredService<IProductDataService>();
            var shopDataService = serviceProvider.GetRequiredService<IShopDataService>();
            return new ImportProductItemHandler(productDataService, shopDataService, eventName);
        });
    }

    public static IServiceCollection AddCategoryItemHandler(this IServiceCollection services, string eventName)
    {
        return services.AddSingleton<IImportItemHandler, ImportCategoryItemHandler>((serviceProvider) =>
        {
            var shopDataService = serviceProvider.GetRequiredService<IShopDataService>();
            return new ImportCategoryItemHandler(shopDataService, eventName);
        });
    }
}
