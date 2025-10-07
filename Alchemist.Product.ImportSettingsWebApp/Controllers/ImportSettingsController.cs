using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Product.ImportSettingsWebApp.Models;
using Alchemist.Product.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Alchemist.Product.ImportSettingsWebApp.Controllers;

public class ShopSettingsData
{
    public int? ShopId { get; set; }

    public int ShopSettingsType { get; set; }
}

public class ImportSettingsController([FromKeyedServices(ShopSettingType.Product)] ISettingsDataAdapter productSettingsDataAdapter,
    [FromKeyedServices(ShopSettingType.Category)] ISettingsDataAdapter categorySettingsDataAdapter) : Controller
{
    private readonly SettingsDataAdapterContainer _settingsDataAdapter = new(productSettingsDataAdapter, categorySettingsDataAdapter);

    [Route("/Import/SettingsTab/")]
    [HttpPost]
    public async Task<IActionResult> ImportSettingsTabAsync([FromBody]ShopSettingsData data)
    {
        var importSettings = data.ShopId != null
            ? await _settingsDataAdapter.GetShopImportSettingsAsync((int)data.ShopId, (ShopSettingType)data.ShopSettingsType)
                ?? ModelHelper.GetShopImportSettingsModel((ShopSettingType)data.ShopSettingsType)
            : ModelHelper.GetShopImportSettingsModel((ShopSettingType)data.ShopSettingsType);

        return PartialView("~/Views/Shared/ShopImportSettingsTab.cshtml", importSettings);
    }
}
