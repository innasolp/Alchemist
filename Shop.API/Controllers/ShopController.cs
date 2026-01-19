using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using MediatR;
using Mediator.Infrastructure.Request;
using Shop.Infrastructure;
using Mediator.Infrastructure.Command;


namespace Shop.API.Controllers;

[ApiController]
[Route("api/Shop")]
public class ShopController(ILogger<ShopController> logger, IMediator mediator) 
    : ControllerBase
{
    private readonly ILogger<ShopController> _logger = logger;
    private readonly IMediator _mediator = mediator;    

    [HttpGet("byName", Name = nameof(GetShopByName))]
    public async Task<Results<BadRequest, NotFound<string>, Ok<Alchemist.Product.Data.Shop>>> 
        GetShopByName(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(name))
            return TypedResults.BadRequest();

        var shop = await _mediator.Send(new FindByNameRequest<Alchemist.Product.Data.Shop>(name, s=>s.Name), cancellationToken);

        return shop != null ? TypedResults.Ok(shop) : TypedResults.NotFound(name);
    }

    [HttpGet("byUrl", Name = nameof(GetShopByUrl))]
    public async Task<Results<BadRequest, NotFound<string>, Ok<Alchemist.Product.Data.Shop>>> 
        GetShopByUrl(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(url))
            return TypedResults.BadRequest();

        var shop = await _mediator.Send(new GetShopByUrlRequest(url), cancellationToken);

        return shop != null ? TypedResults.Ok(shop) : TypedResults.NotFound(url);
    }

    [HttpGet("{id:int}", Name = nameof(GetShop))]
    public async Task<Results<BadRequest<int>, NotFound<int>, Ok<Alchemist.Product.Data.Shop>>> GetShop(int id, CancellationToken cancellationToken = default)
    {
        if (id < 0)
            return TypedResults.BadRequest(id);

        var shop = await _mediator.Send(new GetByIdRequest<Alchemist.Product.Data.Shop>(id), cancellationToken);

        return shop != null
            ? TypedResults.Ok(shop) 
            : TypedResults.NotFound(id);
    }
    
    [HttpGet("Shops", Name = nameof(GetShops))]
    public async Task<Results<NotFound, Ok<List<Alchemist.Product.Data.Shop>>>> GetShops(CancellationToken cancellationToken = default)
    {
        var shops = await _mediator.Send(new GetAllRequest<Alchemist.Product.Data.Shop>(), cancellationToken);
        return shops != null && shops.Count > 0 ? 
            TypedResults.Ok(shops) :
            TypedResults.NotFound();
    }

    [HttpPut(Name = nameof(CreateShop))]
    public async Task<Results<BadRequest, BadRequest<Alchemist.Product.Data.Shop>, Created<Alchemist.Product.Data.Shop>>> 
        CreateShop(Alchemist.Product.Data.Shop shop, CancellationToken cancellationToken = default)
    {
        if (shop == null)
            return TypedResults.BadRequest();

        if (string.IsNullOrEmpty(shop.Name) || string.IsNullOrEmpty(shop.Url))
            return TypedResults.BadRequest(shop);

        var newShop = await _mediator.Send(new CreateCommand<Alchemist.Product.Data.Shop>(shop), cancellationToken);

        var location = Url.Action(nameof(CreateShop), new { id = newShop.Id }) ?? $"/{newShop.Id}";
        return TypedResults.Created(location, newShop);
    }

    [HttpPost("Update", Name = nameof(UpdateShop))]
    public async Task<Results<BadRequest, BadRequest<Alchemist.Product.Data.Shop>, Accepted<Alchemist.Product.Data.Shop>>>
        UpdateShop(Alchemist.Product.Data.Shop shop, CancellationToken cancellationToken = default)
    {
        if (shop == null)
            return TypedResults.BadRequest();

        if (shop.Id <=0 ||  string.IsNullOrEmpty(shop.Name) || string.IsNullOrEmpty(shop.Url))
            return TypedResults.BadRequest(shop);

        var updatedShop = await _mediator.Send(new UpdateCommand<Alchemist.Product.Data.Shop>(shop), cancellationToken);        

        var location = Url.Action(nameof(UpdateShop), new { id = updatedShop.Id }) ?? $"/{updatedShop.Id}";
        return TypedResults.Accepted(location, updatedShop);
    }
}