using Alchemist.Common;
using Alchemist.Product.Data;
using Mediator.Infrastructure.Command;
using Mediator.Infrastructure.Request;
using MediatR;
using Message.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ShopSettings.Infrastructure;
using System.Collections;
using System.Net;

namespace Alchemist.Settings.RestAPI.Controllers;

[ApiController]
[Route("api/Settings")]
public class SettingsController(ILogger<SettingsController> logger, IMediator mediator, IMessageSender messageSender) : ControllerBase
{
    public record ShopSettingsWithServices(Product.Data.ShopSettings ShopSettings, Product.Data.ShopSettings[] Services);

    private readonly IMediator _mediator = mediator;

    private readonly ILogger<SettingsController> _logger = logger;

    private readonly IMessageSender _messageSender = messageSender;

    private async Task SendMessage<T>(T entity, string methodName, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return;

        try
        {
            if(!_messageSender.IsConnected)
                await _messageSender.Start(cancellationToken);
            await _messageSender.Send(entity, methodName, cancellationToken);
            _logger.LogInformation($"Call {methodName} {entity} ");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }

    [HttpGet("byShopId/{shopId:int}/{shopSettingType:int}", Name = nameof(GetShopSettingsByShopId))]
    public async Task<Results<BadRequest<int>, NotFound<int>, Ok<Product.Data.ShopSettings>>> GetShopSettingsByShopId(int shopId, int shopSettingType, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return TypedResults.NotFound(shopId);

        if (shopId < 0)
            return TypedResults.BadRequest(shopId);

        var shopSettings = await _mediator.Send( new GetShopSettingsByShopIdRequest(shopId, (ShopSettingType)shopSettingType), cancellationToken);
        return shopSettings != null ? TypedResults.Ok(shopSettings) : TypedResults.NotFound(shopId);
    }

    [HttpGet("byId/{id:int}", Name = nameof(GetShopSettingsById))]
    public async Task<Results<BadRequest<int>, NotFound<int>, Ok<Product.Data.ShopSettings>>> GetShopSettingsById(int id, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return TypedResults.NotFound(id);

        if (id < 0)
            return TypedResults.BadRequest(id);

        var shopSettings = await _mediator.Send(new GetByIdRequest<Product.Data.ShopSettings>(id), cancellationToken);
        return shopSettings != null ? TypedResults.Ok(shopSettings) : TypedResults.NotFound(id);
    }


    [HttpGet("byName", Name = nameof(GetShopSettingsByName))]
    public async Task<Results<BadRequest, NotFound<string>, Ok<Product.Data.ShopSettings>>> GetShopSettingsByName(string name, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return TypedResults.NotFound(name);

        if (string.IsNullOrEmpty(name))
            return TypedResults.BadRequest();

        var shopSettings = await _mediator.Send(new FindByNameRequest<Product.Data.ShopSettings>(name, s=>s.Name), cancellationToken);
        return shopSettings != null ? TypedResults.Ok(shopSettings) : TypedResults.NotFound(name);
    }

    [HttpPost(Name = nameof(SaveShopSettings))]
    public async Task<Results<BadRequest,BadRequest<Product.Data.ShopSettings>, Created<Product.Data.ShopSettings>, Accepted<Product.Data.ShopSettings>>> 
        SaveShopSettings(Product.Data.ShopSettings shopSettings, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return TypedResults.BadRequest();

        if (shopSettings == null)
            return TypedResults.BadRequest();

        if (shopSettings.ShopId <= 0 || shopSettings.JsonValue == null)
            return TypedResults.BadRequest(shopSettings);

        var shopSettingsId = shopSettings.Id;
        var savedShopSettings = await _mediator.Send(new SaveShopSettingsCommand(shopSettings), cancellationToken);

        if (shopSettingsId == 0)
            await SendMessage(savedShopSettings, Messages.Common.Messages.ShopSettingsCreated, cancellationToken);

        var location = Url.Action(nameof(SaveShopSettings), new { id = savedShopSettings.Id }) ?? $"/{savedShopSettings.Id}";
        return shopSettingsId == 0 
            ? TypedResults.Created(location, savedShopSettings)
            : TypedResults.Accepted(location, savedShopSettings);
    }   


    [HttpPost("save", Name = nameof(SaveShopSettingsWithServices))]
    public async Task<Results<BadRequest, BadRequest<string>,  BadRequest<ShopSettingsWithServices>, StatusCodeHttpResult, Created<ArrayList>, Accepted<ArrayList>>>
        SaveShopSettingsWithServices(ShopSettingsWithServices shopSettingsWithServices, CancellationToken cancellationToken = default)
    {
        if (shopSettingsWithServices == null || shopSettingsWithServices.ShopSettings == null)
            return TypedResults.BadRequest();  

        if (shopSettingsWithServices.ShopSettings.ShopId == 0 || shopSettingsWithServices.ShopSettings.JsonValue == null
            || shopSettingsWithServices.Services == null || shopSettingsWithServices.Services.Any(s => s.JsonValue == null))
            return TypedResults.BadRequest(shopSettingsWithServices);

        var initId = shopSettingsWithServices.ShopSettings.Id;

        var allData = await _mediator.Send(
            new SaveShopSettingsWithChildrenCommand(shopSettingsWithServices.ShopSettings, shopSettingsWithServices.Services), cancellationToken);
        var shopSettingResult = allData.FirstOrDefault(s => s.Type != ShopSettingType.Service);
        if (shopSettingResult == null)
            return TypedResults.StatusCode((int)HttpStatusCode.InternalServerError);

        if (initId == 0)
            await SendMessage(shopSettingResult, Messages.Common.Messages.ShopSettingsCreated, cancellationToken);

        var location = Url.Action(nameof(SaveShopSettingsWithServices), new { id = shopSettingResult.Id }) ?? $"/{shopSettingResult.Id}";
        var result = new ArrayList { shopSettingResult, allData.Where(d => d.Type == ShopSettingType.Service).ToArray() };
        return initId == 0 ? TypedResults.Created(location, result) : TypedResults.Accepted(location, result);
    }
    
    [HttpPut("update", Name = nameof(UpdateShopSettings))]
    public async Task<Results<BadRequest, BadRequest<Product.Data.ShopSettings>, Accepted<Product.Data.ShopSettings>, NotFound<Product.Data.ShopSettings>>> 
        UpdateShopSettings(Product.Data.ShopSettings shopSettings, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return TypedResults.BadRequest();

        if (shopSettings == null)
            return TypedResults.BadRequest();

        if (shopSettings.ShopId <= 0 || shopSettings.JsonValue == null)
            return TypedResults.BadRequest(shopSettings);

        var result = await _mediator.Send(new UpdateCommand<Product.Data.ShopSettings>(shopSettings), cancellationToken);

        var location = Url.Action(nameof(UpdateShopSettings), new { id = shopSettings.Id }) ?? $"/{shopSettings.Id}";
        return result != null ? TypedResults.Accepted(location, shopSettings) : TypedResults.NotFound(shopSettings);
    }

    [HttpGet("childSettings/{parentSettingsId:int}", Name = nameof(GetChildSettings))]
    public async Task<Results<BadRequest<int>, Ok<List<Product.Data.ShopSettings>>>> GetChildSettings(int parentSettingsId, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return TypedResults.BadRequest(parentSettingsId);

        if (parentSettingsId <= 0)
            return TypedResults.BadRequest(parentSettingsId);

        var childSettings = await _mediator.Send(new GetChildSettingsRequest(parentSettingsId), cancellationToken);
        return TypedResults.Ok(childSettings);
    }

    [HttpGet("allParents", Name = nameof(GetAllParentShopSettings))]
    public async Task<Results<BadRequest<int>, Ok<List<Product.Data.ShopSettings>>>> GetAllParentShopSettings(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return TypedResults.BadRequest(0);

        var allParents = await _mediator.Send(new GetAllParentShopSettingsRequest(), cancellationToken);
        return TypedResults.Ok(allParents);
    }
}