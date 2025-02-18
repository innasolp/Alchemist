using Alchemist.Product.Import.WebApp.Infrastructure;
using Alchemist.Product.Import.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Alchemist.Product.Import.WebApp.Controllers;

public record class SettingsData(int ShopId, SettingsType SettingsType, string Json);

public class HomeController(ILogger<HomeController> logger,
    List<ShopModel> shops,
    IReadOnlyCollection<string> tabs,
    IDictionary<int, ShopImportModel> shopImports) : Controller
{
    private readonly List<ShopModel> _shops = shops;

    private readonly IReadOnlyCollection<string> _tabs = tabs;

    private readonly ILogger<HomeController> _logger = logger;

    private readonly IDictionary<int, ShopImportModel> _shopImports = shopImports;

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost("Home/Index/shopId={shopId}&tab={tab}")]
    public IActionResult Index(int shopId, string tab)
    {
        ViewData["ShopId"] = shopId;
        ViewData["Tab"] = tab;

        return View();
    }

    [HttpPost]
    public IActionResult Index(SettingsData prevSettings)
    {
        var shopImport = _shopImports.FirstOrDefault(s => s.Key == prevSettings.ShopId).Value;
        if(shopImport != null)
        {
            var settings = TabFactory.GetSettingsByTypeFromJson(prevSettings.SettingsType, prevSettings.Json);
            TabFactory.UpdateSettings(shopImport, settings);
        }
        return View();
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
    
    public IActionResult Settings()
    {
        return PartialView();
    }

    private IActionResult ServiceSettings(int shopId, int shopSettingsId, 
        Func<ShopSettingsModel?,ServiceSettingsModel?> getServiceSettings,
        Action<ShopSettingsModel, ServiceSettingsModel> setSettingsIfNeed)
    {
        ViewData["ShopId"] = shopId;
        ViewData["ShopSettingsId"] = shopSettingsId;

        var serviceSettingsModel = _shopImports.TryGetValue(shopId, out var shopImportModel)
                && shopImportModel != null && shopImportModel.ShopSettings != null
            ? (getServiceSettings(shopImportModel.ShopSettings) ?? new ServiceSettingsModel(shopId))
            : null;

        if (serviceSettingsModel == null)
            throw new InvalidDataException($"No data for shop {shopId} and settings {shopSettingsId}");

        if (getServiceSettings(shopImportModel.ShopSettings) == null)
            setSettingsIfNeed(shopImportModel.ShopSettings, serviceSettingsModel);

        if (shopImportModel?.ShopSettings?.ServiceSettings.Any(s => s.Guid == serviceSettingsModel?.Guid) != true)
            shopImportModel?.ShopSettings?.ServiceSettings.Add(serviceSettingsModel);

        return PartialView("~/Views/Home/ServiceSettings.cshtml", serviceSettingsModel);
    }

    public IActionResult ImportServiceSettings(int shopId, int shopSettingsId)
    {
        return ServiceSettings(shopId, shopSettingsId, 
            (shopSettings) => shopSettings?.ImportService,
            (shopSettings, serviceSettings) => shopSettings.ImportService = serviceSettings);
    }


    public IActionResult BrowserDataLoaderSettings(int shopId, int shopSettingsId)
    {
        return ServiceSettings(shopId, shopSettingsId,
            (shopSettings) => shopSettings?.BrowserDataLoader,
            (shopSettings, serviceSettings) => shopSettings.BrowserDataLoader = serviceSettings);
    }
    
    public IActionResult WebLoaderSettings(int shopId, int shopSettingsId)
    {
        return ServiceSettings(shopId, shopSettingsId,
            (shopSettings) => shopSettings?.WebLoader,
            (shopSettings, serviceSettings) => shopSettings.WebLoader = serviceSettings);
    }

    [HttpPost]
    public bool SaveServiceSettings(ServiceSettingsModel data)//, Func<ShopSettingsModel?, ServiceSettingsModel?> getServiceSettings)
    {
        if (data != null && data.ShopId != 0 && _shopImports.TryGetValue(data.ShopId, out var shopImportModel) && shopImportModel != null)
        {
            var serviceSettingsModel = shopImportModel.ShopSettings?.ServiceSettings.FirstOrDefault(s=>s.Guid == data.Guid);// getServiceSettings(shopImportModel?.ShopSettings);
            if (serviceSettingsModel == null)
                return false;
            
            serviceSettingsModel.Update(data);

            return true;
        }

        return false;
    }    

    public IActionResult ImportProducts()
    {
        return PartialView();
    }
    
    public IActionResult ImportCategories()
    {
        return PartialView();
    }
}
