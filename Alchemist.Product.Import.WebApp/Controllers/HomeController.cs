using Alchemist.Product.Import.WebApp.Infrastructure;
using Alchemist.Product.Import.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Alchemist.Product.Import.WebApp.Controllers;

public record class SettingsData(int ShopId, TabType SettingsType, string Json);

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

    [Route("Home/Index/shopId={shopId}&tab={tab}")]
    public IActionResult Index(int shopId, string tab)
    {
        ViewData["ShopId"] = shopId;
        ViewData["Tab"] = tab;

        return View();
    }

    [HttpPost]
    public bool SaveTabSettings(int shopId, int tab, string json)
    {
        if (string.IsNullOrEmpty(json)) return false;

        var shopImport = _shopImports.FirstOrDefault(s => s.Key == shopId).Value;
        if (shopImport != null)
        {
            var settings = ((TabType)tab).GetSettingsByTypeFromJson(json);
            shopImport.UpdateSettings(settings);
        }
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
    
    public IActionResult Settings()
    {
        return PartialView();
    }

    private IActionResult ServiceSettings(int shopId, int shopSettingsId, string serviceSettingsName)
    {
        var serviceSettingsModel = _shopImports.TryGetValue(shopId, out var shopImportModel)
                && shopImportModel != null && shopImportModel.ShopSettings != null
            ? (shopImportModel.ShopSettings.GetServiceSettings(serviceSettingsName) ?? new ServiceSettingsModel { ShopId = shopId, ServiceName = serviceSettingsName })
            : null;

        return serviceSettingsModel == null
            ? throw new InvalidDataException($"No data for shop {shopId} and settings {shopSettingsId}")
            : (IActionResult)PartialView("~/Views/Home/ServiceSettings.cshtml", serviceSettingsModel);
    }

    [HttpPost]
    public IActionResult ImportServiceSettings(int shopId, int shopSettingsId)
    {
        return ServiceSettings(shopId, shopSettingsId, nameof(ShopSettingsModel.ImportService));
    }


    [HttpPost]
    public IActionResult BrowserDataLoaderSettings(int shopId, int shopSettingsId)
    {
        return ServiceSettings(shopId, shopSettingsId, nameof(ShopSettingsModel.BrowserDataLoader));
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
            return shopImportModel.ShopSettings.UpdateServiceSettings(data);
        }

        return false;
    }    

    [HttpPost]
    public bool SaveShopSettings(int shopId)
    {
        //todo
        return true;
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
