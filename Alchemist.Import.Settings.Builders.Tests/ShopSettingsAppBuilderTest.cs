using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Alchemist.DataService.Interfaces;
using Alchemist.Import.Settings.Adapter;
using Moq;
using Alchemist.Import.Settings.Model;

namespace Alchemist.Import.Settings.Builders.Tests;

public class ShopSettingsAppBuilderTest
{
    private readonly Mock<IShopDataService> _shopApiClientMock = new();
    private readonly Mock<IShopSettingsDataService> _shopSettingsClientMock = new();
    

    [Fact]
    public async Task BuildSettingsFromApp()
    { 
        //todo setup for mocks
        var builder = new HostApplicationBuilder();
        builder.Services.AddSingleton(_shopApiClientMock.Object);
        builder.Services.AddSingleton(_shopSettingsClientMock.Object);
        builder.Services.AddSettingsDataAdapter<ProductShopImportSettings, CategoryShopImportSettings, ImportServiceSettings>();
        builder.Services.AddSettingsAppBuilder(0);
        var host = builder.Build();

        var shopSettingsAppBuilder = host.Services.GetRequiredService<ISettingsBuilder>() as ShopSettingsAppBuilder;

        var shopSettings = await shopSettingsAppBuilder.Build();
        Assert.NotNull(shopSettings);
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