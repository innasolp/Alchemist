using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Product.ImportSettingsWebApp.Infrastructure;
using Alchemist.Product.ImportSettingsWebApp.Models;
using Alchemist.Product.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Alchemist.Product.ImportSettingsWebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class ImportSettingsApiController([FromKeyedServices(ShopSettingType.Product)] ISettingsDataAdapter productSettingsDataAdapter,
    [FromKeyedServices(ShopSettingType.Category)] ISettingsDataAdapter categorySettingsDataAdapter) : Controller
{
    private readonly SettingsDataAdapterContainer _settingsDataAdapter = new(productSettingsDataAdapter, categorySettingsDataAdapter);

    [HttpPost]
    [Route("/Tab")]
    public async Task<IActionResult> ShopSettingsTab([FromBody] ShopSettingsData data)
    {
        var importSettings = await _settingsDataAdapter.GetShopImportSettingsModel(data.ShopId, (ShopSettingType)data.ShopSettingsType);

        HttpContext.Session.SetImportSettingtoSession(importSettings);

        return PartialView("~/Views/Shared/ShopImportSettingsTab.cshtml", importSettings);
    }
}
