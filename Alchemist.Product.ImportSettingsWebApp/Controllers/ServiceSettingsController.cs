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
    [FromKeyedServices(ShopSettingType.Category)] ISettingsDataAdapter categorySettingsDataAdapter) : Controller
{
    private readonly SettingsDataAdapterContainer _settingsDataAdapter = new(productSettingsDataAdapter, categorySettingsDataAdapter);

    private static ServiceSettingsModel GetServiceSettings(ShopImportSettingsModel shopImportSettings, string? serviceName = null, Guid? guid = null)
    {
        var service = serviceName?.IsPrimaryServiceName() == true
            ? shopImportSettings.GetService<ServiceSettingsModel>(serviceName) ?? new ServiceSettingsModel { Name = serviceName}
            : shopImportSettings.Services.TryGetValue(serviceName, out var serviceSettings) && serviceSettings != null 
                ? serviceSettings
                : guid != null 
                     ? shopImportSettings.Services.FirstOrDefault(s=>s.Value.Guid == guid).Value ?? new ServiceSettingsModel()
                     : new ServiceSettingsModel();

        service.Name = serviceName;

        return service;
    }    

    private async Task<ShopImportSettingsModel> GetShopImportSettingsAsync(int shopId, ShopSettingType shopSettingType)
    {
        try
        {
            var shopImportSettings = await HttpContext.Session.GetShopImportSettingsFromSessionAsync();
            if (shopImportSettings != null &&
                    shopImportSettings.ShopId == shopId && shopImportSettings.ShopSettingType == shopSettingType)
                return shopImportSettings;

            shopImportSettings = await _settingsDataAdapter.GetShopImportSettingsAsync(shopId, shopSettingType);
            if (shopImportSettings == null)
            {
                shopImportSettings = ModelHelper.CreateShopImportSettingsModel(shopSettingType);
                shopImportSettings.ShopId = shopId;
            }

            return shopImportSettings;
        }
        catch(Exception ex) 
        {
            throw ex;
        }
    }

    private async Task<IActionResult> GetServiceSettingsActionAsync(ServiceSettingsData serviceSettingsData)
    {
        var shopImportSettings = await GetShopImportSettingsAsync(serviceSettingsData.ShopId,(ShopSettingType) serviceSettingsData.ShopSettingsType);
        var serviceSettings = GetServiceSettings(shopImportSettings, serviceSettingsData.ServiceName, serviceSettingsData.Guid);
        return PartialView("~/Views/Home/ServiceSettings.cshtml", serviceSettings);
    }

    [Route("/Import/Settings/Service")]
    [HttpPost]
    public async Task<IActionResult> ServiceSettingsAsync([FromBody]ServiceSettingsData data)
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

    [Route("/Import/Settings/Service/Save")]
    [HttpPost]
    public async Task<IActionResult> SaveAsync([FromForm]ServiceSettingsModel data)
    {
        if (string.IsNullOrEmpty(data.Name))
            return BadRequest("Empty service name");

        var shopImportSettings = await HttpContext.Session.GetShopImportSettingsFromSessionAsync();
        if (shopImportSettings == null)
            return Ok();

        var existingService = shopImportSettings.GetService<ServiceSettingsModel>(data.Name) ?? 
            shopImportSettings.GetService(data.Guid);

        if (existingService == null)
            shopImportSettings.Services.Add(data.Name, data);
        else
            existingService.Updateservice(data);

        HttpContext.Session.SetImportSettingtoSession(shopImportSettings);

        return Ok(data.ServiceTypeName);
    }

    [Route("/Import/Settings/Service/IsChanged")]
    [HttpPost]
    public async Task<IActionResult> IsChanged(ServiceSettingsModel data)
    {
        if (string.IsNullOrEmpty(data.Name))
            return BadRequest("Empty service name");

        var shopImportSettings = await HttpContext.Session.GetShopImportSettingsFromSessionAsync();
        if (shopImportSettings == null)
            return Ok(!data.IsEmpty());

        var existingService = shopImportSettings.GetService<ServiceSettingsModel>(data.Name) ??
            shopImportSettings.GetService(data.Guid);

        return Ok(existingService != null && !data.ServiceEquals(existingService));
    }
}
