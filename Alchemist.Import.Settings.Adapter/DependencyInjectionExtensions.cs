using Alchemist.DataService.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Import.Settings.DataAdapter;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddSettingsDataAdapter<TShopImportSettings, TImportServiceSettings>
        (this IServiceCollection services, ShopSettingType shopSettingType)
        where TShopImportSettings : class, IShopImportSettings, IShopSettings
    where TImportServiceSettings : class, IServiceSettings, IShopSettings
    {
        return services.AddSingleton<ISettingsAdapter, SettingsDataAdapter<TShopImportSettings, TImportServiceSettings>>(
            serviceProvider =>
        {
            var dataService = serviceProvider.GetRequiredService<IShopSettingsDataService>();
            return new SettingsDataAdapter<TShopImportSettings, TImportServiceSettings>(dataService, shopSettingType);
        });
    }

    public static IServiceCollection AddKeyedSettingsDataAdapter<TShopImportSettings, TImportServiceSettings>
        (this IServiceCollection services, ShopSettingType shopSettingType, object key)
        where TShopImportSettings : class, IShopImportSettings, IShopSettings
    where TImportServiceSettings : class, IServiceSettings, IShopSettings
    {
        return services.AddKeyedSingleton<ISettingsAdapter, SettingsDataAdapter<TShopImportSettings, TImportServiceSettings>>(
            key,
            (serviceProvider, key) =>
            {
                var dataService = serviceProvider.GetRequiredService<IShopSettingsDataService>();
                return new SettingsDataAdapter<TShopImportSettings, TImportServiceSettings>(dataService, shopSettingType);
            });
    }

    public static IServiceCollection AddTypedSettingsDataAdapter<TShopImportSettings, TImportServiceSettings>
        (this IServiceCollection services, ShopSettingType shopSettingType)
        where TShopImportSettings : class, IShopImportSettings, IShopSettings
    where TImportServiceSettings : class, IServiceSettings, IShopSettings
    {
        return services.AddSingleton<ISettingsDataAdapter, SettingsDataAdapter<TShopImportSettings, TImportServiceSettings>>(
            serviceProvider =>
            {
                var dataService = serviceProvider.GetRequiredService<IShopSettingsDataService>();
                return new SettingsDataAdapter<TShopImportSettings, TImportServiceSettings>(dataService, shopSettingType);
            });
    }

    public static IServiceCollection AddKeyedTypedSettingsDataAdapter<TShopImportSettings, TImportServiceSettings>
        (this IServiceCollection services, ShopSettingType shopSettingType, object key)
        where TShopImportSettings : class, IShopImportSettings, IShopSettings
    where TImportServiceSettings : class, IServiceSettings, IShopSettings
    {
        return services.AddKeyedSingleton<ISettingsDataAdapter, SettingsDataAdapter<TShopImportSettings, TImportServiceSettings>>(
            key,
            (serviceProvider, key) =>
            {
                var dataService = serviceProvider.GetRequiredService<IShopSettingsDataService>();
                return new SettingsDataAdapter<TShopImportSettings, TImportServiceSettings>(dataService, shopSettingType);
            });
    }
}
