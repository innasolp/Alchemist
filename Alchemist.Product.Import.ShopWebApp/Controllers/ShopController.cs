using Alchemist.DataService.Interfaces;
using Alchemist.Exceptions;
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

    private bool IsShopsUploaded()
    {
        return HttpContext.Session.GetInt32("shops_uploaded") == 1;
    }

    private void SetShopsUploaded()
    {
        HttpContext.Session.SetInt32("shops_uploaded", 1);
    }    

    private async Task<IndexModel> GetIndexModel(bool shopsUploaded, int? shopId = null)
    {
        var indexViewModel = new IndexModel() { ShopsUploaded = shopsUploaded };

        if (shopsUploaded)
        {
            var shops = await _shopFacade.GetShops();
            indexViewModel.ShopTab = ModelHelper.GetShopTabModel(shops, shopId);
        }
        else
        {
            indexViewModel.ShopTab = new ShopTabModel { ShopItems = [], CurrentShopModel = new ShopModel { Id = 0 } };
        }

        return indexViewModel;
    }

    private async Task<IActionResult> IndexActionAsync(int shopId)
    {
        try
        {
            if (shopId <= 0)
                return BadRequest($"Invalid shopId : {shopId}");

            var indexViewModel = await GetIndexModel(true, shopId);

            return View("~/Views/Home/Index.cshtml", indexViewModel);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    public async Task<IActionResult> Index()
    {
        var shopsUploaded = IsShopsUploaded();

        var indexViewModel = await GetIndexModel(shopsUploaded);

        return View("~/Views/Home/Index.cshtml", indexViewModel);
    }

    [Route("Shop/Index")]
    [Route("Shop/")]
    [ActionName("Index")]
    public async Task<IActionResult> IndexFromQuery([FromQuery] int shopId)
    {
        return await IndexActionAsync(shopId);
    }

    [Route("Shop/Index/{shopId:int}")]
    [Route("Shop/{shopId:int}")]
    [ActionName("Index")]
    public async Task<IActionResult> IndexRoute(int shopId)
    {
        return await IndexActionAsync(shopId);
    }


    [Route("Shop/ShopTab")]
    [HttpPost]
    public async Task<IActionResult> ShopTab(int? shopId = null)
    {
        if (shopId <= 0)
            return BadRequest($"Invalid shopId : {shopId}");

        var shops = await _shopFacade.GetShops();
        try
        {
            var tabModel = ModelHelper.GetShopTabModel(shops, shopId);
            SetShopsUploaded();
            return PartialView("~/Views/Shared/ShopTab.cshtml", tabModel);
        }
        catch(NotFoundException ex)
        {
            return NotFound(ex.Message);            
        }        
    }

    [Route("Shop/ShopList")]
    [HttpPost]
    public async Task<IActionResult> ShopList(int? shopId = null)
    {
        if (shopId <= 0)
            return BadRequest($"Invalid shopId : {shopId}");

        var shops = await _shopFacade.GetShops();

        var shopList = ModelHelper.GetShopItemModels(shops, shopId);
        SetShopsUploaded();
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


    public async Task<IActionResult> New()
    {
        var shops = await _shopFacade.GetShops();

        var shopTabModel = ModelHelper.GetShopTabModel(shops, 0);
        shopTabModel.CurrentShopModel = new ShopModel { Id = 0 };       

        return View("~/Views/Home/Index.cshtml", new IndexModel { ShopsUploaded = IsShopsUploaded(), ShopTab = shopTabModel });
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
