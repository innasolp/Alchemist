using Alchemist.Import.Settings.Adapter;
using Alchemist.Import.Settings.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using BrowserDataLoader.Interfaces;
using WebLoader.Interfaces;

namespace Alchemist.Import.Factory;

public static class DependencyInjectionExtensions
{
    public static async Task<T> GetProductShopImportSettingsAsync<T>(this IServiceProvider serviceProvider, string key)
        where T: IShopImportSettings
    {
        var productShopImportSettings = serviceProvider.GetKeyedService<T>(key)
            ?? serviceProvider.GetServices<T>().FirstOrDefault(s => s.Name == key);

        if (productShopImportSettings != null)
            return productShopImportSettings;

        var settingsDataAdapter = serviceProvider.GetRequiredService<ISettingsDataAdapter>();
        var shopImportSettings = await settingsDataAdapter.GetShopImportSettings(key)
            ?? throw new InvalidDataException(key);

        if (shopImportSettings is not T result)
            throw new InvalidDataException($"Settings type for {key} is {shopImportSettings.ShopSettingType} ");

        return await Task.FromResult(result);
    }

    public static IBrowserDataLoader? GetBrowserDataLoader(this IServiceProvider serviceProvider, string key)
    {
       return serviceProvider.GetKeyedService<IBrowserDataLoader>(key)
            ?? serviceProvider.GetServices<IBrowserDataLoader>().FirstOrDefault(s => key == s.GetType().Name);
    }

    public static IWebLoaderFactory? GetWebLoaderFactory(this IServiceProvider serviceProvider, string key)
    {
        return serviceProvider.GetKeyedService<IWebLoaderFactory>(key)
           ?? serviceProvider.GetServices<IWebLoaderFactory>().FirstOrDefault(s => key == s.GetType().Name);
    }
}
