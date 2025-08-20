using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Import.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using ModelHelper = Alchemist.Product.Import.WebApp.Models.ModelHelper;

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

        if ((string.IsNullOrEmpty(serviceSettingsName) || !ModelHelper.IsServiceSettingsPrimary(serviceSettingsName))
            && (guid == null || guid == Guid.Empty))
            return BadRequest(nameof(serviceSettingsName));

        if (!_importFacade.TryGetShopImport(shopGuid, out var shopImport))
            return NotFound(shopGuid);

        if (!_importFacade.TryGetShopSettings(shopGuid, shopSettingsGuid, out var shopSettings))
            return NotFound(shopSettingsGuid);

        IServiceSettingsModel? serviceSettingsModel;
        if (!string.IsNullOrEmpty(serviceSettingsName) && ModelHelper.IsServiceSettingsPrimary(serviceSettingsName))
        {
            if (!_importFacade.TryGetServiceSettings(shopGuid, shopSettingsGuid, serviceSettingsName, out serviceSettingsModel))
                return NotFound(serviceSettingsName);
        }
        else
        {
            if (!_importFacade.TryGetServiceSettings(shopGuid, shopSettingsGuid, guid.Value, out serviceSettingsModel))            
                serviceSettingsModel = _importFacade.CreateNewServiceSettings(shopSettings, serviceSettingsName);            
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

    [Route("ServiceSettings/BrowserLauncher")]
    [HttpPost]
    [ProducesResponseType<PartialViewResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    public IActionResult BrowserLauncherSettings(Guid shopGuid, Guid shopSettingsGuid)
    {
        return ServiceSettings(shopGuid, shopSettingsGuid, nameof(ShopSettingsModel.BrowserLauncher));
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
    public IActionResult SaveServiceSettings([ModelBinder(typeof(ModelJsonBinder))] ServiceSettingsModel data)
    {
        if (data == null)
            return BadRequest(data);

        if (!_importFacade.TryGetShopImport(data.ShopGuid, out var shopImport))
            return NotFound(data.ShopGuid);

        if (!_importFacade.TryGetShopSettings(data.ShopSettingsGuid, out var shopSettings))
            return NotFound(data.ShopSettingsGuid);         

        if (ModelHelper.IsServiceSettingsPrimary(data.Name))
        {
            if (!_importFacade.TryGetServiceSettings(data.ShopGuid, data.ShopSettingsGuid, data.Name, out var serviceSettings))
                return NotFound(data.Name);

            serviceSettings.Update(data);

            return Ok(serviceSettings);
        }
        else
        {
            if (!_importFacade.TryGetServiceSettings(data.ShopGuid, data.ShopSettingsGuid, data.Guid, out var serviceSettings))
                _importFacade.AddNewServiceSettings(shopSettings, data.Name, out serviceSettings);

            serviceSettings.Update(data);

            return Ok(serviceSettings);
        }
    }

    [Route("ServiceSettings/IsChanged")]
    [HttpPost]
    [ProducesResponseType<OkObjectResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    public IActionResult IsServiceSettingsChanged([ModelBinder(typeof(ModelJsonBinder))] ServiceSettingsModel data)
    {
        if (data == null)
            return BadRequest(data);

        if (!_importFacade.TryGetShopImport(data.ShopGuid, out _))
            return NotFound(data.ShopGuid);

        if (!_importFacade.TryGetShopSettings(data.ShopSettingsGuid, out _))
            return NotFound(data.ShopSettingsGuid);

        IServiceSettingsModel? serviceSettingsModel;
        if (ModelHelper.IsServiceSettingsPrimary(data.Name))
        {
            if (!_importFacade.TryGetServiceSettings(data.ShopGuid, data.ShopSettingsGuid, data.Name, out serviceSettingsModel))
                return NotFound(data.Name);

            if (data.IsEmpty() && serviceSettingsModel.IsEmpty())
                return Ok(false);
        }
        else
        {
            if (!_importFacade.TryGetServiceSettings(data.ShopGuid, data.ShopSettingsGuid, data.Guid, out serviceSettingsModel))
                return Ok(true);
        }        

        return Ok(!((ServiceSettingsModel)serviceSettingsModel).FieldsEquals(data));
    }
}
