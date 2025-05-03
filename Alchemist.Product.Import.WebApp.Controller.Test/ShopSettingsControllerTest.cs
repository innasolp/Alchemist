using Alchemist.DataService.Interfaces;
using Alchemist.Import.Settings.Adapter;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Import.WebApp.Controllers;
using Alchemist.Product.Interfaces;
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
        var indexViewModel = await GetIndexActionViewModelAfterUpdateShopsAsync();

        var shopSettingController = CreateShopSettingsController();

        var currentShopSettings = Assert.IsType<ProductShopSettingsModel>(indexViewModel.SelectedShopImport.GetSettings(indexViewModel.SelectedTab, true));

        var oldShopSettings = currentShopSettings.GetCopy();

        var changedShopSettings = currentShopSettings.GetCopy();
        changedShopSettings.FillShopSettingsFields();
        var json = JsonSerializer.Serialize(changedShopSettings);

        var actionResult = Assert.IsType<OkObjectResult>(shopSettingController.SaveShopSettings(currentShopSettings.ShopGuid, (int)ShopSettingType.Product, json));
        Assert.True(Assert.IsType<bool>(actionResult.Value));        

        ModelAssert.EqualFields(changedShopSettings, currentShopSettings);
        ModelAssert.NotEqualFields(oldShopSettings, currentShopSettings);
    }

    [Fact]
    public async Task SaveCategoryShopSettingsFieldsOnSaveShopSettingsActionAsync()
    {
        var homeController = CreateHomeController();
        var shopSettingController = CreateShopSettingsController();
        
        var categorySettings = await CommonActions.ChangeShopSettingsAsync<CategoryShopSettingsModel>(homeController, shopSettingController, ShopSettingType.Category);
        var oldShopSettings = categorySettings.GetCopy();

        var changedShopSettings = categorySettings.GetCopy();
        changedShopSettings.FillShopSettingsFields();
        var json = JsonSerializer.Serialize(changedShopSettings);

        var actionResult = Assert.IsType<OkObjectResult>(shopSettingController.SaveShopSettings(categorySettings.ShopGuid, (int)ShopSettingType.Category, json));
        Assert.True(Assert.IsType<bool>(actionResult.Value));

        ModelAssert.EqualFields(changedShopSettings, categorySettings);
        ModelAssert.NotEqualFields(oldShopSettings, categorySettings);
    }

    [Fact]
    public async Task ChangeShopSettingWhenSetShopSettingsActionAsync()
    {         
        await CommonActions.ChangeShopSettingsAsync<CategoryShopSettingsModel>(CreateHomeController(),
            CreateShopSettingsController(), 
            ShopSettingType.Category);
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
        var indexViewModel = await GetIndexActionViewModelAfterUpdateShopsAsync();

        var shopSettings = indexViewModel.SelectedShopImport.GetSettings(indexViewModel.SelectedTab, true);

        var shopSettingController = CreateShopSettingsController();

        var partialViewResult = Assert.IsType<PartialViewResult>(getServiceAction(shopSettingController, shopSettings.ShopGuid, shopSettings.Guid));

        Assert.Equal("~/Views/Home/ServiceSettings.cshtml", partialViewResult.ViewName);
        var serviceSettingsModel = Assert.IsType<ServiceSettingsModel>(partialViewResult.Model);
        Assert.Equal(serviceName, serviceSettingsModel.Name);
    }

    [Fact]
    public async Task SaveServiceFieldsOnSaveServiceSettingsActionAsync()
    {
        var indexViewModel = await GetIndexActionViewModelAfterUpdateShopsAsync();

        var shopSettingController = CreateShopSettingsController();

        var shopSettings = Assert.IsType<ProductShopSettingsModel>(indexViewModel.SelectedShopImport.GetSettings(indexViewModel.SelectedTab, true));
        shopSettings.SetServiceSettings(shopSettings.CreateServiceSettingsModel(nameof(ShopSettingsModel.BrowserDataLoader)));

        var oldService = shopSettings.BrowserDataLoader.GetCopy();

        var changedService = shopSettings.BrowserDataLoader.GetCopy();
        changedService.FillServiceSettingsFields();

        var actionResult = Assert.IsType<OkObjectResult>(shopSettingController.SaveServiceSettings(changedService));
        var result = Assert.IsType<bool>(actionResult.Value);

        ModelAssert.EqualFields(changedService, shopSettings.BrowserDataLoader);
        ModelAssert.NotEqualFields(oldService, shopSettings.BrowserDataLoader);
    }

    [Fact]
    public async Task SaveProductShopSettingsToDbActionAsync()
    {
        var indexViewModel = await GetIndexActionViewModelAfterUpdateShopsAsync();

        await SaveShopSettingsToDbActionAsync<ProductShopSettingsModel>(indexViewModel.SelectedShopImport.ShopGuid,
            ShopSettingType.Product,
            "OzonProductSettings.json",
            (controller, shopSettings) => controller.SaveProductShopSettingsToDb(shopSettings));
    }

    [Fact]
    public async Task SaveCategoryShopSettingsToDbActionAsync()
    {
        var homeController = CreateHomeController();
        var indexViewModel = await CommonActions.GetIndexActionViewModelAfterUpdateShopsAsync(homeController);

        var shopSettingController = CreateShopSettingsController();
        shopSettingController.SetShopSettings(indexViewModel.SelectedShopImport.ShopGuid, (int)ShopSettingType.Category);
        await homeController.IndexFromQueryAsync(indexViewModel.SelectedShopImport.ShopGuid, (int)indexViewModel.SelectedTab);

        await SaveShopSettingsToDbActionAsync<CategoryShopSettingsModel>(indexViewModel.SelectedShopImport.ShopGuid,
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

        _shopSettingsDataServiceMock.Setup(s => s.SaveShopSettings(It.IsAny<IShopSettings>(), It.IsAny<IEnumerable<IShopSettings>>()))
            .Returns<IShopSettings, IEnumerable<IShopSettings>>((settings, services) =>
                    SaveShopSettingsAsync<T>(settings, services, shopSettings));
        var shopSettingController = CreateShopSettingsController();
        var actionResult = Assert.IsType<OkObjectResult>(await saveAction(shopSettingController, shopSettings));
        Assert.IsType<T>(actionResult.Value);
    }

    private static async Task<List<IShopSettings>> SaveShopSettingsAsync<T>(IShopSettings shopSettingsResult, IEnumerable<IShopSettings> childrenSettingResult,
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

        return await Task.FromResult(new List<IShopSettings>());
    }

    private async Task IsServiceActionWithRandomShopGuidNotFoundAsync(Func<ShopSettingsController, Guid, Guid,IActionResult > getServiceSettingsAction)
    {
        var indexViewModel = await GetIndexActionViewModelAfterUpdateShopsAsync();

        var shopSettings = indexViewModel.SelectedShopImport.GetSettings(indexViewModel.SelectedTab, true);

        var shopSettingController = CreateShopSettingsController();

        var shopGuid = Guid.NewGuid();
        
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(getServiceSettingsAction(shopSettingController, shopGuid, shopSettings.Guid));

        var guid = Assert.IsType<Guid>(notFoundResult.Value);
        Assert.Equal(shopGuid, guid);
    }

    private async Task IsServiceActionWithRandomShopSettingsGuidNotFoundAsync(Func<ShopSettingsController, Guid, Guid, IActionResult> getServiceSettingsAction)
    {
        var indexViewModel = await GetIndexActionViewModelAfterUpdateShopsAsync();

        var shopSettings = indexViewModel.SelectedShopImport.GetSettings(indexViewModel.SelectedTab, true);

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
