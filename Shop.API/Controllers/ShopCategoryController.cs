using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Alchemist.Product.Data;
using Db.Infrastructure.Commands;
using Db.Infrastructure;
using Shop.Data.Infrastructure;

namespace Shop.API.Controllers;

[Route("api/ShopCategory")]
[ApiController]
public class ShopCategoryController(ILogger<ShopCategoryController> logger) : ControllerBase
{
    private readonly ILogger<ShopCategoryController> _logger = logger;

    [HttpPut(Name = nameof(AddShopCategory))]
    public async Task<Results<BadRequest, BadRequest<ShopCategory>, Created<ShopCategory>>> AddShopCategory(
            ShopCategory shopCategory,
            [FromServices] ICommandHandler<CreateCommand<ShopCategory>> handler,
            CancellationToken cancellationToken = default)
    {
        if (shopCategory == null)
            return TypedResults.BadRequest();

        if (shopCategory.ShopId <= 0 || shopCategory.ItemId <= 0 || shopCategory.Category == null)
            return TypedResults.BadRequest(shopCategory);

        await handler.Handle(new CreateCommand<ShopCategory>(shopCategory), cancellationToken);

        var location = Url.Action(nameof(AddShopCategory), new { id = shopCategory.Id }) ?? $"/{shopCategory.Id}";
        return TypedResults.Created(location, shopCategory);
    }

    [HttpGet("shopCategories/byShopIdAndItemId/{shopId:int}/{itemId:int}", Name = nameof(GetShopCategoryByShopIdAndItemId))]
    public async Task<Results<BadRequest<int>, NotFound<Tuple<int, int>>, Ok<ShopCategory>>> GetShopCategoryByShopIdAndItemId(
            int shopId,
            int itemId,
            [FromServices] IRequestHandler<GetShopCategoryByShopIdAndItemIdRequest, ShopCategory> handler,
            CancellationToken cancellationToken = default)
    {
        if (shopId <= 0)
            return TypedResults.BadRequest(shopId);

        if (itemId <= 0)
            return TypedResults.BadRequest(itemId);

        var shopCategory = await handler.Handle(new GetShopCategoryByShopIdAndItemIdRequest(shopId, itemId), cancellationToken);

        return shopCategory != null ?
            TypedResults.Ok(shopCategory) :
            TypedResults.NotFound(new Tuple<int, int>(shopId, itemId));
    }

    [HttpGet("shopCategories/{shopId:int}", Name = nameof(GetShopCategories))]
    public async Task<Results<BadRequest<int>, NotFound<int>, Ok<List<ShopCategory>>>> GetShopCategories(
            int shopId,
            [FromServices] IRequestHandler<GetShopCategoriesRequest, List<ShopCategory>> handler,
            CancellationToken cancellationToken = default)
    {
        if (shopId <= 0)
            return TypedResults.BadRequest(shopId);

        var shopCategories = await handler.Handle(new GetShopCategoriesRequest(shopId), cancellationToken);

        return shopCategories != null && shopCategories.Count != 0 ?
            TypedResults.Ok(shopCategories) :
            TypedResults.NotFound(shopId);
    }

    [HttpGet("shopCategories/getAllChildren/{parentId:int}", Name = nameof(GetAllCategoryChildren))]
    public async Task<Results<BadRequest<int>, NotFound<int>, Ok<List<ShopCategory>>>> GetAllCategoryChildren(
            int parentId,
            [FromServices] IRequestHandler<GetAllCategoryChildrenRequest, List<ShopCategory>> handler,
            CancellationToken cancellationToken = default)
    {
        if (parentId <= 0)
            return TypedResults.BadRequest(parentId);

        var shopCategories = await handler.Handle(new GetAllCategoryChildrenRequest(parentId), cancellationToken);

        return shopCategories != null && shopCategories.Count != 0 ?
            TypedResults.Ok(shopCategories) :
            TypedResults.NotFound(parentId);
    }

    [HttpGet("checkancestoritem/{id:int}/{ancestorItemId:int}", Name = nameof(CheckCategoryForAncestorItem))]
    public async Task<Results<BadRequest<int>, NotFound<int>, Ok<bool>>> CheckCategoryForAncestorItem(
            int id, 
            int ancestorItemId,
            [FromServices] IRequestHandler<CheckCategoryForAncestorItemRequest, bool?> handler,
            CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return TypedResults.BadRequest(id);

        var hasAncestor = await handler.Handle(new CheckCategoryForAncestorItemRequest(id, ancestorItemId), cancellationToken);

        return hasAncestor.HasValue ?
            TypedResults.Ok(hasAncestor.Value) :
            TypedResults.NotFound(id);
    }
}