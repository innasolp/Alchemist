using Alchemist.DataService.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using Alchemist.Settings.RestAPI.Controllers;
using Message.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;

namespace Alchemist.Settings.RestAPI.UnitTest;

public class SettingsControllerGetTest
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

    public SettingsControllerGetTest()
    {
        _settingsController = new SettingsController(_logger, _settingsRepository.Object, _messageSender.Object);

        _settingsRepository.Setup(s => s.GetShopSettings(It.IsAny<int>(), It.IsAny<ShopSettingType>()))
            .Returns((int shopId, ShopSettingType shopSettingType) =>
            {
                return Task.FromResult((IShopSettings)_shopSettings.FirstOrDefault(s => s.ShopId == shopId && s.Type == shopSettingType));
            });
        
        _settingsRepository.Setup(s => s.GetShopSettings(It.IsAny<int>()))
            .Returns((int id) =>
            {
                return Task.FromResult((IShopSettings)_shopSettings.FirstOrDefault(s => s.Id == id));
            });

        _settingsRepository.Setup(s => s.GetChildSettings(It.IsAny<int>()))
            .Returns((int parentSettingsId) =>
            {
                return Task.FromResult(_shopSettings.Where(s => s.ParentSettingsId == parentSettingsId).OfType<IShopSettings>().ToList());
            });
    }

    [Fact]
    public async Task GetShopSettingsByShopIdReturnsBadRequestWhenShopIdIsInvalidAsync()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _settingsController.GetShopSettingsByShopId(0, 1));
        var badRequest = Assert.IsType<BadRequest<int>>(result.Result);
        Assert.Equal(0, badRequest.Value);
    }
    
    [Fact]
    public async Task GetShopSettingsByShopIdReturnsNotFoundWhenShopIdNotExistsAsync()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _settingsController.GetShopSettingsByShopId(4, (int)ShopSettingType.Product));
        var notFound = Assert.IsType<NotFound<int>>(result.Result);
        Assert.Equal(4, notFound.Value);
    }

    [Fact]
    public async Task GetShopSettingsByShopIdReturnsOkWhenShopIdExistsAsync()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _settingsController.GetShopSettingsByShopId(2, (int)ShopSettingType.Product));
        var ok = Assert.IsType<Ok<ShopSettings>>(result.Result);
        Assert.Equal(2, ok.Value.Id);
    }
    
    [Fact]
    public async Task GetShopSettingsByIdReturnsBadRequestWhenShopIdIsInvalidAsync()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _settingsController.GetShopSettingsById(-2));
        var badRequest = Assert.IsType<BadRequest<int>>(result.Result);
        Assert.Equal(-2, badRequest.Value);
    }
    
    [Fact]
    public async Task GetShopSettingsByIdReturnsNotFoundWhenShopIdNotExistsAsync()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _settingsController.GetShopSettingsById(15));
        var notFound = Assert.IsType<NotFound<int>>(result.Result);
        Assert.Equal(15, notFound.Value);
    }

    [Fact]
    public async Task GetShopSettingsByIdReturnsOkWhenShopIdExistsAsync()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _settingsController.GetShopSettingsById(4));
        var ok = Assert.IsType<Ok<ShopSettings>>(result.Result);
        Assert.Equal(2, ok.Value.ShopId);
        Assert.Equal(ShopSettingType.Category, ok.Value.Type);
    }

    [Fact]
    public async Task GetChildSettingsByIdReturnsBadRequestWhenParentIdIsInvalidAsync()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _settingsController.GetChildSettings(-2));
        var badRequest = Assert.IsType<BadRequest<int>>(result.Result);
        Assert.Equal(-2, badRequest.Value);
    }    

    [Fact]
    public async Task GetChildSettingsByIdReturnsOkWhenShopIdExistsAsync()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _settingsController.GetChildSettings(3));
        var ok = Assert.IsType<Ok<List<ShopSettings>>>(result.Result);
        Assert.Equal(2, ok.Value.Count);
        Assert.Contains(ok.Value, s => s.Id == 9);
        Assert.Contains(ok.Value, s => s.Id == 10);
    }
    
    [Fact]
    public async Task GetChildSettingsByIdReturnsOkWithEmptyLiustWhenParentHasntChildrenAsync()
    {
        var result = Assert.IsAssignableFrom<INestedHttpResult>(await _settingsController.GetChildSettings(13));
        var ok = Assert.IsType<Ok<List<ShopSettings>>>(result.Result);
        Assert.Empty(ok.Value);
    }
}