using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.Entities;
using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Import.WebApp.Models;
using Alchemist.Product.Import.WebApp.Test.Infrastructure;
using Alchemist.Product.Interfaces;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using Moq;
using System.Text.Json;
using Xunit.Abstractions;

namespace Alchemist.Product.Import.WebApp.Test;

public abstract class ImportWebAppTest : PageTest, IClassFixture<TestImportWebAppFactory>
{
    protected readonly TestImportWebAppFactory _webAppFactory;

    protected readonly ITestOutputHelper _testOutputHelper;

    protected readonly ModelJsonConverter<IServiceSettingsModel> _serviceSettingsModelConverter;

    protected readonly ModelJsonConverter<IShopServicesSettingsModel> _shopImportSettingsJsonConverter;

    protected readonly List<Interfaces.IShop> _shops =
    [
           new Shop { Id = 1, Name = "TestShop1", Url = "https://testshop1" } ,
           new Shop { Id = 2, Name = "TestShop2", Url = "https://testshop2" },
           new Shop { Id = 3, Name = "TestShop3", Url = "https://testshop3" },
    ];

    protected readonly List<Interfaces.IShopSettings> _shopSettings =
    [
            new ShopSettings { Id = 1, ShopId = 1, Type = ShopSettingType.Product, Name=Guid.NewGuid().ToString() },
            new ShopSettings { Id = 2, ShopId = 2, Type = ShopSettingType.Product, Name=Guid.NewGuid().ToString() },
            new ShopSettings { Id = 3, ShopId = 1, Type = ShopSettingType.Category, Name=Guid.NewGuid().ToString() },
            new ShopSettings { Id = 4, ShopId = 2, Type = ShopSettingType.Category, Name=Guid.NewGuid().ToString() },
            new ShopSettings { Id = 5, ShopId = 1, ParentSettingsId = 1, Type = Interfaces.ShopSettingType.Service, Name = nameof(Alchemist.Import.Settings.Interfaces.IShopImportSettings.ImportService) },
            new ShopSettings { Id = 6, ShopId = 1, ParentSettingsId = 1, Type = Interfaces.ShopSettingType.Service, Name = nameof(Alchemist.Import.Settings.Interfaces.IShopImportSettings.WebLoader) },
            new ShopSettings { Id = 7, ShopId = 1, ParentSettingsId = 2, Type = Interfaces.ShopSettingType.Service, Name = nameof(Alchemist.Import.Settings.Interfaces.IShopImportSettings.ImportService) },
            new ShopSettings { Id = 8, ShopId = 1, ParentSettingsId = 2, Type = Interfaces.ShopSettingType.Service, Name = nameof(Alchemist.Import.Settings.Interfaces.IShopImportSettings.BrowserDataLoader) },
            new ShopSettings { Id = 9, ShopId = 2, ParentSettingsId = 3, Type = Interfaces.ShopSettingType.Service, Name = nameof(Alchemist.Import.Settings.Interfaces.IShopImportSettings.ImportService) },
            new ShopSettings { Id = 10, ShopId = 2, ParentSettingsId = 3, Type = Interfaces.ShopSettingType.Service, Name = nameof(Alchemist.Import.Settings.Interfaces.IShopImportSettings.RequestHeaders) },
            new ShopSettings{ Id = 11, ShopId = 2, ParentSettingsId = 4, Type = Interfaces.ShopSettingType.Service, Name = nameof(Alchemist.Import.Settings.Interfaces.IShopImportSettings.ImportService) },
            new ShopSettings { Id = 12, ShopId = 2, ParentSettingsId = 4, Type = Interfaces.ShopSettingType.Service, Name = nameof(Alchemist.Import.Settings.Interfaces.IShopImportSettings.WebLoader) },
            new ShopSettings{ Id = 13, ShopId = 3, Type = ShopSettingType.Product },
        ];

    protected ImportWebAppTest(TestImportWebAppFactory webAppFactory, ITestOutputHelper testOutputHelper, int? httpPort = null, int? httpsPort = null)
    {
        _testOutputHelper = testOutputHelper;


        var defaultServiceProperties = new Dictionary<string, object>()
        {
            { nameof(IServiceSettingsModel.Id), 0 },
            { nameof(IServiceSettingsModel.ParentSettingsId), 0 },
            { nameof(IServiceSettingsModel.ShopId), 0 },
            { nameof(IServiceSettingsModel.ShopGuid), Guid.Empty },
            { nameof(IServiceSettingsModel.ShopSettingsGuid), Guid.Empty },
        };
        _serviceSettingsModelConverter = new ModelJsonConverter<IServiceSettingsModel>(defaultServiceProperties);

        var defaultShopSettingsProperties = new Dictionary<string, object>()
        {
            { nameof(IShopServicesSettingsModel.Id), 0 },
            { nameof(IShopServicesSettingsModel.ShopId), 0 },
            { nameof(IShopServicesSettingsModel.ShopGuid), Guid.Empty }
        };
        _shopImportSettingsJsonConverter = new ModelJsonConverter<IShopServicesSettingsModel>(defaultShopSettingsProperties);

        _webAppFactory = webAppFactory;
        if (httpPort != null) _webAppFactory.HttpPort = httpPort.Value;
        if (httpsPort != null) _webAppFactory.HttpsPort = httpsPort.Value;        

        _webAppFactory.ShopAPIClient.Reset();
        _webAppFactory.SettingsAPIClient.Reset();
        _webAppFactory.Reset();

        _webAppFactory.ShopAPIClient.Setup(s => s.GetShops()).Returns(Task.FromResult(_shops));

        _webAppFactory.SettingsAPIClient.Setup(s => s.GetShopSettings(It.IsAny<int>(), It.IsAny<Interfaces.ShopSettingType>()))
           .Returns((int shopId, ShopSettingType shopSettingType) =>
           {
               return Task.FromResult(_shopSettings.FirstOrDefault(s => s.ShopId == shopId && s.Type == shopSettingType));
           });

        _webAppFactory.SettingsAPIClient.Setup(s => s.GetChildSettings(It.IsAny<int>()))
            .Returns((int parentSettingsId) =>
            {
                return Task.FromResult(_shopSettings.Where(s => s.ParentSettingsId == parentSettingsId && s.Type == Interfaces.ShopSettingType.Service).ToList());
            });

        foreach (var shopSetting in _shopSettings.Where(s => s.Type == ShopSettingType.Product))
        {
            var productShopImportSettings = new ProductShopSettingsModel(shopSetting.ShopId, shopSetting.Id, Guid.NewGuid()) { Name = shopSetting.Name, 
                ProductUrlFormat = $"https://url{shopSetting.Id}_product", 
                CategoryUrlFormat = $"https://url{shopSetting.Id}_category" };
            shopSetting.JsonValue = JsonSerializer.Serialize(productShopImportSettings);
        }

        foreach (var shopSetting in _shopSettings.Where(s => s.Type == ShopSettingType.Category))
        {
            var categoryShopImportSettings = new CategoryShopSettingsModel(shopSetting.ShopId, shopSetting.Id, Guid.NewGuid())
            {
                Name = shopSetting.Name,
                CategorySourceUrl = $"https://url{shopSetting.Id}_category",
            };
            shopSetting.JsonValue = JsonSerializer.Serialize(categoryShopImportSettings);
        }

        foreach (var shopSetting in _shopSettings.Where(s => s.Type == ShopSettingType.Service))
        {
            var serviceSettings = shopSetting.ToImportServiceSettings<ServiceSettingsModel>(_serviceSettingsModelConverter)
                ?? new ServiceSettingsModel(shopSetting.ShopId, shopSetting.Id, 0, Guid.NewGuid(), Guid.NewGuid());
            serviceSettings.AssemblyPath = $"C:\\Folder{shopSetting.Id}";
            serviceSettings.ImplementationTypeName = $"ServiceImplementation{shopSetting.Id}";
            serviceSettings.ServiceTypeName = $"ServiceType{shopSetting.Id}";

            shopSetting.JsonValue = JsonSerializer.Serialize(serviceSettings);
        }
    } 
    
    protected static IShopSettings CreateServiceSettings(int shopId, int shopSettingId, int id)
    {
        IShopSettings serviceSettings = new ShopSettings { Id = id, 
            ShopId = shopId,
            ParentSettingsId = shopSettingId,
            Type = ShopSettingType.Service,
            Name = Guid.NewGuid().ToString()
        };

        var service = serviceSettings.ToImportServiceSettings<ServiceSettingsModel>()
            ?? new ServiceSettingsModel(shopId, id, shopSettingId, Guid.NewGuid(), Guid.NewGuid());
        service.ServiceTypeName = Guid.NewGuid().ToString();
        serviceSettings.JsonValue = JsonSerializer.Serialize(service);
        
        return serviceSettings;
    }

    protected async Task<ShopSettingsModel?> GetShopImportSettingsAsync(int shopId, Interfaces.ShopSettingType shopSettingType)
    {
        var shopSetting = _shopSettings.First(s => s.ShopId == shopId && s.Type == shopSettingType);

        ShopSettingsModel shopImportSettings = shopSetting.Type == ShopSettingType.Product 
            ? await shopSetting.GetShopImportSettings<ProductShopSettingsModel, ServiceSettingsModel>((parentSettingsId) => Task.FromResult(_shopSettings.Where(s => s.ParentSettingsId == parentSettingsId).ToList()))
            : await shopSetting.GetShopImportSettings<CategoryShopSettingsModel, ServiceSettingsModel>((parentSettingsId) => Task.FromResult(_shopSettings.Where(s => s.ParentSettingsId == parentSettingsId).ToList()));

        return shopImportSettings;
    }

    protected async Task<ShopSettingsModel> ExpectLoadIndexPageAsync(IPage page)
    {
        await Context.ClearCookiesAsync();

        var response = await page.GotoAsync(_webAppFactory.ServerAddress);
        Assert.True(response?.Ok);

        var nameLocator = await this.ExpectSingleElementAsync(page, "#ShopSettingsName");        

        var shop = _shops.First();
        var shopImportSettings = await GetShopImportSettingsAsync(shop.Id, ShopSettingType.Product);
        var productShopSettings = Assert.IsType<ProductShopSettingsModel>(shopImportSettings);

        await Expect(nameLocator).ToHaveValueAsync(productShopSettings.Name);

        return await Task.FromResult(productShopSettings);
    }

    protected async Task<ShopSettingsModel> ExpectForSelectShopAsync(IPage page, IShop shop)
    { 
        var locator = page.Locator("form[name='itemShopForm']").GetByText(shop.Name);
        await Expect(locator).ToHaveCountAsync(1);

        await locator.ClickAsync();

        await ExpectSelectedShopAsync(page, shop);

        var shopSettings = await GetShopImportSettingsAsync(shop.Id, ShopSettingType.Product);
        var productShopSettings = Assert.IsType<ProductShopSettingsModel>(shopSettings);
        await Expect(page.Locator("#ProductUrlFormat")).ToHaveValueAsync(productShopSettings.ProductUrlFormat);
        
        return productShopSettings;
    }

    protected async Task ExpectSelectedShopAsync(IPage page, IShop shop)
    {
        var currentSelectedLocator = page.Locator("li[class='left-menu-ul selected']");
        await Expect(currentSelectedLocator).ToHaveTextAsync(shop.Name);
    }

    protected async Task ClickSelectShopAsync(IPage page, IShop shop)
    {
        var locator = page.Locator("form[name='itemShopForm']").GetByText(shop.Name);
        await Expect(locator).ToHaveCountAsync(1);

        await locator.ClickAsync();
    }

    protected async Task ExpectSelectTabAsync(IPage page, TabType tabType, string tabText, TabType prevTabType)
    {
        var menu = page.Locator("#menuDiv");
        var tab = menu.GetByText(TabHelper.TabNames[tabType]);
        await Expect(tab).ToHaveCountAsync(1);

        await tab.ClickAsync();

        await Expect(menu.Locator("div[class = 'menu_item selected']").GetByText(TabHelper.TabNames[prevTabType]))
          .ToHaveCountAsync(0);
        await Expect(menu.Locator("div[class = 'menu_item selected']").GetByText(TabHelper.TabNames[tabType]))
          .ToHaveCountAsync(1);

        await Expect(page.GetByText(tabText)).ToHaveCountAsync(1);
        await Expect(page.Locator("#tabsMenuDiv")).ToHaveCountAsync(0);
    }

    protected async Task ExpectSelectedTabAsync(IPage page, TabType tabType)
    {
        var menu = page.Locator("#menuDiv");
        await Expect(menu.Locator("div[class = 'menu_item selected']").GetByText(TabHelper.TabNames[tabType]))
          .ToHaveCountAsync(1);
    }

    protected async Task ClickSelectTabAsync(IPage page, TabType tabType)
    {
        var menu = page.Locator("#menuDiv");
        var tab = menu.GetByText(TabHelper.TabNames[tabType]);
        await Expect(tab).ToHaveCountAsync(1);

        await tab.ClickAsync();
    }

    protected async Task<ILocator> GetConfirmationLocatorAsync(IPage page)
    {
        var confirmationLocator = page.Locator("div[class='jconfirm-box jconfirm-hilight-shake jconfirm-type-default jconfirm-type-animated']");
        await Expect(confirmationLocator).Not.ToBeVisibleAsync();
        return confirmationLocator;
    }

    protected async Task ExpectConfirmationAsync(ILocator confirmationLocator)
    {
        await Expect(confirmationLocator).ToBeVisibleAsync();

        var buttonsLocator = confirmationLocator.Locator("div[class='jconfirm-buttons']");
        await Expect(buttonsLocator).ToContainTextAsync("Yes");
        await Expect(buttonsLocator).ToContainTextAsync("cancel");
    }

    protected async Task<ILocator> CancelConfirmationAsync(ILocator confirmationLocator)
    {
        var cancel = confirmationLocator.Locator("button").GetByText("cancel");
        await Expect(cancel).ToHaveCountAsync(1);
        await cancel.ClickAsync();
        return cancel;
    }
    
    protected async Task<ILocator> ConfirmAsync(ILocator confirmationLocator)
    {
        var yes = confirmationLocator.Locator("button").GetByText("Yes");
        await Expect(yes).ToHaveCountAsync(1);
        await yes.ClickAsync();
        return yes;
    }
}
