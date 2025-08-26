using Alchemist.DataService.Interfaces;
using Alchemist.Settings.RestAPI.Controllers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using System.Text.Json.Nodes;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Collections;
using Message.Interfaces;

namespace Alchemist.Settings.RestAPI.UnitTest;

public class SettingsControllerPostTest
{
    private readonly Mock<ISettingsRepository> _settingsRepository = new();

    private readonly Mock<IMessageSender> _messageSender = new();

    private readonly ILogger<SettingsController> _logger = new Logger<SettingsController>(new LoggerFactory());

    private readonly SettingsController _settingsController;

    private readonly List<ShopSettings> _shopSettings =
        [
            new(){Id = 1, ShopId = 1, Type = ShopSettingType.Product},
            new(){Id = 2, ShopId = 2, Type = ShopSettingType.Product},
            new(){Id = 3, ShopId = 1, Type = ShopSettingType.Category},
            new(){Id = 4, ShopId = 2, Type = ShopSettingType.Category},
            new(){Id = 5, ShopId = 1, ParentSettingsId = 1, Type = ShopSettingType.Service },
            new(){Id = 6, ShopId = 1, ParentSettingsId = 1, Type = ShopSettingType.Service },
            new(){Id = 7, ShopId = 1, ParentSettingsId = 2, Type = ShopSettingType.Service },
            new(){Id = 8, ShopId = 1, ParentSettingsId = 2, Type = ShopSettingType.Service },
            new(){Id = 9, ShopId = 2, ParentSettingsId = 3, Type = ShopSettingType.Service },
            new(){Id = 10, ShopId = 2, ParentSettingsId = 3, Type = ShopSettingType.Service },
            new(){Id = 11, ShopId = 2, ParentSettingsId = 4, Type = ShopSettingType.Service },
            new(){Id = 12, ShopId = 2, ParentSettingsId = 4, Type = ShopSettingType.Service },
            new(){Id = 13, ShopId = 3, Type = ShopSettingType.Product },
        ];

    public SettingsControllerPostTest()
    {
        _settingsController = new SettingsController(_logger, _settingsRepository.Object, _messageSender.Object);

        Mock<IUrlHelper> _urlHelper = new();
        _settingsController.Url = _urlHelper.Object;
        _urlHelper.Setup(url => url.Action(It.IsAny<UrlActionContext>())).Returns("");

        _settingsRepository.Setup(s => s.SaveShopSettings(It.IsAny<IShopSettings>())).Returns((IShopSettings s) =>
        {
            var newSettings = s.To<ShopSettings>();
            _shopSettings.Add(newSettings);
            return Task.FromResult((IShopSettings)newSettings);
        });

        _settingsRepository.Setup(s => s.UpdateShopSettings(It.IsAny<IShopSettings>())).Returns((IShopSettings s) =>
        {
            var settings = _shopSettings.FirstOrDefault(st => st.Id == s.Id);
            if (settings == null) return Task.FromResult(false);
            settings.JsonValue = s.To<ShopSettings>().JsonValue;
            settings.ShopId = s.ShopId;
            return Task.FromResult(true);
        });

        _settingsRepository.Setup(s => s.SaveShopSettings(It.IsAny<IShopSettings>(), It.IsAny<IEnumerable<IShopSettings>>()))
            .Returns((IShopSettings s, IEnumerable<IShopSettings> services) =>
        {
            var result = new List<IShopSettings>();
            var newSettings = s.To<ShopSettings>();
            if(newSettings.Id == 0) newSettings.Id = _shopSettings.Count + 1;
            _shopSettings.Add(newSettings);
            result.Add(newSettings);
            services.ToList().ForEach(service =>
            {
                service.ParentSettingsId = newSettings.Id;
                _shopSettings.Add(service.To<ShopSettings>());
                result.Add(service.To<ShopSettings>());
            });
            return Task.FromResult(result);
        });
    }

    [Fact]
    public async Task SaveShopSettingsReturnsBadRequestWhenShopIsNullAsync()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _settingsController.SaveShopSettings(null));
        Assert.IsType<BadRequest>(result.Result);        
    }

    [Fact]
    public async Task SaveShopSettingsReturnsBadRequestWhenShopIdIsInvalidAsync()
    {
        var shopSettings = new ShopSettings
        {
            Id = 5,
            ShopId = -2,
            Type = ShopSettingType.Product,
            JsonValue = JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(new { Name = "Test" }))
        };
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _settingsController.SaveShopSettings(shopSettings));
        var badRequest = Assert.IsType<BadRequest<ShopSettings>>(result.Result);
        Assert.Equal(5, badRequest.Value.Id);
    }
    
    [Fact]
    public async Task SaveShopSettingsReturnsBadRequestWhenJsonIsEmptyAsync()
    {
        var shopSettings = new ShopSettings
        {
            Id = 5,
            ShopId = 2,
            Type = ShopSettingType.Product
        };

        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _settingsController.SaveShopSettings(shopSettings));
        var badRequest = Assert.IsType<BadRequest<ShopSettings>>(result.Result);
        Assert.Equal(5, badRequest.Value.Id);
    }

    [Fact]
    public async Task SaveShopSettingsReturnsCreatedWhenValidAsync()
    {        
        var shopSettings = new ShopSettings
        {
            Id = _shopSettings.Count + 1,
            ShopId = 2,
            Type = ShopSettingType.Product,
            JsonValue = JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(new { Name = "Test" }))
        };
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _settingsController.SaveShopSettings(shopSettings));
        var ok = Assert.IsType<Created<ShopSettings>>(result.Result);
        Assert.Equal(shopSettings.Id, ok.Value.Id);
    }

    [Fact]
    public async Task UpdateShopSettingsReturnsBadRequestWhenShopIsNullAsync()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _settingsController.UpdateShopSettings(null));
        Assert.IsType<BadRequest>(result.Result);
    }

    [Fact]
    public async Task UpdateShopSettingsReturnsBadRequestWhenShopIdIsInvalidAsync()
    {
        var shopSettings = new ShopSettings
        {
            Id = 5,
            ShopId = -2,
            Type = ShopSettingType.Product,
            JsonValue = JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(new { Name = "Test" }))
        };
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _settingsController.UpdateShopSettings(shopSettings));
        var badRequest = Assert.IsType<BadRequest<ShopSettings>>(result.Result);
        Assert.Equal(5, badRequest.Value.Id);
    }

    [Fact]
    public async Task UpdateShopSettingsReturnsBadRequestWhenJsonIsEmptyAsync()
    {
        var shopSettings = new ShopSettings
        {
            Id = 5,
            ShopId = 2,
            Type = ShopSettingType.Product
        };

        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _settingsController.UpdateShopSettings(shopSettings));
        var badRequest = Assert.IsType<BadRequest<ShopSettings>>(result.Result);
        Assert.Equal(5, badRequest.Value.Id);
    }
    
    [Fact]
    public async Task UpdateShopSettingsReturnsNotFoundWhenNotExistsAsync()
    {        
        var shopSettings = new ShopSettings
        {
            Id = 100,
            ShopId = 2,
            Type = ShopSettingType.Product,
            JsonValue = JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(new { Name = "Test" }))
        };        

        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _settingsController.UpdateShopSettings(shopSettings));
        var notFound = Assert.IsType<NotFound<ShopSettings>>(result.Result);
        Assert.Equal(shopSettings.Id, notFound.Value.Id);
    }

    [Fact]
    public async Task UpdateShopSettingsReturnsAcceptedWhenValidAsync()
    {        
        var shopSettings = new ShopSettings
        {
            Id = 3,
            ShopId = 1,
            Type = ShopSettingType.Product,
            JsonValue = JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(new { Name = "Test" }))
        };
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _settingsController.UpdateShopSettings(shopSettings));
        var ok = Assert.IsType<Accepted<ShopSettings>>(result.Result);
        Assert.Equal(shopSettings.Id, ok.Value.Id);
    }

    [Fact]
    public async Task SaveShopSettingsWithServicesReturnsBadRequestWhenSettingsListIsEmptyAsync()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _settingsController.SaveShopSettingsWithServices(null));
        Assert.IsType<BadRequest>(result.Result);
        
        result = Assert.IsAssignableFrom<INestedHttpResult>(await _settingsController.SaveShopSettingsWithServices([]));
        Assert.IsType<BadRequest>(result.Result);
    }

    [Fact]
    public async Task SaveShopSettingsWithServicesReturnsBadRequestWhenSettingsListIsInvalidAsync()
    {
        var shopSettings = new ShopSettings
        {
            Id = 5,
            ShopId = -2,
            Type = ShopSettingType.Product,
            JsonValue = JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(new { Name = "Test" }))
        };
        var arrayList = new ArrayList { shopSettings };
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _settingsController.SaveShopSettingsWithServices(arrayList));
        Assert.IsType<BadRequest<ArrayList>>(result.Result);        
    }

    [Fact]
    public async Task SaveShopSettingsWithServicesReturnsBadRequestWhenJsonIsInvalidAsync()
    {
        var shopSettings = _shopSettings[0].To<ShopSettings>();
        var arrayList = new ArrayList
        {
           JsonSerializer.Serialize(shopSettings),
           JsonSerializer.Serialize( _shopSettings.Where(s=>s.ParentSettingsId == shopSettings.Id).ToArray())
        };
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _settingsController.SaveShopSettingsWithServices(arrayList));
        Assert.IsType<BadRequest<ArrayList>>(result.Result);        
    }

    [Fact]
    public async Task SaveShopSettingsWithServicesReturnsCreatedWhenNotExistsAsync()
    {        
        var result = await GetSaveShopSettingsWithServicesResultAsync(0, _shopSettings.Count + 1);
        var created = Assert.IsType<Created<ArrayList>>(result.Result);
        Assert.Equal(2, created.Value.Count);
        Assert.IsType<ShopSettings>(created.Value[0]);
        Assert.IsAssignableFrom<IEnumerable<ShopSettings>>(created.Value[1]);
    }

    [Fact]
    public async Task SaveShopSettingsWithServicesReturnsAcceptedWhenExistsAsync()
    {        
        var result = await GetSaveShopSettingsWithServicesResultAsync(_shopSettings.Count +1, _shopSettings.Count + 2);
        var accepted = Assert.IsType<Accepted<ArrayList>>(result.Result);
        Assert.Equal(2, accepted.Value.Count);
        Assert.IsType<ShopSettings>(accepted.Value[0]);
        Assert.IsAssignableFrom<IEnumerable<ShopSettings>>(accepted.Value[1]);
    }

    private async Task<INestedHttpResult> GetSaveShopSettingsWithServicesResultAsync(int shopSettingsId, int servicesStartId)
    {
        var shopSettings = new ShopSettings
        {
            Id = shopSettingsId,
            ShopId = 5,
            Type = ShopSettingType.Product,
            JsonValue = JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(new { Name = "Test" }))
        };

        var services = new ShopSettings[]
        {
            new() {
                Id = servicesStartId,
                ShopId = shopSettings.ShopId,
                Type = ShopSettingType.Service,
                JsonValue = JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(new { Name = "TestService1" }))
            },
            new() {
                Id = servicesStartId + 1,
                ShopId = shopSettings.ShopId,
                Type = ShopSettingType.Service,
                JsonValue = JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(new { Name = "TestService2" }))
            }
        };

        var arrayList = new ArrayList
        {
           JsonSerializer.Serialize(shopSettings),
           JsonSerializer.Serialize(services)
        };

        return  Assert.IsAssignableFrom<INestedHttpResult>(await _settingsController.SaveShopSettingsWithServices(arrayList));
    }
}
