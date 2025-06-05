using Alchemist.Import.Settings.Interfaces;

namespace Alchemist.Import.Settings.JsonAdapter.Tests;

public static class ShopSettingsAsserts
{
    public static void AssertBrowserWebLoaderShopSettings(IShopImportSettings shopSettings, int serviceCount)
    {
        Assert.NotNull(shopSettings.WebLoader);
        Assert.NotNull(shopSettings.RequestHeaders);
        Assert.NotNull(shopSettings.RequestHeaders.Value);
        Assert.NotNull(shopSettings.BrowserDataLoader);
        Assert.NotNull(shopSettings.ImportService);
        Assert.Equal(serviceCount, shopSettings.Services.Count);
    }

    public static void AssertHttpRequestLoaderShopSettings(IShopImportSettings shopSettings, int serviceCount)
    {
        Assert.NotNull(shopSettings.WebLoader);
        Assert.Null(shopSettings.RequestHeaders);
        Assert.Null(shopSettings.BrowserDataLoader);
        Assert.NotNull(shopSettings.ImportService);
        Assert.Equal(serviceCount, shopSettings.Services.Count);
    }
}
