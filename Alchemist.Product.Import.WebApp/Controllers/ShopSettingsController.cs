using Alchemist.Import.Settings.DataAdapter;
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

        var shopSettings = shopImport.ShopSettingTabs?.GetShopSettingsByType((ShopSettingType)shopSettingType);
        if (shopSettings == null)
        {
            try
            {
                shopSettings = (await _settingsDataAdapter.GetShopImportSettings(shopImport.Shop.Id, (ShopSettingType)shopSettingType) as ShopSettingsModel)
                    ?? shopImport.CreateShopSettings((ShopSettingType)shopSettingType);
            }
            catch (Exception e)
            {
                return new ObjectResult(e) { StatusCode = StatusCodes.Status500InternalServerError };
            }
            shopImport.SetSettings(TabType.Shop, shopSettings);
        }

        shopImport.ShopSettingTabs.SelectedSettingsTab = (ShopSettingType)shopSettingType;

        return PartialView("~/Views/Home/ShopSettings.cshtml", shopSettings);
    }

    [Route("ShopSettings/Save")]
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
            shopSettings = ModelHelper.GetShopSettingsFromJson(json, (ShopSettingType)shopSettingType);
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


    [Route("ShopSettings/Save/Products")]
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

    [Route("ShopSettings/Save/Category")]
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

    [Route("ShopSettings/RootCategory/Set")]
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
