using Alchemist.DataService.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Alchemist.Settings.RestAPI.Controllers;

public record ShopSettingsWithServices (ShopSettings ShopSettings, ShopSettings[] Services);

[ApiController]
[Route("api/Settings")]
public class SettingsController(ILogger<SettingsController> logger, ISettingsRepository settingsRepository) : ControllerBase
{
    private readonly ISettingsRepository _settingsRepository = settingsRepository;

    private readonly ILogger<SettingsController> _logger = logger;

    [HttpGet("shopSettings/byShopId/{shopId:int}/{shopSettingType:int}", Name = nameof(GetShopSettingsByShopId))]
    public async Task<Results<BadRequest, NotFound, Ok<ShopSettings>>> GetShopSettingsByShopId(int shopId, int shopSettingType)
    {
        if (shopId <= 0)
            return TypedResults.BadRequest();

        var shopSettings = await _settingsRepository.GetShopSettings(shopId,(ShopSettingType)shopSettingType);
        return shopSettings != null ? TypedResults.Ok(shopSettings.To<ShopSettings>()) : TypedResults.NotFound();
    }
    
    [HttpGet("shopSettings/byId/{id:int}", Name = nameof(GetShopSettingsById))]
    public async Task<Results<BadRequest, NotFound, Ok<ShopSettings>>> GetShopSettingsById(int id)
    {
        if (id <= 0)
            return TypedResults.BadRequest();

        var shopSettings = await _settingsRepository.GetShopSettings(id);
        return shopSettings != null ? TypedResults.Ok(shopSettings.To<ShopSettings>()) : TypedResults.NotFound();
    }

    [HttpPost("shopSettings", Name = nameof(SaveShopSettings))]
    public async Task<Results<BadRequest<ShopSettings>, Created<ShopSettings>>> SaveShopSettings(ShopSettings shopSettings)
    {
        if (shopSettings == null || shopSettings.ShopId == 0 || string.IsNullOrEmpty(shopSettings.JsonValue))
            return TypedResults.BadRequest(shopSettings);

        var newShopSettings = (await _settingsRepository.SaveShopSettings(shopSettings)).To<ShopSettings>();

        var location = Url.Action(nameof(SaveShopSettings), new { id = newShopSettings.Id }) ?? $"/{newShopSettings.Id}";
        return TypedResults.Created(location, newShopSettings);
    }
    
    [HttpPost("shopSettings/save", Name = nameof(SaveShopSettingsWithServices))]
    public async Task<Results<BadRequest<List<ShopSettings>>, StatusCodeHttpResult, Created <List<ShopSettings>>, Accepted<List<ShopSettings>>>> 
        SaveShopSettingsWithServices(ShopSettingsWithServices shopSettingsWithServices)
    {
        var initId = shopSettingsWithServices.ShopSettings.Id;

        if (shopSettingsWithServices.ShopSettings == null || shopSettingsWithServices.ShopSettings.ShopId == 0
            || string.IsNullOrEmpty(shopSettingsWithServices.ShopSettings.JsonValue)
            || shopSettingsWithServices.Services.Any(s=>string.IsNullOrEmpty(s.JsonValue)))
            return TypedResults.BadRequest(new List<ShopSettings> { shopSettingsWithServices .ShopSettings});

        var allData = (await _settingsRepository.SaveShopSettings(shopSettingsWithServices.ShopSettings, shopSettingsWithServices.Services)).OfType<ShopSettings>().ToList();
        var shopSettingResult = allData.FirstOrDefault(s => s.Type != ShopSettingType.Service);
        if (shopSettingResult == null)
            return TypedResults.StatusCode((int)HttpStatusCode.InternalServerError);

        var location = Url.Action(nameof(SaveShopSettingsWithServices), new { id = shopSettingResult.Id }) ?? $"/{shopSettingResult.Id}";
        return initId == 0 ? TypedResults.Created(location, allData) : TypedResults.Accepted(location, allData);
    }
    
    [HttpPut("shopSettings/update", Name = nameof(UpdateShopSettings))]
    public async Task<Results<BadRequest<ShopSettings>, Ok<ShopSettings>, StatusCodeHttpResult>> UpdateShopSettings(ShopSettings shopSettings)
    {
        if (shopSettings == null || shopSettings.ShopId == 0 || string.IsNullOrEmpty(shopSettings.JsonValue))
            return TypedResults.BadRequest(shopSettings);

        var result = await _settingsRepository.UpdateShopSettings(shopSettings);

        var location = Url.Action(nameof(UpdateShopSettings), new { id = shopSettings.Id }) ?? $"/{shopSettings.Id}";
        return result ? TypedResults.Ok(shopSettings.To<ShopSettings>()) : TypedResults.StatusCode(500);
    }

    [HttpGet("shopSettings/childSettings/{parentSettingsId:int}", Name = nameof(GetChildSettings))]
    public async Task<Results<BadRequest, Ok<List<ShopSettings>>>> GetChildSettings(int parentSettingsId)
    {
        if (parentSettingsId <= 0)
            return TypedResults.BadRequest();

        var childSettings = await _settingsRepository.GetChildSettings(parentSettingsId);
        return TypedResults.Ok(childSettings.OfType<ShopSettings>().ToList());
    }
}
