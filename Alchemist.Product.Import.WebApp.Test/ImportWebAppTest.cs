using Alchemist.Import.Settings.Model;
using Alchemist.Product.Entities;
using Alchemist.Product.Interfaces;
using Microsoft.Playwright.Xunit;
using Moq;
using System.Text.Json;
using Alchemist.Import.Settings.Extensions;
using Xunit.Abstractions;
using Alchemist.Product.Import.Model.Infrastructure;
using Microsoft.Playwright;
using Alchemist.Import.Settings.Interfaces;

namespace Alchemist.Product.Import.WebApp.Test;

public abstract class ImportWebAppTest : PageTest, IClassFixture<TestImportWebAppFactory>
{
    protected readonly TestImportWebAppFactory _webAppFactory;

    protected readonly ITestOutputHelper _testOutputHelper;

    protected readonly List<IShop> _shops =
    [
           new Shop { Id = 1, Name = "TestShop1", Url = "https://testshop1" } ,
           new Shop { Id = 2, Name = "TestShop2", Url = "https://testshop2" },
           new Shop { Id = 3, Name = "TestShop3", Url = "https://testshop3" },
    ];

    protected readonly List<IShopSettings> _shopSettings =
    [
            new ShopSettings { Id = 1, ShopId = 1, Type = ShopSettingType.Product },
            new ShopSettings { Id = 2, ShopId = 2, Type = ShopSettingType.Product },
            new ShopSettings { Id = 3, ShopId = 1, Type = ShopSettingType.Category },
            new ShopSettings { Id = 4, ShopId = 2, Type = ShopSettingType.Category },
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

    protected ImportWebAppTest(TestImportWebAppFactory webAppFactory, ITestOutputHelper testOutputHelper)
    {
        _webAppFactory = webAppFactory;
        _testOutputHelper = testOutputHelper;

        _webAppFactory.ShopAPIClient.Setup(s => s.GetShops()).Returns(Task.FromResult(_shops));

        _webAppFactory.SettingsAPIClient.Setup(s => s.GetShopSettings(It.IsAny<int>(), It.IsAny<ShopSettingType>()))
           .Returns((int shopId, ShopSettingType shopSettingType) =>
           {
               return Task.FromResult(_shopSettings.FirstOrDefault(s => s.ShopId == shopId && s.Type == shopSettingType));
           });

        _webAppFactory.SettingsAPIClient.Setup(s => s.GetChildSettings(It.IsAny<int>()))
            .Returns((int parentSettingsId) =>
            {
                return Task.FromResult(_shopSettings.Where(s => s.ParentSettingsId == parentSettingsId && s.Type == ShopSettingType.Service).ToList());
            });

        foreach (var shopSetting in _shopSettings.Where(s => s.Type == ShopSettingType.Product))
        {
            var productShopImportSettings = new ProductShopImportSettings() { Url = $"https://url{shopSetting.Id}" };
            shopSetting.JsonValue = JsonSerializer.Serialize(productShopImportSettings);
        }

        foreach (var shopSetting in _shopSettings.Where(s => s.Type == ShopSettingType.Service))
        {
            var serviceSettings = shopSetting.ToImportServiceSettings<ImportServiceSettings>();
            serviceSettings.AssemblyPath = $"C:\\Folder{shopSetting.Id}";
            serviceSettings.ImplementationTypeName = $"Service{shopSetting.Id}";

            shopSetting.JsonValue = JsonSerializer.Serialize(serviceSettings);
        }
    }    

    protected async Task<ShopImportSettings?> GetShopImportSettingsAsync(int shopId, ShopSettingType shopSettingType)
    {
        var shopSetting = _shopSettings.First(s => s.ShopId == shopId && s.Type == shopSettingType);        

        ShopImportSettings shopImportSettings = shopSetting.Type == ShopSettingType.Product 
            ? await shopSetting.GetShopImportSettings<ProductShopImportSettings, ImportServiceSettings>((parentSettingsId) => Task.FromResult(_shopSettings.Where(s => s.ParentSettingsId == parentSettingsId).ToList()))
            : await shopSetting.GetShopImportSettings<CategoryShopImportSettings, ImportServiceSettings>((parentSettingsId) => Task.FromResult(_shopSettings.Where(s => s.ParentSettingsId == parentSettingsId).ToList()));

        return shopImportSettings;
    }

    protected async Task<ShopImportSettings> ExpectLoadIndexPageAsync(IPage page)
    {
        await Context.ClearCookiesAsync();

        var response = await page.GotoAsync(_webAppFactory.ServerAddress);
        Assert.True(response?.Ok);

        var urlLocator = page.Locator("#Url");
        await Expect(urlLocator).ToHaveCountAsync(1);

        var shop = _shops.First();
        var productShopSettings = await GetShopImportSettingsAsync(shop.Id, ShopSettingType.Product);

        await Expect(urlLocator).ToHaveValueAsync(productShopSettings.Url);

        return await Task.FromResult(productShopSettings);
    }

    protected async Task<ShopImportSettings> ExpectForSelectShopAsync(IPage page, IShop shop)
    { 
        var locator = page.Locator("form[name='itemShopForm']").GetByText(shop.Name);
        await Expect(locator).ToHaveCountAsync(1);

        await locator.ClickAsync();

        var currentSelectedLocator = page.Locator("li[class='left-menu-ul selected']");
        await Expect(currentSelectedLocator).ToHaveTextAsync(shop.Name);

        var shopSettings = await GetShopImportSettingsAsync(shop.Id, ShopSettingType.Product);
        await Expect(page.Locator("#Url")).ToHaveValueAsync(shopSettings.Url);
        
        return shopSettings;
    }

    protected async Task SelectTabAsync(IPage page, TabType tabType, string tabText, TabType prevTabType)
    {
        var menu = page.Locator("#menuDiv");
        var tab = menu.GetByText(ViewHelper.TabNames[tabType]);
        await Expect(tab).ToHaveCountAsync(1);

        await tab.ClickAsync();

        await Expect(menu.Locator("div[class = 'menu_item selected']").GetByText(ViewHelper.TabNames[prevTabType]))
          .ToHaveCountAsync(0);
        await Expect(menu.Locator("div[class = 'menu_item selected']").GetByText(ViewHelper.TabNames[tabType]))
          .ToHaveCountAsync(1);

        await Expect(page.GetByText(tabText)).ToHaveCountAsync(1);
        await Expect(page.Locator("#tabsMenuDiv")).ToHaveCountAsync(0);
    }

    protected async Task ExpectWithNullValueAsync(ILocator locator, string? value)
    {
        await Expect(locator).ToBeVisibleAsync();

        if (!string.IsNullOrEmpty(value))
            await Expect(locator).ToHaveValueAsync(value);
        else
            await Expect(locator).ToHaveValueAsync("");
    }
}
