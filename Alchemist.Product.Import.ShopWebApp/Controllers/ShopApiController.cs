using Alchemist.DataService.Interfaces;
using Alchemist.Product.ShopWebApp.Models;
using Alchemist.Product.WebApp.Common;
using Microsoft.AspNetCore.Mvc;

namespace Alchemist.Product.ShopWebApp.Controllers;

public class ShopApiData
{
    public int? ShopId { get; set; } = null;

    public string HRefFormat { get; set; }
}

public class ShopContentData
{
    public int? ShopId { get; set; } = null;

    public string Content { get; set; }
}

[ApiController]
[Route("[controller]")]
public class ShopApiController(IShopDataService shopDataService) : Controller
{
    private readonly ShopFacade _shopFacade = new(shopDataService);
    
    [HttpPost("ShopList", Name = "ShopList")]
    public async Task<IActionResult> ShopList(ShopApiData data)
    {
        var shops = await _shopFacade.GetShops();
        var selectedShop = ModelHelper.GetSelectedShop(shops, data.ShopId);
        var shopListModel = ModelHelper.GetShopItemModels(shops, selectedShop?.Id, data.HRefFormat);
        var content = await ViewHelper.GetViewHtml(HttpContext.RequestServices,
            ControllerContext,
            "~/Views/Shared/ShopList.cshtml",
            shopListModel);
        return Ok(new ShopContentData { Content = content, ShopId = selectedShop?.Id });
    }

    [HttpPost]
    [Route("/ShopApi/Shop/{shopId:int}")]
    public async Task<IActionResult> ShopTab(int shopId)
    {
        if (shopId <= 0)
            return BadRequest($"Invalid shopId : {shopId}");
        
        return PartialView("~/Views/Shared/ShopTab.cshtml", new IndexModel { ShopId = shopId});
    }

    [HttpPost]
    [Route("/ShopApi/Shop/New")]
    public IActionResult New()
    {
        return PartialView("~/Views/Shared/ShopTab.cshtml", new IndexModel { ShopId = 0 });
    }
}