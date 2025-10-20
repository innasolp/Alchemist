using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Product.ImportSettingsWebApp.Infrastructure;
using Alchemist.Product.ImportSettingsWebApp.Models;
using Alchemist.Product.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Alchemist.Product.ImportSettingsWebApp.Controllers;

public abstract class SettingsController(ISettingsDataAdapter productSettingsDataAdapter, ISettingsDataAdapter categorySettingsDataAdapter) : Controller
{
    private readonly SettingsDataAdapterContainer _settingsDataAdapter = new(productSettingsDataAdapter, categorySettingsDataAdapter);

    protected async Task<ShopImportSettingsModel> GetShopImportSettingsAsync(int shopId, ShopSettingType shopSettingType)
    {
        var shopImportSettings = await HttpContext.Session.GetShopImportSettingsFromSessionAsync();
        if (shopImportSettings != null &&
                shopImportSettings.ShopId == shopId && shopImportSettings.ShopSettingType == shopSettingType)
            return shopImportSettings;

        shopImportSettings = await _settingsDataAdapter.GetShopImportSettingsAsync(shopId, shopSettingType);
        if (shopImportSettings == null)
        {
            shopImportSettings = ModelHelper.CreateShopImportSettingsModel(shopId, shopSettingType);
            shopImportSettings.ShopId = shopId;
        }

        return shopImportSettings;
    }
}
