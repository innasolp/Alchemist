using Alchemist.DataService.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Import.WebApp.Models;
using Alchemist.Product.Model.ShopSettings;
using Microsoft.AspNetCore.Mvc;

namespace Alchemist.Product.Import.WebApp.Controllers;

public class ShopController(IImportFacade importFacade, IShopDataService shopDataService) : Controller
{
    private readonly IImportFacade _importFacade = importFacade;

    private readonly IShopDataService _shopDataService = shopDataService;
   
    [ProducesResponseType<PartialViewResult>(StatusCodes.Status200OK)]
    public IActionResult New()
    {
        return PartialView("~/Views/Home/Shop.cshtml", new ShopModel(0));
    }

    
    [ProducesResponseType<PartialViewResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestResult>(StatusCodes.Status400BadRequest)]
    public IActionResult Edit(Guid shopGuid)
    {
        if (shopGuid == Guid.Empty)
            return BadRequest(shopGuid);

        if (!_importFacade.TryGetShopImport(shopGuid, out var shopImport))
            return NotFound(shopGuid);

        return PartialView("~/Views/Home/Shop.cshtml", shopImport.Shop);
    }

    [Route("Shop/Save")]
    [HttpPost]
    [ProducesResponseType<OkObjectResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ObjectResult>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SaveAsync([ModelBinder(typeof(ModelImplementationJsonBinder))] ShopModel shop)
    {
        if (shop == null)
            return BadRequest(shop);

        ShopImportModel? shopImport = null;
        if (shop.Id != 0 && !_importFacade.TryGetShopImport(shop.Guid, out shopImport))
            return NotFound(shop);

        try
        {
            var newShop = new Shop
            { 
                Id =  shop.Id,
                Name = shop.Name,
                Caption = shop.Caption,
                Url = shop.Url
            };

            var shopModel = (shop.Id == 0)
              ? await _shopDataService.CreateShop(newShop) :
                await _shopDataService.UpdateShop(newShop);

            if (shopImport == null)
                shopImport = _importFacade.AddNewShop(shopModel);
            else
            {
                shopImport.Shop.SetFrom(shopModel);                
            }

            return Ok(shopImport.Shop);
        }
        catch (Exception e)
        {
            return new ObjectResult(e) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }
}
