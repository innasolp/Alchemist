using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;  
using Db.Infrastructure;
using Db.Infrastructure.Commands;
using Db.Infrastructure.Requests;
using Shop.Data.Infrastructure;


namespace Shop.API.Controllers;

[ApiController]
[Route("api/Shop")]
public class ShopController(ILogger<ShopController> logger) 
    : ControllerBase
{
    private readonly ILogger<ShopController> _logger = logger;

    [HttpGet("byName", Name = nameof(GetShopByName))]
    public async Task<Results<BadRequest, NotFound<string>, Ok<Alchemist.Product.Data.Shop>>> 
        GetShopByName(string name,
            [FromServices] IRequestHandler<FindByNameRequest<Alchemist.Product.Data.Shop>, Alchemist.Product.Data.Shop> handler,
            CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(name))
            return TypedResults.BadRequest();

        var shop = await handler.Handle(new FindByNameRequest<Alchemist.Product.Data.Shop>(name), cancellationToken);

        return shop != null ? TypedResults.Ok(shop) : TypedResults.NotFound(name);
    }

    [HttpGet("byUrl", Name = nameof(GetShopByUrl))]
    public async Task<Results<BadRequest, NotFound<string>, Ok<Alchemist.Product.Data.Shop>>> 
        GetShopByUrl(string url,
            [FromServices] IRequestHandler<GetShopByUrlRequest, Alchemist.Product.Data.Shop> handler,
            CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(url))
            return TypedResults.BadRequest();

        var shop = await handler.Handle(new GetShopByUrlRequest(url), cancellationToken);

        return shop != null ? TypedResults.Ok(shop) : TypedResults.NotFound(url);
    }

    [HttpGet("{id:int}", Name = nameof(GetShop))]
    public async Task<Results<BadRequest<int>, NotFound<int>, Ok<Alchemist.Product.Data.Shop>>> GetShop(
        int id,
        [FromServices] IRequestHandler<GetByIdRequest<Alchemist.Product.Data.Shop>, Alchemist.Product.Data.Shop> handler,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return TypedResults.BadRequest(id);

        var shop = await handler.Handle(new GetByIdRequest<Alchemist.Product.Data.Shop>(id), cancellationToken);

        return shop != null
            ? TypedResults.Ok(shop) 
            : TypedResults.NotFound(id);
    }
    
    [HttpGet("Shops", Name = nameof(GetShops))]
    public async Task<Results<NotFound, Ok<List<Alchemist.Product.Data.Shop>>>> GetShops(
        [FromServices] IRequestHandler<GetAllRequest<Alchemist.Product.Data.Shop>, IEnumerable<Alchemist.Product.Data.Shop>> handler,
        CancellationToken cancellationToken = default)
    {
        var shops = (await handler.Handle(new GetAllRequest<Alchemist.Product.Data.Shop>(), cancellationToken)).ToList();
        return shops != null && shops.Count > 0 ? 
            TypedResults.Ok(shops) :
            TypedResults.NotFound();
    }

    [HttpPut(Name = nameof(CreateShop))]
    public async Task<Results<BadRequest, BadRequest<Alchemist.Product.Data.Shop>, Created<Alchemist.Product.Data.Shop>>> 
        CreateShop(Alchemist.Product.Data.Shop shop,
            [FromServices] ICommandHandler<CreateCommand<Alchemist.Product.Data.Shop>> handler,
            CancellationToken cancellationToken = default)
    {
        if (shop == null)
            return TypedResults.BadRequest();

        if (string.IsNullOrEmpty(shop.Name) || string.IsNullOrEmpty(shop.Url))
            return TypedResults.BadRequest(shop);

        await handler.Handle(new CreateCommand<Alchemist.Product.Data.Shop>(shop), cancellationToken);

        var location = Url.Action(nameof(CreateShop), new { id = shop.Id }) ?? $"/{shop.Id}";
        return TypedResults.Created(location, shop);
    }

    [HttpPost("Update", Name = nameof(UpdateShop))]
    public async Task<Results<BadRequest, BadRequest<Alchemist.Product.Data.Shop>, Accepted<Alchemist.Product.Data.Shop>>>
        UpdateShop(Alchemist.Product.Data.Shop shop,
            [FromServices] ICommandHandler<UpdateCommand<Alchemist.Product.Data.Shop>> handler,
            CancellationToken cancellationToken = default)
    {
        if (shop == null)
            return TypedResults.BadRequest();

        if (shop.Id <= 0 ||  string.IsNullOrEmpty(shop.Name) || string.IsNullOrEmpty(shop.Url))
            return TypedResults.BadRequest(shop);

        await handler.Handle(new UpdateCommand<Alchemist.Product.Data.Shop>(shop), cancellationToken);

        var location = Url.Action(nameof(UpdateShop), new { id = shop.Id }) ?? $"/{shop.Id}";
        return TypedResults.Accepted(location, shop);
    }
}