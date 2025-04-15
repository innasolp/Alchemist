using Alchemist.Import.Settings.Extensions;
using Alchemist.Import.Settings.Model;
using Alchemist.Product.Entities;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Interfaces;
using Microsoft.Playwright.Xunit;
using Moq;
using System.Text.Json;

namespace Alchemist.Product.Import.WebApp.Test;

public class ImportWebAppTest : PageTest, IClassFixture<TestImportWebAppFactory>
{    
    private readonly TestImportWebAppFactory _webAppFactory;

    private readonly List<IShop> _shops =
    [
            new Shop { Id = 1, Name = "TestShop1", Url = "https://testshop1" } ,
            new Shop { Id = 2, Name = "TestShop2", Url = "https://testshop2" },
            new Shop { Id = 3, Name = "TestShop3", Url = "https://testshop3" },
        ];

    private readonly List<IShopSettings> _shopSettings =
    [
            new ShopSettings { Id = 1, ShopId = 1, Type = ShopSettingType.Product },
            new ShopSettings { Id = 2, ShopId = 2, Type = ShopSettingType.Product },
            new ShopSettings { Id = 3, ShopId = 1, Type = ShopSettingType.Category },
            new ShopSettings { Id = 4, ShopId = 2, Type = ShopSettingType.Category },
            new ShopSettings { Id = 5, ShopId = 1, ParentSettingsId = 1, Type = ShopSettingType.Service },
            new ShopSettings { Id = 6, ShopId = 1, ParentSettingsId = 1, Type = ShopSettingType.Service },
            new ShopSettings { Id = 7, ShopId = 1, ParentSettingsId = 2, Type = ShopSettingType.Service },
            new ShopSettings { Id = 8, ShopId = 1, ParentSettingsId = 2, Type = ShopSettingType.Service },
            new ShopSettings { Id = 9, ShopId = 2, ParentSettingsId = 3, Type = ShopSettingType.Service },
            new ShopSettings { Id = 10, ShopId = 2, ParentSettingsId = 3, Type = ShopSettingType.Service },
            new ShopSettings{ Id = 11, ShopId = 2, ParentSettingsId = 4, Type = ShopSettingType.Service },
            new ShopSettings { Id = 12, ShopId = 2, ParentSettingsId = 4, Type = ShopSettingType.Service },
            new ShopSettings{ Id = 13, ShopId = 3, Type = ShopSettingType.Product },
        ];

    public ImportWebAppTest(TestImportWebAppFactory testImportWebAppFactory)
    {
        _webAppFactory = testImportWebAppFactory;
        
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
            var productShopImportSettings = new ProductShopImportSettings(){ Url = $"https://url{shopSetting.Id}"};
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

    [Fact]
    public async Task IndexPageContainsMenuDivAndShopList()
    {
        var response = await Page.GotoAsync(_webAppFactory.ServerAddress);
        Assert.True(response?.Ok);   

        var menudiv = Page.Locator("#menuDiv");
        Assert.Equal(1, await menudiv.CountAsync());
        
        var shopListPartialDiv = Page.Locator("#shopListPartialDiv");
        Assert.Equal(1, await shopListPartialDiv.CountAsync());

        var shopTabsLocator = Page.Locator("#tabsMenuDiv");
        Assert.Equal(1, await shopTabsLocator.CountAsync());
    }

    [Fact]
    public async Task IndexPageUploadShops()
    {     
        var response = await Page.GotoAsync(_webAppFactory.ServerAddress);
        Assert.True(response?.Ok);

        var shop = _shops.First();
        var shopSetting = _shopSettings.First(s => s.ShopId == shop.Id && s.Type == ShopSettingType.Product);
        var productShopSettings = JsonSerializer.Deserialize<ProductShopImportSettings>(shopSetting.JsonValue);

        var shopTabsLocator = Page.Locator("#tabsMenuDiv");
        Assert.Equal(1, await shopTabsLocator.Locator("div[class = 'menu_item-a selected-a']")
            .GetByText(ViewHelper.ShopSettingTypeNames[ShopSettingType.Product]).CountAsync());

        await Expect(Page.Locator("form[name='itemShopForm']")).ToHaveCountAsync(3);

        Assert.Equal(1, await Page.Locator("li[class='left-menu-ul selected']").GetByText(shop.Name).CountAsync());

        var urlLocator = Page.Locator("#Url");
        Assert.Equal(1, await urlLocator.CountAsync());
        await Expect(urlLocator).ToHaveValueAsync(productShopSettings.Url);
    }
}