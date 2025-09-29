using Alchemist.DataService.Interfaces;
using Alchemist.Product.ShopWebApp.Models;
using Alchemist.Product.WebApp.Common;
using Microsoft.AspNetCore.Mvc;

namespace Alchemist.Product.ShopWebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class ShopApiController(IShopDataService shopDataService) : Controller
{
    private readonly ShopFacade _shopFacade = new(shopDataService);

    [HttpGet("ShopList", Name = "ShopList")]
    public async Task<IActionResult> ShopList(int? selectedShopId, string hrefFormat)
    {
        var shops = await _shopFacade.GetShops();
        var selectedShop = ModelHelper.GetSelectedShop(shops, selectedShopId);
        var shopListModel = ModelHelper.GetShopItemModels(shops, selectedShop?.Id, hrefFormat);
        var content = await ViewHelper.GetViewHtml(HttpContext.RequestServices,
            ControllerContext,
            "~/Views/Shared/ShopList.cshtml",
            shopListModel);
        return Ok(content);
    }

    [HttpGet("ShopTab", Name = "ShopTab")]
    public async Task<IActionResult> ShopTab(int? selectedShopId, string hrefFormat)
    {
        var shops = await _shopFacade.GetShops();
        var shopTabModel = ModelHelper.GetShopTabModel(shops, selectedShopId, hrefFormat);
        var content = await ViewHelper.GetViewHtml(HttpContext.RequestServices,
            ControllerContext,
            "~/Views/Shared/ShopTab.cshtml",
            shopTabModel);
        return Ok(content);
    }
}
