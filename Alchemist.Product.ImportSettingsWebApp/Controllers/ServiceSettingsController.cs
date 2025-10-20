using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.ImportSettingsWebApp.Infrastructure;
using Alchemist.Product.ImportSettingsWebApp.Models;
using Alchemist.Product.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Alchemist.Product.ImportSettingsWebApp.Controllers;

public class ServiceSettingsData
{
    public int ShopId { get; set; }

    public int ShopSettingsType { get; set; }

    public string? ServiceName { get; set; }

    public Guid? Guid { get; set; } = System.Guid.NewGuid();
}


[ApiExplorerSettings(IgnoreApi = true)]
public class ServiceSettingsController(
    [FromKeyedServices(ShopSettingType.Product)] ISettingsDataAdapter productSettingsDataAdapter,
    [FromKeyedServices(ShopSettingType.Category)] ISettingsDataAdapter categorySettingsDataAdapter)
    : SettingsController(productSettingsDataAdapter, categorySettingsDataAdapter)
{
    
    private static ServiceSettingsModel GetServiceSettings(ShopImportSettingsModel shopImportSettings, string? serviceName = null, Guid? guid = null)
    {
        var service = serviceName?.IsPrimaryServiceName() == true
            ? shopImportSettings.GetService<ServiceSettingsModel>(serviceName) ?? new ServiceSettingsModel { Name = serviceName}
            : !string.IsNullOrEmpty(serviceName) &&  shopImportSettings.Services.TryGetValue(serviceName, out var serviceSettings) && serviceSettings != null 
                ? serviceSettings
                : guid != null 
                     ? shopImportSettings.Services.FirstOrDefault(s=>s.Value.Guid == guid).Value ?? new ServiceSettingsModel()
                     : new ServiceSettingsModel();

        if(!string.IsNullOrEmpty(serviceName)) service.Name = serviceName;
        service.ParentShopSettingsType = shopImportSettings.ShopSettingType;
        service.ParentSettingsId = shopImportSettings.Id;
        service.ShopId = shopImportSettings.ShopId;

        return service;
    } 
  

    private async Task<IActionResult> GetServiceSettingsActionAsync(ServiceSettingsData serviceSettingsData)
    {
        var shopImportSettings = await GetShopImportSettingsAsync(serviceSettingsData.ShopId,(ShopSettingType) serviceSettingsData.ShopSettingsType);
        var serviceSettings = GetServiceSettings(shopImportSettings, serviceSettingsData.ServiceName, serviceSettingsData.Guid);
        return PartialView("~/Views/Home/ServiceSettings.cshtml", serviceSettings);
    }

    [Route("/Import/Settings/Service")]
    [HttpPost]
    public async Task<IActionResult> ServiceSettingsAsync(ServiceSettingsData data)
    {
        return await GetServiceSettingsActionAsync(data);
    }

    [Route($"/Import/Settings/{nameof(PrimaryServiceName.ImportService)}")]
    [HttpPost]
    public async Task<IActionResult> ImportServiceSettingsAsync(ServiceSettingsData data)
    {
        if (data.ServiceName != nameof(PrimaryServiceName.ImportService))
            return BadRequest($"Invalid request for {nameof(PrimaryServiceName.ImportService)}");

        return await GetServiceSettingsActionAsync(data);
    }

    [Route($"/Import/Settings/{nameof(PrimaryServiceName.BrowserDataLoader)}")]
    [HttpPost]
    public async Task<IActionResult> BrowserDataLoaderSettingsAsync(ServiceSettingsData data)
    {
        if (data.ServiceName != nameof(PrimaryServiceName.BrowserDataLoader))
            return BadRequest($"Invalid request for {nameof(PrimaryServiceName.BrowserDataLoader)}");

        return await GetServiceSettingsActionAsync(data);
    }

    [Route($"/Import/Settings/{nameof(PrimaryServiceName.BrowserLauncher)}")]
    [HttpPost]
    public async Task<IActionResult> BrowserLauncherSettingsAsync(ServiceSettingsData data)
    {
        if (data.ServiceName != nameof(PrimaryServiceName.BrowserLauncher))
            return BadRequest($"Invalid request for {nameof(PrimaryServiceName.BrowserLauncher)}");

        return await GetServiceSettingsActionAsync(data);
    }

    [Route($"/Import/Settings/{nameof(PrimaryServiceName.WebLoader)}")]
    [HttpPost]
    public async Task<IActionResult> WebLoaderSettingsAsync(ServiceSettingsData data)
    {
        if (data.ServiceName != nameof(PrimaryServiceName.WebLoader))
            return BadRequest($"Invalid request for {nameof(PrimaryServiceName.WebLoader)}");

        return await GetServiceSettingsActionAsync(data);
    }

    [Route("/Import/Settings/Service/Set")]
    [HttpPost]
    public async Task<IActionResult> SaveAsync([FromForm]ServiceSettingsModel data)
    {
        if (string.IsNullOrEmpty(data.Name))
            return BadRequest("Empty service name");

        var shopImportSettings = await GetShopImportSettingsAsync(data.ShopId, data.ParentShopSettingsType);
        if (shopImportSettings == null)
            return Ok();

        var existingService = shopImportSettings.GetService<ServiceSettingsModel>(data.Name) ?? 
            shopImportSettings.GetService(data.Guid);

        ServiceSettingsModel result;
        if (existingService == null)
        {
            shopImportSettings.Services.Add(data.Name, data);
            result = data;
        }
        else
        {
            existingService.Updateservice(data);
            result = existingService;
        }

        HttpContext.Session.SetImportSettingtoSession(shopImportSettings);

        return Ok(result);
    }

    [Route("/Import/Settings/Service/IsChanged")]
    [HttpPost]
    public async Task<IActionResult> IsChanged(ServiceSettingsModel data)
    {
        if (string.IsNullOrEmpty(data.Name))
            return BadRequest("Empty service name");

        var shopImportSettings = await GetShopImportSettingsAsync(data.ShopId, data.ParentShopSettingsType);// HttpContext.Session.GetShopImportSettingsFromSessionAsync();
        if (shopImportSettings == null)
            return Ok(!data.IsEmpty());

        var existingService = shopImportSettings.GetService<ServiceSettingsModel>(data.Name) ??
            shopImportSettings.GetService(data.Guid);

        return Ok(existingService != null && !data.ServiceEquals(existingService));
    }

    [Route("/Import/Settings/Service/Item")]
    [HttpPost]
    public IActionResult ServiceItem([FromBody] ServiceSettingsModel data)
    {
        if (data == null) return BadRequest("Empty json for service.");

        return PartialView("~/Views/Home/ServiceSettingsItem.cshtml", data);
    }
}
