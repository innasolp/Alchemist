using Alchemist.Import.Settings.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Settings.JsonAdapter;

public static class ShopSettingsJsonAdapterDependencyInjection
{    
    public static IServiceCollection AddSettingsJsonAdapter(this IServiceCollection services, string shopProductsJsonFile, string shopCategoriesJsonFile)
    {
        return services.AddSingleton<ISettingsAdapter>((serviceProvider) =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<ShopSettingsJsonAdapter>>();
            return new ShopSettingsJsonAdapter(logger, shopProductsJsonFile, shopCategoriesJsonFile);
        });
    }
}
