using Alchemist.Import.Settings.Adapter;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Alchemist.Product.Import.WebApp.Controllers;

public class ShopSettingsController(ILogger<ShopSettingsController> logger, IImportFacade importFacade, 
    ISettingsDataAdapter<ProductShopSettingsModel, CategoryShopSettingsModel, ServiceSettingsModel> settingsDataAdapter) : Controller
{
    private readonly ILogger<ShopSettingsController> _logger = logger;

    private readonly IImportFacade _importFacade = importFacade;

    private readonly ISettingsDataAdapter<ProductShopSettingsModel, CategoryShopSettingsModel, ServiceSettingsModel> _settingsDataAdapter = settingsDataAdapter;

    [HttpPost]
    public bool SaveShopSettings(Guid shopGuid, int shopSettingType, string json)
    {
        if (string.IsNullOrEmpty(json)) return false;

        ShopSettingsModel? shopSettings = json.GetShopSettingsFromJson((ShopSettingType)shopSettingType)
            ?? throw new InvalidOperationException("Invalid json for shop settings");

        if (!_importFacade.TryGetShopSettings(shopGuid, shopSettings.ShopSettingType, out var shopSettingsModel))
            shopSettingsModel = shopGuid.CreateShopSettings(shopSettings.ShopSettingType);

        shopSettingsModel?.Update(shopSettings);

        return true;
    }

    [HttpPost]
    public bool SetShopSettings(Guid shopGuid, int shopSettingType)
    {
        if ((ShopSettingType)shopSettingType == ShopSettingType.Service)
            throw new InvalidOperationException("Invalid json for shop settings");

        if (!_importFacade.TryGetShopImport(shopGuid, out var shopImport))
            return false;

        shopImport.ShopSettingTabs.SelectedSettingsTab = (ShopSettingType)shopSettingType;

        return shopImport != null;
    }

    private IActionResult ServiceSettings(Guid shopGuid, Guid shopSettingsGuid, string serviceSettingsName)
    {
        try
        {
            if (!_importFacade.TryGetServiceSettingsModel(shopGuid, shopSettingsGuid, serviceSettingsName, out var serviceSettingsModel))
                serviceSettingsModel = shopGuid.CreateServiceSettingsModel(shopSettingsGuid, serviceSettingsName);

            return serviceSettingsModel == null
                ? throw new InvalidDataException($"No data for shop {shopGuid} and settings {shopSettingsGuid}")
                : (IActionResult)PartialView("~/Views/Home/ServiceSettings.cshtml", serviceSettingsModel);
        }
        catch (Exception e) when (e is InvalidOperationException || e is InvalidDataException)
        {
            _logger.LogError(e, $"/{nameof(ShopSettingsController)}/{serviceSettingsName}Settings(shopGuid:{shopGuid},shopSettingsGuid:{shopSettingsGuid})");
            return BadRequest();
        }
    }

    [HttpPost]
    public IActionResult ImportServiceSettings(Guid shopGuid, Guid shopSettingsGuid)
    {
        return ServiceSettings(shopGuid, shopSettingsGuid, nameof(ShopSettingsModel.ImportService));
    }


    [HttpPost]
    public IActionResult BrowserDataLoaderSettings(Guid shopGuid, Guid shopSettingsGuid)
    {
        return ServiceSettings(shopGuid, shopSettingsGuid, nameof(ShopSettingsModel.BrowserDataLoader));
    }

    [HttpPost]
    public IActionResult WebLoaderSettings(Guid shopGuid, Guid shopSettingsGuid)
    {
        return ServiceSettings(shopGuid, shopSettingsGuid, nameof(ShopSettingsModel.WebLoader));
    }

    [HttpPost]
    public bool SaveServiceSettings(ServiceSettingsModel data)
    {
        if (data == null || !_importFacade.TryGetShopImport(data.ShopGuid, out var shopImport))
            return false;

        shopImport.ShopSettingTabs?.GetShopSettingsByGuid(data.ShopSettingsGuid)?
                 .UpdateServiceSettings(data);

        return true;
    }

    [HttpPost]
    public async Task<bool> SaveProductShopSettingsToDb(ProductShopSettingsModel productShopSettings)
    {
        if (productShopSettings == null) return false;

        if (!_importFacade.TryGetShopImport(productShopSettings.ShopGuid, out var shopImport) || shopImport == null)
            return false;

        try
        {
            shopImport.ShopSettingTabs?.ShopProductsSettings?.Update(productShopSettings);

            await _settingsDataAdapter.Save(shopImport.ShopSettingTabs.ShopProductsSettings);

            return true;
        }
        catch(Exception e)
        {
            _logger.LogError(e, $"/{nameof(ShopSettingsController)}/{nameof(SaveProductShopSettingsToDb)}/({productShopSettings})");
            return false;
        }
    }

    [HttpPost]
    public async Task<bool> SaveCategoryShopSettingsToDb(CategoryShopSettingsModel categoryShopSettings)
    {
        if (categoryShopSettings == null) return false;

        if (!_importFacade.TryGetShopImport(categoryShopSettings.ShopGuid, out var shopImport) || shopImport == null)
            return false;

        try
        {
            shopImport.ShopSettingTabs.ShopCategoriesSettings.Update(categoryShopSettings);

            await _settingsDataAdapter.Save(shopImport.ShopSettingTabs.ShopCategoriesSettings);

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"/{nameof(ShopSettingsController)}/{nameof(SaveCategoryShopSettingsToDb)}/({categoryShopSettings})");
            return false;
        }    
    }

}
