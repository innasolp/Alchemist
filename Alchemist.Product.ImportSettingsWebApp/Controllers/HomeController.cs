using Alchemist.Product.ImportSettingsWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using ShopSettings.Interfaces;
using System.Diagnostics;

namespace Alchemist.Product.ImportSettingsWebApp.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class HomeController(ILogger<HomeController> logger) : Controller
{
    private readonly ILogger<HomeController> _logger = logger;

    public IActionResult Index()
    {
        return View(new IndexModel());
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

    [Route("/Import/Settings/{shopId:int}/{shopSettingsType:ShopSettingType}")]
    [HttpGet]
    public IActionResult ImportSettings(int? shopId, ShopSettingType shopSettingsType)
    {
        return View("~/Views/Home/Index.cshtml", new IndexModel
        {
            ShopId = shopId,
            ShopSettingType = shopSettingsType
        }
        );
    }
}