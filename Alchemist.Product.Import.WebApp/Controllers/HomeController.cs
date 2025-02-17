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

    public IActionResult ImportServiceSettings(int shopId, int id, int shopSettingsId)
    {
        ViewData["ShopId"] = shopId;
        ViewData["Id"] = id;
        ViewData["ShopSettingsId"] = shopSettingsId;

        var serviceSettingsModel = _shopImports.TryGetValue(shopId, out var shopImportModel)
            ? (shopImportModel.ShopSettings?.ImportService ?? new ServiceSettingsModel(shopId))
            : null;

        return PartialView("~/Views/Home/ServiceSettings.cshtml", serviceSettingsModel);
    }

    public IActionResult ServiceSettings(int shopId, int id, int shopSettingsId)
    {
        ViewData["ShopId"] = shopId;
        ViewData["Id"] = id;
        ViewData["ShopSettingsId"] = shopSettingsId;

        var serviceSettingsModel = _shopImports.TryGetValue(shopId, out var shopImportModel)
            ? (shopImportModel.ShopSettings?.ServiceSettings.First(s=>s.Id == id) ?? new ServiceSettingsModel(shopId ))
            : null;

        return PartialView(serviceSettingsModel);
    }    

    [HttpPost]
    public IActionResult SaveImportServiceSettings(ServiceSettingsModel data)
    {
        if (data != null && data.ShopId != 0 && data.ShopSettingsId != 0
            && _shopImports.TryGetValue(data.ShopId, out var shopImportModel))
        {
            if (shopImportModel.ShopSettings.ImportService == null)
                shopImportModel.ShopSettings.ImportService = new ServiceSettingsModel(data.ShopId);

            

            shopImportModel.ShopSettings.ImportService.Update(data);
        }

        return View("~/Views/Home/Index.cshtml");
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
