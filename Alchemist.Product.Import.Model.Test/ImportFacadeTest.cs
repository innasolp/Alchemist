using Alchemist.Product.Entities;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Import.Settings.Interfaces;
using Moq;
using Alchemist.Import.Settings.Extensions;

using SettingsCommon = Alchemist.Import.Settings.Extensions.Common;

namespace Alchemist.Product.Import.Model.Test;

public class ImportFacadeTest
{
    private readonly Mock<IModelFactory> _modelFactoryMock = new();

    private readonly List<Interfaces.IShop> _shops = [
        new Shop { Id = 1, Name = "Shop1", Url = "https://shop1" },
        new Shop { Id = 2, Name = "Shop2", Url = "https://shop2" },
        new Shop { Id = 3, Name = "Shop3", Url = "https://shop3" }
    ];

    private readonly IImportFacade _importFacade;

    private readonly List<Mock<IShopModel>> _shopModelMocks = [];

    public ImportFacadeTest()
    {
        _importFacade = new ImportFacade(_modelFactoryMock.Object);

        _modelFactoryMock.Setup(m => m.CreateShopModel(It.IsAny<int>())).Returns((int shopId) => CreateShopModel(shopId));

        _modelFactoryMock.Setup(m => m.CreateShopSettingsTabsModel(It.IsAny<int>(), It.IsAny<Guid>())).
            Returns(CreateShopSettingsTabsModel);

        _modelFactoryMock.Setup(m => m.CreateServiceSettingsModel(It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<Guid>(),
            It.IsAny<Guid>())).Returns(CreateServiceSettings);
    }

    private IShopSettingTabsModel CreateShopSettingsTabsModel(int shopId, Guid shopGuid)
    {
        var guid = Guid.NewGuid();

        var shopSettingsTabsMock = new Mock<IShopSettingTabsModel>();
        shopSettingsTabsMock.Setup(s => s.Guid).Returns(guid);
        shopSettingsTabsMock.Setup(s => s.ShopGuid).Returns(shopGuid);

        var productShopSettings = CreateShopSettingsModel< IProductShopSettingsModel>(shopId, shopGuid, ShopSettingType.Product);        
        shopSettingsTabsMock.Setup(s => s.ShopProductsSettings).Returns(productShopSettings);

        var categoryShopSettings = CreateShopSettingsModel< ICategoryShopSettingsModel>(shopId, shopGuid, ShopSettingType.Category);
        shopSettingsTabsMock.Setup(s => s.ShopCategoriesSettings).Returns(categoryShopSettings);

        return shopSettingsTabsMock.Object;
    }

    private static T CreateShopSettingsModel<T>(int shopId, Guid shopGuid, ShopSettingType shopSettingType)
        where T: class, IShopImportSettingsModel
    {
        var guid = Guid.NewGuid();
        var shopProductSettingsMock = new Mock<T>();
        shopProductSettingsMock.Setup(s => s.ShopSettingType).Returns(shopSettingType);
        shopProductSettingsMock.Setup(s => s.Guid).Returns(guid);
        shopProductSettingsMock.Setup(s => s.ShopGuid).Returns(shopGuid);
        shopProductSettingsMock.Setup(s => s.ShopId).Returns(shopId);

        var services = new List<IServiceSettingsModel>();
        shopProductSettingsMock.Setup(s => s.Services).Returns(services);

        foreach (var primaryServiceName in SettingsCommon.GetPrimaryServiceNames())
        {
            var service = CreateService(shopId, shopGuid, guid, primaryServiceName);
            shopProductSettingsMock.Object.Services.Add(service);
        }        

        return shopProductSettingsMock.Object;
    }

    private static IServiceSettingsModel CreateService(int shopId, Guid shopGuid, Guid shopSettingsGuid, string serviceName)
    {
        var guid = Guid.NewGuid();
        var serviceSettingsMock = new Mock<IServiceSettingsModel>();
        serviceSettingsMock.Setup(s => s.Guid).Returns(guid);
        serviceSettingsMock.Setup(s => s.ShopGuid).Returns(shopGuid);
        serviceSettingsMock.Setup(s => s.ShopSettingsGuid).Returns(shopSettingsGuid);
        serviceSettingsMock.Setup(s => s.ShopId).Returns(shopId);
        serviceSettingsMock.Setup(s => s.Name).Returns(serviceName);
        return serviceSettingsMock.Object;
    }

    private static IServiceSettingsModel CreateServiceSettings(int shopId, int id, int parentId, Guid shopGuid, Guid shopSettingsGuid)
    {
        var guid = Guid.NewGuid();
        var serviceSettingsMock = new Mock<IServiceSettingsModel>();
        serviceSettingsMock.Setup(s => s.Guid).Returns(guid);
        serviceSettingsMock.Setup(s => s.ShopGuid).Returns(shopGuid);
        serviceSettingsMock.Setup(s => s.ParentSettingsId).Returns(parentId);
        serviceSettingsMock.Setup(s => s.ShopSettingsGuid).Returns(shopSettingsGuid);
        serviceSettingsMock.Setup(s => s.ShopId).Returns(shopId);
        return serviceSettingsMock.Object;
    }

    private  IShopModel CreateShopModel(int shopId)
    {
        var shopModelMock = new Mock<IShopModel>();
        var guid = Guid.NewGuid();
        shopModelMock.SetupGet(s=>s.Id).Returns(shopId);
        shopModelMock.SetupGet(s=>s.Guid).Returns(guid);
        
        shopModelMock.SetupSet((s)=> s.Name = It.IsAny<string>());
        
        _shopModelMocks.Add(shopModelMock);               

        return shopModelMock.Object;
    }


    [Fact]
    public async Task LoadNewShops()
    {
        var shops = new List<Interfaces.IShop>([.. _shops.Take(2)]);        

        var shopImports = await _importFacade.LoadShops(shops);
        Assert.NotNull(shopImports);
        Assert.Equal(2, shopImports.Count);
        Assert.Contains(shopImports, s => s.Shop.Id == _shops[0].Id);
    }

    [Fact]
    public async Task ReloadShopsWhenAnyDeprecated()
    {
        var shops = _shops;

        var shopImports = await _importFacade.LoadShops(shops);
        Assert.NotNull(shopImports);
        Assert.Equal(3, shopImports.Count);

        shops = new List<Interfaces.IShop>([.. _shops.Take(2)]);
        var reloadedShops = await _importFacade.LoadShops(shops);

        var shopMock = _shopModelMocks.FirstOrDefault(s => s.Object.Id == _shops[2].Id);
        Assert.NotNull(shopMock);
        shopMock.VerifySet(s => s.IsDeprecated = true);
    }

    [Fact]
    public async Task AddNewShop()
    {
        var shops = new List<Interfaces.IShop>([.. _shops.Take(2)]);        

        var shopImports = await _importFacade.LoadShops(shops);
        Assert.NotNull(shopImports);
        Assert.Equal(2, shopImports.Count);

        var newShopImport = _importFacade.AddNewShop(_shops[2]);
        Assert.True(_importFacade.TryGetShopImport(newShopImport.ShopGuid, out var result));

        var shopMock = _shopModelMocks.FirstOrDefault(s => s.Object.Id == result.Shop.Id);
        Assert.NotNull(shopMock);
        shopMock.VerifySet(s => s.Name = _shops[2].Name);
    }

    [Fact]
    public async Task TryGetShopImport()
    {
        var shops = _shops;
        var shopImports = await _importFacade.LoadShops(shops);
        Assert.True(_importFacade.TryGetShopImport(shopImports[0].ShopGuid, out var shopImport));
        Assert.NotNull(shopImport);
        Assert.Contains(_shops, s => s.Id == shopImport.Shop.Id);
    }

    [Fact]
    public async Task TryGetShopImportSettings()
    {
        var shops = _shops;

        var shopImports = await _importFacade.LoadShops(shops);
        Assert.True(_importFacade.TryGetShopImport(shopImports[1].ShopGuid, out var shopImport));
        Assert.NotNull(shopImport);
        Assert.Contains(_shops, s => s.Id == shopImport.Shop.Id);
        Assert.True(shopImports.All(s => s.ShopSettingTabs.ShopProductsSettings.IsEmpty() && s.ShopSettingTabs.ShopCategoriesSettings.IsEmpty()));

        var productShopSettings = shopImport.ShopSettingTabs.ShopProductsSettings;
        Assert.True(_importFacade.TryGetShopSettings(shopImport.ShopGuid, productShopSettings.Guid, out var result));
        Assert.Equal(productShopSettings.Guid, result.Guid);
        Assert.Equal(ShopSettingType.Product, result.ShopSettingType);
    }

    [Fact]
    public async Task TryGetServiceSettingsOnSetNew()
    {
        var shops = _shops;

        var shopImports = await _importFacade.LoadShops(shops);
        
        Assert.True(_importFacade.TryGetShopImport(shopImports[1].ShopGuid, out var shopImport));
        
        Assert.True(_importFacade.TryGetShopSettings(shopImport.ShopGuid, shopImport.ShopSettingTabs.ShopProductsSettings.Guid, out var shopSettings));

        foreach(var primaryServiceName in SettingsCommon.GetPrimaryServiceNames())
        {
            if(shopSettings.GetService(primaryServiceName) is IServiceSettingsModel primaryService)
              Assert.True(primaryService?.IsEmpty());
        }        

        var name = Guid.NewGuid().ToString();

        Assert.False(_importFacade.TryGetServiceSettings(shopImports[1].ShopGuid, 
            shopSettings.Guid,
            name,
            out var serviceSettings));

        _importFacade.AddNewServiceSettings(shopSettings, name, out serviceSettings);        
        serviceSettings.ServiceTypeName = "ServiceType1";
        
        Assert.True(_importFacade.TryGetServiceSettings(shopImports[1].ShopGuid, shopSettings.Guid, serviceSettings.Guid, out var result));
        Assert.Equal(serviceSettings.ServiceTypeName, result.ServiceTypeName);
    }


    [Fact]
    public async Task TryGetServiceSettingsAfterUpdated()
    {
        var shops = _shops;

        var shopImports =  await _importFacade.LoadShops(shops);

        Assert.True(_importFacade.TryGetShopImport(shopImports[1].ShopGuid, out var shopImport));

        Assert.True(_importFacade.TryGetShopSettings(shopImport.ShopGuid, shopImport.ShopSettingTabs.ShopProductsSettings.Guid, out var shopSettings));
        
        foreach (var primaryServiceName in SettingsCommon.GetPrimaryServiceNames())
        {
            if (shopSettings.GetService(primaryServiceName) is IServiceSettingsModel primaryService)
                Assert.True(primaryService?.IsEmpty());
        }
        
        Assert.False(_importFacade.TryGetServiceSettings(shopImports[1].ShopGuid,
            shopSettings.Guid,
            Guid.NewGuid().ToString(),
            out var serviceSettings));

        Assert.True(_importFacade.TryGetServiceSettings(shopImports[1].ShopGuid, shopSettings.Guid, nameof(PrimaryServiceName.WebLoader), out serviceSettings));
        serviceSettings.ServiceTypeName = "ServiceType1";
        
        Assert.True(_importFacade.TryGetServiceSettings(shopImports[1].ShopGuid, shopSettings.Guid, nameof(PrimaryServiceName.WebLoader), out var result));
        Assert.Equal(serviceSettings.ServiceTypeName, result.ServiceTypeName);
    }
}