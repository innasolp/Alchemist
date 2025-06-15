using Alchemist.DataService.Interfaces;
using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Import.Settings.JsonAdapter;
using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.WebApp.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Reflection;

namespace Alchemist.Product.Import.WebApp.Controller.Test;

public class ServiceSettingsControllerTest:ControllerTest<ServiceSettingsController>
{
    private readonly ISettingsDataAdapter _settingsDataAdapter;

    private readonly Mock<IShopSettingsDataService> _shopSettingsDataServiceMock = new();

    private readonly ISettingsAdapter _jsonAdapter;

    public ServiceSettingsControllerTest()
    {
        _settingsDataAdapter = new SettingsDataAdapter<ProductShopSettingsModel, CategoryShopSettingsModel, ServiceSettingsModel>(_shopSettingsDataServiceMock.Object);

        var builder = new HostApplicationBuilder();
        builder.Services.AddSettingsJsonAdapter<ProductShopSettingsModel, CategoryShopSettingsModel>(
            $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Content/OzonProductSettings.json",
            $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Content/ozoncategories.json");
        var host = builder.Build();

        _jsonAdapter = host.Services.GetRequiredService<ISettingsAdapter>();
    }

    private ServiceSettingsController CreateServiceSettingsController()
    {
        return new ServiceSettingsController(_importFacade);
    }
    [Fact]
    public async Task SetServiceSettingsBadRequestWhenShopGuidIsEmpty()
    {
        var controller = CreateServiceSettingsController();
        var actionResult = Assert.IsType<BadRequestObjectResult>(controller.ImportServiceSettings(Guid.Empty, Guid.NewGuid()));
        Assert.Equal("shopGuid", Assert.IsType<string>(actionResult.Value));
    }

    [Fact]
    public async Task SetServiceSettingsBadRequestWhenShopSettingsGuidIsEmpty()
    {
        var controller = CreateServiceSettingsController();
        var actionResult = Assert.IsType<BadRequestObjectResult>(controller.ImportServiceSettings(Guid.NewGuid(), Guid.Empty));
        Assert.Equal("shopSettingsGuid", Assert.IsType<string>(actionResult.Value));
    }

    [Fact]
    public async Task SetServiceSettingsIsNotFoundWhenNotExistingShop()
    {
        var controller = CreateServiceSettingsController();
        var shopGuid = Guid.NewGuid();
        var actionResult = Assert.IsType<NotFoundObjectResult>(controller.ImportServiceSettings(shopGuid, Guid.NewGuid()));
        Assert.Equal(shopGuid, Assert.IsType<Guid>(actionResult.Value));
    }

    [Fact]
    public async Task SetServiceSettingsIsNotFoundWhenNotExistingShopSettings()
    {
        await SetShopsAsync();
        var controller = CreateServiceSettingsController();
        var shopSettingsGuid = Guid.NewGuid();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        var actionResult = Assert.IsType<NotFoundObjectResult>(controller.ImportServiceSettings(shopGuid, shopSettingsGuid));
        Assert.Equal(shopSettingsGuid, Assert.IsType<Guid>(actionResult.Value));
    }

    [Fact]
    public async Task SetShopSettingsWebLoaderActionAsync()
    {
        await SetServiceSettingsActionAsync((shopSettingController, shopGuid, shopSettingsGuid) => shopSettingController.WebLoaderSettings(shopGuid, shopSettingsGuid),
           nameof(ShopSettingsModel.WebLoader));
    }

    [Fact]
    public async Task SetShopSettingsImportServiceActionAsync()
    {
        await SetServiceSettingsActionAsync((shopSettingController, shopGuid, shopSettingsGuid) => shopSettingController.ImportServiceSettings(shopGuid, shopSettingsGuid),
           nameof(ShopSettingsModel.ImportService));
    }

    [Fact]
    public async Task SetShopSettingsBrowserDataLoaderActionAsync()
    {
        await SetServiceSettingsActionAsync((shopSettingController, shopGuid, shopSettingsGuid) => shopSettingController.BrowserDataLoaderSettings(shopGuid, shopSettingsGuid),
           nameof(ShopSettingsModel.BrowserDataLoader));
    }

    [Fact]
    public async Task SetSecondaryServiceSettings()
    {
        var guid = Guid.NewGuid();
        await SetServiceSettingsActionAsync((shopSettingController, shopGuid, shopSettingsGuid) => shopSettingController.ServiceSettings(shopGuid, shopSettingsGuid, guid),
          "", guid);
    }

    private async Task SetServiceSettingsActionAsync(Func<ServiceSettingsController, Guid, Guid, IActionResult> getServiceAction, string? serviceName, Guid? guid = null)
    {
        await SetShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        var shopSettings = await SetShopSettingAsync(shopGuid, ShopSettingType.Product);

        var controller = CreateServiceSettingsController();

        var partialViewResult = Assert.IsType<PartialViewResult>(getServiceAction(controller, shopSettings.ShopGuid, shopSettings.Guid));

        Assert.Equal("~/Views/Home/ServiceSettings.cshtml", partialViewResult.ViewName);
        var serviceSettingsModel = Assert.IsType<ServiceSettingsModel>(partialViewResult.Model);
        Assert.Equal(serviceName, serviceSettingsModel.Name);
    }

    [Fact]
    public async Task SaveServiceSettingsBadRequestWhenDataIsNull()
    {
        var controller = CreateServiceSettingsController();
        Assert.IsType<BadRequestObjectResult>(controller.SaveServiceSettings(null));
    }

    [Fact]
    public async Task SaveServiceSettingsNotFoundWhenDataShopIsNotExists()
    {
        var controller = CreateServiceSettingsController();
        var guid = Guid.NewGuid();
        var actionResult = Assert.IsType<NotFoundObjectResult>(controller.SaveServiceSettings(new ServiceSettingsModel { ShopGuid = guid }));
        Assert.Equal(guid, Assert.IsType<Guid>(actionResult.Value));
    }

    [Fact]
    public async Task SaveServiceFieldsOnSaveServiceSettingsActionAsync()
    {
        await SetShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        var shopSettings = await SetShopSettingAsync(shopGuid, ShopSettingType.Product);

        var controller = CreateServiceSettingsController();

        var oldService = shopSettings.BrowserDataLoader.GetCopy();

        var changedService = shopSettings.BrowserDataLoader.GetCopy();
        changedService.FillServiceSettingsFields();

        var actionResult = Assert.IsType<OkObjectResult>(controller.SaveServiceSettings(changedService));
        var resultService = Assert.IsType<ServiceSettingsModel>(actionResult.Value);

        ModelAssert.EqualFields(changedService, resultService);
        ModelAssert.NotEqualFields(oldService, resultService);
    }

    [Fact]
    public async Task IsServiceSettingsChangedBadRequestWhenDataIsNull()
    {
        var controller = CreateServiceSettingsController();
        Assert.IsType<BadRequestObjectResult>(controller.IsServiceSettingsChanged(null));
    }

    [Fact]
    public async Task IsServiceSettingsChangedNotFoundWhenDataShopIsNotExists()
    {
        var controller = CreateServiceSettingsController();
        var guid = Guid.NewGuid();
        var actionResult = Assert.IsType<NotFoundObjectResult>(controller.IsServiceSettingsChanged(new ServiceSettingsModel { ShopGuid = guid }));
        Assert.Equal(guid, Assert.IsType<Guid>(actionResult.Value));
    }

    [Fact]
    public async Task IsServiceSettingsChangedOkTrueWhenOrigignalSettingsEmpty()
    {
        await SetShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        var shopSettings = await SetShopSettingAsync(shopGuid, ShopSettingType.Category);
        shopSettings.ImportService = null;
        var data = new ServiceSettingsModel
        {
            ShopGuid = shopGuid,
            ShopSettingsGuid = shopSettings.Guid,
            Name = nameof(ShopSettingsModel.ImportService),
            ServiceTypeName = Guid.NewGuid().ToString()
        };

        var controller = CreateServiceSettingsController();
        var actionResult = Assert.IsType<OkObjectResult>(controller.IsServiceSettingsChanged(data));
        Assert.True(Assert.IsType<bool>(actionResult.Value));
    }

    [Fact]
    public async Task IsServiceSettingsChangedOkFalseWhenOrigignalSettingsEmptyAndSettingsNotChanged()
    {
        await SetShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        var shopSettings = await SetShopSettingAsync(shopGuid, ShopSettingType.Category);
        shopSettings.ImportService = null;
        var data = new ServiceSettingsModel
        {
            ShopGuid = shopGuid,
            ShopSettingsGuid = shopSettings.Guid,
            Name = nameof(ShopSettingsModel.ImportService)
        };

        var controller = CreateServiceSettingsController();
        var actionResult = Assert.IsType<OkObjectResult>(controller.IsServiceSettingsChanged(data));
        Assert.False(Assert.IsType<bool>(actionResult.Value));
    }

    [Fact]
    public async Task IsServiceSettingsChangedOkTrueWhenSettingsChanged()
    {
        await SetShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        var shopSettings = await SetShopSettingAsync(shopGuid, ShopSettingType.Category);
        var data = shopSettings.ImportService.GetCopy();
        data.ServiceTypeName = Guid.NewGuid().ToString();

        var controller = CreateServiceSettingsController();
        var actionResult = Assert.IsType<OkObjectResult>(controller.IsServiceSettingsChanged(data));
        Assert.True(Assert.IsType<bool>(actionResult.Value));
    }

    [Fact]
    public async Task IsServiceSettingsChangedOkFalseWhenSettingsNotChanged()
    {
        await SetShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        var shopSettings = await SetShopSettingAsync(shopGuid, ShopSettingType.Category);
        var data = shopSettings.ImportService.GetCopy();

        var controller = CreateServiceSettingsController();
        var actionResult = Assert.IsType<OkObjectResult>(controller.IsServiceSettingsChanged(data));
        Assert.False(Assert.IsType<bool>(actionResult.Value));
    }
}
