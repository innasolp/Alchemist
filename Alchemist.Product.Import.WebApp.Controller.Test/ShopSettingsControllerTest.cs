using Alchemist.DataService.Interfaces;
using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Import.WebApp.Controllers;
using Alchemist.Import.Settings.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Text.Json;

namespace Alchemist.Product.Import.WebApp.Controller.Test;

public class ShopSettingsControllerTest : ControllerTest<ShopSettingsController>
{
    private readonly ISettingsDataAdapter _settingsDataAdapter;

    private readonly Mock<IShopSettingsDataService> _shopSettingsDataServiceMock = new();
    public ShopSettingsControllerTest()
    {
        _settingsDataAdapter = new SettingsDataAdapter<ProductShopSettingsModel, CategoryShopSettingsModel, ServiceSettingsModel>(_shopSettingsDataServiceMock.Object);
    }

    private ShopSettingsController CreateShopSettingsController()
    {
        return new ShopSettingsController(_loggerMock.Object, _importFacade, _settingsDataAdapter);
    }    

    [Fact]
    public async Task SaveProductShopSettingsFieldsOnSaveShopSettingsActionAsync()
    {
        await SaveShopSettingsAsync(ShopSettingType.Product);
    }

    [Fact]
    public async Task SaveCategoryShopSettingsFieldsOnSaveShopSettingsActionAsync()
    {
        await SaveShopSettingsAsync(ShopSettingType.Category);
    }

    private async Task SaveShopSettingsAsync(ShopSettingType shopSettingType)
    {
        await SetShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;

        var shopSettings = await SetShopSettingAsync(shopGuid, shopSettingType);
        var oldShopSettings = shopSettings.GetCopy();

        var changedShopSettings = shopSettings.GetCopy();
        changedShopSettings.FillShopSettingsFields();
        var json = JsonSerializer.Serialize(changedShopSettings);

        var shopSettingController = CreateShopSettingsController();
        var actionResult = Assert.IsType<OkObjectResult>(shopSettingController.SaveShopSettings(shopSettings.ShopGuid, (int)shopSettingType, json));
        Assert.True(Assert.IsType<bool>(actionResult.Value));

        ModelAssert.EqualFields(changedShopSettings, shopSettings);
        ModelAssert.NotEqualFields(oldShopSettings, shopSettings);
    }    

    [Fact]
    public async Task ChangeShopSettingWhenSetShopSettingsActionAsync()
    {
        await SetShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;

        var shopSettingType = ShopSettingType.Category;
        var shopSettings = await SetShopSettingAsync(shopGuid, shopSettingType);

        var shopSettingController = CreateShopSettingsController();
        var actionResult = Assert.IsType<OkObjectResult>(shopSettingController.SetShopSettings(shopSettings.ShopGuid, (int)shopSettingType));
        Assert.True(Assert.IsType<bool>(actionResult.Value));
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

    private async Task SetServiceSettingsActionAsync(Func<ShopSettingsController, Guid, Guid, IActionResult> getServiceAction, string serviceName)
    {
        await SetShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        var shopSettings = await SetShopSettingAsync(shopGuid, ShopSettingType.Product);

        var shopSettingController = CreateShopSettingsController();

        var partialViewResult = Assert.IsType<PartialViewResult>(getServiceAction(shopSettingController, shopSettings.ShopGuid, shopSettings.Guid));

        Assert.Equal("~/Views/Home/ServiceSettings.cshtml", partialViewResult.ViewName);
        var serviceSettingsModel = Assert.IsType<ServiceSettingsModel>(partialViewResult.Model);
        Assert.Equal(serviceName, serviceSettingsModel.Name);
    }

    [Fact]
    public async Task SaveServiceFieldsOnSaveServiceSettingsActionAsync()
    {
        await SetShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        var shopSettings = await SetShopSettingAsync(shopGuid, ShopSettingType.Product);

        var shopSettingController = CreateShopSettingsController();       

        var oldService = shopSettings.BrowserDataLoader.GetCopy();

        var changedService = shopSettings.BrowserDataLoader.GetCopy();
        changedService.FillServiceSettingsFields();

        var actionResult = Assert.IsType<OkObjectResult>(shopSettingController.SaveServiceSettings(changedService));
        Assert.IsType<bool>(actionResult.Value);

        ModelAssert.EqualFields(changedService, shopSettings.BrowserDataLoader);
        ModelAssert.NotEqualFields(oldService, shopSettings.BrowserDataLoader);
    }

    [Fact]
    public async Task SaveProductShopSettingsToDbActionAsync()
    {
        await SetShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        var currentShopSettings = await SetShopSettingAsync(shopGuid, ShopSettingType.Product);

        await SaveShopSettingsToDbActionAsync<ProductShopSettingsModel>(currentShopSettings.ShopGuid,
            ShopSettingType.Product,
            "OzonProductSettings.json",
            (controller, shopSettings) => controller.SaveProductShopSettingsToDb(shopSettings));
    }

    [Fact]
    public async Task SaveCategoryShopSettingsToDbActionAsync()
    {
        await SetShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        var currentShopSettings = await SetShopSettingAsync(shopGuid, ShopSettingType.Category);

        await SaveShopSettingsToDbActionAsync<CategoryShopSettingsModel>(shopGuid,
            ShopSettingType.Category,
            "ozoncategories.json",
            (controller, shopSettings) => controller.SaveCategoryShopSettingsToDb(shopSettings));
    }

    private async Task SaveShopSettingsToDbActionAsync<T>(Guid shopGuid, ShopSettingType shopSettingType, string fileName, Func<ShopSettingsController, T, Task<IActionResult>> saveAction)
        where T:ShopSettingsModel,new()
    {
        Assert.True(_importFacade.TryGetShopSettings(shopGuid, shopSettingType,out var settings));
        var shopSettings = Assert.IsType<T>(settings);

        var productShopSettings = await fileName.ReadFromFileAsync<ProductShopSettingsModel>();
        shopSettings.Update(productShopSettings, true);

        _shopSettingsDataServiceMock.Setup(s => s.SaveShopSettings(It.IsAny<Interfaces.IShopSettings>(), It.IsAny<IEnumerable<Interfaces.IShopSettings>>()))
            .Returns<Interfaces.IShopSettings, IEnumerable<Interfaces.IShopSettings>>((settings, services) =>
                    SaveShopSettingsAsync<T>(settings, services, shopSettings));

        var shopSettingController = CreateShopSettingsController();
        var actionResult = Assert.IsType<OkObjectResult>(await saveAction(shopSettingController, shopSettings));
        Assert.IsType<T>(actionResult.Value);
    }

    private static async Task<List<Interfaces.IShopSettings>> SaveShopSettingsAsync<T>(Interfaces.IShopSettings shopSettingsResult, 
        IEnumerable<Interfaces.IShopSettings> childrenSettingResult,
        ShopSettingsModel expect)
        where T:ShopSettingsModel,new()
    {
        var result = shopSettingsResult.ToShopImportSettings<T>();
        var servicesResult = new List<ServiceSettingsModel>();
        childrenSettingResult.ToList().ForEach(s => servicesResult.Add(s.ToImportServiceSettings<ServiceSettingsModel>()));
        servicesResult.ForEach(result.SetServiceSettings);

        ModelAssert.EqualFields(expect, result);
        ModelAssert.EqualServices(expect, result);
        ModelAssert.EqualCollections(expect.Services, servicesResult);        

        return await Task.FromResult(new List<Interfaces.IShopSettings>());
    }

    private async Task IsServiceActionWithRandomShopGuidNotFoundAsync(Func<ShopSettingsController, Guid, Guid,IActionResult > getServiceSettingsAction)
    {
        await SetShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        var shopSettings = await SetShopSettingAsync(shopGuid, ShopSettingType.Product);

        var shopSettingController = CreateShopSettingsController();

        var newShopGuid = Guid.NewGuid();
        
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(getServiceSettingsAction(shopSettingController, newShopGuid, shopSettings.Guid));

        var guid = Assert.IsType<Guid>(notFoundResult.Value);
        Assert.Equal(newShopGuid, guid);
    }

    private async Task IsServiceActionWithRandomShopSettingsGuidNotFoundAsync(Func<ShopSettingsController, Guid, Guid, IActionResult> getServiceSettingsAction)
    {
        await SetShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        var shopSettings = await SetShopSettingAsync(shopGuid, ShopSettingType.Product);
        
        var shopSettingController = CreateShopSettingsController();

        var shopSettingsGuid = Guid.NewGuid();

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(getServiceSettingsAction(shopSettingController, shopSettings.ShopGuid, shopSettingsGuid));

        var guid = Assert.IsType<Guid>(notFoundResult.Value);
        Assert.Equal(shopSettingsGuid, guid);
    }


    [Fact]
    public async Task ImportServiceActionWithRandomShopGuidIsNotFoundAsync()
    {
        await IsServiceActionWithRandomShopGuidNotFoundAsync((controller, shopGuid, shopSettingsGuid) => controller.ImportServiceSettings(shopGuid, shopSettingsGuid));
    }

    [Fact]
    public async Task ImportServiceActionWithRandomShopSettingsGuidIsNotFoundAsync()
    {
        await IsServiceActionWithRandomShopSettingsGuidNotFoundAsync((controller, shopGuid, shopSettingsGuid) => controller.ImportServiceSettings(shopGuid, shopSettingsGuid));
    }

    [Fact]
    public async Task BrowserDataLoaderActionWithRandomShopGuidIsNotFoundAsync()
    {
        await IsServiceActionWithRandomShopGuidNotFoundAsync((controller, shopGuid, shopSettingsGuid) => controller.BrowserDataLoaderSettings(shopGuid, shopSettingsGuid));
    }

    [Fact]
    public async Task BrowserDataLoaderActionWithRandomShopSettingsGuidIsNotFoundAsync()
    {
        await IsServiceActionWithRandomShopSettingsGuidNotFoundAsync((controller, shopGuid, shopSettingsGuid) => controller.BrowserDataLoaderSettings(shopGuid, shopSettingsGuid));
    }
    
    [Fact]
    public async Task WebLoaderActionWithRandomShopGuidIsNotFoundAsync()
    {
        await IsServiceActionWithRandomShopGuidNotFoundAsync((controller, shopGuid, shopSettingsGuid) => controller.WebLoaderSettings(shopGuid, shopSettingsGuid));
    }

    [Fact]
    public async Task WebLoaderActionWithRandomShopSettingsGuidIsNotFoundAsync()
    {
        await IsServiceActionWithRandomShopSettingsGuidNotFoundAsync((controller, shopGuid, shopSettingsGuid) => controller.WebLoaderSettings(shopGuid, shopSettingsGuid));
    }
}
