using Alchemist.Import.Settings.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Alchemist.Import.Settings.Adapter;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddSettingsDataAdapter<TProductShopImportSettings, TCategoryShopImportSettings, TImportServiceSettings>
        (this IServiceCollection services)
        where TProductShopImportSettings : class, IProductShopImportSettings
    where TCategoryShopImportSettings : class, ICategoryShopImportSettings
    where TImportServiceSettings : class, IImportServiceSettings, new()
    {
        services.AddSingleton<ISettingsAdapter, SettingsDataAdapter<TProductShopImportSettings, TCategoryShopImportSettings, TImportServiceSettings>>();
        return services.AddSingleton<ISettingsDataAdapter, SettingsDataAdapter<TProductShopImportSettings, TCategoryShopImportSettings, TImportServiceSettings>>();
    }
}
