using Alchemist.DataService.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Alchemist.Settings.RestAPI.Controllers
{
    [ApiController]
    [Route("api/Settings")]
    public class SettingsController(ILogger<SettingsController> logger, ISettingsRepository settingsRepository) : ControllerBase
    {
        private readonly ISettingsRepository _settingsRepository = settingsRepository;

        private readonly ILogger<SettingsController> _logger = logger;

        [HttpGet("shopSettings/byShopId/{shopId:int}/{shopSettingType:short}", Name = nameof(GetShopSettingsByShopId))]
        public async Task<Results<NotFound, Ok<ShopSettings>>> GetShopSettingsByShopId(int shopId, ShopSettingType shopSettingType)
        {
            var shopSettings = await _settingsRepository.GetShopSettings(shopId, shopSettingType);
            return shopSettings != null ? TypedResults.Ok(shopSettings.To<ShopSettings>()) : TypedResults.NotFound();
        }
        
        [HttpGet("shopSettings/byId/{id:int}", Name = nameof(GetShopSettingsById))]
        public async Task<Results<NotFound, Ok<ShopSettings>>> GetShopSettingsById(int id)
        {
            var shopSettings = await _settingsRepository.GetShopSettings(id);
            return shopSettings != null ? TypedResults.Ok(shopSettings.To<ShopSettings>()) : TypedResults.NotFound();
        }

        [HttpPost("shopSettings", Name = nameof(AddShopSettings))]
        public async Task<Results<BadRequest<ShopSettings>, Created<ShopSettings>>> AddShopSettings(ShopSettings shopSettings)
        {
            if (shopSettings == null || shopSettings.ShopId == 0 || string.IsNullOrEmpty(shopSettings.JsonValue))
                return TypedResults.BadRequest(shopSettings);

            var newShopSettings = (await _settingsRepository.AddShopSettings(shopSettings)).To<ShopSettings>();

            var location = Url.Action(nameof(AddShopSettings), new { id = newShopSettings.Id }) ?? $"/{newShopSettings.Id}";
            return TypedResults.Created(location, newShopSettings);
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
    }
}
