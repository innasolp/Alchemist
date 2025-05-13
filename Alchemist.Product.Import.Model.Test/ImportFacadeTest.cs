using Alchemist.DataService.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Import.Settings.Interfaces;
using Moq;

namespace Alchemist.Product.Import.Model.Test;

public class ImportFacadeTest
{
    private readonly Mock<IShopDataService> _shopDataServiceMock = new();

    private readonly List<Interfaces.IShop> _shops = [
        new Shop { Id = 1, Name = "Shop1", Url = "https://shop1" },
        new Shop { Id = 2, Name = "Shop2", Url = "https://shop2" },
        new Shop { Id = 3, Name = "Shop3", Url = "https://shop3" }
    ];

    [Fact]
    public async Task LoadNewShops()
    {
        _shopDataServiceMock.Setup(s => s.GetShops()).Returns(Task.FromResult(new List<Interfaces.IShop>([.. _shops.Take(2)])));

        var importFacade = new ImportFacade(_shopDataServiceMock.Object);

        var shops = await importFacade.LoadShops();
        Assert.NotNull(shops);
        Assert.Equal(2, shops.Count);
        Assert.Contains(shops, s => s.Shop.Id == _shops[0].Id);
    }

    [Fact]
    public async Task ReloadShopsWhenAnyDeprecated()
    {
        var importFacade = new ImportFacade(_shopDataServiceMock.Object);

        _shopDataServiceMock.Setup(s => s.GetShops()).Returns(Task.FromResult(_shops));

        var shops = await importFacade.LoadShops();
        Assert.NotNull(shops);
        Assert.Equal(3, shops.Count);

        _shopDataServiceMock.Setup(s => s.GetShops()).Returns(Task.FromResult(new List<Interfaces.IShop>([.. _shops.Take(2)])));
        var reloadedShops = await importFacade.LoadShops();
        Assert.Contains(reloadedShops, s => s.Shop.Id == _shops[2].Id && s.Shop.IsDeprecated);
    }

    [Fact]
    public async Task AddNewShop()
    {
        _shopDataServiceMock.Setup(s => s.GetShops()).Returns(Task.FromResult(new List<Interfaces.IShop>([.. _shops.Take(2)])));

        var importFacade = new ImportFacade(_shopDataServiceMock.Object);

        var shops = await importFacade.LoadShops();
        Assert.NotNull(shops);
        Assert.Equal(2, shops.Count);

        var newShopImport = importFacade.AddNewShop(_shops[2]);
        Assert.True(importFacade.TryGetShopImport(newShopImport.ShopGuid, out var result));
        Assert.Equal(_shops[2].Name, result.Shop.Name);
    }

    [Fact]
    public async Task TryGetShopImport()
    {
        var importFacade = new ImportFacade(_shopDataServiceMock.Object);

        _shopDataServiceMock.Setup(s => s.GetShops()).Returns(Task.FromResult(_shops));
        var shops = await importFacade.LoadShops();
        Assert.True(importFacade.TryGetShopImport(shops[0].ShopGuid, out var shopImport));
        Assert.NotNull(shopImport);
        Assert.Contains(_shops, s => s.Id == shopImport.Shop.Id);
    }

    [Fact]
    public async Task TryGetShopImportSettings()
    {
        var importFacade = new ImportFacade(_shopDataServiceMock.Object);

        _shopDataServiceMock.Setup(s => s.GetShops()).Returns(Task.FromResult(_shops));
        var shops = await importFacade.LoadShops();
        Assert.True(importFacade.TryGetShopImport(shops[1].ShopGuid, out var shopImport));
        Assert.NotNull(shopImport);
        Assert.Contains(_shops, s => s.Id == shopImport.Shop.Id);
        Assert.True(shops.All(s => s.ShopSettingTabs == null));

        var productShopSettings = shopImport.ShopGuid.CreateShopSettings(ShopSettingType.Product);
        shopImport.SetSettings(TabType.Shop, productShopSettings);
        Assert.True(importFacade.TryGetShopSettings(shopImport.ShopGuid, productShopSettings.Guid, out var result));
        Assert.NotNull(shopImport.ShopSettingTabs);
        Assert.Equal(productShopSettings.Guid, result.Guid);
        Assert.Equal(ShopSettingType.Product, result.ShopSettingType);
        Assert.Null(shopImport.ShopSettingTabs.ShopCategoriesSettings);
    }

    [Fact]
    public async Task TryGetServiceSettingsOnSetNew()
    {
        var importFacade = new ImportFacade(_shopDataServiceMock.Object);

        _shopDataServiceMock.Setup(s => s.GetShops()).Returns(Task.FromResult(_shops));
        var shops = await importFacade.LoadShops();
        
        Assert.True(importFacade.TryGetShopImport(shops[1].ShopGuid, out var shopImport));

        var productShopSettings = shopImport.ShopGuid.CreateShopSettings(ShopSettingType.Product);        
        shopImport.SetSettings(TabType.Shop, productShopSettings);
        Assert.True(importFacade.TryGetShopSettings(shopImport.ShopGuid, productShopSettings.Guid, out var shopSettings));

        Assert.Null(shopSettings.ImportService);
        Assert.Null(shopSettings.RequestHeaders);
        Assert.Null(shopSettings.WebLoader);
        Assert.Null(shopSettings.BrowserDataLoader);

        Assert.False(importFacade.TryGetServiceSettingsModel(shops[1].ShopGuid, 
            shopSettings.Guid, 
            nameof(ShopSettingsModel.WebLoader),
            out var serviceSettings));

        serviceSettings = shopSettings.CreateServiceSettingsModel(nameof(ShopSettingsModel.WebLoader));
        serviceSettings.ServiceTypeName = "ServiceType1";
        shopSettings.SetServiceSettings(serviceSettings);
        Assert.True(importFacade.TryGetServiceSettingsModel(shops[1].ShopGuid, shopSettings.Guid, nameof(ShopSettingsModel.WebLoader), out var result));
        Assert.Equal(serviceSettings.ServiceTypeName, result.ServiceTypeName);
    }


    [Fact]
    public async Task TryGetServiceSettingsAfterUpdated()
    {
        var importFacade = new ImportFacade(_shopDataServiceMock.Object);

        _shopDataServiceMock.Setup(s => s.GetShops()).Returns(Task.FromResult(_shops));
        var shops = await importFacade.LoadShops();

        Assert.True(importFacade.TryGetShopImport(shops[1].ShopGuid, out var shopImport));

        var productShopSettings = shopImport.ShopGuid.CreateShopSettings(ShopSettingType.Product);
        shopImport.SetSettings(TabType.Shop, productShopSettings);
        Assert.True(importFacade.TryGetShopSettings(shopImport.ShopGuid, productShopSettings.Guid, out var shopSettings));

        Assert.Null(shopSettings.ImportService);
        Assert.Null(shopSettings.RequestHeaders);
        Assert.Null(shopSettings.WebLoader);
        Assert.Null(shopSettings.BrowserDataLoader);

        Assert.False(importFacade.TryGetServiceSettingsModel(shops[1].ShopGuid,
            shopSettings.Guid,
            nameof(ShopSettingsModel.WebLoader),
            out var serviceSettings));

        serviceSettings = shopSettings.CreateServiceSettingsModel(nameof(ShopSettingsModel.WebLoader));
        serviceSettings.ServiceTypeName = "ServiceType1";
        shopSettings.SetServiceSettings(serviceSettings);

        var newService = shopSettings.CreateServiceSettingsModel(nameof(ShopSettingsModel.WebLoader));
        newService.ServiceTypeName = "ServiceType2";
        shopSettings.UpdateServiceSettings(newService);

        Assert.True(importFacade.TryGetServiceSettingsModel(shops[1].ShopGuid, shopSettings.Guid, nameof(ShopSettingsModel.WebLoader), out var result));
        Assert.Equal(newService.ServiceTypeName, result.ServiceTypeName);
    }
}