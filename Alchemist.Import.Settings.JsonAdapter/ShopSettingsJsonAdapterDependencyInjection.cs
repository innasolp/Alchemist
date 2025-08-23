using Alchemist.Import.Settings.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Settings.JsonAdapter;

public static class ShopSettingsJsonAdapterDependencyInjection
{
    public static IServiceCollection AddSettingsJsonAdapter<TShopImportSettings>
        (this IServiceCollection services, string jsonFilePath)
        where TShopImportSettings : class, IShopImportSettings
    {
        return services.AddSingleton<ISettingsAdapter>((serviceProvider) =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<ShopSettingsJsonAdapter<TShopImportSettings>>>();
            return new ShopSettingsJsonAdapter<TShopImportSettings>(logger, jsonFilePath);
        });
    }

    public static IServiceCollection AddKeyedSettingsJsonAdapter<TShopImportSettings>
        (this IServiceCollection services, string jsonFilePath, object key)
        where TShopImportSettings : class, IShopImportSettings
    {
        return services.AddKeyedSingleton<ISettingsAdapter>(key, (serviceProvider, key) =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<ShopSettingsJsonAdapter<TShopImportSettings>>>();
            return new ShopSettingsJsonAdapter<TShopImportSettings>(logger, jsonFilePath);
        });
    }
}
