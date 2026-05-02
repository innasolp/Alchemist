using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Product.ImportSettingsWebApp.Infrastructure;
using Alchemist.Product.ImportSettingsWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using ShopSettings.Interfaces;

namespace Alchemist.Product.ImportSettingsWebApp.Controllers;


[ApiExplorerSettings(IgnoreApi = true)]
public class ImportSettingsActionController : Controller
{
    private readonly ShopImportSettingsFacade _facade;

    public ImportSettingsActionController([FromKeyedServices(ShopSettingType.Product)] ISettingsDataAdapter productSettingsDataAdapter,
        [FromKeyedServices(ShopSettingType.Category)] ISettingsDataAdapter categorySettingsDataAdapter)
    {
        _facade = new ShopImportSettingsFacade(productSettingsDataAdapter, categorySettingsDataAdapter, this);
    }

    [Route($"/{ControllerPrefix.Action}/{{shopId:int}}/{{shopSettingsType:ShopSettingType}}")]
    [HttpPost]
    public async Task<IActionResult> ImportSettingsAsync(int shopId, ShopSettingType shopSettingsType, CancellationToken cancellationToken = default)
    {
        var importSettings = await _facade.GetShopImportSettingsModel(shopId, shopSettingsType, cancellationToken: cancellationToken);

        HttpContext.Session.SetImportSettingToSession(importSettings);

        return PartialView("~/Views/Shared/ImportSettings.cshtml", importSettings);
    }

    [Route($"/{ControllerPrefix.Action}/Set/{{shopId:int}}/Product")]
    [HttpPost]
    public async Task<IActionResult> LoadProductShopImportSettings(int shopId, [FromBody]ProductShopImportSettingsModel data, CancellationToken cancellationToken = default)
    {
        if (data == null)
            return BadRequest("Empty json for product shopsettings.");

        if(await _facade.GetCurrentShopImportSettingsAsync(shopId, ShopSettingType.Product, cancellationToken)
            is not ProductShopImportSettingsModel currentProductSettings)
            throw new InvalidOperationException("Invalid shopId");

        currentProductSettings.Update(data);

        HttpContext.Session.SetImportSettingToSession(currentProductSettings);

        return PartialView("~/Views/Shared/ImportSettings.cshtml", currentProductSettings);
    }

    [Route($"/{ControllerPrefix.Action}/Set/{{shopId:int}}/Category")]
    [HttpPost]
    public async Task<IActionResult> LoadCategoryShopImportSettings(int shopId, [FromBody]CategoryShopImportSettingsModel data, CancellationToken cancellationToken = default)
    {
        if (data == null)
            return BadRequest("Empty json for category shopsettings.");

        if (await _facade.GetCurrentShopImportSettingsAsync(shopId, ShopSettingType.Category, cancellationToken)
            is not CategoryShopImportSettingsModel currentCategorySettings)
            throw new InvalidOperationException("Invalid shopId");

        currentCategorySettings.Update(data);

        HttpContext.Session.SetImportSettingToSession(currentCategorySettings);

        return PartialView("~/Views/Shared/ImportSettings.cshtml", currentCategorySettings);
    }

    [Route($"/{ControllerPrefix.Action}/Product/IsChanged")]
    [Route($"/{ControllerPrefix.Action}/1/IsChanged")]
    [HttpPost]
    public async Task<IActionResult> ProductSettingsIsChangedAsync(ProductShopImportSettingsModel data, CancellationToken cancellationToken = default)
    {
        if (data == null)
            return BadRequest("Empty json for product shopsettings.");

        var result = await _facade.IsProductShopSettingsChangedAsync(data, cancellationToken);

        return Ok(result);
    }

    [Route($"/{ControllerPrefix.Action}/Category/IsChanged")]
    [Route($"/{ControllerPrefix.Action}/2/IsChanged")]
    [HttpPost]
    public async Task<IActionResult> CategorySettingsIsChangedAsync(CategoryShopImportSettingsModel data, CancellationToken cancellationToken = default)
    {
        if (data == null)
            return BadRequest("Empty json for category shopsettings.");

        var result = await _facade.IsCategoryShopSettingsChangedAsync(data, cancellationToken);

        return Ok(result);
    }

    [Route($"/{ControllerPrefix.Action}/Product/Save")]
    [HttpPost]
    public async Task<IActionResult> ProductSettingsSave(ProductShopImportSettingsModel data, CancellationToken cancellationToken = default)
    {
        if (data == null) return BadRequest("Empty json for product shopsettings.");
        
        await _facade.SaveProductShopSettings(data, cancellationToken);

        return Ok(true);
    }

    [Route($"/{ControllerPrefix.Action}/Category/Save")]
    [HttpPost]
    public async Task<IActionResult> CategorySettingsSave(CategoryShopImportSettingsModel data, CancellationToken cancellationToken = default)
    {
        if (data == null) return BadRequest("Empty json for category shopsettings.");

        await _facade.SaveCategoryShopSettings(data, cancellationToken);

        return Ok(true);
    }

    [Route($"/{ControllerPrefix.Action}/Product/CategoryUrl")]
    [HttpPost]
    public IActionResult RootCategory(CategoryUrlModel data)
    {
        if (data == null) return BadRequest("Empty json for category url.");

        return PartialView("~/Views/Home/RootCategoryUrl.cshtml", data);
    }

    [Route($"/{ControllerPrefix.Action}/Product/{{shopId:int}}/CategoryUrl/Set")]
    [HttpPost]
    public async Task<IActionResult> SetRootCategory(int shopId, [FromForm]CategoryUrlModel data, CancellationToken cancellationToken = default)
    {
        if (data == null) return BadRequest("Empty json for category url.");

        if (await _facade.GetCurrentShopImportSettingsAsync(shopId, ShopSettingType.Product, cancellationToken) is not ProductShopImportSettingsModel productShopSettings)
            return BadRequest("Invalid shopId");

        var result = productShopSettings.SetRootCategory(data);

        HttpContext.Session.SetImportSettingToSession(productShopSettings);

        return Ok(result);
    }

    [Route($"/{ControllerPrefix.Action}/Product/{{shopId:int}}/CategoryUrl/IsChanged")]
    [HttpPost]
    public async Task<IActionResult> RootCategoryIsChanged(int shopId, [FromBody]CategoryUrlModel data, CancellationToken cancellationToken = default)
    {
        if (data == null) return BadRequest("Empty json for category url.");

        if (await _facade.GetCurrentShopImportSettingsAsync(shopId, ShopSettingType.Product, cancellationToken)
            is not ProductShopImportSettingsModel productShopSettings)
            return BadRequest("Invalid shopId");

        var currentRootCategory = productShopSettings.RootCategories.FirstOrDefault(c =>
                    c.Guid == data.Guid ||
                    (c.Url.Equals(data.Url, StringComparison.InvariantCultureIgnoreCase)
                    && c.Item == data.Item));

        return currentRootCategory == null ? Ok(!data.IsEmpty()) : Ok(!currentRootCategory.IsEquals(data));
    }

    [Route($"/{ControllerPrefix.Action}/Product/CategoryUrl/Item")]
    [HttpPost]
    public IActionResult RootCategoryItem([FromBody]CategoryUrlModel data)
    {
        if (data == null) return BadRequest("Empty json for category url.");

        return PartialView("~/Views/Home/RootCategoryUrlItem.cshtml", data);
    }
}