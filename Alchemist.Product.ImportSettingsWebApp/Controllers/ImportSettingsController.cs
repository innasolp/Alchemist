using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Product.ImportSettingsWebApp.Infrastructure;
using Alchemist.Product.ImportSettingsWebApp.Models;
using Alchemist.Product.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Alchemist.Product.ImportSettingsWebApp.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class ImportSettingsController([FromKeyedServices(ShopSettingType.Product)] ISettingsDataAdapter productSettingsDataAdapter,
    [FromKeyedServices(ShopSettingType.Category)] ISettingsDataAdapter categorySettingsDataAdapter) : Controller
{
    private readonly SettingsDataAdapterContainer _settingsDataAdapter = new(productSettingsDataAdapter, categorySettingsDataAdapter);

    private async Task SaveShopImportSettingsAsync<T>(T data, Action<T, T> updateFields)
        where T : ShopImportSettingsModel
    {
        var existing = await _settingsDataAdapter.GetShopImportSettingsAsync(data.ShopId, data.ShopSettingType);

        var sessionShopSettings = await HttpContext.Session.GetShopImportSettingsFromSessionAsync() as T;

        if (existing is T existingShopSettings)
        {
            existingShopSettings.UpdateFields(data);

            updateFields(existingShopSettings, data);

            if (sessionShopSettings != null && sessionShopSettings.ShopId == data.ShopId)
                existingShopSettings.UpdateServices(sessionShopSettings.Services);

            await _settingsDataAdapter.SaveAsync(existingShopSettings);
        }
        else
        {
            var toSave = sessionShopSettings != null && sessionShopSettings.ShopId == data.ShopId
                ? sessionShopSettings
                : data;

            toSave.UpdateFields(data);
            updateFields(toSave, data);

            if (sessionShopSettings != null && sessionShopSettings.ShopId == data.ShopId)
                toSave.UpdateServices(sessionShopSettings.Services);

            await _settingsDataAdapter.SaveAsync(toSave);
        }
    }


    [Route("/Import/Settings/Tab/")]
    [HttpPost]
    public async Task<IActionResult> ImportSettingsTabAsync([FromBody] ShopSettingsData data)
    {
        var importSettings = await _settingsDataAdapter.GetShopImportSettingsModel(data.ShopId, (ShopSettingType)data.ShopSettingsType);

        HttpContext.Session.SetImportSettingtoSession(importSettings);

        return PartialView("~/Views/Shared/ShopImportSettingsTab.cshtml", importSettings);
    }

    [Route("/Import/Settings/Product/")]
    [HttpPost]
    public IActionResult LoadProductShopImportSettings([FromBody] ProductShopImportSettingsModel data)
    {
        if (data == null)
            return BadRequest("Empty json for product shopsettings.");

        HttpContext.Session.SetImportSettingtoSession(data);

        return PartialView("~/Views/Shared/ImportSettings.cshtml", data);
    }

    [Route("/Import/Settings/Category/")]
    [HttpPost]
    public IActionResult LoadCategoryShopImportSettings(CategoryShopImportSettingsModel data)
    {
        if (data == null)
            return BadRequest("Empty json for category shopsettings.");

        HttpContext.Session.SetImportSettingtoSession(data);

        return PartialView("~/Views/Shared/ShopImportSettingsTab.cshtml", data);
    }

    private async Task<bool> IsSettingsChanged<T>(T data, Func<T, bool> isEmpty, Func<T,T?, bool> typedFieldsEquals)
        where T:ShopImportSettingsModel
    {
        try
        {
            var existingShopSettings = await _settingsDataAdapter.GetShopImportSettingsAsync(data.ShopId, data.ShopSettingType)
                as T;

            var sessionShopSettings = await HttpContext.Session.GetShopImportSettingsFromSessionAsync();
            if (sessionShopSettings != null && sessionShopSettings.ShopSettingType == data.ShopSettingType
                && sessionShopSettings.ShopId == data.ShopId)
            {
                return existingShopSettings == null
                    ? !(isEmpty(data) && sessionShopSettings.ShopImportSettingsIsEmpty())
                    : !(typedFieldsEquals(data, existingShopSettings)
                            && sessionShopSettings.Services.ServicesAreEquals(existingShopSettings.Services));
            }
            else
            {
                return existingShopSettings == null
                    ? !isEmpty(data)
                    : !typedFieldsEquals(data, existingShopSettings);
            }
        }
        catch (Exception ex) 
        { 
            throw ex;
        }
    }

    [Route("/Import/Settings/Product/IsChanged")]
    [HttpPost]
    public async Task<IActionResult> ProductSettingsIsChangedAsync(ProductShopImportSettingsModel data)
    {
        if (data == null)
            return BadRequest("Empty json for product shopsettings.");

        var result = await IsSettingsChanged(data, (settings) => settings.IsEmpty(), 
            (target, source) => target.ProductShopSettingsFieldsEquals(source));
        return Ok(result);
    }

    [Route("/Import/Settings/Category/IsChanged")]
    [HttpPost]
    public async Task<IActionResult> CategorySettingsIsChangedAsync(CategoryShopImportSettingsModel data)
    {
        if (data == null)
            return BadRequest("Empty json for category shopsettings.");

        var result = await IsSettingsChanged(data, (settings) => settings.IsEmpty(), 
            (target, source) => target.CategoryShopSettingsFieldsEquals(source));
        return Ok(result);
    }

    [Route("/Import/Settings/Product/Save")]
    [HttpPost]
    public async Task<IActionResult> ProductSettingsSave(ProductShopImportSettingsModel data)
    {
        if (data == null) return BadRequest("Empty json for product shopsettings.");

        await SaveShopImportSettingsAsync(data, 
            (target, source)=>target.UpdateProductShopImportSettings (source));

        return Ok(true);
    }

    [Route("/Import/Settings/Category/Save")]
    [HttpPost]
    public async Task<IActionResult> CategorySettingsSave(CategoryShopImportSettingsModel data)
    {
        if (data == null) return BadRequest("Empty json for category shopsettings.");

        await SaveShopImportSettingsAsync(data, 
            (target, source)=>target.UpdateCategoryShopImportSettings (source));

        return Ok(true);
    }
}
