using Alchemist.DataService.Interfaces;
using Alchemist.Product.ShopWebApp.Models;
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
    public async Task<IActionResult> ShopList(ShopApiData data, CancellationToken cancellationToken = default)
    {
        var shops = await _shopFacade.GetShops(cancellationToken);
        var selectedShop = ModelHelper.GetSelectedShop(shops, data.ShopId);
        var shopListModel = ModelHelper.GetShopItemModels(shops, selectedShop?.Id, data.HRefFormat);

        return PartialView("~/Views/Shared/ShopList.cshtml", shopListModel);
    }

    [HttpPost]
    [Route("/ShopApi/Shop/{shopId:int?}")]
    public IActionResult ShopTab(int? shopId)
    {
        if (shopId < 0)
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