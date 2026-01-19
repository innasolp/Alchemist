using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Alchemist.Product.Data;
using Mediator.Infrastructure.Command;
using Shop.Infrastructure;

namespace Shop.API.Controllers;

[Route("api/ShopCategory")]
[ApiController]
public class ShopCategoryController(ILogger<ShopCategoryController> logger, IMediator mediator) : ControllerBase
{
    private readonly ILogger<ShopCategoryController> _logger = logger;
    private readonly IMediator _mediator = mediator;    

    [HttpPut(Name = nameof(AddShopCategory))]
    public async Task<Results<BadRequest, BadRequest<ShopCategory>, Created<ShopCategory>>> AddShopCategory(ShopCategory shopCategory, CancellationToken cancellationToken = default)
    {
        if (shopCategory == null)
            return TypedResults.BadRequest();

        if (shopCategory.ShopId <= 0 || shopCategory.ItemId <= 0 || shopCategory.Category == null)
            return TypedResults.BadRequest(shopCategory);

        var newShopCategory = await _mediator.Send(new CreateCommand<ShopCategory>(shopCategory), cancellationToken);

        var location = Url.Action(nameof(AddShopCategory), new { id = newShopCategory.Id }) ?? $"/{newShopCategory.Id}";
        return TypedResults.Created(location, newShopCategory);
    }

    [HttpGet("shopCategories/byShopIdAndItemId/{shopId:int}/{itemId:int}", Name = nameof(GetShopCategoryByShopIdAndItemId))]
    public async Task<Results<BadRequest<int>, NotFound<Tuple<int, int>>, Ok<ShopCategory>>> GetShopCategoryByShopIdAndItemId(int shopId, int itemId, CancellationToken cancellationToken = default)
    {
        if (shopId <= 0)
            return TypedResults.BadRequest(shopId);

        if (itemId <= 0)
            return TypedResults.BadRequest(itemId);

        var shopCategory = await _mediator.Send(new GetShopCategoryByShopIdAndItemIdRequest(shopId, itemId), cancellationToken);

        return shopCategory != null ?
            TypedResults.Ok(shopCategory) :
            TypedResults.NotFound(new Tuple<int, int>(shopId, itemId));
    }

    [HttpGet("shopCategories/{shopId:int}", Name = nameof(GetShopCategories))]
    public async Task<Results<BadRequest<int>, NotFound<int>, Ok<List<ShopCategory>>>> GetShopCategories(int shopId, CancellationToken cancellationToken = default)
    {
        if (shopId <= 0)
            return TypedResults.BadRequest(shopId);

        var shopCategories = await _mediator.Send(new GetShopCategoriesRequest(shopId), cancellationToken);

        return shopCategories != null && shopCategories.Count != 0 ?
            TypedResults.Ok(shopCategories) :
            TypedResults.NotFound(shopId);
    }

    [HttpGet("shopCategories/getAllChildren/{parentId:int}", Name = nameof(GetAllCategoryChildren))]
    public async Task<Results<BadRequest<int>, NotFound<int>, Ok<List<ShopCategory>>>> GetAllCategoryChildren(int parentId, CancellationToken cancellationToken = default)
    {
        if (parentId <= 0)
            return TypedResults.BadRequest(parentId);

        var shopCategories = await _mediator.Send(new GetAllCategoryChildrenRequest(parentId), cancellationToken);

        return shopCategories != null && shopCategories.Count != 0 ?
            TypedResults.Ok(shopCategories) :
            TypedResults.NotFound(parentId);
    }
}
