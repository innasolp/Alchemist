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
    public async Task<string> ShopList(int? selectedShopId, string hrefFromat)
    {
        var shops = await _shopFacade.GetShops();
        var shopListModel = ModelHelper.GetShopItemModels(shops, selectedShopId, hrefFromat);
        return await ViewHelper.GetViewHtml(HttpContext.RequestServices,
            ControllerContext,
            "~/Views/Shared/ShopList.cshtml", 
            shopListModel);
    }

    [HttpGet("ShopTab", Name = "ShopTab")]
    public async Task<string> ShopTab(int? selectedShopId, string hrefFromat)
    {
        var shops = await _shopFacade.GetShops();
        var shopTabModel = ModelHelper.GetShopTabModel(shops, selectedShopId, hrefFromat);
        return await ViewHelper.GetViewHtml(HttpContext.RequestServices,
            ControllerContext,
            "~/Views/Shared/ShopTab.cshtml", 
            shopTabModel);
    }
}
