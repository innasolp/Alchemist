using Alchemist.Product.Import.WebApp.Infrastructure;
using Alchemist.Product.Import.WebApp.Models;
using Alchemist.Product.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Alchemist.Product.Import.WebApp.Controllers;

public class HomeController(ILogger<HomeController> logger,
    List<ShopModel> shops,
    IDictionary<int, ShopImportModel> shopImports) : Controller
{
    private readonly List<ShopModel> _shops = shops;

    private readonly ILogger<HomeController> _logger = logger;

    private readonly IDictionary<int, ShopImportModel> _shopImports = shopImports;

    public IActionResult Index()
    {
        return View();
    }

    [Route("Home/Index/shopId={shopId}&tab={tab}")]
    public IActionResult Index(int shopId, int tab)
    {
        ViewData["ShopId"] = shopId;
        ViewData["Tab"] = (TabType)tab;

        return View();
    }

    [HttpPost]
    public bool SaveTabSettings(int shopId, int tab, string json)
    {
        if (string.IsNullOrEmpty(json)) return false;

        if (!_shopImports.TryGetValue(shopId, out var shopImport) || shopImport == null)
            return false;

        var settings = ((TabType)tab).GetSettingsByTypeFromJson(json);
        shopImport.UpdateSettings(settings);
        
        return true;
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
    public IActionResult ShopSettingTabs()
    {
        return PartialView();
    }

    private ShopSettingsModel? GetShopSettings(int shopId, ShopSettingType shopSettingType)
    {
        return _shopImports.TryGetValue(shopId, out var shopImportModel)
               && shopImportModel != null && shopImportModel.ShopSettingTabs != null
           ? (shopImportModel.ShopSettingTabs.GetShopSettingsByType(shopSettingType)
              ?? new ShopSettingsModel { ShopId = shopId, ShopSettingType = shopSettingType })
           : null;
    }

    [HttpPost]
    public bool SaveShopSettings(int shopId, string json)
    {
        if (string.IsNullOrEmpty(json)) return false;

        var shopSettings = json.DeserializeWithNumberHandling<ShopSettingsModel>();
        if (shopSettings == null || shopSettings?.ShopSettingType == ShopSettingType.Service)
            throw new InvalidOperationException("Invalid json for shop settings");

        var shopSettingsModel = GetShopSettings(shopId, shopSettings.ShopSettingType);

        shopSettingsModel?.Update(shopSettings);

        return true;
    }

    [HttpPost]
    public bool SetShopSettings(int shopId, int shopSettingType)
    {
        if ((ShopSettingType)shopSettingType == ShopSettingType.Service)
            throw new InvalidOperationException("Invalid json for shop settings");

        var shopImport = _shopImports.FirstOrDefault(s => s.Key == shopId).Value;
        if (shopImport != null)
        {
            shopImport.ShopSettingTabs.SelectedSettingsTab = (ShopSettingType)shopSettingType;
        }

        return shopImport != null;
    }

    private ServiceSettingsModel? GetServiceSettingsModel(int shopId, int shopSettingType, string serviceSettingsName)
    {
        return _shopImports.TryGetValue(shopId, out var shopImportModel)
                && shopImportModel != null && shopImportModel.ShopSettingTabs != null
            ? (shopImportModel.ShopSettingTabs.GetShopSettingsByType((ShopSettingType)shopSettingType)?.GetServiceSettings(serviceSettingsName) ?? new ServiceSettingsModel { ShopId = shopId, ServiceName = serviceSettingsName })
            : null;
    }

    private IActionResult ServiceSettings(int shopId, int shopSettingType, string serviceSettingsName)
    {
        var serviceSettingsModel = GetServiceSettingsModel(shopId, shopSettingType, serviceSettingsName);

        return serviceSettingsModel == null
            ? throw new InvalidDataException($"No data for shop {shopId} and settings {(ShopSettingType)shopSettingType}")
            : (IActionResult)PartialView("~/Views/Home/ServiceSettings.cshtml", serviceSettingsModel);
    }

    [HttpPost]
    public IActionResult ImportServiceSettings(int shopId, int shopSettingType)
    {
        return ServiceSettings(shopId, shopSettingType, nameof(ShopSettingsModel.ImportService));
    }


    [HttpPost]
    public IActionResult BrowserDataLoaderSettings(int shopId, int shopSettingType)
    {
        return ServiceSettings(shopId, shopSettingType, nameof(ShopSettingsModel.BrowserDataLoader));
    }

    [HttpPost]
    public IActionResult WebLoaderSettings(int shopId, int shopSettingsId)
    {
        return ServiceSettings(shopId, shopSettingsId, nameof(ShopSettingsModel.WebLoader));
    }

    [HttpPost]
    public bool SaveServiceSettings(ServiceSettingsModel data)
    {
        if (data != null && _shopImports.TryGetValue(data.ShopId, out var shopImportModel) && shopImportModel != null)
        {
            return shopImportModel.ShopSettingTabs.GetShopSettingsByType(data.ShopSettingType)
                .UpdateServiceSettings(data);
        }

        return false;
    }    

    //todo
    //[HttpPost]
    //public bool SaveShopSettings(int shopId)
    //{
    //    //todo
    //    return true;
    //}

    public IActionResult ImportProducts()
    {
        return PartialView();
    }
    
    public IActionResult ImportCategories()
    {
        return PartialView();
    }
}
