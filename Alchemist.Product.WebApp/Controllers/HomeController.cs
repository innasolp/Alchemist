using Alchemist.Product.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Alchemist.Product.WebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return Redirect("/Shop/1/1");
    }

    [Route("/Shop/{shopId:int}")]
    public IActionResult IndexShops(int shopId)
    {
        return View("~/Views/Home/Index.cshtml", new IndexModel { Tab = Tab.Shop, Data = shopId});
    }

    [Route("/Import/Settings/{shopId:int}/{shopSettingsType:int}")]    
    public IActionResult IndexImportSettings(int shopId, int shopSettingsType)
    {
        return View("~/Views/Home/Index.cshtml", new IndexModel
        {
            Tab = Tab.ImportSettings,
            Data = new { ShopId = shopId, ShopSettingsType = shopSettingsType }
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
