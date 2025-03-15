using Microsoft.Extensions.Hosting;

namespace Alchemist.Import.Settings.Builders.Tests;

public class ShopSettingsJsonBuilderTest
{
    [Fact]
    public async Task BuildSettingsFromJson()
    {
        var builder = new HostApplicationBuilder();
        var shopSettingsJsonBuilder = new ShopSettingsJsonBuilder("shopProducts.json", "shopCategories.json");
        var host = builder.Build();
        var shopSettings = await shopSettingsJsonBuilder.Build(host);
        Assert.NotEmpty(shopSettings);
        Assert.Equal(2, shopSettings.Count);
        Assert.Contains(shopSettings, s => s.ProductShopImportSettings != null);
        Assert.Contains(shopSettings, s => s.CategoryShopImportSettings != null);

        var ozonShop = shopSettings[0];
        ShopSettingsAsserts.AssertBrowserWebLoaderShopSettings(ozonShop.ProductShopImportSettings, 0);
        ShopSettingsAsserts.AssertBrowserWebLoaderShopSettings(ozonShop.CategoryShopImportSettings, 3);

        var goldAppleShop = shopSettings[1];
        ShopSettingsAsserts.AssertHttpRequestLoaderShopSettings(goldAppleShop.ProductShopImportSettings, 0);
        ShopSettingsAsserts.AssertBrowserWebLoaderShopSettings(goldAppleShop.CategoryShopImportSettings, 3);
    }
}
