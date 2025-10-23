using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.ImportSettingsWebApp.Infrastructure;
using Alchemist.Product.ImportSettingsWebApp.Models;
using Alchemist.Product.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Alchemist.Product.ImportSettingsWebApp.Controllers;


[ApiExplorerSettings(IgnoreApi = true)]
public class ServiceSettingsController : Controller
{

    ServiceSettingsFacade _serviceSettingsFacade;

    public ServiceSettingsController(
        [FromKeyedServices(ShopSettingType.Product)] ISettingsDataAdapter productSettingsDataAdapter,
        [FromKeyedServices(ShopSettingType.Category)] ISettingsDataAdapter categorySettingsDataAdapter) 
    {
        _serviceSettingsFacade = new ServiceSettingsFacade(productSettingsDataAdapter, categorySettingsDataAdapter, this);
    }    

    [Route("/Import/Settings/{shopId:int}/{shopSettingsType:ShopSettingType}/Service")]
    [HttpGet]
    public async Task<IActionResult> NewServiceSettingsAsync(int shopId, ShopSettingType shopSettingsType)
    {
        var serviceSettings = await _serviceSettingsFacade.GetServiceSettingsAsync(Guid.NewGuid(), shopId, shopSettingsType);

        return PartialView("~/Views/Home/ServiceSettings.cshtml", serviceSettings);
    }

    [Route("/Import/Settings/{shopId:int}/{shopSettingsType:ShopSettingType}/Service/{guid:Guid}")]
    [HttpGet]
    public async Task<IActionResult> ServiceSettingsAsync(int shopId, ShopSettingType shopSettingsType, Guid guid)
    {
        var serviceSettings = await _serviceSettingsFacade.GetServiceSettingsAsync(guid, shopId, shopSettingsType);

        return PartialView("~/Views/Home/ServiceSettings.cshtml", serviceSettings);        
    }

    [Route("/Import/Settings/{shopId:int}/{shopSettingsType:ShopSettingType}/PrimaryService/{serviceName}")]
    [HttpGet]
    public async Task<IActionResult> PrimaryServiceAsync(int shopId, ShopSettingType shopSettingsType, string serviceName)
    {
        if (string.IsNullOrEmpty(serviceName))
            return BadRequest("Service name is empty");

        if (!serviceName.IsPrimaryServiceName() 
            || serviceName.Equals(PrimaryServiceName.RequestHeaders.ToString(), StringComparison.InvariantCultureIgnoreCase))
            return BadRequest($"Service name {serviceName} is invalid.");

        var serviceSettings = await _serviceSettingsFacade.GetServiceSettingsAsync(serviceName, shopId, shopSettingsType);

        return PartialView("~/Views/Home/ServiceSettings.cshtml", serviceSettings);
    }

    [Route("/Import/Settings/{shopId:int}/{shopSettingsType:ShopSettingType}/PrimaryService/Set/{serviceName}")]
    [HttpPost]
    public async Task<IActionResult> SetPrimaryServiceAsync(int shopId, ShopSettingType shopSettingsType, string serviceName, [FromBody]ServiceSettingsModel data)
    {
        if (string.IsNullOrEmpty(serviceName))
            return BadRequest("Empty service name");

        var serviceSettings = await _serviceSettingsFacade.SaveServiceSettingsAsync(shopId, shopSettingsType, serviceName, data);

        return Ok(serviceSettings);
    }

    [Route("/Import/Settings/{shopId:int}/{shopSettingsType:ShopSettingType}/Service/Set/{guid}")]
    [HttpPost]
    public async Task<IActionResult> SetServiceAsync(int shopId, ShopSettingType shopSettingsType, Guid guid, [FromBody]ServiceSettingsModel data)
    {
        if (guid == Guid.Empty)
            return BadRequest("Empty service guid");

        var serviceSettings = await _serviceSettingsFacade.SaveServiceSettingsAsync(shopId, shopSettingsType, guid, data);

        return Ok(serviceSettings);
    }


    [Route("/Import/Settings/{shopId:int}/{shopSettingsType:ShopSettingType}/Service/Set")]
    [HttpPost]
    public async Task<IActionResult> SaveAsync(int shopId, ShopSettingType shopSettingsType, [FromBody] ServiceSettingsModel data)
    {
        if (string.IsNullOrEmpty(data?.Name))
            return BadRequest("Empty service name");

        var serviceSettings = await _serviceSettingsFacade.SaveServiceSettingsAsync(shopId, shopSettingsType, data.Name, data);

        return Ok(serviceSettings);
    }

    [Route("/Import/Settings/{shopId:int}/{shopSettingsType:ShopSettingType}/Service/IsChanged")]
    [HttpPost]
    public async Task<IActionResult> IsChanged(int shopId, ShopSettingType shopSettingsType, [FromBody] ServiceSettingsModel data)
    {
        if (string.IsNullOrEmpty(data?.Name))
            return BadRequest("Empty service name");

        return Ok(await _serviceSettingsFacade.IsChanged(shopId, shopSettingsType,data));
    }

    [Route("/Import/Settings/Service/Item")]
    [HttpPost]
    public IActionResult ServiceItem([FromBody] ServiceSettingsModel data)
    {
        if (data == null) return BadRequest("Empty json for service.");

        return PartialView("~/Views/Home/ServiceSettingsItem.cshtml", data);
    }
}