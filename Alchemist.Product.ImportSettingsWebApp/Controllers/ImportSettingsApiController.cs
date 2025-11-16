using Alchemist.Product.ImportSettingsWebApp.Models;
using Alchemist.Product.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Alchemist.Product.ImportSettingsWebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class ImportSettingsApiController : Controller
{    

    [HttpPost]
    [Route("/ImportSettingsApi/Import/Settings/{shopId:int}/{shopSettingsType:ShopSettingType}")]
    public IActionResult ShopImportSettings(int shopId, ShopSettingType shopSettingsType)
    {
        if (shopId < 0)
            return BadRequest($"shopId {shopId} is invalid.");

        if (shopSettingsType == ShopSettingType.Service)
            return BadRequest($"shopSettingsType {shopSettingsType} is invalid.");

        var model = new IndexModel
        {
            ShopId = shopId,
            ShopSettingType = shopSettingsType
        };
        return PartialView("~/Views/Shared/ShopImportSettingsTab.cshtml", model);
    }    

    [HttpPost]
    [Route("/ImportSettingsApi/Import/Settings")]
    public IActionResult Default()
    {
        return PartialView("~/Views/Shared/ShopImportSettingsTab.cshtml", new IndexModel());
    }
}
