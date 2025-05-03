using Alchemist.DataService.Interfaces;
using Alchemist.Import.Settings.Adapter;
using Alchemist.Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Settings.Builders;

public class ShopSettingsAppBuilder : ISettingsBuilder
{
    private readonly IShopDataService _shopDataService;
    private readonly ISettingsDataAdapter _settingsDataAdapter;
    private readonly ILogger<ShopSettingsAppBuilder> _logger;

    internal ShopSettingsAppBuilder(ILogger<ShopSettingsAppBuilder> logger, int priority, IShopDataService shopDataService, ISettingsDataAdapter settingsDataAdapter)
    {
        _shopDataService = shopDataService;
        _settingsDataAdapter = settingsDataAdapter;
        _logger = logger;
        Priority = priority;
    }

    public int Priority { get; }

    public async Task<List<ShopSettingsContainer>> Build()
    {
        try
        {
            var result = await GetResult();
            _logger.LogInformation("Shop import settings built from application.");
            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return await Task.FromResult(new List<ShopSettingsContainer>());
        }
    }

    private async Task<List<ShopSettingsContainer>> GetResult()
    {
        var result = new List<ShopSettingsContainer>();
        var shops = await _shopDataService.GetShops();
        foreach (var shop in shops)
        {
            var productShopSettings = await _settingsDataAdapter.GetShopSettings(shop.Id, Product.Interfaces.ShopSettingType.Product);
            var categoryShopSettings = await _settingsDataAdapter.GetShopSettings(shop.Id, Product.Interfaces.ShopSettingType.Category);
            result.Add(new ShopSettingsContainer(shop,
                productShopSettings as IProductShopImportSettings,
                categoryShopSettings as ICategoryShopImportSettings));
        }
        return result;
    }
}
