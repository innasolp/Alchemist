using Alchemist.Product.Data;
using Db.Infrastructure.Requests;
using Db.Infrastructure;
using ShopSettings.Data.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Db.Infrastructure.Commands;

namespace Alchemist.Settings.RestAPI.Controllers;

[ApiController]
[Route("api/Settings")]
public class SettingsController(ILogger<SettingsController> logger)
    : ControllerBase
{
    public record ShopSettingsWithServices(Product.Data.ShopSettings ShopSettings, Product.Data.ShopSettings[] Services);

    private readonly ILogger<SettingsController> _logger = logger;

    [HttpGet("byShopId/{shopId:int}/{shopSettingType:int}", Name = nameof(GetShopSettingsByShopId))]
    public async Task<Results<BadRequest<int>, NotFound<int>, Ok<Product.Data.ShopSettings>>> GetShopSettingsByShopId(int shopId, int shopSettingType,
        [FromServices] IRequestHandler<GetShopSettingsByShopIdRequest, Product.Data.ShopSettings> handler,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return TypedResults.NotFound(shopId);

        if (shopId < 0)
            return TypedResults.BadRequest(shopId);

        var shopSettings = await handler.Handle(new GetShopSettingsByShopIdRequest(shopId, (ShopSettingType)shopSettingType), cancellationToken);
        return shopSettings != null ? TypedResults.Ok(shopSettings) : TypedResults.NotFound(shopId);
    }

    [HttpGet("byId/{id:int}", Name = nameof(GetShopSettingsById))]
    public async Task<Results<BadRequest<int>, NotFound<int>, Ok<Product.Data.ShopSettings>>> GetShopSettingsById(int id,
        [FromServices] IRequestHandler<GetByIdRequest<Product.Data.ShopSettings>, Product.Data.ShopSettings> handler,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return TypedResults.NotFound(id);

        if (id < 0)
            return TypedResults.BadRequest(id);

        var shopSettings = await handler.Handle(new GetByIdRequest<Product.Data.ShopSettings>(id), cancellationToken);
        return shopSettings != null ? TypedResults.Ok(shopSettings) : TypedResults.NotFound(id);
    }


    [HttpGet("byName", Name = nameof(GetShopSettingsByName))]
    public async Task<Results<BadRequest, NotFound<string>, Ok<Product.Data.ShopSettings>>> GetShopSettingsByName(string name,
        [FromServices] IRequestHandler<FindByNameRequest<Product.Data.ShopSettings>, Product.Data.ShopSettings> handler,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return TypedResults.NotFound(name);

        if (string.IsNullOrEmpty(name))
            return TypedResults.BadRequest();

        var shopSettings = await handler.Handle(new FindByNameRequest<Product.Data.ShopSettings>(name), cancellationToken);
        return shopSettings != null ? TypedResults.Ok(shopSettings) : TypedResults.NotFound(name);
    }

    [HttpPost(Name = nameof(SaveShopSettings))]
    public async Task<Results<BadRequest,BadRequest<Product.Data.ShopSettings>, Created<Product.Data.ShopSettings>, Accepted<Product.Data.ShopSettings>>> 
        SaveShopSettings(Product.Data.ShopSettings shopSettings,
        [FromServices] ICommandHandler<SaveShopSettingsCommand> handler,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return TypedResults.BadRequest();

        if (shopSettings == null)
            return TypedResults.BadRequest();

        if (shopSettings.ShopId <= 0 || shopSettings.JsonValue == null)
            return TypedResults.BadRequest(shopSettings);

        var shopSettingsId = shopSettings.Id;
        await handler.Handle(new SaveShopSettingsCommand(shopSettings), cancellationToken);

        var location = Url.Action(nameof(SaveShopSettings), new { id = shopSettings.Id }) ?? $"/{shopSettings.Id}";
        return shopSettingsId == 0 
            ? TypedResults.Created(location, shopSettings)
            : TypedResults.Accepted(location, shopSettings);
    }   


    [HttpPost("save", Name = nameof(SaveShopSettingsWithServices))]
    public async Task<Results<BadRequest, BadRequest<string>,  BadRequest<ShopSettingsWithServices>, StatusCodeHttpResult, Created<ShopSettingsWithServices>, Accepted<ShopSettingsWithServices>>> 
        SaveShopSettingsWithServices(ShopSettingsWithServices shopSettingsWithServices,
        [FromServices] ICommandHandler<SaveShopSettingsWithChildrenCommand> handler,
        CancellationToken cancellationToken = default)
    {
        if (shopSettingsWithServices == null || shopSettingsWithServices.ShopSettings == null)
            return TypedResults.BadRequest();  

        if (shopSettingsWithServices.ShopSettings.ShopId == 0 || shopSettingsWithServices.ShopSettings.JsonValue == null
            || shopSettingsWithServices.Services == null || shopSettingsWithServices.Services.Any(s => s.JsonValue == null))
            return TypedResults.BadRequest(shopSettingsWithServices);

        var initId = shopSettingsWithServices.ShopSettings.Id;

        await handler.Handle(new SaveShopSettingsWithChildrenCommand(shopSettingsWithServices.ShopSettings, shopSettingsWithServices.Services), cancellationToken);

        var location = Url.Action(nameof(SaveShopSettingsWithServices), new { id = shopSettingsWithServices.ShopSettings.Id }) ?? $"/{shopSettingsWithServices.ShopSettings.Id}";

        var result = new ShopSettingsWithServices(shopSettingsWithServices.ShopSettings, shopSettingsWithServices.Services);
        return initId == 0 ? TypedResults.Created(location, result) : TypedResults.Accepted(location, result);
    }
    
    [HttpPut("update", Name = nameof(UpdateShopSettings))]
    public async Task<Results<BadRequest, BadRequest<Product.Data.ShopSettings>, Accepted<Product.Data.ShopSettings>, NotFound<Product.Data.ShopSettings>>> 
        UpdateShopSettings(Product.Data.ShopSettings shopSettings,
        [FromServices] ICommandHandler<UpdateCommand<Product.Data.ShopSettings>> handler,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return TypedResults.BadRequest();

        if (shopSettings == null)
            return TypedResults.BadRequest();

        if (shopSettings.ShopId <= 0 || shopSettings.JsonValue == null)
            return TypedResults.BadRequest(shopSettings);

        await handler.Handle(new UpdateCommand<Product.Data.ShopSettings>(shopSettings), cancellationToken);

        var location = Url.Action(nameof(UpdateShopSettings), new { id = shopSettings.Id }) ?? $"/{shopSettings.Id}";
        return TypedResults.Accepted(location, shopSettings);
    }

    [HttpGet("childSettings/{parentSettingsId:int}", Name = nameof(GetChildSettings))]
    public async Task<Results<BadRequest<int>, Ok<List<Product.Data.ShopSettings>>>> GetChildSettings(int parentSettingsId,
        [FromServices] IRequestHandler<GetChildSettingsRequest, List<Product.Data.ShopSettings>> handler,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return TypedResults.BadRequest(parentSettingsId);

        if (parentSettingsId <= 0)
            return TypedResults.BadRequest(parentSettingsId);

        var childSettings = await handler.Handle(new GetChildSettingsRequest(parentSettingsId), cancellationToken);
        return TypedResults.Ok(childSettings);
    }

    [HttpGet("allParents", Name = nameof(GetAllParentShopSettings))]
    public async Task<Results<BadRequest<int>, Ok<List<Product.Data.ShopSettings>>>> GetAllParentShopSettings(
        [FromServices] IRequestHandler<GetAllParentShopSettingsRequest, List<Product.Data.ShopSettings>> handler,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return TypedResults.BadRequest(0);

        var allParents = await handler.Handle(new GetAllParentShopSettingsRequest(), cancellationToken);
        return TypedResults.Ok(allParents);
    }
}