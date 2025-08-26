using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using Message.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Alchemist.Settings.RestAPI.Controllers;

[ApiController]
[Route("api/Settings")]
public class SettingsController(ILogger<SettingsController> logger, ISettingsRepository settingsRepository, IMessageSender messageSender) : ControllerBase
{
    private readonly ISettingsRepository _settingsRepository = settingsRepository;

    private readonly ILogger<SettingsController> _logger = logger;

    private readonly IMessageSender _messageSender = messageSender;

    private async Task SendMessage<T>(T entity, string methodName)
    {
        try
        {
            if(!_messageSender.IsConnected)
                await _messageSender.Start();
            await _messageSender.Send(entity, methodName);
            _logger.LogInformation($"Call {methodName} {entity} ");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }

    [HttpGet("byShopId/{shopId:int}/{shopSettingType:int}", Name = nameof(GetShopSettingsByShopId))]
    public async Task<Results<BadRequest<int>, NotFound<int>, Ok<ShopSettings>>> GetShopSettingsByShopId(int shopId, int shopSettingType)
    {
        if (shopId <= 0)
            return TypedResults.BadRequest(shopId);

        var shopSettings = await _settingsRepository.GetShopSettings(shopId, (ShopSettingType)shopSettingType);
        return shopSettings != null ? TypedResults.Ok(shopSettings.To<ShopSettings>()) : TypedResults.NotFound(shopId);
    }

    [HttpGet("byId/{id:int}", Name = nameof(GetShopSettingsById))]
    public async Task<Results<BadRequest<int>, NotFound<int>, Ok<ShopSettings>>> GetShopSettingsById(int id)
    {
        if (id <= 0)
            return TypedResults.BadRequest(id);

        var shopSettings = await _settingsRepository.GetShopSettings(id);
        return shopSettings != null ? TypedResults.Ok(shopSettings.To<ShopSettings>()) : TypedResults.NotFound(id);
    }


    [HttpGet("byName", Name = nameof(GetShopSettingsByName))]
    public async Task<Results<BadRequest, NotFound<string>, Ok<ShopSettings>>> GetShopSettingsByName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return TypedResults.BadRequest();

        var shopSettings = await _settingsRepository.GetShopSettings(name);
        return shopSettings != null ? TypedResults.Ok(shopSettings.To<ShopSettings>()) : TypedResults.NotFound(name);
    }

    [HttpPost(Name = nameof(SaveShopSettings))]
    public async Task<Results<BadRequest,BadRequest<ShopSettings>, Created<ShopSettings>, Accepted<ShopSettings>>> SaveShopSettings(ShopSettings shopSettings)
    {
        if (shopSettings == null)
            return TypedResults.BadRequest();

        if (shopSettings.ShopId <= 0 || shopSettings.JsonValue == null)
            return TypedResults.BadRequest(shopSettings);

        var savedShopSettings = (await _settingsRepository.SaveShopSettings(shopSettings)).To<ShopSettings>();

        if (shopSettings.Id == 0)
            await SendMessage(savedShopSettings, Messages.SendShopSettingsCreated);

        var location = Url.Action(nameof(SaveShopSettings), new { id = savedShopSettings.Id }) ?? $"/{savedShopSettings.Id}";
        return  shopSettings.Id == 0 
            ? TypedResults.Created(location, savedShopSettings)
            : TypedResults.Accepted(location, savedShopSettings);
    }

    [HttpPost("save", Name = nameof(SaveShopSettingsWithServices))]
    public async Task<Results<BadRequest, BadRequest<string>,  BadRequest<ArrayList>, StatusCodeHttpResult, Created<ArrayList>, Accepted<ArrayList>>>
        SaveShopSettingsWithServices(ArrayList shopSettingsWithServices)
    {
        if (shopSettingsWithServices == null || shopSettingsWithServices.Count == 0)
            return TypedResults.BadRequest();

        if (shopSettingsWithServices.Count < 2 || shopSettingsWithServices.Contains(null))
            return TypedResults.BadRequest(shopSettingsWithServices);        
        
        ShopSettings shopSettings;
        var services = new List<ShopSettings>();

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

        if (shopSettings == null || shopSettings.ShopId == 0 || shopSettings.JsonValue == null
            || services == null || services.Any(s => s.JsonValue == null))
            return TypedResults.BadRequest(shopSettingsWithServices);

        var initId = shopSettings.Id;

        var allData = (await _settingsRepository.SaveShopSettings(shopSettings, services)).Select(s => s.To<ShopSettings>()).ToList();
        var shopSettingResult = allData.FirstOrDefault(s => s.Type != ShopSettingType.Service);
        if (shopSettingResult == null)
            return TypedResults.StatusCode((int)HttpStatusCode.InternalServerError);

        if(initId == 0)
            await SendMessage(shopSettingResult, Messages.SendShopSettingsCreated);

        var location = Url.Action(nameof(SaveShopSettingsWithServices), new { id = shopSettingResult.Id }) ?? $"/{shopSettingResult.Id}";
        var result = new ArrayList { shopSettingResult, allData.Where(d => d.Type == ShopSettingType.Service).ToArray() };
        return initId == 0 ? TypedResults.Created(location, result) : TypedResults.Accepted(location, result);
    }

    private static Tuple<ShopSettings, IEnumerable<ShopSettings>> DeserializeShopSettings(ArrayList shopSettingsWithServices)
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
                Modifiers = { JsonExtensions.IgnorePropertiesForSerialize(typeof(ShopSettings),
                nameof(ShopSettings.JsonValue)) }
            }
        };

        ShopSettings shopSettings;
        var services = new List<ShopSettings>();

        shopSettings = JsonSerializer.Deserialize<ShopSettings>(shopSettingsWithServices[0].ToString(), serializationOptions);

        var jsonServices = JsonSerializer.Deserialize<JsonObject[]>(shopSettingsWithServices[1].ToString(), serializationOptions);
        foreach (var jsonService in jsonServices)
        {
            if (!jsonService.TryGetPropertyValue("jsonValue", out var jsonValue) || jsonValue == null)
                throw new InvalidDataException(jsonService.ToString());

            jsonService.Remove("jsonValue");
            var service = JsonSerializer.Deserialize<ShopSettings>(jsonService.ToString(), serviceSerializationOptions);
            service.JsonValue = JsonSerializer.Deserialize<JsonObject>(jsonValue.ToString());

            services.Add(service);
        }

        return new Tuple<ShopSettings, IEnumerable<ShopSettings>>(shopSettings, services);
    }

    [HttpPut("update", Name = nameof(UpdateShopSettings))]
    public async Task<Results<BadRequest, BadRequest<ShopSettings>, Accepted<ShopSettings>, NotFound<ShopSettings>>> 
        UpdateShopSettings(ShopSettings shopSettings)
    {
        if (shopSettings == null)
            return TypedResults.BadRequest();

        if (shopSettings.ShopId <= 0 || shopSettings.JsonValue == null)
            return TypedResults.BadRequest(shopSettings);

        var result = await _settingsRepository.UpdateShopSettings(shopSettings);

        var location = Url.Action(nameof(UpdateShopSettings), new { id = shopSettings.Id }) ?? $"/{shopSettings.Id}";
        return result ? TypedResults.Accepted(location, shopSettings.To<ShopSettings>()) : TypedResults.NotFound(shopSettings);
    }

    [HttpGet("childSettings/{parentSettingsId:int}", Name = nameof(GetChildSettings))]
    public async Task<Results<BadRequest<int>, Ok<List<ShopSettings>>>> GetChildSettings(int parentSettingsId)
    {
        if (parentSettingsId <= 0)
            return TypedResults.BadRequest(parentSettingsId);

        var childSettings = await _settingsRepository.GetChildSettings(parentSettingsId);
        return TypedResults.Ok(childSettings.Select(c => c.To<ShopSettings>()).ToList());
    }

    [HttpGet("allParents", Name = nameof(GetAllParentShopSettings))]
    public async Task<Results<BadRequest<int>, Ok<List<ShopSettings>>>> GetAllParentShopSettings()
    {
        var allParents = await _settingsRepository.GetAllParentShopSettings();
        return TypedResults.Ok(allParents.Select(c => c.To<ShopSettings>()).ToList());
    }
}
