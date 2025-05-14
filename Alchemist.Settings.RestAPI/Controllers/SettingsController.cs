using Alchemist.DataService.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Net;
using System.Text.Json;

namespace Alchemist.Settings.RestAPI.Controllers;

[ApiController]
[Route("api/Settings")]
public class SettingsController(ILogger<SettingsController> logger, ISettingsRepository settingsRepository) : ControllerBase
{
    private readonly ISettingsRepository _settingsRepository = settingsRepository;

    private readonly ILogger<SettingsController> _logger = logger;

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
    public async Task<Results<BadRequest, BadRequest<ShopSettings>, Created<ShopSettings>>> SaveShopSettings(ShopSettings shopSettings)
    {
        if (shopSettings == null)
            return TypedResults.BadRequest();

        if (shopSettings.ShopId <= 0 || shopSettings.JsonValue == null)
            return TypedResults.BadRequest(shopSettings);

        var newShopSettings = (await _settingsRepository.SaveShopSettings(shopSettings)).To<ShopSettings>();

        var location = Url.Action(nameof(SaveShopSettings), new { id = newShopSettings.Id }) ?? $"/{newShopSettings.Id}";
        return TypedResults.Created(location, newShopSettings);
    }

    [HttpPost("save", Name = nameof(SaveShopSettingsWithServices))]
    public async Task<Results<BadRequest, BadRequest<ArrayList>, StatusCodeHttpResult, Created<ArrayList>, Accepted<ArrayList>>>
        SaveShopSettingsWithServices(ArrayList shopSettingsWithServices)
    {
        if (shopSettingsWithServices == null || shopSettingsWithServices.Count == 0)
            return TypedResults.BadRequest();

        if (shopSettingsWithServices.Count < 2 || shopSettingsWithServices.Contains(null))
            return TypedResults.BadRequest(shopSettingsWithServices);

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true };

        ShopSettings shopSettings;
        ShopSettings[] services;

        try
        {
            shopSettings = JsonSerializer.Deserialize<ShopSettings>(shopSettingsWithServices[0].ToString(), options);
            services = JsonSerializer.Deserialize<ShopSettings[]>(shopSettingsWithServices[1].ToString(), options);
        }
        catch
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

        var location = Url.Action(nameof(SaveShopSettingsWithServices), new { id = shopSettingResult.Id }) ?? $"/{shopSettingResult.Id}";
        var result = new ArrayList { shopSettingResult, allData.Where(d => d.Type == ShopSettingType.Service).ToArray() };
        return initId == 0 ? TypedResults.Created(location, result) : TypedResults.Accepted(location, result);
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
