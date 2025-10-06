using Alchemist.Product.ImportSettingsWebApp.Models;
using Alchemist.Product.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Alchemist.Product.ImportSettingsWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

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

        [Route("/Import/Settings/{shopId:int}/{shopSettingsType:int}")]
        [HttpGet]
        public async Task<IActionResult> ImportSettings(int? shopId, int shopSettingsType)
        {
            return View("~/Views/Home/Index.cshtml", new IndexModel
                {
                    ShopImportSettingsTabModel = new ShopImportSettingsTabModel
                    {
                        ShopId = shopId,
                        ShopSettingType = (ShopSettingType)shopSettingsType
                    }
                }
            );
        }
    }
}
