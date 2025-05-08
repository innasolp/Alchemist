using Alchemist.DataService.Interfaces;
using Alchemist.DependencyInjection.Common;
using Alchemist.Import.Settings.Model;
using Alchemist.Settings.RestAPIClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Alchemist.Import.Settings.Adapter.Tests;

public class SettingsDataAdapterLoadSettingsFromDB
{
    private readonly ISettingsDataAdapter _adapter;

    public SettingsDataAdapterLoadSettingsFromDB()
    {
        var builder = new HostApplicationBuilder();
        builder.AddRestApiClient<IShopSettingsDataService, SettingsAPIClient>("SettingsAPIHost", nameof(SettingsAPIClient));
        builder.Services.AddSettingsDataAdapter<ProductShopImportSettings, CategoryShopImportSettings, ImportServiceSettings>();
        var host = builder.Build();

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