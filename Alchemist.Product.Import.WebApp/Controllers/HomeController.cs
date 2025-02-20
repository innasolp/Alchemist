using Alchemist.Product.Import.WebApp.Infrastructure;
using Alchemist.Product.Import.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

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
            shopImport.UpdateSettings(settings);
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

    private IActionResult ServiceSettings(int shopId, int shopSettingsId, string serviceSettingsName)
    {
        ViewData["ShopId"] = shopId;
        ViewData["ShopSettingsId"] = shopSettingsId;

        var serviceSettingsModel = _shopImports.TryGetValue(shopId, out var shopImportModel)
                && shopImportModel != null && shopImportModel.ShopSettings != null
            ? (shopImportModel.ShopSettings.GetServiceSettings(serviceSettingsName) ?? new ServiceSettingsModel { ShopId = shopId, ServiceName = serviceSettingsName })
            : null;

        if (serviceSettingsModel == null)
            throw new InvalidDataException($"No data for shop {shopId} and settings {shopSettingsId}");

        return PartialView("~/Views/Home/ServiceSettings.cshtml", serviceSettingsModel);
    }

    public IActionResult ImportServiceSettings(int shopId, int shopSettingsId)
    {
        return ServiceSettings(shopId, shopSettingsId, nameof(ShopSettingsModel.ImportService));
    }


    public IActionResult BrowserDataLoaderSettings(int shopId, int shopSettingsId)
    {
        return ServiceSettings(shopId, shopSettingsId, nameof(ShopSettingsModel.BrowserDataLoader));
    }
    
    public IActionResult WebLoaderSettings(int shopId, int shopSettingsId)
    {
        return ServiceSettings(shopId, shopSettingsId, nameof(ShopSettingsModel.WebLoader));
    }

    [HttpPost]
    public bool SaveServiceSettings(ServiceSettingsModel data)//, Func<ShopSettingsModel?, ServiceSettingsModel?> getServiceSettings)
    {
        if (data != null && data.ShopId != 0 && _shopImports.TryGetValue(data.ShopId, out var shopImportModel) && shopImportModel != null)
        {
            return shopImportModel.ShopSettings.UpdateServiceSettings(data);
        }

        return false;
    }

    [HttpPost]
    public ServiceSettingsModel? SetServiceSettings(int shopId, string serviceSettingsName, string json)
    {
        //todo for net.9
        //JsonSerializerOptions options = new()
        //{
        //    RespectRequiredConstructorParameters = true
        //};

        var serviceSettings = JsonSerializer.Deserialize<ServiceSettingsModel>(json);

        if (serviceSettings == null) return null;

        if (_shopImports.TryGetValue(shopId, out var shopImportModel)
            && shopImportModel != null && shopImportModel.ShopSettings != null)
        {
            serviceSettings.ServiceName = serviceSettingsName;
            shopImportModel.ShopSettings.UpdateServiceSettings(serviceSettings);

            return serviceSettings;
        }

        return null;
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
