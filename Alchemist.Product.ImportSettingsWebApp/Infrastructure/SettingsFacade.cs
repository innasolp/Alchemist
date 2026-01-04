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

    public async Task<ShopImportSettingsModel?> GetShopImportSettingsAsync(int shopId, ShopSettingType shopSettingType,
        CancellationToken cancellationToken = default)
    {
        switch (shopSettingType)
        {
            case ShopSettingType.Product:
                return await _productSettingsDataAdapter.GetShopImportSettings(shopId, cancellationToken) as ShopImportSettingsModel;

            case ShopSettingType.Category:
                return await _categorySettingsDataAdapter.GetShopImportSettings(shopId, cancellationToken) as ShopImportSettingsModel;

            default:
                throw new InvalidOperationException($"Invalid shopSettingType {shopSettingType}");
        }
    }

    protected async Task SaveAsync(ShopImportSettingsModel shopImportSettings, CancellationToken cancellationToken = default)
    {
        switch (shopImportSettings.ShopSettingType)
        {
            case ShopSettingType.Product:
                await _productSettingsDataAdapter.Save(shopImportSettings, cancellationToken);
                break;

            case ShopSettingType.Category:
                await _categorySettingsDataAdapter.Save(shopImportSettings, cancellationToken);
                break;
        }
    }

    public async Task<ShopImportSettingsModel> GetCurrentShopImportSettingsAsync(int shopId,
        ShopSettingType shopSettingType, 
        CancellationToken cancellationToken = default)
    {
        var shopImportSettings = await Controller.HttpContext.Session.GetShopImportSettingsFromSessionAsync(cancellationToken: cancellationToken);
        if (shopImportSettings != null &&
                shopImportSettings.ShopId == shopId && shopImportSettings.ShopSettingType == shopSettingType)
            return shopImportSettings;

        shopImportSettings = await GetShopImportSettingsAsync(shopId, shopSettingType, cancellationToken);
        if (shopImportSettings == null)
        {
            shopImportSettings = ModelHelper.CreateShopImportSettingsModel(shopId, shopSettingType);
            shopImportSettings.ShopId = shopId;
        }

        return shopImportSettings;
    }
}
