using Alchemist.Import.Settings.Interfaces;
using Alchemist.Import.Settings.JsonAdapter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Alchemist.Import.Settings.Builders.Tests;

public class ShopSettingsJsonAdapterTest
{
    [Fact]
    public async Task BuildSettingsFromJson()
    {
        var builder = new HostApplicationBuilder();
        builder.Services.AddSettingsJsonAdapter("shopProducts.json", "shopCategories.json");
        var host = builder.Build();

        var shopSettingsJsonBuilder = host.Services.GetRequiredService<ISettingsAdapter>() as ShopSettingsJsonAdapter;             
        
        var shopSettings = await shopSettingsJsonBuilder.GetAllShopImportSettings();
        Assert.NotNull(shopSettings);
        Assert.Equal(2, shopSettings.Count);
        Assert.Contains(shopSettings, s => s.ShopSettingType == Product.Interfaces.ShopSettingType.Product);
        Assert.Contains(shopSettings, s => s.ShopSettingType == Product.Interfaces.ShopSettingType.Category);

        //todo
        //var ozonShop = shopSettings[0];
        //ShopSettingsAsserts.AssertBrowserWebLoaderShopSettings(ozonShop.ProductShopImportSettings, 0);
        //ShopSettingsAsserts.AssertBrowserWebLoaderShopSettings(ozonShop.CategoryShopImportSettings, 3);

        //var goldAppleShop = shopSettings[1];
        //ShopSettingsAsserts.AssertHttpRequestLoaderShopSettings(goldAppleShop.ProductShopImportSettings, 0);
        //ShopSettingsAsserts.AssertBrowserWebLoaderShopSettings(goldAppleShop.CategoryShopImportSettings, 3);
    }
}
