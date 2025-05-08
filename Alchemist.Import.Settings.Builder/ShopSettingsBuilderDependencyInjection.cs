using Alchemist.DataService.Interfaces;
using Alchemist.Import.Settings.Adapter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Settings.Builders;

public static class ShopSettingsBuilderDependencyInjection
{
    public static IServiceCollection AddSettingsAppBuilder(this IServiceCollection services, int priority)
    {
        return services.AddSingleton<ISettingsBuilder>((serviceProvider) =>
        {
            var shopDataService = serviceProvider.GetRequiredService<IShopDataService>();
            var settingsDataAdapter = serviceProvider.GetRequiredService<ISettingsDataAdapter>();
            var logger = serviceProvider.GetRequiredService<ILogger<ShopSettingsAppBuilder>>();
            return new ShopSettingsAppBuilder(logger, priority, shopDataService, settingsDataAdapter);
        });
    }

    [Obsolete]
    public static IServiceCollection AddSettingsJsonBuilder(this IServiceCollection services, int priority, string shopProductsJsonFile, string shopCategoriesJsonFile)
    {
        return services.AddSingleton<ISettingsBuilder>((serviceProvider) =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<ShopSettingsJsonBuilder>>();
            return new ShopSettingsJsonBuilder(logger, priority, shopProductsJsonFile, shopCategoriesJsonFile);
        });
    }

    public static IServiceCollection AddSettingsJsonBuilder(this IServiceCollection services, int priority, string[] jsonFiles)
    {
        return services.AddSingleton<ISettingsBuilder>((serviceProvider) =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<ShopSettingsJsonBuilder>>();
            return new ShopSettingsJsonBuilder(logger, priority, jsonFiles);
        });
    }
}
