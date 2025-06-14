using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Alchemist.Product.Import.WebApp.Controllers;

public class ShopSettingsController(ILogger<ShopSettingsController> logger, IImportFacade importFacade, ISettingsDataAdapter settingsDataAdapter) : Controller
{
    private readonly ILogger<ShopSettingsController> _logger = logger;

    private readonly IImportFacade _importFacade = importFacade;

    private readonly ISettingsDataAdapter _settingsDataAdapter = settingsDataAdapter;

    [HttpPost]
    [ProducesResponseType<PartialViewResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ShopSettings(Guid shopGuid, int shopSettingType)
    {
        if (shopGuid == Guid.Empty)
            return BadRequest(shopGuid);

        if ((ShopSettingType)shopSettingType == ShopSettingType.Service)
            return BadRequest((ShopSettingType)shopSettingType);

        if (!_importFacade.TryGetShopImport(shopGuid, out var shopImport))
            return NotFound(shopGuid);
        
        var shopSettings = shopImport.ShopSettingTabs?.GetShopSettingsByType((ShopSettingType)shopSettingType);
        if (shopSettings == null)
        {
            shopSettings = shopImport.CreateShopSettings((ShopSettingType)shopSettingType);
            shopImport.SetSettings(TabType.Shop, shopSettings);
        }

        shopImport.ShopSettingTabs.SelectedSettingsTab = (ShopSettingType)shopSettingType;

        return PartialView("~/Views/Home/ShopSettings.cshtml", shopSettings);
    }

    [HttpPost]
    [ProducesResponseType<OkObjectResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    public IActionResult SaveShopSettings(Guid shopGuid, int shopSettingType, string json)
    {
        if (string.IsNullOrEmpty(json)) return BadRequest(json);

        ShopSettingsModel shopSettings;
        try
        {
            shopSettings = ModelHelper.GetShopSettingsFromJson(json,(ShopSettingType)shopSettingType);
        }
        catch (JsonException)
        {
            return BadRequest(json);
        }
        if (shopSettings == null)
            return BadRequest(json);

        if (!_importFacade.TryGetShopImport(shopGuid, out var shopImport))
            return NotFound(shopGuid);

        if (!_importFacade.TryGetShopSettings(shopGuid, shopSettings.ShopSettingType, out var shopSettingsModel))
            shopSettingsModel = ModelHelper.CreateShopSettings(shopGuid, shopSettings.ShopId, shopSettings.ShopSettingType);

        shopSettingsModel?.Update(shopSettings);

        return Ok(true);
    }

    private IActionResult ServiceSettings(Guid shopGuid, Guid shopSettingsGuid, string? serviceSettingsName, Guid? guid = null)
    {
        if(shopSettingsGuid == Guid.Empty)
            return BadRequest(nameof(shopSettingsGuid));
        
        if(shopGuid == Guid.Empty)
            return BadRequest(nameof(shopGuid));
        
        if(string.IsNullOrEmpty(serviceSettingsName) && guid == null)
            return BadRequest(nameof(serviceSettingsName));

        if (!_importFacade.TryGetShopImport(shopGuid, out var shopImport))
            return NotFound(shopGuid);

        if (!_importFacade.TryGetShopSettings(shopGuid, shopSettingsGuid, out var shopSettings))
            return NotFound(shopSettingsGuid);

        ServiceSettingsModel? serviceSettingsModel;
        if (!string.IsNullOrEmpty(serviceSettingsName))
        { 
            if(!_importFacade.TryGetServiceSettingsModel(shopGuid, shopSettingsGuid, serviceSettingsName, out serviceSettingsModel))
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

    [HttpPost]
    [ProducesResponseType<PartialViewResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    public IActionResult ImportServiceSettings(Guid shopGuid, Guid shopSettingsGuid)
    {
        return ServiceSettings(shopGuid, shopSettingsGuid, nameof(ShopSettingsModel.ImportService));
    }


    [HttpPost]
    [ProducesResponseType<PartialViewResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    public IActionResult BrowserDataLoaderSettings(Guid shopGuid, Guid shopSettingsGuid)
    {
        return ServiceSettings(shopGuid, shopSettingsGuid, nameof(ShopSettingsModel.BrowserDataLoader));
    }

    [HttpPost]
    [ProducesResponseType<PartialViewResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    public IActionResult WebLoaderSettings(Guid shopGuid, Guid shopSettingsGuid)
    {
        return ServiceSettings(shopGuid, shopSettingsGuid, nameof(ShopSettingsModel.WebLoader));
    }

    [HttpPost]
    [ProducesResponseType<PartialViewResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    public IActionResult ServiceSettings(Guid shopGuid, Guid shopSettingsGuid, Guid? guid)
    {
        return ServiceSettings(shopGuid, shopSettingsGuid, "", guid ?? Guid.NewGuid());
    }

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
                shopSettings.SetServiceSettings(shopSettings.CreateServiceSettingsModel(data.Name));

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


    [HttpPost]
    [ProducesResponseType<OkObjectResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ObjectResult>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SaveProductShopSettingsToDb(ProductShopSettingsModel productShopSettings)
    {
        if (productShopSettings == null) return BadRequest(nameof(productShopSettings));

        if (!_importFacade.TryGetShopImport(productShopSettings.ShopGuid, out var shopImport) || shopImport == null)
            return NotFound(productShopSettings.ShopGuid);

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
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ObjectResult>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SaveCategoryShopSettingsToDb(CategoryShopSettingsModel categoryShopSettings)
    {
        if (categoryShopSettings == null) return BadRequest(nameof(categoryShopSettings));

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

    [HttpPost]
    [ProducesResponseType<PartialViewResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    public IActionResult RootCategory(CategoryUrlModel data)
    {
        if (data == null)
            return BadRequest("category url is null");
        
        if (data.ShopSettingsGuid == Guid.Empty)
            return BadRequest("category shopSettingsGuid is empty");

        if (data.Guid == Guid.Empty)
            data.Guid = Guid.NewGuid();

        return PartialView("~/Views/Home/RootCategoryUrl.cshtml",data );
    }

    [HttpPost]
    [ProducesResponseType<OkObjectResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    public IActionResult SetRootCategory(CategoryUrlModel data)
    {
        if (data == null)
            return BadRequest(data);

        if (!_importFacade.TryGetShopSettings(data.ShopSettingsGuid, out var shopSettings) 
            || shopSettings is not ProductShopSettingsModel productShopSettings )
            return NotFound(data.ShopSettingsGuid);

        var rootCategory = productShopSettings.RootCategories.FirstOrDefault(c=>c.Guid == data.Guid);
        if (rootCategory != null)
            rootCategory.Update(data);
        else productShopSettings.RootCategories.Add(data);

        return Ok(data);
    }
}
