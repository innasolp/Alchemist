using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Alchemist.Product.Import.WebApp.Controllers;

public class ShopSettingsController(ILogger<ShopSettingsController> logger, IImportFacade importFacade, ISettingsDataAdapter settingsDataAdapter) : Controller
{
    private readonly ILogger<ShopSettingsController> _logger = logger;

    private readonly IImportFacade _importFacade = importFacade;

    private readonly ISettingsDataAdapter _settingsDataAdapter = settingsDataAdapter;

    [HttpPost]
    [ProducesResponseType<OkObjectResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestResult>(StatusCodes.Status400BadRequest)]
    public IActionResult SaveShopSettings(Guid shopGuid, int shopSettingType, string json)
    {
        if (string.IsNullOrEmpty(json)) return BadRequest(json);

        var shopSettings = json.GetShopSettingsFromJson((ShopSettingType)shopSettingType);
        if (shopSettings == null)
            return BadRequest(json);

        if (!_importFacade.TryGetShopImport(shopGuid, out var shopImport))
            return NotFound(shopGuid);

        if (!_importFacade.TryGetShopSettings(shopGuid, shopSettings.ShopSettingType, out var shopSettingsModel))
            shopSettingsModel = shopGuid.CreateShopSettings(shopSettings.ShopSettingType);

        shopSettingsModel?.Update(shopSettings);

        return Ok(true);
    }

    [HttpPost]
    [ProducesResponseType<OkObjectResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestResult>(StatusCodes.Status400BadRequest)]
    public IActionResult SetShopSettings(Guid shopGuid, int shopSettingType)
    {
        if ((ShopSettingType)shopSettingType == ShopSettingType.Service)
            return BadRequest(shopSettingType);

        if (!_importFacade.TryGetShopImport(shopGuid, out var shopImport))
            return NotFound(shopGuid);

        shopImport.ShopSettingTabs.SelectedSettingsTab = (ShopSettingType)shopSettingType;

        return Ok(true);
    }

    private IActionResult ServiceSettings(Guid shopGuid, Guid shopSettingsGuid, string serviceSettingsName)
    {
        if (!_importFacade.TryGetShopImport(shopGuid, out var shopImport))
            return NotFound(shopGuid);

        if (!_importFacade.TryGetShopSettings(shopGuid, shopSettingsGuid, out var shopSettings))
            return NotFound(shopSettingsGuid);

        if (!_importFacade.TryGetServiceSettingsModel(shopGuid, shopSettingsGuid, serviceSettingsName, out var serviceSettingsModel))
            serviceSettingsModel = shopGuid.CreateServiceSettingsModel(shopSettingsGuid, serviceSettingsName);

        return PartialView("~/Views/Home/ServiceSettings.cshtml", serviceSettingsModel);
    }

    [HttpPost]
    [ProducesResponseType<PartialViewResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestResult>(StatusCodes.Status400BadRequest)]
    public IActionResult ImportServiceSettings(Guid shopGuid, Guid shopSettingsGuid)
    {
        return ServiceSettings(shopGuid, shopSettingsGuid, nameof(ShopSettingsModel.ImportService));
    }


    [HttpPost]
    [ProducesResponseType<PartialViewResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestResult>(StatusCodes.Status400BadRequest)]
    public IActionResult BrowserDataLoaderSettings(Guid shopGuid, Guid shopSettingsGuid)
    {
        return ServiceSettings(shopGuid, shopSettingsGuid, nameof(ShopSettingsModel.BrowserDataLoader));
    }

    [HttpPost]
    [ProducesResponseType<PartialViewResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestResult>(StatusCodes.Status400BadRequest)]
    public IActionResult WebLoaderSettings(Guid shopGuid, Guid shopSettingsGuid)
    {
        return ServiceSettings(shopGuid, shopSettingsGuid, nameof(ShopSettingsModel.WebLoader));
    }

    [HttpPost]
    [ProducesResponseType<OkObjectResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestResult>(StatusCodes.Status400BadRequest)]
    public IActionResult SaveServiceSettings(ServiceSettingsModel data)
    {
        if (data == null)
            return BadRequest(data);

        if (!_importFacade.TryGetShopImport(data.ShopGuid, out var shopImport))
            return NotFound(data.ShopGuid);

        var shopSettings = shopImport.ShopSettingTabs.GetShopSettingsByGuid(data.ShopSettingsGuid);

        if (shopSettings.GetServiceSettings(data.Name) == null)
            shopSettings.SetServiceSettings(shopSettings.CreateServiceSettingsModel(data.Name));

        shopImport.ShopSettingTabs?.GetShopSettingsByGuid(data.ShopSettingsGuid)?
                 .UpdateServiceSettings(data);

        return Ok(true);
    }

    [HttpPost]
    [ProducesResponseType<OkObjectResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestResult>(StatusCodes.Status400BadRequest)]
    public IActionResult IsServiceSettingsChanged(ServiceSettingsModel data)
    {
        if (data == null)
            return BadRequest(data);

        if (!_importFacade.TryGetShopImport(data.ShopGuid, out var shopImport))
            return NotFound(data.ShopGuid);

        var shopSettings = shopImport.ShopSettingTabs.GetShopSettingsByGuid(data.ShopSettingsGuid);
        var serviceSettings = shopSettings.GetServiceSettings(data.Name);

        if (serviceSettings == null) return Ok(!data.IsEmpty());

        return Ok(!serviceSettings.Equals(data));
    }


    [HttpPost]
    [ProducesResponseType<OkObjectResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ObjectResult>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SaveProductShopSettingsToDb(ProductShopSettingsModel productShopSettings)
    {
        if (productShopSettings == null) return BadRequest(productShopSettings);

        if (!_importFacade.TryGetShopImport(productShopSettings.ShopGuid, out var shopImport) || shopImport == null)
            return NotFound(productShopSettings);

        try
        {
            shopImport.ShopSettingTabs?.ShopProductsSettings?.Update(productShopSettings);

            await _settingsDataAdapter.Save(shopImport.ShopSettingTabs.ShopProductsSettings);

            return Ok(productShopSettings);
        }
        catch (Exception e)
        {
            return new ObjectResult(e) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }

    [HttpPost]
    [ProducesResponseType<OkObjectResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFound>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ObjectResult>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SaveCategoryShopSettingsToDb(CategoryShopSettingsModel categoryShopSettings)
    {
        if (categoryShopSettings == null) return BadRequest(categoryShopSettings);

        if (!_importFacade.TryGetShopImport(categoryShopSettings.ShopGuid, out var shopImport) || shopImport == null)
            return NotFound(categoryShopSettings.ShopGuid);

        try
        {
            shopImport.ShopSettingTabs.ShopCategoriesSettings.Update(categoryShopSettings);

            await _settingsDataAdapter.Save(shopImport.ShopSettingTabs.ShopCategoriesSettings);

            return Ok(categoryShopSettings);
        }
        catch (Exception e)
        {
            return new ObjectResult(e) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }

}
