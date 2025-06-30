using Alchemist.DataService.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Text.Json;
using Alchemist.Import.Settings.Extensions; 


using ShopSettingType = Alchemist.Product.Interfaces.ShopSettingType;
using Alchemist.Import.Settings.Test.Model;

namespace Alchemist.Import.Settings.DataAdapter.Tests;

public class LoadSettingsFromDB
{
    private readonly ISettingsDataAdapter _adapter;

    private readonly Mock<IShopSettingsDataService> _shopSettingsDataServiceMock = new();

    private readonly List<IShopSettings> _shopSettings =
    [
            new ShopSettings { Id = 1, ShopId = 1, Type = ShopSettingType.Product, Name="shop1Products" },
            new ShopSettings { Id = 2, ShopId = 2, Type = ShopSettingType.Product, Name="Shop2Products" },
            new ShopSettings { Id = 3, ShopId = 1, Type = ShopSettingType.Category, Name= "shop1Category" },
            new ShopSettings { Id = 4, ShopId = 2, Type = ShopSettingType.Category, Name="shop2Category" },
            new ShopSettings { Id = 5, ShopId = 1, ParentSettingsId = 1, Type = ShopSettingType.Service, Name = nameof(IShopImportSettings.ImportService) },
            new ShopSettings { Id = 6, ShopId = 1, ParentSettingsId = 1, Type = ShopSettingType.Service, Name = nameof(IShopImportSettings.WebLoader) },
            new ShopSettings { Id = 7, ShopId = 1, ParentSettingsId = 2, Type = ShopSettingType.Service, Name = nameof(IShopImportSettings.ImportService) },
            new ShopSettings { Id = 8, ShopId = 1, ParentSettingsId = 2, Type = ShopSettingType.Service, Name = nameof(IShopImportSettings.BrowserDataLoader) },
            new ShopSettings { Id = 9, ShopId = 2, ParentSettingsId = 3, Type = ShopSettingType.Service, Name = nameof(IShopImportSettings.ImportService) },
            new ShopSettings { Id = 10, ShopId = 2, ParentSettingsId = 3, Type = ShopSettingType.Service, Name = nameof(IShopImportSettings.RequestHeaders) },
            new ShopSettings{ Id = 11, ShopId = 2, ParentSettingsId = 4, Type = ShopSettingType.Service, Name = nameof(IShopImportSettings.ImportService) },
            new ShopSettings { Id = 12, ShopId = 2, ParentSettingsId = 4, Type = ShopSettingType.Service, Name = nameof(IShopImportSettings.WebLoader) },
            new ShopSettings{ Id = 13, ShopId = 3, Type = ShopSettingType.Product },
        ];

    public LoadSettingsFromDB()
    {
        var builder = new HostApplicationBuilder();
        builder.Services.AddSingleton(_shopSettingsDataServiceMock.Object);
        builder.Services.AddSettingsDataAdapter<TestProductShopImportSettings, TestCategoryShopImportSettings, TestImportServiceSettings>();
        var host = builder.Build();
        _adapter = host.Services.GetRequiredService<ISettingsDataAdapter>();

        InitializeSettings();

        _shopSettingsDataServiceMock.Setup(s => s.GetShopSettings(It.IsAny<int>(), It.IsAny<ShopSettingType>())).Returns(GetShopImportSettingsByShopIdAndSettingsType);
        _shopSettingsDataServiceMock.Setup(s => s.GetShopSettings(It.IsAny<int>())).Returns(GetShopImportSettingsById);
        _shopSettingsDataServiceMock.Setup(s => s.GetShopSettings(It.IsAny<string>())).Returns(GetShopImportSettingsByName);
        _shopSettingsDataServiceMock.Setup(s => s.GetChildSettings(It.IsAny<int>())).Returns(GetChildSettings);
        _shopSettingsDataServiceMock.Setup(s => s.GetAllParentShopSettings()).Returns(GetAllParentdSettings);
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
            var serviceSettings = shopServiceSetting.ToImportServiceSettings<TestImportServiceSettings>();
            serviceSettings.AssemblyPath = $"C:\\Folder{shopServiceSetting.Id}";
            serviceSettings.ImplementationTypeName = $"ServiceImplementation_{serviceSettings.Name}_{shopServiceSetting.ParentSettingsId}";
            serviceSettings.ServiceTypeName = $"ServiceType_{serviceSettings.Name}_{shopServiceSetting.ParentSettingsId}";

            shopServiceSetting.JsonValue = JsonSerializer.Serialize(serviceSettings);
        }
    }

    private async Task<IShopSettings?> GetShopImportSettingsByShopIdAndSettingsType(int shopId, ShopSettingType shopSettingType )
    {
        return await Task.FromResult(_shopSettings.FirstOrDefault(s => s.ShopId == shopId && s.Type == shopSettingType));
    }

    private async Task<IShopSettings?> GetShopImportSettingsById(int id)
    {
        return await Task.FromResult(_shopSettings.FirstOrDefault(s => s.Id == id));
    }

    private async Task<IShopSettings?> GetShopImportSettingsByName(string name)
    {
        return await Task.FromResult(_shopSettings.FirstOrDefault(s => s.Name == name));
    }

    private async Task<List<IShopSettings>?> GetChildSettings(int parentId)
    {
        return await Task.FromResult(_shopSettings.Where(s => s.ParentSettingsId == parentId).ToList());
    }

    private async Task<List<IShopSettings>> GetAllParentdSettings()
    {
        return await Task.FromResult(_shopSettings.Where(s => s.ParentSettingsId == null).ToList());
    }

    [Fact]
    public async Task LoadProductSettingsByShopIdAndSettingsType()
    {        
        var shopSettings = await _adapter.GetShopImportSettings(1, Interfaces.ShopSettingType.Product);
        Assert.NotNull(shopSettings);
        Assert.Equal(2, shopSettings.Services.Count);
        Assert.NotNull(shopSettings.ImportService);
        Assert.Null(shopSettings.BrowserDataLoader);
        Assert.NotNull(shopSettings.WebLoader);
        Assert.Null(shopSettings.RequestHeaders);
        
        Assert.Equal($"ServiceType_ImportService_{shopSettings.Id}", shopSettings.ImportService.ServiceTypeName);
        Assert.Equal($"ServiceImplementation_ImportService_{shopSettings.Id}", shopSettings.ImportService.ImplementationTypeName);
    }

    [Fact]
    public async Task LoadCategorySettingsByShopIdAndSettingsType()
    {
        var shopSettings = await _adapter.GetShopImportSettings(1, Interfaces.ShopSettingType.Category);
        Assert.NotNull(shopSettings);
        Assert.NotEmpty(shopSettings.Services);
        Assert.NotNull(shopSettings.ImportService);
        Assert.NotNull(shopSettings.RequestHeaders);
        Assert.Null(shopSettings.WebLoader);
        Assert.Null(shopSettings.BrowserDataLoader);

        Assert.Equal($"ServiceType_ImportService_{shopSettings.Id}", shopSettings.ImportService.ServiceTypeName);
        Assert.Equal($"ServiceImplementation_ImportService_{shopSettings.Id}", shopSettings.ImportService.ImplementationTypeName);
    }

    [Fact]
    public async Task LoadSettingsByName()
    {
        var shopSettings = await _adapter.GetShopImportSettings("shop1Category");
        Assert.NotNull(shopSettings);
        Assert.Equal(Interfaces.ShopSettingType.Category, shopSettings.ShopSettingType);
        Assert.NotEmpty(shopSettings.Services);
        Assert.NotNull(shopSettings.ImportService);
        Assert.NotNull(shopSettings.RequestHeaders);
        Assert.Null(shopSettings.WebLoader);
        Assert.Null(shopSettings.BrowserDataLoader);

        Assert.Equal($"ServiceType_ImportService_{shopSettings.Id}", shopSettings.ImportService.ServiceTypeName);
        Assert.Equal($"ServiceImplementation_ImportService_{shopSettings.Id}", shopSettings.ImportService.ImplementationTypeName);
    }

    [Fact]
    public async Task LoadAllSettings()
    {
        var allSettings = await _adapter.GetAllShopImportSettings();
        Assert.Equal(5, allSettings.Count);
        Assert.Equal(8, allSettings.SelectMany(s => s.Services.OfType<TestImportServiceSettings>()).Count());
    }
}