using Alchemist.Import.Settings.Interfaces;
using Alchemist.Import.Settings.Model;
using Alchemist.Product.Interfaces;
using Json.FileExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Alchemist.Import.Settings.JsonAdapter;

public class ShopSettingsJsonAdapter : ISettingsAdapter
{
    private readonly string _shopProductsJsonFile;
    private readonly string _shopCategoriesJsonFile;
    private readonly ILogger<ShopSettingsJsonAdapter> _logger;

    internal ShopSettingsJsonAdapter(ILogger<ShopSettingsJsonAdapter> logger,
        [FromKeyedServices($"{nameof(ShopSettingsJsonAdapter)}Products")] string shopProductsJsonFile,
        [FromKeyedServices($"{nameof(ShopSettingsJsonAdapter)}Categories")] string shopCategoriesJsonFile)
    {
        _shopProductsJsonFile = shopProductsJsonFile;
        _shopCategoriesJsonFile = shopCategoriesJsonFile;
        _logger = logger;        
    }

    public async Task<IShopImportSettings?> GetShopImportSettings(string shopSettingsName, ShopSettingType shopSettingType)
    {

        IShopImportSettings? shopImportSettings = shopSettingType == ShopSettingType.Product
            ? (await _shopProductsJsonFile.ReadFromJsonFileAsync<ProductShopImportSettings[]>())?.FirstOrDefault(s => s.Name == shopSettingsName)
            : (await _shopCategoriesJsonFile.ReadFromJsonFileAsync<CategoryShopImportSettings[]>())?.FirstOrDefault(s => s.Name == shopSettingsName);

        if (shopImportSettings != null)
            return await Task.FromResult(shopImportSettings);

        return null;
    }

    public async Task<List<IShopImportSettings>> GetAllShopImportSettings()
    {
        return (await _shopProductsJsonFile.ReadFromJsonFileAsync<ProductShopImportSettings[]>())?.OfType<IShopImportSettings>()
            .Union((await _shopCategoriesJsonFile.ReadFromJsonFileAsync<CategoryShopImportSettings[]>())?.OfType<IShopImportSettings>())
            .ToList();
    }
}