using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using Message.Interfaces;
using MediatR;
using Mediator.Infrastructure.Request;
using Shop.Infrastructure;
using Mediator.Infrastructure.Command;


namespace Alchemist.Product.RestAPI.Controllers;

[ApiController]
[Route("api/Shop")]
public class ShopController(ILogger<ShopController> logger, IMediator mediator, IMessageSender messageSender) 
    : ControllerBase
{
    private readonly ILogger<ShopController> _logger = logger;
    private readonly IMediator _mediator = mediator;
    private readonly IMessageSender _messageSender = messageSender;

    private async Task SendMessage<T>(T entity, string methodName, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return;

        try
        {
            if (!_messageSender.IsConnected)
                await _messageSender.Start(cancellationToken);

            await _messageSender.Send(entity, methodName, cancellationToken);
            _logger.LogInformation($"Call {methodName} {entity} ");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }

    [HttpGet("byName", Name = nameof(GetShopByName))]
    public async Task<Results<BadRequest, NotFound<string>, Ok<Data.Shop>>> GetShopByName(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(name))
            return TypedResults.BadRequest();

        var shop = await _mediator.Send(new FindByNameRequest<Data.Shop>(name, s=>s.Name), cancellationToken);

        return shop != null ? TypedResults.Ok(shop) : TypedResults.NotFound(name);
    }

    [HttpGet("byUrl", Name = nameof(GetShopByUrl))]
    public async Task<Results<BadRequest, NotFound<string>, Ok<Data.Shop>>> GetShopByUrl(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(url))
            return TypedResults.BadRequest();

        var shop = await _mediator.Send(new GetShopByUrlRequest(url), cancellationToken);

        return shop != null ? TypedResults.Ok(shop) : TypedResults.NotFound(url);
    }

    [HttpGet("{id:int}", Name = nameof(GetShop))]
    public async Task<Results<BadRequest<int>, NotFound<int>, Ok<Data.Shop>>> GetShop(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return TypedResults.BadRequest(id);

        var shop = await _mediator.Send(new GetByIdRequest<Data.Shop>(id), cancellationToken);

        return shop != null
            ? TypedResults.Ok(shop) 
            : TypedResults.NotFound(id);
    }
    
    [HttpGet("Shops", Name = nameof(GetShops))]
    public async Task<Results<NotFound, Ok<List<Data.Shop>>>> GetShops(CancellationToken cancellationToken = default)
    {
        var shops = await _mediator.Send(new GetAllRequest<Data.Shop>(), cancellationToken);
        return shops != null && shops.Count > 0 ? 
            TypedResults.Ok(shops) :
            TypedResults.NotFound();
    }

    [HttpPut(Name = nameof(CreateShop))]
    public async Task<Results<BadRequest, BadRequest<Data.Shop>, Created<Data.Shop>>> CreateShop(Data.Shop shop, CancellationToken cancellationToken = default)
    {
        if (shop == null)
            return TypedResults.BadRequest();

        if (string.IsNullOrEmpty(shop.Name) || string.IsNullOrEmpty(shop.Url))
            return TypedResults.BadRequest(shop);

        var newShop = await _mediator.Send(new CreateCommand<Data.Shop>(shop), cancellationToken);
        
        await SendMessage(newShop, Messages.Common.Messages.ShopCreated, cancellationToken);

        var location = Url.Action(nameof(CreateShop), new { id = newShop.Id }) ?? $"/{newShop.Id}";
        return TypedResults.Created(location, newShop);
    }

    [HttpPost("Update", Name = nameof(UpdateShop))]
    public async Task<Results<BadRequest, BadRequest<Data.Shop>, Accepted<Data.Shop>>> UpdateShop(Data.Shop shop, CancellationToken cancellationToken = default)
    {
        if (shop == null)
            return TypedResults.BadRequest();

        if (shop.Id <=0 ||  string.IsNullOrEmpty(shop.Name) || string.IsNullOrEmpty(shop.Url))
            return TypedResults.BadRequest(shop);

        var updatedShop = await _mediator.Send( new UpdateCommand<Data.Shop>(shop), cancellationToken);        

        var location = Url.Action(nameof(UpdateShop), new { id = updatedShop.Id }) ?? $"/{updatedShop.Id}";
        return TypedResults.Accepted(location, updatedShop);
    }
}