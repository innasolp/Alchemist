using Alchemist.Common;
using Alchemist.DataService.Interfaces;
using Alchemist.Import.Settings.DataAdapter;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Import.WebApp.Controllers;
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
    
    protected readonly IImportFacade _importFacade;

    protected readonly Mock<ISettingsDataAdapter> _settingsDataAdapterMock = new();

    protected readonly List<IShop> _shops = [
        new Shop { Id = 1, Name = "Shop1", Url = "https://shop1" },
        new Shop { Id = 2, Name = "Shop2", Url = "https://shop2" },
        new Shop { Id = 3, Name = "Shop3", Url = "https://shop3" }
    ];

    private int _settingsIdCounter = 0;

    protected readonly Mock<ILogger<T>> _loggerMock = new();

    protected readonly Mock<ILogger<HomeController>> _loggerHomeControllerMock = new();
    protected ControllerTest()
    {
        _importFacade = new ImportFacade(_shopDataServiceMock.Object);
        
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
            _settingsDataAdapterMock.Object,
            _importFacade,
            _messageReceiverMock.Object);
    }

    protected async Task SetShopsAsync()
    {
        await _importFacade.LoadShops();
    }  


    //todo change to get or create
    protected async Task<IShopImportSettings> GetShopSettingsModelAsync(int shopId, ShopSettingType shopSettingType)
    {
        if (shopSettingType == ShopSettingType.Service)
            throw new InvalidOperationException();        

        ShopSettingsModel shopSettings = shopSettingType == ShopSettingType.Product
            ? new ProductShopSettingsModel { Id = _settingsIdCounter++}
            : new CategoryShopSettingsModel { Id = _settingsIdCounter++ };

        ((ISettings)shopSettings).ShopId = shopId;

        SetServiceSetting(shopSettings, nameof(ShopSettingsModel.ImportService));
        SetServiceSetting(shopSettings, nameof(ShopSettingsModel.BrowserDataLoader));
        SetServiceSetting(shopSettings, nameof(ShopSettingsModel.RequestHeaders));
        SetServiceSetting(shopSettings, nameof(ShopSettingsModel.WebLoader));
    
        return await Task.FromResult(shopSettings);
    }

    private void SetServiceSetting(ShopSettingsModel shopSettings, string serviceName)
    {
        var service = new ServiceSettingsModel { Name = serviceName, Id = _settingsIdCounter++ };
        ((ISettings)service).ParentSettingsId = shopSettings.Id;
        ((ISettings)service).ShopId = ((ISettings)shopSettings).ShopId;
        service.ServiceTypeName = $"{shopSettings.ShopSettingType}{serviceName}Type{shopSettings.Id}";
        shopSettings.SetServiceSettings(service);
    }

    protected void SetShopSettings(ShopImportModel shopImport, ShopSettingType shopSettingType)
    {
        var shopSettings = shopImport.ShopGuid.CreateShopSettings(shopSettingType);
        SetServiceSetting(shopSettings, nameof(ShopSettingsModel.ImportService));
        SetServiceSetting(shopSettings, nameof(ShopSettingsModel.BrowserDataLoader));
        SetServiceSetting(shopSettings, nameof(ShopSettingsModel.RequestHeaders));
        SetServiceSetting(shopSettings, nameof(ShopSettingsModel.WebLoader));
        shopImport.SetSettings(TabType.Shop, shopSettings);
    }

    protected async Task<ShopSettingsModel> SetShopSettingAsync(Guid shopGuid, ShopSettingType shopSettingType)
    {    
        Assert.True(_importFacade.TryGetShopImport(shopGuid, out var shopImport));

        SetShopSettings(shopImport, shopSettingType);

        return shopSettingType == ShopSettingType.Product ? shopImport.ShopSettingTabs.ShopProductsSettings
            : shopSettingType == ShopSettingType.Category ? shopImport.ShopSettingTabs.ShopCategoriesSettings 
            : throw new InvalidOperationException(ShopSettingType.Service.ToString());
    }
}
