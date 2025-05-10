using Alchemist.DataService.Interfaces;
using Alchemist.Import.Settings.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace Alchemist.Import.Settings.DataAdapter.Tests;

public class SettingsDataAdapterLoadSettingsFromDB
{
    private readonly ISettingsDataAdapter _adapter;

    private readonly Mock<IShopSettingsDataService> _shopSettingsDataServiceMock = new();

    public SettingsDataAdapterLoadSettingsFromDB()
    {
        var builder = new HostApplicationBuilder();
        builder.Services.AddSingleton(_shopSettingsDataServiceMock.Object);
        builder.Services.AddSettingsDataAdapter<ProductShopImportSettings, CategoryShopImportSettings, ImportServiceSettings>();
        var host = builder.Build();

        //todo setup _shopSettingsDataServiceMock

        _adapter = host.Services.GetRequiredService<ISettingsDataAdapter>();
    }

    [Fact]
    public async Task LoadProductSettings()
    {        
        var shopSettings = await _adapter.GetShopImportSettings(1, Product.Interfaces.ShopSettingType.Product);
        Assert.NotNull(shopSettings);
        Assert.NotEmpty(shopSettings.Services);
        Assert.NotNull(shopSettings.RequestHeaders);
        Assert.NotNull(shopSettings.RequestHeaders.Value);
    }

    [Fact]
    public async Task LoadCategorySettings()
    {
        var shopSettings = await _adapter.GetShopImportSettings(1, Product.Interfaces.ShopSettingType.Category);
        Assert.NotNull(shopSettings);
        Assert.NotEmpty(shopSettings.Services);
    }
}