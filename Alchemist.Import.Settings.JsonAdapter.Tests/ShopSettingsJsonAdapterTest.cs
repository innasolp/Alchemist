using Alchemist.Import.Settings.Interfaces;
using Alchemist.Import.Settings.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Alchemist.Import.Settings.JsonAdapter.Tests;

public class ShopSettingsJsonAdapterTest
{
    private readonly ISettingsAdapter _shopSettingsJsonAdapter;

    public ShopSettingsJsonAdapterTest()
    {
        var builder = new HostApplicationBuilder();
        builder.Services.AddSettingsJsonAdapter<ProductShopImportSettings, CategoryShopImportSettings>("shopProducts.json", "shopCategories.json");
        var host = builder.Build();

        _shopSettingsJsonAdapter = host.Services.GetRequiredService<ISettingsAdapter>() as ShopSettingsJsonAdapter<ProductShopImportSettings, CategoryShopImportSettings>;
    }

    [Fact]
    public async Task GetAllShopImportSettingsSuccessWhenJsonFileIsValid()
    {
        var shopSettings = await _shopSettingsJsonAdapter.GetAllShopImportSettings();
        Assert.NotNull(shopSettings);
        Assert.Equal(4, shopSettings.Count);
        Assert.Equal(2, shopSettings.Count(s => s.ShopSettingType == ShopSettingType.Product));
        Assert.Equal(2, shopSettings.Count(s => s.ShopSettingType == ShopSettingType.Category));

        var ozonProducts = shopSettings.Where(s=>s.ShopSettingType == ShopSettingType.Product && s.ShopUrl.Contains("ozon", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        Assert.NotNull(ozonProducts);
        ShopSettingsAsserts.AssertBrowserWebLoaderShopSettings(ozonProducts, 0);

        var ozonCategories = shopSettings.Where(s => s.ShopSettingType == ShopSettingType.Category && s.ShopUrl.Contains("ozon", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        Assert.NotNull(ozonCategories);
        ShopSettingsAsserts.AssertBrowserWebLoaderShopSettings(ozonCategories, 3);

        var goldAppleProducts = shopSettings.Where(s => s.ShopSettingType == ShopSettingType.Product && s.ShopUrl.Contains("goldapple", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        Assert.NotNull(goldAppleProducts); 
        ShopSettingsAsserts.AssertHttpRequestLoaderShopSettings(goldAppleProducts, 0);

        var goldAppleCategories = shopSettings.Where(s => s.ShopSettingType == ShopSettingType.Category && s.ShopUrl.Contains("goldapple", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        Assert.NotNull(goldAppleCategories);
        ShopSettingsAsserts.AssertBrowserWebLoaderShopSettings(goldAppleCategories, 3);
    }

    [Fact]
    public async Task GetShopImportSettingsProductsSuccessWhenLoadingFromJsonByValidNames()
    {
        var shopSettings = await _shopSettingsJsonAdapter.GetShopImportSettings("Ozon", ShopSettingType.Product);
        Assert.NotNull(shopSettings);
        Assert.Contains("ozon", shopSettings.ShopUrl);
    }

    [Fact]
    public async Task GetShopImportSettingsCategoriessSuccessWhenLoadingFromJsonByValidNames()
    {
        var shopSettings = await _shopSettingsJsonAdapter.GetShopImportSettings("GoldAppleCategories", ShopSettingType.Category);
        Assert.NotNull(shopSettings);
        Assert.Contains("goldapple", shopSettings.ShopUrl);
    }
}
