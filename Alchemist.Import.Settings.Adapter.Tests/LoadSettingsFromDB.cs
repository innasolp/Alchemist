using Alchemist.DataService.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using Moq;
using System.Text.Json;
using Alchemist.Import.Settings.Extensions;
using ShopSettingType = Alchemist.Product.Interfaces.ShopSettingType;
using Alchemist.Import.Settings.Test.Model;

namespace Alchemist.Import.Settings.DataAdapter.Tests;

public class LoadSettingsFromDB
{
    private readonly Dictionary<ShopSettingType, ISettingsDataAdapter> _adapters = [];

    private readonly Mock<IShopSettingsDataService> _shopSettingsDataServiceMock = new();

    private readonly List<IShopSettings> _shopSettings =
    [
            new ShopSettings { Id = 1, ShopId = 1, Type = ShopSettingType.Product, Name="shop1Products" },
            new ShopSettings { Id = 2, ShopId = 2, Type = ShopSettingType.Product, Name="Shop2Products" },
            new ShopSettings { Id = 3, ShopId = 1, Type = ShopSettingType.Category, Name= "shop1Category" },
            new ShopSettings { Id = 4, ShopId = 2, Type = ShopSettingType.Category, Name="shop2Category" },
            new ShopSettings { Id = 5, ShopId = 1, ParentSettingsId = 1, Type = ShopSettingType.Service, Name = nameof(PrimaryServiceName.ImportService) },
            new ShopSettings { Id = 6, ShopId = 1, ParentSettingsId = 1, Type = ShopSettingType.Service, Name = nameof(PrimaryServiceName.WebLoader) },
            new ShopSettings { Id = 7, ShopId = 1, ParentSettingsId = 2, Type = ShopSettingType.Service, Name = nameof(PrimaryServiceName.ImportService) },
            new ShopSettings { Id = 8, ShopId = 1, ParentSettingsId = 2, Type = ShopSettingType.Service, Name = nameof(PrimaryServiceName.BrowserDataLoader) },
            new ShopSettings { Id = 9, ShopId = 2, ParentSettingsId = 3, Type = ShopSettingType.Service, Name = nameof(PrimaryServiceName.ImportService) },
            new ShopSettings { Id = 10, ShopId = 2, ParentSettingsId = 3, Type = ShopSettingType.Service, Name = nameof(PrimaryServiceName.RequestHeaders) },
            new ShopSettings{ Id = 11, ShopId = 2, ParentSettingsId = 4, Type = ShopSettingType.Service, Name = nameof(PrimaryServiceName.ImportService) },
            new ShopSettings { Id = 12, ShopId = 2, ParentSettingsId = 4, Type = ShopSettingType.Service, Name = nameof(PrimaryServiceName.WebLoader) },
            new ShopSettings { Id = 14, ShopId = 2, ParentSettingsId = 4, Type = ShopSettingType.Service, Name = nameof(PrimaryServiceName.BrowserLauncher) },
            new ShopSettings{ Id = 13, ShopId = 3, Type = ShopSettingType.Product,  Name="shop3Products" },
        ];

    public LoadSettingsFromDB()
    {
        _adapters.Add(ShopSettingType.Product, new SettingsDataAdapter<TestProductShopImportSettings, TestImportServiceSettings>
            (
            _shopSettingsDataServiceMock.Object,
            ShopSettingType.Product
            ));

        _adapters.Add(ShopSettingType.Category, new SettingsDataAdapter<TestCategoryShopImportSettings, TestImportServiceSettings>
            (
            _shopSettingsDataServiceMock.Object,
            ShopSettingType.Category
            ));

        InitializeSettings();

        _shopSettingsDataServiceMock.Setup(s => s.GetShopSettings(It.IsAny<int>(), It.IsAny<ShopSettingType>(), It.IsAny<CancellationToken>()))
            .Returns(GetShopImportSettingsByShopIdAndSettingsType);
        _shopSettingsDataServiceMock.Setup(s => s.GetShopSettings(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(GetShopImportSettingsById);
        _shopSettingsDataServiceMock.Setup(s => s.GetShopSettings(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(GetShopImportSettingsByName);
        _shopSettingsDataServiceMock.Setup(s => s.GetChildSettings(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(GetChildSettings);
        _shopSettingsDataServiceMock.Setup(s => s.GetAllParentShopSettings(It.IsAny<CancellationToken>()))
            .Returns(GetAllParentdSettings);
    }

    private void InitializeSettings()
    {
        foreach (var shopSetting in _shopSettings.Where(s => s.Type == ShopSettingType.Product))
        {
            var importSettings = new TestProductShopImportSettings() { ShopUrl = $"https://url{shopSetting.Id}" };
            shopSetting.JsonValue = JsonSerializer.Serialize(importSettings);
        }        
        
        foreach (var shopSetting in _shopSettings.Where(s => s.Type == ShopSettingType.Category))
        {
            var importSettings = new TestCategoryShopImportSettings() { ShopUrl = $"https://url{shopSetting.Id}" };
            shopSetting.JsonValue = JsonSerializer.Serialize(importSettings);
        }

        foreach (var shopServiceSetting in _shopSettings.Where(s => s.Type == ShopSettingType.Service))
        {
            var serviceSettings = shopServiceSetting.ToImportServiceSettings<TestImportServiceSettings>()
                ?? new TestImportServiceSettings
                {
                    Name = shopServiceSetting.Name,
                    ParentSettingsId = shopServiceSetting.ParentSettingsId,
                    Id = shopServiceSetting.Id,
                    ShopId = shopServiceSetting.ShopId
                };
            serviceSettings.AssemblyPath = $"C:\\Folder{shopServiceSetting.Id}";
            serviceSettings.ImplementationTypeName = $"ServiceImplementation_{serviceSettings.Name}_{shopServiceSetting.ParentSettingsId}";
            serviceSettings.ServiceTypeName = $"ServiceType_{serviceSettings.Name}_{shopServiceSetting.ParentSettingsId}";

            shopServiceSetting.JsonValue = JsonSerializer.Serialize(serviceSettings);
        }
    }

    private async Task<IShopSettings?> GetShopImportSettingsByShopIdAndSettingsType(int shopId, ShopSettingType shopSettingType, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_shopSettings.FirstOrDefault(s => s.ShopId == shopId && s.Type == shopSettingType));
    }

    private async Task<IShopSettings?> GetShopImportSettingsById(int id, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_shopSettings.FirstOrDefault(s => s.Id == id));
    }

    private async Task<IShopSettings?> GetShopImportSettingsByName(string name, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_shopSettings.FirstOrDefault(s => s.Name == name));
    }

    private async Task<List<IShopSettings>?> GetChildSettings(int parentId, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_shopSettings.Where(s => s.ParentSettingsId == parentId).ToList());
    }

    private async Task<List<IShopSettings>> GetAllParentdSettings(CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_shopSettings.Where(s => s.ParentSettingsId == null).ToList());
    }

    [Fact]
    public async Task LoadProductSettingsByShopIdAndSettingsType()
    {
        var productShopSettings = _shopSettings.Where(s => s.Type == ShopSettingType.Product 
            && _shopSettings.Count(s1 => s1.Type == ShopSettingType.Service && s1.ParentSettingsId == s.Id) > 0).ToArray();
        var shopSettings = productShopSettings[new Random().Next(0, productShopSettings.Length)];
        var services = _shopSettings.Where(s => s.Type == ShopSettingType.Service && s.ParentSettingsId == shopSettings.Id);

        var shopImportSettings = (await _adapters[ShopSettingType.Product].GetShopImportSettings(shopSettings.ShopId)) as TestShopImportSettings;

        Assert.NotNull(shopImportSettings);
        Assert.Equal(services.Count(), shopImportSettings.Services.Count);

        AssertService.PrimaryServicesExist(services, shopImportSettings);
        
        Assert.Equal($"ServiceType_ImportService_{shopImportSettings.Id}", 
            shopImportSettings.GetImportService<TestImportServiceSettings>()?.ServiceTypeName);

        Assert.Equal($"ServiceImplementation_ImportService_{shopImportSettings.Id}", 
            shopImportSettings.GetImportService<TestImportServiceSettings>()?.ImplementationTypeName);
    }

    [Fact]
    public async Task LoadCategorySettingsByShopIdAndSettingsType()
    {
        var categoryShopSettings = _shopSettings.Where(s => s.Type == ShopSettingType.Category
            && _shopSettings.Count(s1 => s1.Type == ShopSettingType.Service && s1.ParentSettingsId == s.Id) > 0).ToArray();
        var shopSettings = categoryShopSettings[new Random().Next(0, categoryShopSettings.Length)];
        var services = _shopSettings.Where(s => s.Type == ShopSettingType.Service && s.ParentSettingsId == shopSettings.Id);

        var shopImportSettings = (await _adapters[ShopSettingType.Category].GetShopImportSettings(shopSettings.ShopId)) as TestShopImportSettings;

        Assert.NotNull(shopImportSettings);
        Assert.NotEmpty(shopImportSettings.Services);
        AssertService.PrimaryServicesExist(services, shopImportSettings);
       
        Assert.Equal($"ServiceType_ImportService_{shopImportSettings.Id}",
            shopImportSettings.GetImportService<TestImportServiceSettings>()?.ServiceTypeName);

        Assert.Equal($"ServiceImplementation_ImportService_{shopImportSettings.Id}", 
            shopImportSettings.GetImportService<TestImportServiceSettings>()?.ImplementationTypeName);
    }

    [Fact]
    public async Task LoadSettingsByName()
    {
        var allServices = _shopSettings.Where(s => s.Type == ShopSettingType.Service);
        var allShopSettingsWithServices = _shopSettings.Where(s => s.Type != ShopSettingType.Service && allServices.Any(ss=>ss.ParentSettingsId == s.Id)).ToArray();
        var shopSettings = allShopSettingsWithServices[new Random().Next(0, allShopSettingsWithServices.Length)];
        var services = _shopSettings.Where(s => s.Type == ShopSettingType.Service && s.ParentSettingsId == shopSettings.Id);

        var shopImportSettings = (await _adapters[shopSettings.Type].GetShopImportSettings(shopSettings.Name)) as TestShopImportSettings ;

        Assert.NotNull(shopImportSettings);

        Assert.Equal(shopSettings.Type, shopImportSettings.Type);
        Assert.NotEmpty(shopImportSettings.Services);
        AssertService.PrimaryServicesExist(services, shopImportSettings);

        Assert.Equal($"ServiceType_ImportService_{shopImportSettings.Id}", shopImportSettings.GetImportService<TestImportServiceSettings>()?.ServiceTypeName);
        Assert.Equal($"ServiceImplementation_ImportService_{shopImportSettings.Id}", shopImportSettings.GetImportService<TestImportServiceSettings>()?.ImplementationTypeName);
    }

    [Fact]
    public async Task LoadAllSettings()
    {
        var allNotServiceSettings = _shopSettings.Where(s => s.Type != ShopSettingType.Service).ToArray();

        var allSettings = await _adapters[ShopSettingType.Product].GetAllShopImportSettings();
        Assert.Equal(allNotServiceSettings.Length, allSettings.Count);
        Assert.Equal(_shopSettings.Count - allNotServiceSettings.Length, 
            allSettings.SelectMany(s => s.Value.Services.OfType<KeyValuePair<string,TestImportServiceSettings>>()).Count());
    }
}