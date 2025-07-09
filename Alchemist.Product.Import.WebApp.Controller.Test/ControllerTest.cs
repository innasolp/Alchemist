using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.WebApp.Controllers;
using Alchemist.Product.Import.WebApp.Models;
using Alchemist.Product.Interfaces;
using Message.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

using ShopSettingType = Alchemist.Import.Settings.Interfaces.ShopSettingType;

namespace Alchemist.Product.Import.WebApp.Controller.Test;

public abstract class ControllerTest<T>
    where T: Microsoft.AspNetCore.Mvc.Controller
{
    protected readonly Mock<IShopDataService> _shopDataServiceMock = new();

    protected readonly Mock<IMessageReceiver> _messageReceiverMock = new();
    
    protected readonly IModelFactory ModelFactory = new ModelFactory();

    protected readonly IImportFacade _importFacade;

    protected readonly Mock<ISettingsDataAdapter> _settingsDataAdapterMock = new();

    protected readonly List<IShop> _shops = [
        new Shop { Id = 1, Name = "Shop1", Url = "https://shop1" },
        new Shop { Id = 2, Name = "Shop2", Url = "https://shop2" },
        new Shop { Id = 3, Name = "Shop3", Url = "https://shop3" }
    ];    

    protected readonly Mock<ILogger<T>> _loggerMock = new();

    protected readonly Mock<ILogger<HomeController>> _loggerHomeControllerMock = new();

    protected readonly List<Mock<IShopModel>> ShopModelMocks = [];

    protected ControllerTest()
    {
        _importFacade = new ImportFacade(ModelFactory);
        
        _shopDataServiceMock.Setup(s => s.GetShops()).Returns(async () => { return _shops; });

        _settingsDataAdapterMock.Setup(s => s.GetShopImportSettings(It.IsAny<int>(), It.IsAny<ShopSettingType>()))
            .Returns(GetShopSettingsModelAsync);

        _messageReceiverMock.Setup(m => m.On(Messages.ReceiveShopCreated, It.IsAny<Action<Shop>>())).Callback(() => { });

        //todo
        _loggerHomeControllerMock.Setup(l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>())).
            Callback(() => { });

        _loggerHomeControllerMock.Setup(l => l.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>())).
            Callback(() => { });

        _loggerHomeControllerMock.Setup(l => l.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>())).
            Callback(() => { });

        //todo
        _loggerMock.Setup(l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>())).
            Callback(() => { });

        _loggerMock.Setup(l => l.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>())).
            Callback(() => { });

        _loggerMock.Setup(l => l.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception?>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>())).
            Callback(() => { });
    }

    protected HomeController CreateHomeController()
    {
        return new HomeController(_loggerHomeControllerMock.Object,
            _shopDataServiceMock.Object,
            _settingsDataAdapterMock.Object,
            _importFacade,
            ModelFactory,
            _messageReceiverMock.Object);
    }

    protected async Task<List<ShopImportModel>> LoadShopsAsync()
    {
        var shops = await _shopDataServiceMock.Object.GetShops();
        return await _importFacade.LoadShops(shops);
    } 
    

    //todo change to get or create
    protected async Task<IShopImportSettings> GetShopSettingsModelAsync(int shopId, ShopSettingType shopSettingType)
    {
        if (shopSettingType == ShopSettingType.Service)
            throw new InvalidOperationException();

        var shop = _importFacade.GetShops().FirstOrDefault(s => s.Shop.Id == shopId);// ?? throw new InvalidOperationException("shops are not set");
        if (shop == null)
            return null;
    
        return !_importFacade.TryGetShopSettings(shop.ShopGuid, shopSettingType, out var shopSettings) ? null : await Task.FromResult(shopSettings);
    }

    public static void SetServiceTypeName(IServiceSettingsModel service, ShopSettingType shopSettingType)
    {
        service.ServiceTypeName = $"{shopSettingType}{service.Name}Type{service.ParentSettingsId}";
    }
    

    protected void FillShopSettingsFields(IShopServicesSettingsModel shopSettings)
    {
        shopSettings.Name = Guid.NewGuid().ToString();

        if (shopSettings is IProductShopSettingsModel productShopSettings)
            SetProductShopSettingsFields(productShopSettings);
        else if (shopSettings is ICategoryShopSettingsModel categoryShopSettings)
            SetCategoryShopSettingsFields(categoryShopSettings);        

        if (_importFacade.TryGetServiceSettings(shopSettings.ShopGuid, shopSettings.Guid, nameof(ShopSettingsModel.ImportService), out var importService))
            importService.ServiceTypeName = $"{shopSettings.ShopSettingType}{nameof(ShopSettingsModel.ImportService)}Type{shopSettings.Id}_{Guid.NewGuid()}";

        if (_importFacade.TryGetServiceSettings(shopSettings.ShopGuid, shopSettings.Guid, nameof(ShopSettingsModel.BrowserDataLoader), out var browserDataLoader))
            browserDataLoader.ServiceTypeName = $"{shopSettings.ShopSettingType}{nameof(ShopSettingsModel.BrowserDataLoader)}Type{shopSettings.Id}_{Guid.NewGuid()}";

        if (_importFacade.TryGetServiceSettings(shopSettings.ShopGuid, shopSettings.Guid, nameof(ShopSettingsModel.RequestHeaders), out var requestHeaders))
            requestHeaders.ServiceTypeName = $"{shopSettings.ShopSettingType}{nameof(ShopSettingsModel.RequestHeaders)}Type{shopSettings.Id}_{Guid.NewGuid()}";

        if (_importFacade.TryGetServiceSettings(shopSettings.ShopGuid, shopSettings.Guid, nameof(ShopSettingsModel.WebLoader), out var webLoader))
            webLoader.ServiceTypeName = $"{shopSettings.ShopSettingType}{nameof(ShopSettingsModel.WebLoader)}Type{shopSettings.Id}_{Guid.NewGuid()}";

    }

    private void SetProductShopSettingsFields(IProductShopSettingsModel shopSettingsModel)
    {
        shopSettingsModel.ProductUrlFormat = Guid.NewGuid().ToString();
        shopSettingsModel.CategoryUrlFormat = Guid.NewGuid().ToString();
        shopSettingsModel.PageProductCount = new Random().Next(30);

        shopSettingsModel.RootCategories.Clear();
        shopSettingsModel.RootCategories.Add(new CategoryUrlModel(shopSettingsModel.Guid) { Item = new Random().Next(10000), Url = Guid.NewGuid().ToString() });
        shopSettingsModel.RootCategories.Add(new CategoryUrlModel(shopSettingsModel.Guid) { Item = new Random().Next(10000), Url = Guid.NewGuid().ToString() });
    }

    private void SetCategoryShopSettingsFields(ICategoryShopSettingsModel shopSettingsModel)
    {
        shopSettingsModel.CategorySourceUrl = Guid.NewGuid().ToString();
    }
 }
