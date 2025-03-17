using Alchemist.Product.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Alchemist.Product.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Message.Interfaces;
using Alchemist.Common;
using Alchemist.DataService.Interfaces;


namespace Alchemist.Product.RestAPI.Controllers;

[ApiController]
[Route("api/Shop")]
public class ShopController(ILogger<ShopController> logger, IAlchemyRepository alchemyRepository, IMessageSender messageSender) 
    : ControllerBase
{
    private readonly ILogger<ShopController> _logger = logger;
    private readonly IAlchemyRepository _alchemyRepository = alchemyRepository;
    private readonly IMessageSender _messageSender = messageSender;

    private async Task SendMessage<T>(T entity, string methodName)
    {
        try
        {
            await _messageSender.Start();
            await _messageSender.Send(entity, methodName);
            _logger.LogInformation($"Call {methodName} {entity} ");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }

    [HttpGet("byName", Name = nameof(GetShopByName))]
    public async Task<Results<BadRequest, NotFound, Ok<Shop>>> GetShopByName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return TypedResults.BadRequest();
        var shop = await _alchemyRepository.GetShopByName(name);
        return shop != null ? TypedResults.Ok(shop.To<Shop>()) : TypedResults.NotFound();
    }

    [HttpGet("byUrl", Name = nameof(GetShopByUrl))]
    public async Task<Results<BadRequest, NotFound, Ok<Shop>>> GetShopByUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return TypedResults.BadRequest();
        var shop = await _alchemyRepository.GetShopByUrl(url);
        return shop != null ? TypedResults.Ok(shop.To<Shop>()) : TypedResults.NotFound();
    }

    [HttpGet("{id:int}", Name = nameof(GetShop))]
    public async Task<Results<NotFound, Ok<Shop>>> GetShop(int id)
    {
        var shop = await _alchemyRepository.GetShop(id);
        return shop != null ? TypedResults.Ok(shop.To<Shop>()) : TypedResults.NotFound();
    }
    
    [HttpGet("Shops", Name = nameof(GetShops))]
    public async Task<Results<NotFound, Ok<List<Shop>>>> GetShops()
    {
        var shops = await _alchemyRepository.GetShops();
        return shops != null && shops.Count > 0 ? TypedResults.Ok(shops.Select(s=>s.To<Shop>()).ToList()) : TypedResults.NotFound();
    }


    [HttpPut(Name = nameof(CreateShop))]
    public async Task<Results<BadRequest<Shop>, Created<Shop>>> CreateShop(Shop shop)
    {
        if (shop == null || string.IsNullOrEmpty(shop.Name) || string.IsNullOrEmpty(shop.Url))
            return TypedResults.BadRequest(shop);

        var newShop = (await _alchemyRepository.CreateShop(shop)).To<Shop>();

        await SendMessage(newShop, Messages.SendShopCreated);

        var location = Url.Action(nameof(CreateShop), new { id = newShop.Id }) ?? $"/{newShop.Id}";
        return TypedResults.Created(location, newShop);
    }

    [HttpPost("Update", Name = nameof(UpdateShop))]
    public async Task<Results<BadRequest<Shop>, Accepted<Shop>, StatusCodeHttpResult>> UpdateShop(Shop shop)
    {
        if (shop == null || string.IsNullOrEmpty(shop.Name) || string.IsNullOrEmpty(shop.Url))
            return TypedResults.BadRequest(shop);

        var updatedShop = (await _alchemyRepository.UpdateShop(shop)).To<Shop>();        

        var location = Url.Action(nameof(UpdateShop), new { id = updatedShop.Id }) ?? $"/{updatedShop.Id}";
        return TypedResults.Accepted(location, updatedShop);
    }    

    [HttpPost("shopCategory", Name = nameof(AddShopCategory))]
    public async Task<Results<BadRequest<ShopCategory>, Created<ShopCategory>>> AddShopCategory(ShopCategory shopCategory)
    {
        if (shopCategory == null || shopCategory.ShopId == 0 || shopCategory.ItemId == 0 || string.IsNullOrEmpty(shopCategory.Category))
            return TypedResults.BadRequest(shopCategory);

        var newShopCategory = (await _alchemyRepository.AddShopCategory(shopCategory)).To<ShopCategory>();

        await SendMessage(newShopCategory, Messages.SendCategoryAdded);

        var location = Url.Action(nameof(AddShopCategory), new { id = newShopCategory.Id }) ?? $"/{newShopCategory.Id}";
        return TypedResults.Created(location, newShopCategory);
    }    

    [HttpGet("shopCategories/byShopIdAndItemId/{shopId:int}/{itemId:int}", Name = nameof(GetShopCategoryByShopIdAndItemId))]
    public async Task<Results<NotFound, Ok<ShopCategory>>> GetShopCategoryByShopIdAndItemId(int shopId, int itemId)
    {
        var shopCategory = await _alchemyRepository.GetShopCategory(shopId, itemId);
        return shopCategory != null ? TypedResults.Ok(shopCategory.To<ShopCategory>()) : TypedResults.NotFound();
    }

    [HttpGet("shopCategories/{shopId:int}", Name = nameof(GetShopCategories))]
    public async Task<Results<NotFound, Ok<List<ShopCategory>>>> GetShopCategories(int shopId)
    {
        var shopCategories = await _alchemyRepository.GetShopCategories(shopId);
        return shopCategories != null && shopCategories.Count != 0 ? TypedResults.Ok(shopCategories.Select(sc => sc.To<ShopCategory>()).ToList()) : TypedResults.NotFound();
    }   
    
}
