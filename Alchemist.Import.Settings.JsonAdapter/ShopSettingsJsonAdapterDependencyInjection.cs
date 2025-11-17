using Alchemist.Import.Settings.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Import.Settings.JsonAdapter;

public static class ShopSettingsJsonAdapterDependencyInjection
{
    public static IServiceCollection AddSettingsJsonAdapter<TShopImportSettings>
        (this IServiceCollection services, string jsonFilePath)
        where TShopImportSettings : class, IShopImportSettings
    {
        return services.AddSingleton<ISettingsAdapter>(new ShopSettingsJsonAdapter<TShopImportSettings>(jsonFilePath));
    }

    public static IServiceCollection AddKeyedSettingsJsonAdapter<TShopImportSettings>
        (this IServiceCollection services, string jsonFilePath, object key)
        where TShopImportSettings : class, IShopImportSettings
    {
        return services.AddKeyedSingleton<ISettingsAdapter>(key, new ShopSettingsJsonAdapter<TShopImportSettings>(jsonFilePath));
    }
}
