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
        
        var shop = _importFacade.GetShops().FirstOrDefault(s=>s.Shop.Id == shopId) ?? throw new InvalidOperationException("shops are not set");

        if (!_importFacade.TryGetShopSettings(shop.ShopGuid, shopSettingType, out var shopSettings))
            shopSettings = await SetShopSettingAsync(shop.ShopGuid, shopSettingType);        

        ((ISettings)shopSettings).ShopId = shopId;
    
        return await Task.FromResult(shopSettings);
    }

    private static void SetServiceSetting(ShopSettingsModel shopSettings, string serviceName, int id)
    {
        var service = new ServiceSettingsModel { Name = serviceName, Id = id };
        ((ISettings)service).ParentSettingsId = shopSettings.Id;
        ((ISettings)service).ShopId = ((ISettings)shopSettings).ShopId;
        service.ServiceTypeName = $"{shopSettings.ShopSettingType}{serviceName}Type{shopSettings.Id}";
        shopSettings.SetServiceSettings(service, serviceName);
    }

    protected static void SetShopSettings(ShopImportModel shopImport, ShopSettingType shopSettingType)
    {
        var shopSettings = shopImport.CreateShopSettings(shopSettingType);

        shopSettings.Name = Guid.NewGuid().ToString();

        shopSettings.Id = shopImport.Shop.Id;

        SetServiceSetting(shopSettings, nameof(ShopSettingsModel.ImportService), shopSettings.Id + 1);
        SetServiceSetting(shopSettings, nameof(ShopSettingsModel.BrowserDataLoader), shopSettings.Id + 2);
        SetServiceSetting(shopSettings, nameof(ShopSettingsModel.RequestHeaders), shopSettings.Id + 3);
        SetServiceSetting(shopSettings, nameof(ShopSettingsModel.WebLoader), shopSettings.Id + 4);

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
