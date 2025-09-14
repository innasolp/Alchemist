using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.Product.Entities;
using Message.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Alchemist.Product.Interfaces;
using Alchemist.Messages.Common;

namespace Alchemist.Product.RestAPI.Controllers;

[Route("api/ShopCategory")]
[ApiController]
public class ShopCategoryController(ILogger<ShopCategoryController> logger, IAlchemyRepository alchemyRepository, IMessageSender messageSender) : ControllerBase
{
    private readonly ILogger<ShopCategoryController> _logger = logger;
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

    [HttpPut(Name = nameof(AddShopCategory))]
    public async Task<Results<BadRequest, BadRequest<ShopCategory>, Created<ShopCategory>>> AddShopCategory(ShopCategory shopCategory)
    {
        if (shopCategory == null)
            return TypedResults.BadRequest();

        if (shopCategory.ShopId <= 0 || shopCategory.ItemId <= 0 || shopCategory.Category == null)
            return TypedResults.BadRequest(shopCategory);

        var newShopCategory = (await _alchemyRepository.AddShopCategory(shopCategory)).To<ShopCategory>();

        await SendMessage(newShopCategory, Messages.Common.Messages.SendCategoryAdded);

        var location = Url.Action(nameof(AddShopCategory), new { id = newShopCategory.Id }) ?? $"/{newShopCategory.Id}";
        return TypedResults.Created(location, newShopCategory);
    }

    [HttpGet("shopCategories/byShopIdAndItemId/{shopId:int}/{itemId:int}", Name = nameof(GetShopCategoryByShopIdAndItemId))]
    public async Task<Results<BadRequest<int>, NotFound<Tuple<int, int>>, Ok<ShopCategory>>> GetShopCategoryByShopIdAndItemId(int shopId, int itemId)
    {
        if (shopId <= 0)
            return TypedResults.BadRequest(shopId);

        if (itemId <= 0)
            return TypedResults.BadRequest(itemId);

        var shopCategory = await _alchemyRepository.GetShopCategory(shopId, itemId);

        return shopCategory != null ?
            TypedResults.Ok(shopCategory.To<ShopCategory>()) :
            TypedResults.NotFound(new Tuple<int, int>(shopId, itemId));
    }

    [HttpGet("shopCategories/{shopId:int}", Name = nameof(GetShopCategories))]
    public async Task<Results<BadRequest<int>, NotFound<int>, Ok<List<ShopCategory>>>> GetShopCategories(int shopId)
    {
        if (shopId <= 0)
            return TypedResults.BadRequest(shopId);

        var shopCategories = await _alchemyRepository.GetShopCategories(shopId);

        return shopCategories != null && shopCategories.Count != 0 ?
            TypedResults.Ok(shopCategories.Select(sc => sc.To<ShopCategory>()).ToList()) :
            TypedResults.NotFound(shopId);
    }

    [HttpGet("shopCategories/getAllChildren/{parentId:int}", Name = nameof(GetAllCategoryChildren))]
    public async Task<Results<BadRequest<int>, NotFound<int>, Ok<List<ShopCategory>>>> GetAllCategoryChildren(int parentId)
    {
        if (parentId <= 0)
            return TypedResults.BadRequest(parentId);

        var shopCategories = await _alchemyRepository.GetAllCategoryChildren(parentId);

        return shopCategories != null && shopCategories.Count != 0 ?
            TypedResults.Ok(shopCategories.Select(sc => sc.To<ShopCategory>()).ToList()) :
            TypedResults.NotFound(parentId);
    }
}
