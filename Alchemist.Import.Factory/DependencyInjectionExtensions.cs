using Alchemist.Import.Settings.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using BrowserDataLoader.Interfaces;
using WebLoader.Interfaces;
using Alchemist.Import.Settings.Builders;
using Alchemist.Product.Interfaces;

namespace Alchemist.Import.Factory;

public static class DependencyInjectionExtensions
{   
    public static async Task<IShopImportSettings?> GetAvailableSettingsWithHighestPriority(this IServiceProvider services, string shopSettingsName, ShopSettingType shopSettingType)
    {
        var settingsBuilders = services.GetServices<ISettingsBuilder>().OrderBy(b => b.Priority).ToList();
        foreach (var settingsBuilder in settingsBuilders)
        {
            var shopImportSettings = await settingsBuilder.Build(shopSettingsName, shopSettingType);
            if (shopImportSettings != null)
                return shopImportSettings;
        }

        return await Task.FromResult(default(IShopImportSettings));
    }

    public static IBrowserDataLoader? GetBrowserDataLoader(this IServiceProvider serviceProvider, string key)
    {
       return serviceProvider.GetKeyedService<IBrowserDataLoader>(key)
            ?? serviceProvider.GetServices<IBrowserDataLoader>().FirstOrDefault(s => key == s.GetType().Name);
    }

    public static IWebLoaderFactory? GetWebLoaderFactory(this IServiceProvider serviceProvider, string key)
    {
        return serviceProvider.GetKeyedService<IWebLoaderFactory>(key)
           ?? serviceProvider.GetServices<IWebLoaderFactory>().FirstOrDefault(s => key == s.GetType().Name)
           ?? serviceProvider.GetService<IWebLoaderFactory>();
    }
}
