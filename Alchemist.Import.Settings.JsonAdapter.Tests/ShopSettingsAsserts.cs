using Alchemist.Import.Settings.Interfaces;
using Alchemist.Import.Settings.Extensions;

namespace Alchemist.Import.Settings.JsonAdapter.Tests;

public static class ShopSettingsAsserts
{
    public static void AssertBrowserWebLoaderShopSettings(IShopImportSettings shopSettings)
    {
        Assert.NotNull(shopSettings.GetWebLoader());
        Assert.NotNull(shopSettings.GetRequestHeaders());
        Assert.NotNull(shopSettings.GetRequestHeaders().Value);
        Assert.NotNull(shopSettings.GetBrowserDataLoader());
        Assert.NotNull(shopSettings.GetBrowserLauncher());
        Assert.NotNull(shopSettings.GetImportService());        
    }

    public static void AssertHttpRequestLoaderShopSettings(IShopImportSettings shopSettings)
    {
        Assert.NotNull(shopSettings.GetWebLoader());
        Assert.Null(shopSettings.GetRequestHeaders());
        Assert.Null(shopSettings.GetBrowserDataLoader());
        Assert.Null(shopSettings.GetBrowserLauncher());
        Assert.NotNull(shopSettings.GetImportService());
    }
}
