using Alchemist.DataService.Interfaces;
using Alchemist.Product.ShopWebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alchemist.Product.ShopWebApp.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class ShopActionController(ILogger<ShopActionController> logger, IShopDataService shopDataService) : Controller
{
    private readonly ILogger<ShopActionController> _logger = logger;

    private readonly ShopFacade _shopFacade = new(shopDataService);  

    [HttpPost]
    public async Task<IActionResult> ShopList(int? shopId = null, CancellationToken cancellationToken = default)
    {
        if (shopId < 0)
            return BadRequest($"Invalid shopId : {shopId}");

        var shops = await _shopFacade.GetShops(cancellationToken);

        var shopList = ModelHelper.GetShopItemModels(shops, shopId);

        if (shopId == null && shopList.Any())
           shopList.First().IsSelected = true;

        return PartialView("~/Views/Shared/ShopList.cshtml", shopList);
    }

    [HttpPost]
    [Route($"/{ControllerPrefix.Action}/{{shopId:int}}")]
    public async Task<IActionResult> Shop(int shopId, CancellationToken cancellationToken = default)
    {
        if (shopId < 0)
            return BadRequest($"Invalid shopId : {shopId}");

        var shopModel = await _shopFacade.GetShop(shopId, cancellationToken);
        return PartialView("~/Views/Home/ShopEdit.cshtml", shopModel);
    }

    [HttpPost]
    public async Task<IActionResult> Save(ShopModel shop, CancellationToken cancellationToken = default)
    {
        if (shop == null)
            return BadRequest("shop is null");

        if (shop.Id < 0)
            return BadRequest($"Invalid shopId : {shop.Id}");

        var savedShop = await _shopFacade.SaveShop(shop, cancellationToken);

        return Ok(savedShop);
    }

    [HttpPost]
    public async Task<IActionResult> IsChanged(ShopModel shop, CancellationToken cancellationToken = default)
    {
        if (shop == null)
            return BadRequest("shop is null");

        if (shop.Id < 0)
            return BadRequest($"Invalid shopId : {shop.Id}");

        var existingShop = await shopDataService.GetShop(shop.Id, cancellationToken);
        if (existingShop == null) return Ok(false);

        var changed = !(shop.Name == existingShop.Name
                        && shop.Url == existingShop.Url
                        && ((string.IsNullOrEmpty(shop.Caption) && string.IsNullOrEmpty(existingShop.Caption)) || shop.Caption == existingShop.Caption));

        return Ok(changed);
    }
}