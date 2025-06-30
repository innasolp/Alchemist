using Alchemist.DataService.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Import.WebApp.Models;
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

    [HttpPost]
    [ProducesResponseType<OkObjectResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ObjectResult>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Save(ShopModel shopModel)
    {
        if (shopModel == null)
            return BadRequest(shopModel);

        ShopImportModel? shopImport = null;
        if (shopModel.Id != 0 && !_importFacade.TryGetShopImport(shopModel.Guid, out shopImport))
            return NotFound(shopModel);

        try
        {
            var shop = new Shop
            { 
                Id =  shopModel.Id,
                Name = shopModel.Name,
                Caption = shopModel.Caption,
                Url = shopModel.Url
            };

            var savedShop = (shopModel.Id == 0)
              ? await _shopDataService.CreateShop(shop) :
                await _shopDataService.UpdateShop(shop);

            if (shopImport == null)
                shopImport = _importFacade.AddNewShop(savedShop);
            else
            {
                shopImport.Shop.SetFrom(savedShop);                
            }

            return Ok(shopImport.Shop);
        }
        catch (Exception e)
        {
            return new ObjectResult(e) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }
}
