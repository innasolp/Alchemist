using Alchemist.Product.Import.ShopWebApp.Models;
using Alchemist.Product.ShopWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Alchemist.Product.ShopWebApp.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class HomeController(ILogger<HomeController> logger) : Controller
{
    private readonly ILogger<HomeController> _logger = logger;

    public IActionResult Index()
    {
        return View("~/Views/Home/Index.cshtml", new IndexModel());
    }

    [Route($"/{ControllerPrefix.App}/Index")]
    [Route($"/{ControllerPrefix.App}/")]
    [ActionName("Index")]
    public IActionResult IndexFromQuery([FromQuery] int shopId)
    {
        if (shopId <= 0)
            return BadRequest($"Invalid shopId : {shopId}");

        return View("~/Views/Home/Index.cshtml", new IndexModel() { ShopId = shopId });
    }

    [Route($"/{ControllerPrefix.App}/Index/{{shopId:int}}")]
    [Route($"/{ControllerPrefix.App}/{{shopId:int}}")]
    [ActionName("Index")]
    public IActionResult IndexRoute(int shopId)
    {
        if (shopId < 0)
            return BadRequest($"Invalid shopId : {shopId}");

        return View("~/Views/Home/Index.cshtml", new IndexModel() { ShopId = shopId });
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

    [Route($"{ControllerPrefix.App}/New")]
    [Route($"Home/New")]
    public IActionResult New()
    {
        return View("~/Views/Home/Index.cshtml", new IndexModel { ShopId = 0 });
    }
}
