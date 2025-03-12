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
        return services.AddSingleton<ISettingsDataAdapter<TProductShopImportSettings, TCategoryShopImportSettings, TImportServiceSettings>,
            SettingsDataAdapter<TProductShopImportSettings, TCategoryShopImportSettings, TImportServiceSettings>>();
    }
}
