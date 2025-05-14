using Alchemist.Import.Settings.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Settings.JsonAdapter;

public static class ShopSettingsJsonAdapterDependencyInjection
{    
    public static IServiceCollection AddSettingsJsonAdapter<TProductShopImportSettings, TCategoryShopImportSettings>
        (this IServiceCollection services, string shopProductsJsonFile, string shopCategoriesJsonFile)
        where TProductShopImportSettings : class, IProductShopImportSettings, new()
    where TCategoryShopImportSettings : class, ICategoryShopImportSettings, new()
    {
        return services.AddSingleton<ISettingsAdapter>((serviceProvider) =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<ShopSettingsJsonAdapter<TProductShopImportSettings, TCategoryShopImportSettings>>>();
            return new ShopSettingsJsonAdapter<TProductShopImportSettings, TCategoryShopImportSettings>(logger, shopProductsJsonFile, shopCategoriesJsonFile);
        });
    }
}
