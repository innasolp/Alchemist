using Alchemist.DataService.Interfaces;
using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Import.Settings.JsonAdapter;
using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Import.WebApp.Controllers;
using Alchemist.Product.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Reflection;
using System.Text.Json;
using ShopSettingType = Alchemist.Import.Settings.Interfaces.ShopSettingType;

namespace Alchemist.Product.Import.WebApp.Controller.Test;

public class ShopSettingsControllerTest : ControllerTest<ShopSettingsController>
{ 
    private readonly ISettingsDataAdapter _settingsDataAdapter;

    private readonly Mock<IShopSettingsDataService> _shopSettingsDataServiceMock = new();

    private readonly ISettingsAdapter _jsonAdapter;

    public ShopSettingsControllerTest()
    {
        _settingsDataAdapter = new SettingsDataAdapter<ProductShopSettingsModel, CategoryShopSettingsModel, ServiceSettingsModel>(_shopSettingsDataServiceMock.Object);

        var builder = new HostApplicationBuilder();
        builder.Services.AddSettingsJsonAdapter<ProductShopSettingsModel, CategoryShopSettingsModel>(
            $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Content/OzonProductSettings.json",
            $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Content/ozoncategories.json");
        var host = builder.Build();

        _jsonAdapter = host.Services.GetRequiredService<ISettingsAdapter>();
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
    public async Task SaveShopSettingsIsBadRequestWhenJsonIsEmpty()
    {
        var shopSettingController = CreateShopSettingsController();
        var actionResult = Assert.IsType<BadRequestObjectResult>(shopSettingController.SaveShopSettings(Guid.NewGuid(), (int)ShopSettingType.Product, string.Empty));
        Assert.Equal(string.Empty, Assert.IsType<string>(actionResult.Value));
    }

    [Fact]
    public async Task SaveShopSettingsIsBadRequestWhenInvalidJson()
    {
        var shopSettingController = CreateShopSettingsController();
        var json = Guid.NewGuid().ToString();
        var actionResult = Assert.IsType<BadRequestObjectResult>(shopSettingController.SaveShopSettings(Guid.NewGuid(), (int)ShopSettingType.Product, json));
        Assert.Equal(json, Assert.IsType<string>(actionResult.Value));
    }

    [Fact]
    public async Task SaveShopSettingsIsNotFoundWhenNotExistingShop()
    {
        await SetShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        var shopSettings = await SetShopSettingAsync(shopGuid, ShopSettingType.Product);

        var shopSettingController = CreateShopSettingsController();
        var json = JsonSerializer.Serialize(shopSettings);
        var guid = Guid.NewGuid();
        var actionResult = Assert.IsType<NotFoundObjectResult>(shopSettingController.SaveShopSettings(guid, (int)ShopSettingType.Product, json));
        Assert.Equal(guid, Assert.IsType<Guid>(actionResult.Value));
    }


    [Fact]
    public async Task ShopSettingViewResultSuccessWhenDataIsValid()
    {
        await SetShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;

        var shopSettingType = ShopSettingType.Category;
        var shopSettings = await SetShopSettingAsync(shopGuid, shopSettingType);

        var shopSettingController = CreateShopSettingsController();
        var actionResult = Assert.IsType<PartialViewResult>(await shopSettingController.ShopSettings(shopSettings.ShopGuid, (int)shopSettingType));
        ModelAssert.EqualFields(shopSettings, Assert.IsAssignableFrom<ShopSettingsModel>(actionResult.Model));
    }

    [Fact]
    public async Task ShopSettingsIsBadRequestWhenService()
    {
        var shopSettingController = CreateShopSettingsController();
        var actionResult = Assert.IsType<BadRequestObjectResult>(await shopSettingController.ShopSettings(Guid.NewGuid(), (int)ShopSettingType.Service));
        Assert.Equal(ShopSettingType.Service, Assert.IsType<ShopSettingType>(actionResult.Value));
    }


    [Fact]
    public async Task ShopSettingsIsNotFoundWhenNotExistingShop()
    {
        var shopSettingController = CreateShopSettingsController();
        var guid = Guid.NewGuid();
        var actionResult = Assert.IsType<NotFoundObjectResult>(await shopSettingController.ShopSettings(guid, (int)ShopSettingType.Category));
        Assert.Equal(guid, Assert.IsType<Guid>(actionResult.Value));
    }

    [Fact]
    public async Task SaveProductShopSettingsToDbBadRequestWhenDataIsNull()
    {
        var shopSettingController = CreateShopSettingsController();
        var actionResult = Assert.IsType<BadRequestObjectResult>(await shopSettingController.SaveProductShopSettingsToDb(null));
        Assert.Equal("productShopSettings", Assert.IsType<string>(actionResult.Value));
    }
    
    [Fact]
    public async Task SaveCategoryShopSettingsToDbBadRequestWhenDataIsNull()
    {
        var shopSettingController = CreateShopSettingsController();
        var actionResult = Assert.IsType<BadRequestObjectResult>(await shopSettingController.SaveCategoryShopSettingsToDb(null));
        Assert.Equal("categoryShopSettings", Assert.IsType<string>(actionResult.Value));
    }

    [Fact]
    public async Task SaveProductShopSettingsToDbNotFoundWhenNotExistingShop()
    {
        var shopSettingController = CreateShopSettingsController();
        var data = new ProductShopSettingsModel() { ShopGuid = Guid.NewGuid() };
        var actionResult = Assert.IsType<NotFoundObjectResult>(await shopSettingController.SaveProductShopSettingsToDb(data));
        Assert.Equal(data.ShopGuid, Assert.IsType<Guid>(actionResult.Value));
    }


    [Fact]
    public async Task SaveCategoryShopSettingsToDbNotFoundWhenNotExistingShop()
    {
        var shopSettingController = CreateShopSettingsController();
        var data = new CategoryShopSettingsModel() { ShopGuid = Guid.NewGuid() };
        var actionResult = Assert.IsType<NotFoundObjectResult>(await shopSettingController.SaveCategoryShopSettingsToDb(data));
        Assert.Equal(data.ShopGuid, Assert.IsType<Guid>(actionResult.Value));
    }

    private async Task SaveShopSettingsToDbInternalServerErrorWhenSettingsDataAdapterSaveThrowsExceptionAsync<T>(Func<ShopSettingsController, T, Task<IActionResult>> saveToDbAction,
        ShopSettingType shopSettingType)
        where T:ShopSettingsModel
    {
        await SetShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        var shopSettings = await SetShopSettingAsync(shopGuid, shopSettingType);
        var exception = new InvalidOperationException("testError");
        _shopSettingsDataServiceMock.Setup(s => s.SaveShopSettings(It.IsAny<IShopSettings>(), It.IsAny<IEnumerable<IShopSettings>>())).Throws(exception);

        var shopSettingController = CreateShopSettingsController();
        var data = shopSettings.GetCopy() as T;
        var actionResult = Assert.IsType<ObjectResult>(await saveToDbAction(shopSettingController, data));
        Assert.Equal(StatusCodes.Status500InternalServerError, actionResult.StatusCode);
        Assert.Equal(exception, Assert.IsType<InvalidOperationException>(actionResult.Value));
    }


    [Fact]
    public async Task SaveCategoryShopSettingsToDbInternalServerErrorWhenSettingsDataAdapterSaveThrowsException()
    {
        await SaveShopSettingsToDbInternalServerErrorWhenSettingsDataAdapterSaveThrowsExceptionAsync<CategoryShopSettingsModel>(
            async (c, data)=>await c.SaveCategoryShopSettingsToDb(data), ShopSettingType.Category);
    }

    [Fact]
    public async Task SaveProductShopSettingsToDbInternalServerErrorWhenSettingsDataAdapterSaveThrowsException()
    {
        await SaveShopSettingsToDbInternalServerErrorWhenSettingsDataAdapterSaveThrowsExceptionAsync<ProductShopSettingsModel>(
            async (c, data) => await c.SaveProductShopSettingsToDb(data), ShopSettingType.Product);
    }
    

    [Fact]
    public async Task SaveProductShopSettingsToDbActionAsync()
    {
        await SetShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        var currentShopSettings = await SetShopSettingAsync(shopGuid, ShopSettingType.Product) as ProductShopSettingsModel;
        currentShopSettings.RootCategories =
        [
            new CategoryUrlModel{ ShopSettingsGuid = currentShopSettings.Guid, Item = 6500, Url = Guid.NewGuid().ToString() },
            new CategoryUrlModel{ ShopSettingsGuid = currentShopSettings.Guid, Item = 6501, Url = Guid.NewGuid().ToString() },
            new CategoryUrlModel{ ShopSettingsGuid = currentShopSettings.Guid, Item = 6502, Url = Guid.NewGuid().ToString() },
        ];

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

    private async Task<T> SaveShopSettingsToDbActionAsync<T>(Guid shopGuid, ShopSettingType shopSettingType, string fileName, Func<ShopSettingsController, T, Task<IActionResult>> saveAction)
        where T:ShopSettingsModel,new()
    {
        Assert.True(_importFacade.TryGetShopSettings(shopGuid, shopSettingType,out var settings));
        var shopSettings = Assert.IsType<T>(settings);

        var shopSettingsModel = await fileName.ReadFromFileAsync<ProductShopSettingsModel>();
        shopSettings.Update(shopSettingsModel, true);

        shopSettings.ImportService.Name = nameof(IShopImportSettings.ImportService);
        shopSettings.RequestHeaders.Name = nameof(IShopImportSettings.RequestHeaders);
        shopSettings.BrowserDataLoader.Name = nameof(IShopImportSettings.BrowserDataLoader);
        shopSettings.WebLoader.Name = nameof(IShopImportSettings.WebLoader);
        shopSettings.Services.Add(shopSettings.ImportService);
        shopSettings.Services.Add(shopSettings.RequestHeaders);
        shopSettings.Services.Add(shopSettings.BrowserDataLoader);
        shopSettings.Services.Add(shopSettings.WebLoader);

        _shopSettingsDataServiceMock.Setup(s => s.SaveShopSettings(It.IsAny<IShopSettings>(), It.IsAny<IEnumerable<IShopSettings>>()))
            .Returns<IShopSettings, IEnumerable<IShopSettings>>((settings, services) =>
                    SaveShopSettingsAsync<T>(settings, services, shopSettings));

        var shopSettingController = CreateShopSettingsController();
        var actionResult = Assert.IsType<OkObjectResult>(await saveAction(shopSettingController, shopSettings));
        return Assert.IsType<T>(actionResult.Value);
    }

    private static async Task<List<IShopSettings>> SaveShopSettingsAsync<T>(IShopSettings shopSettingsResult, 
        IEnumerable<IShopSettings> childrenSettingResult,
        ShopSettingsModel expect)
        where T:ShopSettingsModel,new()
    {
        var result = shopSettingsResult.ToShopImportSettings<T>();
        var servicesResult = new List<ServiceSettingsModel>();
        childrenSettingResult.ToList().ForEach(s => servicesResult.Add(s.ToImportServiceSettings<ServiceSettingsModel>()));
        servicesResult.ForEach(s=>result.SetServiceSettings(s, s.Name));

        ModelAssert.EqualFields(expect, result);
        ModelAssert.EqualServices(expect, result);
        ModelAssert.EqualCollections(expect.Services, servicesResult);        

        return await Task.FromResult(new List<IShopSettings>());
    }    

    


    

    [Fact]
    public async Task RootCategoryBadRequestWhenDataNotValid()
    {
        var shopSettingController = CreateShopSettingsController();
        var actionResult = Assert.IsType<BadRequestObjectResult>(shopSettingController.RootCategory(null));
        Assert.Equal("category url is null", Assert.IsType<string>(actionResult.Value));

        actionResult = Assert.IsType<BadRequestObjectResult>(shopSettingController.RootCategory(new CategoryUrlModel()));
        Assert.Equal("category shopSettingsGuid is empty", Assert.IsType<string>(actionResult.Value));
    }

    [Fact]
    public async Task RootCategoryPartialViewResultWhenDataIsValid()
    {
        var shopSettingController = CreateShopSettingsController();
        var data = new CategoryUrlModel { ShopSettingsGuid = Guid.NewGuid() };
        var actionResult = Assert.IsType<PartialViewResult>(shopSettingController.RootCategory(data));
        Assert.Equal(data, Assert.IsType<CategoryUrlModel>(actionResult.Model));
    }

    [Fact]
    public async Task SetRootCategoryBadRequestWhenDataIsNull()
    {
        var shopSettingController = CreateShopSettingsController();
        Assert.IsType<BadRequestObjectResult>(shopSettingController.SetRootCategory(null));        
    }

    [Fact]
    public async Task SetRootCategoryNotFoundWhenShopSettingsGuidNotExists()
    {
        var shopSettingController = CreateShopSettingsController();
        var data = new CategoryUrlModel { ShopSettingsGuid = Guid.NewGuid() };
        var actionResult = Assert.IsType<NotFoundObjectResult>(shopSettingController.SetRootCategory(data ));
        Assert.Equal(data.ShopSettingsGuid, Assert.IsType<Guid>(actionResult.Value));
    }

    [Fact]
    public async Task SetRootCategoryOkWhenDataIsValid()
    {
        await SetShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        var shopSettings = await SetShopSettingAsync(shopGuid, ShopSettingType.Product);

        var shopSettingController = CreateShopSettingsController();
        var data = new CategoryUrlModel { ShopSettingsGuid = shopSettings.Guid };
        var actionResult = Assert.IsType<OkObjectResult>(shopSettingController.SetRootCategory(data));
        Assert.Equal(data, Assert.IsType<CategoryUrlModel>(actionResult.Value));
    }
}
