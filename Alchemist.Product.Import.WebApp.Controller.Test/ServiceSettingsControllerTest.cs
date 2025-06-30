using Alchemist.DataService.Interfaces;
using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Import.Settings.JsonAdapter;
using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.WebApp.Controllers;
using Alchemist.Product.Import.WebApp.Models;
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
        await LoadShopsAsync();
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
        await LoadShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;

        Assert.True(_importFacade.TryGetShopSettings(shopGuid, ShopSettingType.Product, out var shopSettings));
        FillShopSettingsFields(shopSettings);

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
        var shopGuid = Guid.NewGuid();
        var actionResult = Assert.IsType<NotFoundObjectResult>(controller.SaveServiceSettings(new ServiceSettingsModel(0,
            0,
            0,
            Guid.NewGuid(),
            shopGuid,
            Guid.NewGuid().ToString())));
            
        Assert.Equal(shopGuid, Assert.IsType<Guid>(actionResult.Value));
    }

    [Fact]
    public async Task SaveServiceFieldsOnSaveServiceSettingsActionAsync()
    {
        await LoadShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;

        Assert.True(_importFacade.TryGetShopSettings(shopGuid, ShopSettingType.Product, out var shopSettings));
        FillShopSettingsFields(shopSettings);

        var controller = CreateServiceSettingsController();

        var oldService =  ModelFactoryMock.Object.GetCopy(shopSettings.BrowserDataLoader);

        var changedService = ModelFactoryMock.Object.GetCopy(shopSettings.BrowserDataLoader);
        changedService.FillServiceSettingsFields();

        var actionResult = Assert.IsType<OkObjectResult>(controller.SaveServiceSettings(changedService as ServiceSettingsModel));
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
        var shopGuid = Guid.NewGuid();
        var actionResult = Assert.IsType<NotFoundObjectResult>(controller.IsServiceSettingsChanged(new ServiceSettingsModel(0,
            0,
            0,
            Guid.NewGuid(),
            shopGuid,
            Guid.NewGuid().ToString())));
        Assert.Equal(shopGuid, Assert.IsType<Guid>(actionResult.Value));
    }

    [Fact]
    public async Task IsServiceSettingsChangedOkTrueWhenOrigignalSettingsEmpty()
    {
        await LoadShopsAsync();
        var shop = _importFacade.GetShops().Last();

        Assert.True(_importFacade.TryGetShopSettings(shop.ShopGuid, ShopSettingType.Category, out var shopSettings));
        
        var data = new ServiceSettingsModel(
            shop.Shop.Id,
            0,
            shopSettings.Id,
            shopSettings.Guid,
            shopSettings.ShopGuid,
            nameof(ShopSettingsModel.ImportService))
        {
            ServiceTypeName = Guid.NewGuid().ToString()
        };

        var controller = CreateServiceSettingsController();
        var actionResult = Assert.IsType<OkObjectResult>(controller.IsServiceSettingsChanged(data));
        Assert.True(Assert.IsType<bool>(actionResult.Value));
    }

    [Fact]
    public async Task IsServiceSettingsChangedOkFalseWhenOrigignalSettingsEmptyAndSettingsNotChanged()
    {
        await LoadShopsAsync();
        var shop = _importFacade.GetShops().Last().Shop;

        Assert.True(_importFacade.TryGetShopSettings(shop.Guid, ShopSettingType.Category, out var shopSettings));        
        
        var data = new ServiceSettingsModel(shop.Id,
            0,
            0,
            shopSettings.Guid,
            shop.Guid,
            nameof(ShopSettingsModel.ImportService));

        var controller = CreateServiceSettingsController();
        var actionResult = Assert.IsType<OkObjectResult>(controller.IsServiceSettingsChanged(data));
        Assert.False(Assert.IsType<bool>(actionResult.Value));
    }

    [Fact]
    public async Task IsServiceSettingsChangedOkTrueWhenSettingsChanged()
    {
        await LoadShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;

        Assert.True(_importFacade.TryGetShopSettings(shopGuid, ShopSettingType.Category, out var shopSettings));
        FillShopSettingsFields(shopSettings);

        var data = ModelFactoryMock.Object.GetCopy(shopSettings.ImportService);
        data.ServiceTypeName = Guid.NewGuid().ToString();

        var controller = CreateServiceSettingsController();
        var actionResult = Assert.IsType<OkObjectResult>(controller.IsServiceSettingsChanged(data as ServiceSettingsModel));
        Assert.True(Assert.IsType<bool>(actionResult.Value));
    }

    [Fact]
    public async Task IsServiceSettingsChangedOkFalseWhenSettingsNotChanged()
    {
        await LoadShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;

        Assert.True(_importFacade.TryGetShopSettings(shopGuid, ShopSettingType.Product, out var shopSettings));
        FillShopSettingsFields(shopSettings);
        
        var data = ModelFactoryMock.Object.GetCopy(shopSettings.ImportService);

        var controller = CreateServiceSettingsController();
        var actionResult = Assert.IsType<OkObjectResult>(controller.IsServiceSettingsChanged(data as ServiceSettingsModel));
        Assert.False(Assert.IsType<bool>(actionResult.Value));
    }
}
