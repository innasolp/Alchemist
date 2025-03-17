using Alchemist.DataService.Interfaces;
using Alchemist.Product.Import.Model;
using Microsoft.AspNetCore.Mvc;

namespace Alchemist.Product.Import.WebApp.Controllers;

public class ShopController(IImportFacade importFacade, IShopDataService shopDataService) : Controller
{
    private readonly IImportFacade _importFacade = importFacade;

    private readonly IShopDataService _shopDataService = shopDataService;
   
    [ProducesResponseType<PartialViewResult>(StatusCodes.Status200OK)]
    public IActionResult New()
    {
        return PartialView("~/Views/Home/Shop.cshtml", new ShopModel());
    }

    
    [ProducesResponseType<PartialViewResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestResult>(StatusCodes.Status400BadRequest)]
    public IActionResult Edit(Guid? shopGuid)
    {
        if (shopGuid == null || shopGuid == Guid.Empty)
            return BadRequest(shopGuid);

        if (!_importFacade.TryGetShopImport((Guid)shopGuid, out var shopImport))
            return NotFound(shopGuid);

        return PartialView("~/Views/Home/Shop.cshtml", shopImport.Shop);
    }

    [HttpPost]
    [ProducesResponseType<OkObjectResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<NotFoundObjectResult>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<BadRequestObjectResult>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ObjectResult>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Save(ShopModel shop)
    {
        if (shop == null)
            return BadRequest(shop);

        ShopImportModel shopImport = null;
        if (shop.Id != 0 && !_importFacade.TryGetShopImport(shop.Guid, out shopImport))
            return NotFound(shop);

        try
        {
            var savedShop = (shop.Id == 0)
              ? await _shopDataService.CreateShop(shop) :
                await _shopDataService.UpdateShop(shop);

            if (shopImport == null)
                shopImport = _importFacade.AddNewShop(savedShop);
            else
            {
                shopImport.Shop.Update(savedShop);
                shopImport.Shop.Id = savedShop.Id;
            }

            return Ok(shopImport.Shop.Guid);
        }
        catch (Exception e)
        {
            return new ObjectResult(e) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }
}
