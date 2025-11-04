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
    [Route("/Tab/{shopId:int}/{shopSettingsType:ShopSettingType}")]
    public async Task<IActionResult> ShopSettingsTab(int shopId, ShopSettingType shopSettingsType)
    {
        var importSettings = await _settingsDataAdapter.GetShopImportSettingsModel(shopId, shopSettingsType);

        HttpContext.Session.SetImportSettingToSession(importSettings);

        return PartialView("~/Views/Shared/ShopImportSettingsTab.cshtml", importSettings);
    }

    [HttpPost]
    [Route("/ImportSettingsApi/Import/Settings/{shopId:int}/{shopSettingsType:int}")]
    public IActionResult ShopImportSettings(int shopId, int shopSettingsType)
    {
        var model = new IndexModel
        {
            ShopId = shopId,
            ShopSettingType = (ShopSettingType)shopSettingsType
        };
        return PartialView("~/Views/Home/Index.cshtml", model);
    }

    [HttpPost]
    [Route("/ImportSettingsApi/Import/Settings")]
    public IActionResult Default()
    {
        return PartialView("~/Views/Home/Index.cshtml", new IndexModel());
    }
}
