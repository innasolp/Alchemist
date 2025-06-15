using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Alchemist.Product.Import.WebApp.Controllers;

public class ServiceSettingsController(IImportFacade importFacade) : Controller
{
    private readonly IImportFacade _importFacade = importFacade;

    private IActionResult ServiceSettings(Guid shopGuid, Guid shopSettingsGuid, string? serviceSettingsName, Guid? guid = null)
    {
        if (shopSettingsGuid == Guid.Empty)
            return BadRequest(nameof(shopSettingsGuid));

        if (shopGuid == Guid.Empty)
            return BadRequest(nameof(shopGuid));

        if (string.IsNullOrEmpty(serviceSettingsName) && guid == null)
            return BadRequest(nameof(serviceSettingsName));

        if (!_importFacade.TryGetShopImport(shopGuid, out var shopImport))
            return NotFound(shopGuid);

        if (!_importFacade.TryGetShopSettings(shopGuid, shopSettingsGuid, out var shopSettings))
            return NotFound(shopSettingsGuid);

        ServiceSettingsModel? serviceSettingsModel;
        if (!string.IsNullOrEmpty(serviceSettingsName))
        {
            if (!_importFacade.TryGetServiceSettingsModel(shopGuid, shopSettingsGuid, serviceSettingsName, out serviceSettingsModel))
                serviceSettingsModel = ModelHelper.CreateServiceSettingsModel(shopGuid, shopSettings.ShopId, shopSettingsGuid, serviceSettingsName);
        }
        else
        {
            if (!_importFacade.TryGetServiceSettingsModel(shopGuid, shopSettingsGuid, guid.Value, out serviceSettingsModel))
            {
                serviceSettingsModel = ModelHelper.CreateServiceSettingsModel(shopGuid, shopSettings.ShopId, shopSettingsGuid, serviceSettingsName);
                serviceSettingsModel.Guid = guid.Value;
            }
        }

        return PartialView("~/Views/Home/ServiceSettings.cshtml", serviceSettingsModel);
    }

    [Route("ServiceSettings/ImportService")]
    [HttpPost]
    [ProducesResponseType<PartialViewResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    public IActionResult ImportServiceSettings(Guid shopGuid, Guid shopSettingsGuid)
    {
        return ServiceSettings(shopGuid, shopSettingsGuid, nameof(ShopSettingsModel.ImportService));
    }

    [Route("ServiceSettings/BrowserDataLoader")]
    [HttpPost]
    [ProducesResponseType<PartialViewResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    public IActionResult BrowserDataLoaderSettings(Guid shopGuid, Guid shopSettingsGuid)
    {
        return ServiceSettings(shopGuid, shopSettingsGuid, nameof(ShopSettingsModel.BrowserDataLoader));
    }

    [Route("ServiceSettings/WebLoader")]
    [HttpPost]
    [ProducesResponseType<PartialViewResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    public IActionResult WebLoaderSettings(Guid shopGuid, Guid shopSettingsGuid)
    {
        return ServiceSettings(shopGuid, shopSettingsGuid, nameof(ShopSettingsModel.WebLoader));
    }

    [Route("ServiceSettings/")]
    [HttpPost]
    [ProducesResponseType<PartialViewResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    public IActionResult ServiceSettings(Guid shopGuid, Guid shopSettingsGuid, Guid? guid)
    {
        return ServiceSettings(shopGuid, shopSettingsGuid, "", guid ?? Guid.NewGuid());
    }

    [Route("ServiceSettings/Save")]
    [HttpPost]
    [ProducesResponseType<OkObjectResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    public IActionResult SaveServiceSettings(ServiceSettingsModel data)
    {
        if (data == null)
            return BadRequest(data);

        if (!_importFacade.TryGetShopImport(data.ShopGuid, out var shopImport))
            return NotFound(data.ShopGuid);

        var shopSettings = shopImport.ShopSettingTabs.GetShopSettingsByGuid(data.ShopSettingsGuid);

        if (ModelHelper.IsServiceSettingsPrimary(data.Name))
        {
            if (shopSettings.GetServiceSettings(data.Name) == null)
                shopSettings.SetServiceSettings(shopSettings.CreateServiceSettingsModel(data.Name), data.Name);

            shopImport.ShopSettingTabs?.GetShopSettingsByGuid(data.ShopSettingsGuid)?
                     .UpdateServiceSettings(data);

            return Ok(shopImport.ShopSettingTabs?.GetShopSettingsByGuid(data.ShopSettingsGuid)?.GetServiceSettings(data.Name));
        }
        else
        {
            var serviceSettings = shopSettings.Services.FirstOrDefault(s => s.Guid == data.Guid);
            if (serviceSettings == null)
            {
                serviceSettings = shopSettings.CreateServiceSettingsModel(data.Name);
                serviceSettings.Update(data);
                shopSettings.Services.Add(serviceSettings);
            }
            else
                serviceSettings.Update(data);

            return Ok(serviceSettings);
        }
    }

    [Route("ServiceSettings/IsChanged")]
    [HttpPost]
    [ProducesResponseType<OkObjectResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
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

}
