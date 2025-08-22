using Alchemist.DataService.Interfaces;
using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Import.Settings.JsonAdapter;
using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Import.WebApp.Controllers;
using Alchemist.Product.Import.WebApp.Models;
using Alchemist.Product.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Reflection;
using System.Text.Json;

using ShopSettingType = Alchemist.Import.Settings.Interfaces.ShopSettingType;
using SettingsCommon = Alchemist.Import.Settings.Extensions.Common;

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
        return new ShopSettingsController(_loggerMock.Object, _importFacade, ModelFactory, _settingsDataAdapter);
    }    

    [Fact]
    public async Task SaveProductShopSettingsFieldsOnSaveShopSettingsActionAsync()
    {
        await AssertSaveShopSettingsAsync(ShopSettingType.Product);
    }

    [Fact]
    public async Task SaveCategoryShopSettingsFieldsOnSaveShopSettingsActionAsync()
    {
        await AssertSaveShopSettingsAsync(ShopSettingType.Category);
    }

    //[Fact]
    //public void DeserializeService()
    //{
    //    //var json = "{\"Id\":0,\r\n\"ServiceTypeName\":\"CategoryImportServiceType0_d5246a67-edb1-493f-b9c1-c16526604420\",\r\n\"ImplementationTypeName\":null,\r\n\"AssemblyPath\":null,\r\n\"ServiceProviderPath\":null,\r\n\"FileName\":null,\r\n\"StringValue\":null,\r\n\"Tab\":0,\r\n\"ParentSettingsId\":0,\r\n\"ShopSettingsGuid\":\"247a4b67-20a1-4855-b659-4e99036f3c05\",\r\n\"Guid\":\"9d98ad48-ec0b-47f9-888d-f49b428b5ecf\",\r\n\"ShopGuid\":\"09f7193f-476d-4a7b-a62f-a00046d0f0d7\",\r\n\"ShopId\":3,\r\n\"Name\":\"ImportService\"}";
    //    //var json = "{\"Id\":0,\r\n\"ParentSettingsId\":0,\r\n\"ShopSettingsGuid\":\"247a4b67-20a1-4855-b659-4e99036f3c05\",\r\n\"ShopGuid\":\"09f7193f-476d-4a7b-a62f-a00046d0f0d7\",\r\n\"ShopId\":3,\r\n\"Name\":\"ImportService\"}";
    //    var json = "{\"id\":0,\r\n\"parentSettingsId\":0,\r\n\"shopSettingsGuid\":\"247a4b67-20a1-4855-b659-4e99036f3c05\",\r\n\"shopGuid\":\"09f7193f-476d-4a7b-a62f-a00046d0f0d7\",\r\n\"shopId\":3,\r\n\"name\":\"ImportService\"}";

    //    //var source = new ServiceSettingsModel(0, 0, 0, Guid.NewGuid(), Guid.NewGuid());
    //    //var json = JsonSerializer.Serialize(source);

    //    var option = new JsonSerializerOptions
    //    {
    //        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    //        IgnoreReadOnlyProperties = false,
    //        RespectRequiredConstructorParameters = true,
    //        PropertyNameCaseInsensitive = true,
    //        IgnoreReadOnlyFields = true,
    //        //PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    //    };
    //    var service = JsonSerializer.Deserialize<ServiceSettingsModel>(json);//, option);
    //}

    private async Task AssertSaveShopSettingsAsync(ShopSettingType shopSettingType)
    {
        await LoadShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;

        if (!_importFacade.TryGetShopSettings(shopGuid, shopSettingType, out var shopSettings))
            return;

        var oldShopSettings =  ModelFactory.GetCopy(shopSettings);

        FillShopSettingsFields(shopSettings);        

        var changedShopSettings = ModelFactory.GetCopy(shopSettings);
        FillShopSettingsFields(changedShopSettings);
        var json = JsonSerializer.Serialize(changedShopSettings, changedShopSettings.GetType());

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
        await LoadShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        
        Assert.True(_importFacade.TryGetShopSettings(shopGuid, ShopSettingType.Product, out var shopSettings));

        var shopSettingController = CreateShopSettingsController();
        var json = JsonSerializer.Serialize(shopSettings);
        var guid = Guid.NewGuid();
        var actionResult = Assert.IsType<NotFoundObjectResult>(shopSettingController.SaveShopSettings(guid, (int)ShopSettingType.Product, json));
        Assert.Equal(guid, Assert.IsType<Guid>(actionResult.Value));
    }


    [Fact]
    public async Task ShopSettingViewResultSuccessWhenDataIsValid()
    {
        await LoadShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;

        var shopSettingType = ShopSettingType.Category;
        Assert.True(_importFacade.TryGetShopSettings(shopGuid, shopSettingType, out var shopSettings));

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
        Assert.Equal("data", Assert.IsType<string>(actionResult.Value));
    }
    
    [Fact]
    public async Task SaveCategoryShopSettingsToDbBadRequestWhenDataIsNull()
    {
        var shopSettingController = CreateShopSettingsController();
        var actionResult = Assert.IsType<BadRequestObjectResult>(await shopSettingController.SaveCategoryShopSettingsToDb(null));
        Assert.Equal("data", Assert.IsType<string>(actionResult.Value));
    }

    [Fact]
    public async Task SaveProductShopSettingsToDbNotFoundWhenNotExistingShop()
    {
        var shopSettingController = CreateShopSettingsController();
        var data = new ProductShopSettingsModel(0, 0, Guid.NewGuid());
        var actionResult = Assert.IsType<NotFoundObjectResult>(await shopSettingController.SaveProductShopSettingsToDb(data));
        Assert.Equal(data.ShopGuid, Assert.IsType<Guid>(actionResult.Value));
    }


    [Fact]
    public async Task SaveCategoryShopSettingsToDbNotFoundWhenNotExistingShop()
    {
        var shopSettingController = CreateShopSettingsController();
        var data = new CategoryShopSettingsModel(0,0, Guid.NewGuid());
        var actionResult = Assert.IsType<NotFoundObjectResult>(await shopSettingController.SaveCategoryShopSettingsToDb(data));
        Assert.Equal(data.ShopGuid, Assert.IsType<Guid>(actionResult.Value));
    }

    private async Task SaveShopSettingsToDbInternalServerErrorWhenSettingsDataAdapterSaveThrowsExceptionAsync<T>(Func<ShopSettingsController, T, Task<IActionResult>> saveToDbAction,
        ShopSettingType shopSettingType)
        where T:ShopSettingsModel
    {
        await LoadShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        Assert.True(_importFacade.TryGetShopSettings(shopGuid, shopSettingType, out var shopSettings));
        var exception = new InvalidOperationException("testError");
        _shopSettingsDataServiceMock.Setup(s => s.SaveShopSettings(It.IsAny<IShopSettings>(), It.IsAny<IEnumerable<IShopSettings>>())).Throws(exception);

        var shopSettingController = CreateShopSettingsController();
        var data = ModelFactory.GetCopy(shopSettings) as T;
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
        await LoadShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;

        Assert.True(_importFacade.TryGetShopSettings(shopGuid, ShopSettingType.Product, out var shopSettings));
        var currentShopSettings = shopSettings as ProductShopSettingsModel;

        currentShopSettings.RootCategories.AddRange(
        [
            new CategoryUrlModel(currentShopSettings.Guid) { Item = 6500, Url = Guid.NewGuid().ToString() },
            new CategoryUrlModel(currentShopSettings.Guid){ Item = 6501, Url = Guid.NewGuid().ToString() },
            new CategoryUrlModel(currentShopSettings.Guid){ Item = 6502, Url = Guid.NewGuid().ToString() },
        ]);

        await SaveShopSettingsToDbActionAsync<ProductShopSettingsModel>(currentShopSettings.ShopGuid,
            ShopSettingType.Product,
            "OzonProductSettings.json",
            (controller, shopSettings) => controller.SaveProductShopSettingsToDb(shopSettings));
    }

    [Fact]
    public async Task SaveCategoryShopSettingsToDbActionAsync()
    {
        await LoadShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        Assert.True(_importFacade.TryGetShopSettings(shopGuid, ShopSettingType.Product, out var currentShopSettings));

        await SaveShopSettingsToDbActionAsync<CategoryShopSettingsModel>(shopGuid,
            ShopSettingType.Category,
            "ozoncategories.json",
            (controller, shopSettings) => controller.SaveCategoryShopSettingsToDb(shopSettings));
    }

    private async Task<T> SaveShopSettingsToDbActionAsync<T>(Guid shopGuid, ShopSettingType shopSettingType, string fileName, Func<ShopSettingsController, T, Task<IActionResult>> saveAction)
        where T:ShopSettingsModel
    {
        Assert.True(_importFacade.TryGetShopSettings(shopGuid, shopSettingType,out var settings));
        var shopSettings = Assert.IsType<T>(settings);

        var defaultServiceProperties = new Dictionary<string, object>()
        {
            { nameof(IServiceSettingsModel.Id), 0 },
            { nameof(IServiceSettingsModel.ParentSettingsId), 0 },
            { nameof(IServiceSettingsModel.ShopId), 0 },
            { nameof(IServiceSettingsModel.ShopGuid), Guid.Empty },
            { nameof(IServiceSettingsModel.ShopSettingsGuid), Guid.Empty },
        };
        var serviceSettingsModelConverter = new ModelJsonConverter<IServiceSettingsModel>(defaultServiceProperties);
        
        var defaultShopSettingsProperties = new Dictionary<string, object>()
        {
            { nameof(IShopImportSettingsModel.Id), 0 },
            { nameof(IShopImportSettingsModel.ShopId), 0 },
            { nameof(IShopImportSettingsModel.ShopGuid), Guid.Empty }
        };
        var shopImportSettingsJsonConverter = new ModelJsonConverter<IShopImportSettingsModel>(defaultShopSettingsProperties);

        var shopSettingsModel = await fileName.ReadFromFileAsync<T>([serviceSettingsModelConverter,shopImportSettingsJsonConverter]);
        ModelFactory.Update(shopSettings, shopSettingsModel);

        foreach(var primaryServiceName in SettingsCommon.GetPrimaryServiceNames())
        {
            var service = shopSettings.GetService(primaryServiceName) ??
                ModelFactory.CreateServiceSettingsModel(shopSettings.ShopId, 0, shopSettings.Id, shopSettingsModel.ShopGuid, shopSettingsModel.Guid);

            service.Name = primaryServiceName;
            shopSettings.Services.Add(service as ServiceSettingsModel);
        }

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
        where T:ShopSettingsModel
    {
        var result = shopSettingsResult.ToShopImportSettings<T>();
        var servicesResult = new List<ServiceSettingsModel>();
        childrenSettingResult.ToList().ForEach(s => servicesResult.Add(s.ToImportServiceSettings<ServiceSettingsModel>()));
        servicesResult.ForEach(s=>result.UpdateServiceSettings(s.Name, s));

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

        actionResult = Assert.IsType<BadRequestObjectResult>(shopSettingController.RootCategory(new CategoryUrlModel(Guid.Empty)));
        Assert.Equal("category shopSettingsGuid is empty", Assert.IsType<string>(actionResult.Value));
    }

    [Fact]
    public async Task RootCategoryPartialViewResultWhenDataIsValid()
    {
        var shopSettingController = CreateShopSettingsController();
        var data = new CategoryUrlModel(Guid.NewGuid());
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
        var data = new CategoryUrlModel(Guid.NewGuid());
        var actionResult = Assert.IsType<NotFoundObjectResult>(shopSettingController.SetRootCategory(data ));
        Assert.Equal(data.ShopSettingsGuid, Assert.IsType<Guid>(actionResult.Value));
    }

    [Fact]
    public async Task SetRootCategoryOkWhenDataIsValid()
    {
        await LoadShopsAsync();
        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        Assert.True(_importFacade.TryGetShopSettings(shopGuid, ShopSettingType.Product, out var shopSettings));

        var shopSettingController = CreateShopSettingsController();
        var data = new CategoryUrlModel(shopSettings.Guid);
        var actionResult = Assert.IsType<OkObjectResult>(shopSettingController.SetRootCategory(data));
        Assert.Equal(data, Assert.IsType<CategoryUrlModel>(actionResult.Value));
    }
}
