using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.Interfaces;
using Import.Settings.Interfaces;

namespace Alchemist.Import.Settings.JsonAdapter.Tests;

public static class ShopSettingsAsserts
{
    public static void AssertBrowserWebLoaderShopSettings<T>(IShopImportSettings shopSettings)
        where T:class, IServiceSettings, IShopSettings
    {
        Assert.NotNull(shopSettings.GetWebLoader<T>());
        Assert.NotNull(shopSettings.GetRequestHeaders<T>());
        Assert.NotNull(shopSettings.GetRequestHeaders<T>().Value);
        Assert.NotNull(shopSettings.GetBrowserDataLoader<T>());
        Assert.NotNull(shopSettings.GetBrowserLauncher<T>());
        Assert.NotNull(shopSettings.GetImportService<T>());        
    }

    public static void AssertHttpRequestLoaderShopSettings<T>(IShopImportSettings shopSettings)
         where T : class, IServiceSettings, IShopSettings
    {
        Assert.NotNull(shopSettings.GetWebLoader<T>());
        Assert.Null(shopSettings.GetRequestHeaders<T>());
        Assert.Null(shopSettings.GetBrowserDataLoader<T>());
        Assert.Null(shopSettings.GetBrowserLauncher<T>());
        Assert.NotNull(shopSettings.GetImportService<T>());
    }
}