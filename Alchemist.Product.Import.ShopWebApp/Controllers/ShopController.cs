using Alchemist.DataService.Interfaces;
using Alchemist.Exceptions;
using Alchemist.Product.Import.ShopWebApp.Models;
using Alchemist.Product.ShopWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Alchemist.Product.ShopWebApp.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class ShopController(ILogger<ShopController> logger, IShopDataService shopDataService) : Controller
{
    private readonly ILogger<ShopController> _logger = logger;

    private readonly ShopFacade _shopFacade = new(shopDataService);

    private bool IsShopsUploaded()
    {
        return HttpContext.Session.GetInt32("shops_uploaded") == 1;
    }

    private void SetShopsUploaded()
    {
        HttpContext.Session.SetInt32("shops_uploaded", 1);
    }    

    private async Task<IndexModel> GetIndexModel(int? shopId = null)
    {
        var shopsUploaded = IsShopsUploaded();

        var indexViewModel = new IndexModel() { ShopsUploaded = shopsUploaded };

        if (shopsUploaded)
        {
            var shops = await _shopFacade.GetShops();
            indexViewModel.ShopTab = ModelHelper.GetShopTabModel(shops, shopId);
        }
        else
        {
            indexViewModel.ShopTab = new ShopTabModel { ShopItems = new List<ShopItemModel>(), CurrentShopModel = new ShopModel { Id = 0 } };
        }

        return indexViewModel;
    }

    public async Task<IActionResult> Index()
    {
        var indexViewModel = await GetIndexModel();

        return View("~/Views/Home/Index.cshtml", indexViewModel);
    }

    [Route("Shop/Index")]
    [Route("Shop/")]
    [ActionName("Index")]
    public async Task<IActionResult> IndexFromQuery([FromQuery] int shopId)
    {
        try
        {
            var indexViewModel = await GetIndexModel(shopId);

            return View("~/Views/Home/Index.cshtml", indexViewModel);
        }
        catch (NotFoundException)
        {
            return NotFound(shopId);
        }
    }

    [Route("Shop/Index/{shopId:int}")]
    [Route("Shop/{shopId:int}")]
    [ActionName("Index")]
    public async Task<IActionResult> IndexRoute(int shopId)
    {
        try
        {
            var indexViewModel = await GetIndexModel(shopId);
            return View("~/Views/Home/Index.cshtml", indexViewModel);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }


    [Route("Shop/ShopTab")]
    public async Task<IActionResult> ShopTab(int? selectedShopId = null)
    {
        var shops = await _shopFacade.GetShops();
        try
        {
            var tabModel = ModelHelper.GetShopTabModel(shops, selectedShopId);
            SetShopsUploaded();
            return PartialView("~/Views/Shared/ShopTab.cshtml", tabModel);
        }
        catch(NotFoundException ex)
        {
            return NotFound(ex.Message);            
        }        
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


    public async Task<IActionResult> New()
    {
        var shops = await _shopFacade.GetShops();

        var shopTabModel = ModelHelper.GetShopTabModel(shops, 0);
        shopTabModel.CurrentShopModel = new ShopModel { Id = 0 };

        return View("~/Views/Home/Index.cshtml", new IndexModel { ShopsUploaded = IsShopsUploaded(), ShopTab = shopTabModel });
    }


    [HttpPost]
    public async Task<IActionResult> Save([FromForm] ShopModel shop)
    {
        var savedShop = await _shopFacade.SaveShop(shop);

        return RedirectToAction("Index", "Shop", new { shopId = savedShop.Id });
    }

    [HttpPost]
    public async Task<IActionResult> IsChanged([FromForm] ShopModel shop)
    {
        var existingShop = await shopDataService.GetShop(shop.Id);
        if (existingShop == null) return Ok(false);

        var changed = !(shop.Name == existingShop.Name
                        && shop.Url == existingShop.Url
                        && ((string.IsNullOrEmpty(shop.Caption) && string.IsNullOrEmpty(existingShop.Caption)) || shop.Caption == existingShop.Caption));

        return Ok(changed);
    }
}
