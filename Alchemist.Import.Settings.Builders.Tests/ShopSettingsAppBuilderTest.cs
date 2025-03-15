using Microsoft.Extensions.Hosting;

namespace Alchemist.Import.Settings.Builders.Tests;

public class ShopSettingsAppBuilderTest
{
    [Fact]
    public async Task BuildSettingsFromApp()
    {
        var builder = new HostApplicationBuilder();
        var shopSettingsAppBuilder = new ShopSettingsAppBuilder(builder, "SettingsAPIHost", "RestAPIHost");
        var host = builder.Build();
        var shopSettings = await shopSettingsAppBuilder.Build(host);
        Assert.NotEmpty(shopSettings);
        Assert.True(shopSettings.Count(s=>s.ProductShopImportSettings != null) >= 2);

        var ozonShop = shopSettings[0];
        ShopSettingsAsserts.AssertBrowserWebLoaderShopSettings(ozonShop.ProductShopImportSettings, 4);
        ShopSettingsAsserts.AssertBrowserWebLoaderShopSettings(ozonShop.CategoryShopImportSettings, 7);

        var goldAppleShop = shopSettings[1];
        ShopSettingsAsserts.AssertBrowserWebLoaderShopSettings(goldAppleShop.ProductShopImportSettings, 4);
        ShopSettingsAsserts.AssertBrowserWebLoaderShopSettings(goldAppleShop.CategoryShopImportSettings, 6);
    }
}