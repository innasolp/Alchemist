using Alchemist.Import.Settings.Interfaces;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Import.Settings.Test.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Alchemist.Product.Interfaces;

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
        var shopSettings = (await _productShopSettingsJsonAdapter.GetAllShopImportSettings()).ToDictionary(s=>s.Key, s=>s.Value as TestShopImportSettings);
        Assert.NotNull(shopSettings);
        Assert.Equal(2, shopSettings.Count());
        Assert.Equal(2, shopSettings.Count(s => s.Value.Type == ShopSettingType.Product));
        Assert.Equal(0, shopSettings.Count(s => s.Value.Type == ShopSettingType.Category));

        var ozonProducts = shopSettings.Where(s=>s.Value.Type == ShopSettingType.Product && s.Value.ShopUrl.Contains("ozon", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        Assert.NotNull(ozonProducts.Value);
        ShopSettingsAsserts.AssertBrowserWebLoaderShopSettings<TestImportServiceSettings>(ozonProducts.Value);
        Assert.Empty(ozonProducts.Value.Services.Values.OfType<TestImportServiceSettings>().Where(s => !s.Name.IsPrimaryServiceName()));

        var goldAppleProducts = shopSettings.Where(s => s.Value.Type == ShopSettingType.Product 
            && s.Value.ShopUrl.Contains("goldapple", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        Assert.NotNull(goldAppleProducts.Value); 
        ShopSettingsAsserts.AssertHttpRequestLoaderShopSettings<TestImportServiceSettings>(goldAppleProducts.Value);
        Assert.Empty(goldAppleProducts.Value.Services.Values.OfType<TestImportServiceSettings>().Where(s => !s.Name.IsPrimaryServiceName()));
    }

    [Fact]
    public async Task GetAllCategoryShopImportSettingsSuccessWhenJsonFileIsValid()
    {
        var allShopImportSettings = await _categoryShopSettingsJsonAdapter.GetAllShopImportSettings();
        var shopSettings = allShopImportSettings.ToDictionary(s=>s.Key, s=>s.Value as TestShopImportSettings);
        Assert.NotNull(shopSettings);
        Assert.Equal(2, shopSettings.Count());
        Assert.Equal(0, shopSettings.Count(s => s.Value.Type == ShopSettingType.Product));
        Assert.Equal(2, shopSettings.Count(s => s.Value.Type == ShopSettingType.Category));

        var ozonCategories = shopSettings.Where(s => s.Value.Type == ShopSettingType.Category && s.Value.ShopUrl.Contains("ozon", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        Assert.NotNull(ozonCategories.Value);
        ShopSettingsAsserts.AssertBrowserWebLoaderShopSettings<TestImportServiceSettings>(ozonCategories.Value);
        Assert.Equal(3, ozonCategories.Value.Services.Values.OfType < TestImportServiceSettings > ().Where(s => !s.Name.IsPrimaryServiceName()).Count());

        var goldAppleCategories = shopSettings.Where(s => s.Value.Type == ShopSettingType.Category &&
            s.Value.ShopUrl.Contains("goldapple", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        Assert.NotNull(goldAppleCategories.Value);
        ShopSettingsAsserts.AssertBrowserWebLoaderShopSettings<TestImportServiceSettings>(goldAppleCategories.Value);
        Assert.Equal(3, goldAppleCategories.Value.Services.Values.OfType<TestImportServiceSettings>().Where(s => !s.Name.IsPrimaryServiceName()).Count());
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
