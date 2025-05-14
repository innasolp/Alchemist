using Alchemist.Import.Settings.Interfaces;
using Json.FileExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace Alchemist.Import.Settings.JsonAdapter;

public class ShopSettingsJsonAdapter<TProductShopImportSettings, TCategoryShopImportSettings> : ISettingsAdapter
    where TProductShopImportSettings : class, IProductShopImportSettings, new()
    where TCategoryShopImportSettings : class, ICategoryShopImportSettings, new()
{
    private readonly string _shopProductsJsonFile;
    private readonly string _shopCategoriesJsonFile;
    private readonly ILogger<ShopSettingsJsonAdapter<TProductShopImportSettings, TCategoryShopImportSettings>> _logger;

    internal ShopSettingsJsonAdapter(ILogger<ShopSettingsJsonAdapter<TProductShopImportSettings, TCategoryShopImportSettings>> logger,
        [FromKeyedServices($"{nameof(ShopSettingsJsonAdapter<TProductShopImportSettings, TCategoryShopImportSettings>)}Products")] string shopProductsJsonFile,
        [FromKeyedServices($"{nameof(ShopSettingsJsonAdapter<TProductShopImportSettings, TCategoryShopImportSettings>)}Categories")] string shopCategoriesJsonFile)
    {
        _shopProductsJsonFile = shopProductsJsonFile;
        _shopCategoriesJsonFile = shopCategoriesJsonFile;
        _logger = logger;        
    }

    public async Task<IShopImportSettings?> GetShopImportSettings(string shopSettingsName, ShopSettingType shopSettingType)
    {

        IShopImportSettings? shopImportSettings = shopSettingType == ShopSettingType.Product
            ? (await _shopProductsJsonFile.ReadFromJsonFileAsync<TProductShopImportSettings[]>())?.FirstOrDefault(s => s.Name == shopSettingsName)
            : (await _shopCategoriesJsonFile.ReadFromJsonFileAsync<TCategoryShopImportSettings[]>())?.FirstOrDefault(s => s.Name == shopSettingsName);

        if (shopImportSettings != null)
            return await Task.FromResult(shopImportSettings);

        return null;
    }

    public async Task<List<IShopImportSettings>> GetAllShopImportSettings()
    {
        return (await _shopProductsJsonFile.ReadFromJsonFileAsync<TProductShopImportSettings[]>())?.OfType<IShopImportSettings>()
            .Union((await _shopCategoriesJsonFile.ReadFromJsonFileAsync<TCategoryShopImportSettings[]>())?.OfType<IShopImportSettings>())
            .ToList();
    }
}