using Alchemist.DataService.Interfaces;
using Alchemist.DependencyInjection.Common;
using Alchemist.Import.Settings.Adapter;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Import.Settings.Model;
using Alchemist.Product.RestAPIClient;
using Alchemist.Settings.RestAPIClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Settings.Builders;

public class ShopSettingsAppBuilder : ISettingsBuilder
{
    public ShopSettingsAppBuilder(IHostApplicationBuilder builder, string settingsApiHostSection,
            string shopApiHostSection)
    {

        builder.AddRestApiClient<IShopSettingsDataService, SettingsAPIClient>(settingsApiHostSection, nameof(SettingsAPIClient));
        builder.Services.AddSettingsDataAdapter<ProductShopImportSettings, CategoryShopImportSettings, ImportServiceSettings>();

        builder.AddRestApiClient<IShopDataService, ShopApiClient>(shopApiHostSection, nameof(ShopApiClient));
    }

    public async Task<List<IShopImportData>> Build(IHost host)
    {
        var logger = host.Services.GetRequiredService<ILogger<ShopSettingsAppBuilder>>();
        try
        {
            var settingsAdapter = host.Services.GetRequiredService<ISettingsDataAdapter<ProductShopImportSettings, CategoryShopImportSettings, ImportServiceSettings>>();
            var shopApiClient = host.Services.GetRequiredService<IShopDataService>();

            var result = await GetResult(settingsAdapter, shopApiClient);
            logger.LogInformation("Shop import settings built from application.");
            return result;
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return await Task.FromResult(new List<IShopImportData>());
        }
    }

    private static async Task<List<IShopImportData>> GetResult(ISettingsDataAdapter<ProductShopImportSettings, CategoryShopImportSettings, ImportServiceSettings> settingsAdapter,
        IShopDataService shopApiClient)
    {
        var result = new List<IShopImportData>();
        var shops = await shopApiClient.GetShops();
        foreach (var shop in shops)
        {
            var productShopSettings = await settingsAdapter.GetShopSettings(shop.Id, Product.Interfaces.ShopSettingType.Product);
            var categoryShopSettings = await settingsAdapter.GetShopSettings(shop.Id, Product.Interfaces.ShopSettingType.Category);
            var shopUrl = await shopApiClient.GetShopUrl(shop.Id);
            result.Add(new ShopImportData(shop, shopUrl,
                productShopSettings as IProductShopImportSettings,
                categoryShopSettings as ICategoryShopImportSettings));
        }
        return result;
    }

}
