using Alchemist.Import.Settings.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Interfaces;
using System.Text.Json;
using Xunit.Abstractions;

namespace Alchemist.Product.Import.WebApp.Test;

public class HomePageTest(TestImportWebAppFactory testImportWebAppFactory, ITestOutputHelper testOutputHelper)
    : ImportWebAppTest(testImportWebAppFactory, testOutputHelper)
{
    [Fact]
    public async Task IndexPageContainsMenuDivAndShopList()
    {
        var response = await Page.GotoAsync(_webAppFactory.ServerAddress);
        Assert.True(response?.Ok);

        var menudiv = Page.Locator("#menuDiv");
        await Expect(menudiv).ToHaveCountAsync(1);

        var shopListPartialDiv = Page.Locator("#shopListPartialDiv");
        await Expect(shopListPartialDiv).ToHaveCountAsync(1);

        var shopTabsLocator = Page.Locator("#tabsMenuDiv");
        await Expect(shopTabsLocator).ToHaveCountAsync(1);
    }

    [Fact]
    public async Task IndexPageUploadShops()
    {
        var response = await Page.GotoAsync(_webAppFactory.ServerAddress);
        Assert.True(response?.Ok);

        var shop = _shops.First();
        var shopSetting = _shopSettings.First(s => s.ShopId == shop.Id && s.Type == ShopSettingType.Product);
        var productShopSettings = JsonSerializer.Deserialize<ProductShopImportSettings>(shopSetting.JsonValue);

        await Expect(Page.Locator("#menuDiv").Locator("div[class = 'menu_item selected']").GetByText(ViewHelper.TabNames[TabType.Shop]))
           .ToHaveCountAsync(1);

        var tabsLocator = Page.Locator("#tabsMenuDiv");
        await Expect(tabsLocator.Locator("div[class = 'menu_item-a selected-a']").GetByText(ViewHelper.ShopSettingTypeNames[ShopSettingType.Product]))
            .ToHaveCountAsync(1);

        await Expect(Page.Locator("form[name='itemShopForm']")).ToHaveCountAsync(3);

        await Expect(Page.Locator("li[class='left-menu-ul selected']").GetByText(shop.Name)).ToHaveCountAsync(1);

        var urlLocator = Page.Locator("#Url");
        await Expect(urlLocator).ToHaveCountAsync(1);
        await Expect(urlLocator).ToHaveValueAsync(productShopSettings.Url);
    }

    [Fact]
    public async Task SelectShop()
    {
        var startShopSettings =  await ExpectLoadIndexPageAsync(Page);

        var nextShop = _shops.FirstOrDefault(s => s.Id != startShopSettings.ShopId);

        await ExpectForSelectShopAsync(Page, nextShop);

        await Expect(Page.Locator("#Url")).Not.ToHaveValueAsync(startShopSettings.Url);
    }

    [Fact]
    public async Task SelectProductImportTab()
    {
        var startShopSettings = await ExpectLoadIndexPageAsync(Page);

        await SelectTabAsync(Page, TabType.Products, "Hello Products", TabType.Shop);
    }

    [Fact]
    public async Task SelectCategoryImportTab()
    {
        await ExpectLoadIndexPageAsync(Page);

        await SelectTabAsync(Page, TabType.Categories, "Hello Categories", TabType.Shop);
    }

    [Fact]
    public async Task SaveInputShopSettingsDataToSession()
    {
        var startShopImportSettings = await ExpectLoadIndexPageAsync(Page);       

        startShopImportSettings.Url = Guid.NewGuid().ToString();
        await Page.Locator("#Url").FillAsync(startShopImportSettings.Url);

        startShopImportSettings.Name = Guid.NewGuid().ToString();
        await Page.Locator("#Name").FillAsync(startShopImportSettings.Name);

        var startShopSettings = _shopSettings.FirstOrDefault(s => s.Id == startShopImportSettings.Id);
        startShopSettings.JsonValue = JsonSerializer.Serialize(startShopImportSettings);

        var nextShop = _shops.FirstOrDefault(s => s.Id != startShopImportSettings.ShopId);

        await ExpectForSelectShopAsync(Page, nextShop);

        var startShop = _shops.SingleOrDefault(s => s.Id == startShopImportSettings.ShopId);
        await ExpectForSelectShopAsync(Page, startShop);
    }
}