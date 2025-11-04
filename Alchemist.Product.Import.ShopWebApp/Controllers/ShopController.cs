using Alchemist.DataService.Interfaces;
using Alchemist.Product.Import.ShopWebApp.Models;
using Alchemist.Product.Interfaces;
using Alchemist.Product.ShopWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Alchemist.Product.ShopWebApp.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class ShopController(ILogger<ShopController> logger, IShopDataService shopDataService) : Controller
{
    private readonly ILogger<ShopController> _logger = logger;

    private readonly ShopFacade _shopFacade = new(shopDataService);
    

    public IActionResult Index()
    {
        return View("~/Views/Home/Index.cshtml", new IndexModel());
    }

    [Route("/Shop/Index")]
    [Route("/Shop/")]
    [ActionName("Index")]
    public IActionResult IndexFromQuery([FromQuery] int shopId)
    {
        if (shopId <= 0)
            return BadRequest($"Invalid shopId : {shopId}");

        return View("~/Views/Home/Index.cshtml", new IndexModel() { ShopId = shopId});
    }

    [Route("/Shop/Index/{shopId:int}")]
    [Route("/Shop/{shopId:int}")]
    [ActionName("Index")]
    public IActionResult IndexRoute(int shopId)
    {
        if (shopId < 0)
            return BadRequest($"Invalid shopId : {shopId}");

        return View("~/Views/Home/Index.cshtml", new IndexModel() { ShopId = shopId });
    }    

    [Route("/Shop/ShopList")]
    [HttpPost]
    public async Task<IActionResult> ShopList(int? shopId = null)
    {
        if (shopId < 0)
            return BadRequest($"Invalid shopId : {shopId}");

        var shops = await _shopFacade.GetShops();

        var shopList = ModelHelper.GetShopItemModels(shops, shopId);

        if (shopId == null && shopList.Any())
           shopList.First().IsSelected = true;

        return PartialView("~/Views/Shared/ShopList.cshtml", shopList);
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

    public IActionResult New()
    {
        return View("~/Views/Home/Index.cshtml", new IndexModel { ShopId = 0 });
    }

    [HttpPost]
    [Route("/Shop/{shopId:int}")]
    public async Task<IActionResult> Shop(int shopId)
    {
        if (shopId < 0)
            return BadRequest($"Invalid shopId : {shopId}");

        var shopModel = await _shopFacade.GetShop(shopId);
        return PartialView("~/Views/Home/ShopEdit.cshtml", shopModel);
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromForm][ModelBinder(BinderType = typeof(ShopModelFormBinder), Name = "Shop")] IShop shop)
    {
        if (shop == null)
            return BadRequest("shop is null");

        if (shop.Id < 0)
            return BadRequest($"Invalid shopId : {shop.Id}");

        var savedShop = await _shopFacade.SaveShop(shop);

        return Ok(savedShop);
    }

    [HttpPost]
    public async Task<IActionResult> IsChanged([FromForm][ModelBinder(BinderType = typeof(ShopModelFormBinder), Name = "Shop")] IShop shop)
    {
        if (shop == null)
            return BadRequest("shop is null");

        if (shop.Id < 0)
            return BadRequest($"Invalid shopId : {shop.Id}");

        var existingShop = await shopDataService.GetShop(shop.Id);
        if (existingShop == null) return Ok(false);

        var changed = !(shop.Name == existingShop.Name
                        && shop.Url == existingShop.Url
                        && ((string.IsNullOrEmpty(shop.Caption) && string.IsNullOrEmpty(existingShop.Caption)) || shop.Caption == existingShop.Caption));

        return Ok(changed);
    }
}
