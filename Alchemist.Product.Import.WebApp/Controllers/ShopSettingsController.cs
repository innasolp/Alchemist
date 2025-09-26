using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Import.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ModelHelper = Alchemist.Product.Import.WebApp.Models.ModelHelper;

namespace Alchemist.Product.Import.WebApp.Controllers;

public class ShopSettingsController(ILogger<ShopSettingsController> logger,
    IImportFacade importFacade,
    IModelFactory modelFactory,
    [FromKeyedServices(ShopSettingType.Product)] ISettingsDataAdapter productSettingsDataAdapter,
    [FromKeyedServices(ShopSettingType.Category)] ISettingsDataAdapter categorySettingsDataAdapter) : Controller
{
    private readonly ILogger<ShopSettingsController> _logger = logger;

    private readonly IImportFacade _importFacade = importFacade;

    private readonly IModelFactory _modelFactory = modelFactory;

    private readonly SettingsDataAdapterContainer _settingsDataAdapter = new(productSettingsDataAdapter, categorySettingsDataAdapter);

    [Route("ShopSettings/")]
    [HttpPost]
    [ProducesResponseType<PartialViewResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ObjectResult>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ShopSettings(Guid shopGuid, int shopSettingType)
    {
        if (shopGuid == Guid.Empty)
            return BadRequest(shopGuid);

        if ((ShopSettingType)shopSettingType == ShopSettingType.Service)
            return BadRequest((ShopSettingType)shopSettingType);

        if (!_importFacade.TryGetShopImport(shopGuid, out var shopImport))
            return NotFound(shopGuid);

        var shopSettingsModel = shopImport.ShopSettingTabs.GetShopSettingsByType((ShopSettingType)shopSettingType);
        if (shopSettingsModel.IsEmpty())
        {
            try
            {
                var shopSettings = await _settingsDataAdapter.GetShopImportSettingsAsync(shopImport.Shop.Id, (ShopSettingType)shopSettingType);
                if(shopSettings != null)
                    _modelFactory.Update(shopSettingsModel, shopSettings);
            }
            catch (Exception e)
            {
                return new ObjectResult(e) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        if(shopImport.ShopSettingTabs is  ShopSettingTabsModel shopSettingsTab)
            shopSettingsTab.SelectedSettingsTab = (ShopSettingType)shopSettingType;

        return PartialView("~/Views/Home/ShopSettings.cshtml", shopSettingsModel);
    }

    [Route("ShopSettings/Save")]
    [HttpPost]
    [ProducesResponseType<OkObjectResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    public IActionResult SaveShopSettings(Guid shopGuid, int shopSettingType, string json)
    {
        if (string.IsNullOrEmpty(json)) return BadRequest(json);

        ShopSettingsModel shopSettingsFromJson;
        try
        {
            shopSettingsFromJson = ModelHelper.GetShopSettingsFromJson(json, (ShopSettingType)shopSettingType);
        }
        catch (JsonException)
        {
            return BadRequest(json);
        }
        if (shopSettingsFromJson == null)
            return BadRequest(json);

        if (!_importFacade.TryGetShopImport(shopGuid, out var shopImport)
            || !_importFacade.TryGetShopSettings(shopGuid, shopSettingsFromJson.ShopSettingType, out var shopSettingsModel))
            return NotFound(shopGuid);        

        _modelFactory.Update(shopSettingsModel, shopSettingsFromJson);

        return Ok(true);
    }


    [Route("ShopSettings/Save/Product")]
    [HttpPost]
    [ProducesResponseType<OkObjectResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ObjectResult>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SaveProductShopSettingsToDb([ModelBinder(typeof(ModelImplementationJsonBinder))] ProductShopSettingsModel data)
    {
        if (data == null) return BadRequest(nameof(data));

        if (!_importFacade.TryGetShopImport(data.ShopGuid, out var shopImport) || shopImport == null)
            return NotFound(data.ShopGuid);

        try
        {
            shopImport.ShopSettingTabs?.ShopProductsSettings?.UpdateProductShopSettingsWithoutServices(data);            

            await _settingsDataAdapter.SaveAsync(shopImport.ShopSettingTabs.ShopProductsSettings);

            return Ok(data);
        }
        catch (Exception e)
        {
            return new ObjectResult(e) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }

    [Route("ShopSettings/Save/Category")]
    [HttpPost]
    [ProducesResponseType<OkObjectResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ObjectResult>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SaveCategoryShopSettingsToDb([ModelBinder(typeof(ModelImplementationJsonBinder))] CategoryShopSettingsModel data)
    {
        if (data == null) return BadRequest(nameof(data));

        if (!_importFacade.TryGetShopImport(data.ShopGuid, out var shopImport) || shopImport == null)
            return NotFound(data.ShopGuid);

        try
        {
            shopImport.ShopSettingTabs.ShopCategoriesSettings.UpdateCategoryShopSettingsWithoutServices(data);

            await _settingsDataAdapter.SaveAsync(shopImport.ShopSettingTabs.ShopCategoriesSettings);

            return Ok(data);
        }
        catch (Exception e)
        {
            return new ObjectResult(e) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }

    [Route("ShopSettings/RootCategory/Edit")]
    [HttpPost]
    [ProducesResponseType<PartialViewResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    public IActionResult RootCategory([ModelBinder(typeof(ModelImplementationJsonBinder))] CategoryUrlModel data)
    {
        if (data == null)
            return BadRequest("category url is null");
        
        if (data.ShopSettingsGuid == Guid.Empty)
            return BadRequest("category shopSettingsGuid is empty");        

        return PartialView("~/Views/Home/RootCategoryUrl.cshtml",data);
    }

    [Route("ShopSettings/RootCategory/New")]
    [HttpPost]
    [ProducesResponseType<PartialViewResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    public IActionResult AddRootCategory(Guid shopSettingsGuid)
    {     
        if (shopSettingsGuid == Guid.Empty)
            return BadRequest("category shopSettingsGuid is empty");

        if (!_importFacade.TryGetShopSettings(shopSettingsGuid, out var shopSettingsModel)
            || shopSettingsModel is not IProductShopSettingsModel productShopSettingsModel )
            return NotFound(shopSettingsGuid);

        var rootCategory = new CategoryUrlModel(shopSettingsGuid);

        return PartialView("~/Views/Home/RootCategoryUrl.cshtml", rootCategory);
    }

    [Route("ShopSettings/RootCategory/Set")]
    [HttpPost]
    [ProducesResponseType<OkObjectResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    public IActionResult SetRootCategory([ModelBinder(typeof(ModelImplementationJsonBinder))] CategoryUrlModel data)
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
