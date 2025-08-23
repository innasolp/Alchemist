using Alchemist.Import.Settings.Interfaces;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Import.Settings.Test.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Alchemist.Import.Settings.JsonAdapter.Tests;

public class ShopSettingsJsonAdapterTest
{
    private readonly ISettingsAdapter _productShopSettingsJsonAdapter;
    private readonly ISettingsAdapter _categoryShopSettingsJsonAdapter;

    public ShopSettingsJsonAdapterTest()
    {
        var builder = new HostApplicationBuilder();
        builder.Services.AddKeyedSettingsJsonAdapter<TestProductShopImportSettings>("shopProducts.json", "shopProducts");
        builder.Services.AddKeyedSettingsJsonAdapter<TestCategoryShopImportSettings>("shopCategories.json", "shopCategories");
        var host = builder.Build();

        _productShopSettingsJsonAdapter = host.Services.GetRequiredKeyedService<ISettingsAdapter>("shopProducts");
        _categoryShopSettingsJsonAdapter = host.Services.GetRequiredKeyedService<ISettingsAdapter>("shopCategories");
    }

    [Fact]
    public async Task GetAllProductShopImportSettingsSuccessWhenJsonFileIsValid()
    {
        var shopSettings = await _productShopSettingsJsonAdapter.GetAllShopImportSettings();
        Assert.NotNull(shopSettings);
        Assert.Equal(2, shopSettings.Count);
        Assert.Equal(2, shopSettings.Count(s => s.ShopSettingType == ShopSettingType.Product));
        Assert.Equal(0, shopSettings.Count(s => s.ShopSettingType == ShopSettingType.Category));

        var ozonProducts = shopSettings.Where(s=>s.ShopSettingType == ShopSettingType.Product && s.ShopUrl.Contains("ozon", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        Assert.NotNull(ozonProducts);
        ShopSettingsAsserts.AssertBrowserWebLoaderShopSettings(ozonProducts);
        Assert.Equal(0, ozonProducts.Services.OfType<IImportServiceSettings>().Where(s => !s.IsPrimary()).Count());

        var goldAppleProducts = shopSettings.Where(s => s.ShopSettingType == ShopSettingType.Product && s.ShopUrl.Contains("goldapple", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        Assert.NotNull(goldAppleProducts); 
        ShopSettingsAsserts.AssertHttpRequestLoaderShopSettings(goldAppleProducts);
        Assert.Equal(0, goldAppleProducts.Services.OfType<IImportServiceSettings>().Where(s => !s.IsPrimary()).Count());
    }

    [Fact]
    public async Task GetAllCategoryShopImportSettingsSuccessWhenJsonFileIsValid()
    {
        var shopSettings = await _categoryShopSettingsJsonAdapter.GetAllShopImportSettings();
        Assert.NotNull(shopSettings);
        Assert.Equal(2, shopSettings.Count);
        Assert.Equal(0, shopSettings.Count(s => s.ShopSettingType == ShopSettingType.Product));
        Assert.Equal(2, shopSettings.Count(s => s.ShopSettingType == ShopSettingType.Category));

        var ozonCategories = shopSettings.Where(s => s.ShopSettingType == ShopSettingType.Category && s.ShopUrl.Contains("ozon", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        Assert.NotNull(ozonCategories);
        ShopSettingsAsserts.AssertBrowserWebLoaderShopSettings(ozonCategories);
        Assert.Equal(3, ozonCategories.Services.OfType<IImportServiceSettings>().Where(s => !s.IsPrimary()).Count());

        var goldAppleCategories = shopSettings.Where(s => s.ShopSettingType == ShopSettingType.Category && s.ShopUrl.Contains("goldapple", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        Assert.NotNull(goldAppleCategories);
        ShopSettingsAsserts.AssertBrowserWebLoaderShopSettings(goldAppleCategories);
        Assert.Equal(3, goldAppleCategories.Services.OfType<IImportServiceSettings>().Where(s => !s.IsPrimary()).Count());
    }

    [Fact]
    public async Task GetShopImportSettingsProductsSuccessWhenLoadingFromJsonByValidNames()
    {
        var shopSettings = await _productShopSettingsJsonAdapter.GetShopImportSettings("Ozon");
        Assert.NotNull(shopSettings);
        Assert.Contains("ozon", shopSettings.ShopUrl);
    }

    [Fact]
    public async Task GetShopImportSettingsCategoriessSuccessWhenLoadingFromJsonByValidNames()
    {
        var shopSettings = await _categoryShopSettingsJsonAdapter.GetShopImportSettings("GoldAppleCategories");
        Assert.NotNull(shopSettings);
        Assert.Contains("goldapple", shopSettings.ShopUrl);
    }
}
