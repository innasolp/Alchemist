using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Product.ImportSettingsWebApp.Models;
using Alchemist.Product.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Alchemist.Product.ImportSettingsWebApp.Infrastructure;

internal abstract class SettingsFacade(ISettingsDataAdapter productSettingsDataAdapter,
    ISettingsDataAdapter categorySettingsDataAdapter, Controller controller)
{
    private readonly ISettingsDataAdapter _productSettingsDataAdapter = productSettingsDataAdapter;

    private readonly ISettingsDataAdapter _categorySettingsDataAdapter = categorySettingsDataAdapter;

    protected Controller Controller { get; } = controller;

    public async Task<ShopImportSettingsModel?> GetShopImportSettingsAsync(int shopId, ShopSettingType shopSettingType)
    {
        switch (shopSettingType)
        {
            case ShopSettingType.Product:
                return await _productSettingsDataAdapter.GetShopImportSettings(shopId) as ShopImportSettingsModel;

            case ShopSettingType.Category:
                return await _categorySettingsDataAdapter.GetShopImportSettings(shopId) as ShopImportSettingsModel;

            default:
                throw new InvalidOperationException($"Invalid shopSettingType {shopSettingType}");
        }
    }

    protected async Task SaveAsync(ShopImportSettingsModel shopImportSettings)
    {
        switch (shopImportSettings.ShopSettingType)
        {
            case ShopSettingType.Product:
                await _productSettingsDataAdapter.Save(shopImportSettings);
                break;

            case ShopSettingType.Category:
                await _categorySettingsDataAdapter.Save(shopImportSettings);
                break;
        }
    }

    public async Task<ShopImportSettingsModel> GetCurrentShopImportSettingsAsync(int shopId, ShopSettingType shopSettingType)
    {
        var shopImportSettings = await Controller.HttpContext.Session.GetShopImportSettingsFromSessionAsync();
        if (shopImportSettings != null &&
                shopImportSettings.ShopId == shopId && shopImportSettings.ShopSettingType == shopSettingType)
            return shopImportSettings;

        shopImportSettings = await GetShopImportSettingsAsync(shopId, shopSettingType);
        if (shopImportSettings == null)
        {
            shopImportSettings = ModelHelper.CreateShopImportSettingsModel(shopId, shopSettingType);
            shopImportSettings.ShopId = shopId;
        }

        return shopImportSettings;
    }
}
