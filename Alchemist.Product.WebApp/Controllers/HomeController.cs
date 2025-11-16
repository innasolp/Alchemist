using Alchemist.Product.Interfaces;
using Alchemist.Product.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Alchemist.Product.WebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppStore _appStore;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
        _appStore = new AppStore(this);
    }

    public IActionResult Index()
    {
        return Redirect("/Shop/");
    }

    [Route("/Shop/{shopId:int?}")]
    public IActionResult IndexShops(int? shopId)
    {
        _appStore.SetAppUrl(AppStore.ShopApp, Request.Path.Value);
        return View("~/Views/Home/Index.cshtml", 
            new IndexModel { Tab = Tab.Shop, AppName = AppStore.ShopApp, Data = new { ShopId = shopId }, AppUrls = _appStore.GetAppUrls() });
    }

    [Route("/Shop/New")]
    public IActionResult NewShop()
    {
        _appStore.SetAppUrl(AppStore.ShopApp, Request.Path.Value);
        return View("~/Views/Home/Index.cshtml", 
            new IndexModel { Tab = Tab.Shop, AppName = AppStore.ShopApp, Data = new { ShopId = 0 } , AppUrls = _appStore.GetAppUrls() });
    }

    [Route("/Import/Settings/{shopId:int?}/{shopSettingsType:ShopSettingType?}")]    
    public IActionResult IndexImportSettings(int? shopId, ShopSettingType? shopSettingsType)
    {
        _appStore.SetAppUrl(AppStore.ImportSettingsApp, Request.Path.Value);
        return View("~/Views/Home/Index.cshtml", new IndexModel
        {
            Tab = Tab.ImportSettings,
            AppName = AppStore.ImportSettingsApp,
            Data =  new { ShopId = shopId, ShopSettingsType = shopSettingsType },
            AppUrls = _appStore.GetAppUrls()
        });
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
}
