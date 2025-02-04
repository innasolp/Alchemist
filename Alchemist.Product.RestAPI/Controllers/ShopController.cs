using Alchemist.Product.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Alchemist.Product.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Message.Interfaces;
using Alchemist.Product.DataService.Interfaces;
using Alchemist.Common;


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


    [HttpPost(Name = nameof(CreateShop))]
    public async Task<Results<BadRequest<Shop>, Created<Shop>>> CreateShop(Shop shop)
    {
        if (shop == null || string.IsNullOrEmpty(shop.Name) || string.IsNullOrEmpty(shop.Url))
            return TypedResults.BadRequest(shop);

        var newShop = (await _alchemyRepository.CreateShop(shop)).To<Shop>();

        await SendMessage(newShop, Messages.SendShopCreated);

        var location = Url.Action(nameof(CreateShop), new { id = newShop.Id }) ?? $"/{newShop.Id}";
        return TypedResults.Created(location, newShop);
    }

    [HttpPost("shopUrl", Name = nameof(AddShopUrl))]
    public async Task<Results<BadRequest<ShopUrl>, Created<ShopUrl>>> AddShopUrl(ShopUrl shopUrl)
    {
        if (shopUrl == null || shopUrl.ShopId == 0 || string.IsNullOrEmpty(shopUrl.ProductUrl) || string.IsNullOrEmpty(shopUrl.CategoryUrl))
            return TypedResults.BadRequest(shopUrl);

        var newShopUrl = (await _alchemyRepository.AddShopUrl(shopUrl)).To<ShopUrl>();

        await SendMessage(newShopUrl, Messages.SendShopUrlSet);

        var location = Url.Action(nameof(AddShopUrl), new { id = newShopUrl.ShopId }) ?? $"/{newShopUrl.ShopId}";
        return TypedResults.Created(location, newShopUrl);
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


    [HttpGet("shopUrl/{shopId:int}", Name = nameof(GetShopUrl))]
    public async Task<Results<NotFound, Ok<ShopUrl>>> GetShopUrl(int shopId)
    {
        var shopUrl = await _alchemyRepository.GetShopUrl(shopId);
        return shopUrl != null ? TypedResults.Ok(shopUrl.To<ShopUrl>()) : TypedResults.NotFound();
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
    
    [HttpGet("shopSettings/{shopId:int}/{shopSettingType:short}", Name = nameof(GetShopSettings))]
    public async Task<Results<NotFound, Ok<ShopSettings>>> GetShopSettings(int shopId, ShopSettingType shopSettingType)
    {
        var shopSettings = await _alchemyRepository.GetShopSettings(shopId, shopSettingType);
        return shopSettings != null ? TypedResults.Ok(shopSettings.To<ShopSettings>()) : TypedResults.NotFound();
    }

    [HttpPost("shopSettings", Name = nameof(AddShopSettings))]
    public async Task<Results<BadRequest<ShopSettings>, Created<ShopSettings>>> AddShopSettings(ShopSettings shopSettings)
    {
        if (shopSettings == null || shopSettings.ShopId == 0 || string.IsNullOrEmpty(shopSettings.JsonValue))
            return TypedResults.BadRequest(shopSettings);

        var newShopSettings = (await _alchemyRepository.AddShopSettings(shopSettings)).To<ShopSettings>();

        await SendMessage(newShopSettings, Messages.SendCategoryAdded);

        var location = Url.Action(nameof(AddShopSettings), new { id = newShopSettings.Id }) ?? $"/{newShopSettings.Id}";
        return TypedResults.Created(location, newShopSettings);
    }
}
