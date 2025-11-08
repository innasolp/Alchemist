using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Product.ImportSettingsWebApp.Infrastructure;
using Alchemist.Product.ImportSettingsWebApp.Models;
using Alchemist.Product.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Alchemist.Product.ImportSettingsWebApp.Controllers;


[ApiExplorerSettings(IgnoreApi = true)]
public class ImportSettingsController : Controller
{
    private readonly ShopImportSettingsFacade _facade;

    public ImportSettingsController([FromKeyedServices(ShopSettingType.Product)] ISettingsDataAdapter productSettingsDataAdapter,
        [FromKeyedServices(ShopSettingType.Category)] ISettingsDataAdapter categorySettingsDataAdapter)
    {
        _facade = new ShopImportSettingsFacade(productSettingsDataAdapter, categorySettingsDataAdapter, this);
    }

    [Route("/Import/Settings/Tab/{shopId:int}/{shopSettingsType:ShopSettingType}")]
    [HttpPost]
    public async Task<IActionResult> ImportSettingsAsync(int shopId, ShopSettingType shopSettingsType)
    {
        var importSettings = await _facade.GetShopImportSettingsModel(shopId, shopSettingsType);

        HttpContext.Session.SetImportSettingToSession(importSettings);

        return PartialView("~/Views/Shared/ImportSettings.cshtml", importSettings);
    }

    [Route("/Import/Settings/Set/{shopId:int}/Product")]
    [HttpPost]
    public async Task<IActionResult> LoadProductShopImportSettings(int shopId, [FromBody]ProductShopImportSettingsModel data)
    {
        if (data == null)
            return BadRequest("Empty json for product shopsettings.");

        if(await _facade.GetCurrentShopImportSettingsAsync(shopId, ShopSettingType.Product) 
            is not ProductShopImportSettingsModel currentProductSettings)
            throw new InvalidOperationException("Invalid shopId");

        currentProductSettings.Update(data);

        HttpContext.Session.SetImportSettingToSession(currentProductSettings);

        return PartialView("~/Views/Shared/ImportSettings.cshtml", currentProductSettings);
    }

    [Route("/Import/Settings/Set/{shopId:int}/Category")]
    [HttpPost]
    public async Task<IActionResult> LoadCategoryShopImportSettings(int shopId, [FromBody]CategoryShopImportSettingsModel data)
    {
        if (data == null)
            return BadRequest("Empty json for category shopsettings.");

        if (await _facade.GetCurrentShopImportSettingsAsync(shopId, ShopSettingType.Category)
            is not CategoryShopImportSettingsModel currentCategorySettings)
            throw new InvalidOperationException("Invalid shopId");

        currentCategorySettings.Update(data);

        HttpContext.Session.SetImportSettingToSession(currentCategorySettings);

        return PartialView("~/Views/Shared/ImportSettings.cshtml", currentCategorySettings);
    }

    [Route("/Import/Settings/Product/IsChanged")]
    [HttpPost]
    public async Task<IActionResult> ProductSettingsIsChangedAsync(ProductShopImportSettingsModel data)
    {
        if (data == null)
            return BadRequest("Empty json for product shopsettings.");

        var result = await _facade.IsProductShopSettingsChangedAsync(data);

        return Ok(result);
    }

    [Route("/Import/Settings/Category/IsChanged")]
    [HttpPost]
    public async Task<IActionResult> CategorySettingsIsChangedAsync(CategoryShopImportSettingsModel data)
    {
        if (data == null)
            return BadRequest("Empty json for category shopsettings.");

        var result = await _facade.IsCategoryShopSettingsChangedAsync(data);

        return Ok(result);
    }

    [Route("/Import/Settings/Product/Save")]
    [HttpPost]
    public async Task<IActionResult> ProductSettingsSave(ProductShopImportSettingsModel data)
    {
        if (data == null) return BadRequest("Empty json for product shopsettings.");
        
        await _facade.SaveProductShopSettings(data);

        return Ok(true);
    }

    [Route("/Import/Settings/Category/Save")]
    [HttpPost]
    public async Task<IActionResult> CategorySettingsSave(CategoryShopImportSettingsModel data)
    {
        if (data == null) return BadRequest("Empty json for category shopsettings.");

        await _facade.SaveCategoryShopSettings(data);

        return Ok(true);
    }

    [Route("/Import/Settings/Product/CategoryUrl")]
    [HttpPost]
    public IActionResult RootCategory(CategoryUrlModel data)
    {
        if (data == null) return BadRequest("Empty json for category url.");

        return PartialView("~/Views/Home/RootCategoryUrl.cshtml", data);
    }

    [Route("/Import/Settings/Product/{shopId:int}/CategoryUrl/Set")]
    [HttpPost]
    public async Task<IActionResult> SetRootCategory(int shopId, [FromForm]CategoryUrlModel data)
    {
        if (data == null) return BadRequest("Empty json for category url.");

        if (await _facade.GetCurrentShopImportSettingsAsync(shopId, ShopSettingType.Product) is not ProductShopImportSettingsModel productShopSettings)
            return BadRequest("Invalid shopId");

        var result = productShopSettings.SetRootCategory(data);

        HttpContext.Session.SetImportSettingToSession(productShopSettings);

        return Ok(result);
    }

    [Route("/Import/Settings/Product/{shopId:int}/CategoryUrl/IsChanged")]
    [HttpPost]
    public async Task<IActionResult> RootCategoryIsChanged(int shopId, [FromBody]CategoryUrlModel data)
    {
        if (data == null) return BadRequest("Empty json for category url.");

        if (await _facade.GetCurrentShopImportSettingsAsync(shopId, ShopSettingType.Product) 
            is not ProductShopImportSettingsModel productShopSettings)
            return BadRequest("Invalid shopId");

        var currentRootCategory = productShopSettings.RootCategories.FirstOrDefault(c =>
                    c.Guid == data.Guid ||
                    (c.Url.Equals(data.Url, StringComparison.InvariantCultureIgnoreCase)
                    && c.Item == data.Item));

        return currentRootCategory == null ? Ok(!data.IsEmpty()) : Ok(!currentRootCategory.IsEquals(data));
    }

    [Route("/Import/Settings/Product/CategoryUrl/Item")]
    [HttpPost]
    public IActionResult RootCategoryItem([FromBody]CategoryUrlModel data)
    {
        if (data == null) return BadRequest("Empty json for category url.");

        return PartialView("~/Views/Home/RootCategoryUrlItem.cshtml", data);
    }
}
