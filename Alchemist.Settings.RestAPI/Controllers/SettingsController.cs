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
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Alchemist.Settings.RestAPI.Controllers;

[ApiController]
[Route("api/Settings")]
public class SettingsController(ILogger<SettingsController> logger, IMediator mediator, IMessageSender messageSender) : ControllerBase
{
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
    public async Task<Results<BadRequest, BadRequest<string>,  BadRequest<ArrayList>, StatusCodeHttpResult, Created<ArrayList>, Accepted<ArrayList>>>
        SaveShopSettingsWithServices(ArrayList shopSettingsWithServices, CancellationToken cancellationToken = default)
    {
        if (shopSettingsWithServices == null || shopSettingsWithServices.Count == 0)
            return TypedResults.BadRequest();

        if (shopSettingsWithServices.Count < 2 || shopSettingsWithServices.Contains(null))
            return TypedResults.BadRequest(shopSettingsWithServices);

        Product.Data.ShopSettings shopSettings;
        var services = new List<Product.Data.ShopSettings>();

        try
        {
            var data = DeserializeShopSettings(shopSettingsWithServices);
            shopSettings = data.Item1;
            services.AddRange(data.Item2);
        }
        catch (Exception e)
        {
            return TypedResults.BadRequest(shopSettingsWithServices);
        }

        if (cancellationToken.IsCancellationRequested)
            return TypedResults.BadRequest(shopSettingsWithServices);

        if (shopSettings == null || shopSettings.ShopId == 0 || shopSettings.JsonValue == null
            || services == null || services.Any(s => s.JsonValue == null))
            return TypedResults.BadRequest(shopSettingsWithServices);

        var initId = shopSettings.Id;

        var allData = await _mediator.Send(new SaveShopSettingsWithChildrenCommand(shopSettings, services), cancellationToken);
        var shopSettingResult = allData.FirstOrDefault(s => s.Type != ShopSettingType.Service);
        if (shopSettingResult == null)
            return TypedResults.StatusCode((int)HttpStatusCode.InternalServerError);

        if (initId == 0)
            await SendMessage(shopSettingResult, Messages.Common.Messages.ShopSettingsCreated, cancellationToken);

        var location = Url.Action(nameof(SaveShopSettingsWithServices), new { id = shopSettingResult.Id }) ?? $"/{shopSettingResult.Id}";
        var result = new ArrayList { shopSettingResult, allData.Where(d => d.Type == ShopSettingType.Service).ToArray() };
        return initId == 0 ? TypedResults.Created(location, result) : TypedResults.Accepted(location, result);
    }

    private static Tuple<Product.Data.ShopSettings, IEnumerable<Product.Data.ShopSettings>> DeserializeShopSettings(ArrayList shopSettingsWithServices)
    {
        var serializationOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            WriteIndented = true
        };

        var serviceSerializationOptions = new JsonSerializerOptions(serializationOptions)
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { JsonExtensions.IgnorePropertiesForSerialize(typeof(Product.Data.ShopSettings),
                nameof(Product.Data.ShopSettings.JsonValue)) }
            }
        };

        Product.Data.ShopSettings shopSettings;
        var services = new List<Product.Data.ShopSettings>();

        shopSettings = JsonSerializer.Deserialize<Product.Data.ShopSettings>(shopSettingsWithServices[0].ToString(), serializationOptions);

        var jsonServices = JsonSerializer.Deserialize<JsonObject[]>(shopSettingsWithServices[1].ToString(), serializationOptions);
        foreach (var jsonService in jsonServices)
        {
            if (!jsonService.TryGetPropertyValue("jsonValue", out var jsonValue) || jsonValue == null)
                throw new InvalidDataException(jsonService.ToString());

            jsonService.Remove("jsonValue");
            var service = JsonSerializer.Deserialize<Product.Data.ShopSettings>(jsonService.ToString(), serviceSerializationOptions);
            service.JsonValue = JsonSerializer.Serialize(jsonValue.ToString());

            services.Add(service);
        }

        return new Tuple<Product.Data.ShopSettings, IEnumerable<Product.Data.ShopSettings>>(shopSettings, services);
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