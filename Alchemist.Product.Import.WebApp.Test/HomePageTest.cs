using Alchemist.Import.Settings.Model;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Import.Settings.Interfaces;
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
        var shopSetting = _shopSettings.First(s => s.ShopId == shop.Id && s.Type == Interfaces.ShopSettingType.Product);
        var productShopSettings = JsonSerializer.Deserialize<ProductShopImportSettings>(shopSetting.JsonValue);

        await Expect(Page.Locator("#menuDiv").Locator("div[class = 'menu_item selected']").GetByText(TabHelper.TabNames[TabType.Shop]))
           .ToHaveCountAsync(1);

        var tabsLocator = Page.Locator("#tabsMenuDiv");
        await Expect(tabsLocator.Locator("div[class = 'menu_item-a selected-a']").GetByText(TabHelper.ShopSettingTypeNames[ShopSettingType.Product]))
            .ToHaveCountAsync(1);

        await Expect(Page.Locator("form[name='itemShopForm']")).ToHaveCountAsync(3);

        await Expect(Page.Locator("li[class='left-menu-ul selected']").GetByText(shop.Name)).ToHaveCountAsync(1);

        await Task.Delay(1000);

        var shopSettingsNameLocator = Page.Locator("#ShopSettingsName");
        await Expect(shopSettingsNameLocator).ToHaveCountAsync(1);
        await Expect(shopSettingsNameLocator).ToHaveValueAsync(productShopSettings.Name);

        var urlLocator = Page.Locator("#ProductUrlFormat");
        await Expect(urlLocator).ToHaveCountAsync(1);
        await Expect(urlLocator).ToHaveValueAsync(productShopSettings.ProductUrlFormat);
    }

    [Fact]
    public async Task SelectShop()
    {
        var startShopSettings =  await ExpectLoadIndexPageAsync(Page);
        var productShopSettings = Assert.IsType<ProductShopImportSettings>(startShopSettings);

        var nextShop = _shops.FirstOrDefault(s => s.Id != productShopSettings.ShopId);

        await ExpectForSelectShopAsync(Page, nextShop);

        await Expect(Page.Locator("#ProductUrlFormat")).Not.ToHaveValueAsync(productShopSettings.ProductUrlFormat);
    }

    [Fact]
    public async Task SelectProductImportTab()
    {
        var startShopSettings = await ExpectLoadIndexPageAsync(Page);

        await ExpectSelectTabAsync(Page, TabType.Products, "Hello Products", TabType.Shop);
    }

    [Fact]
    public async Task SelectCategoryImportTab()
    {
        await ExpectLoadIndexPageAsync(Page);

        await ExpectSelectTabAsync(Page, TabType.Categories, "Hello Categories", TabType.Shop);
    }    
    
    [Fact]
    public async Task ShowConfirmationWindowWhenSelectOtherTabAfterDataEditing()
    {
        var startShopImportSettings = await ExpectLoadIndexPageAsync(Page);
        var productShopSettings = Assert.IsType<ProductShopImportSettings>(startShopImportSettings);

        productShopSettings.ProductUrlFormat = Guid.NewGuid().ToString();
        await Page.Locator("#ProductUrlFormat").FillAsync(productShopSettings.ProductUrlFormat);

        startShopImportSettings.Name = Guid.NewGuid().ToString();
        await Page.Locator("#ShopSettingsName").FillAsync(startShopImportSettings.Name);

        var confirmationLocator = await GetConfirmationLocatorAsync(Page);

        await ClickSelectTabAsync(Page, TabType.Products);

        await ExpectConfirmationAsync(confirmationLocator);
    }

    [Fact]
    public async Task ShowConfirmationWindowWhenSelectOtherShopAfterDataEditing()
    {
        var startShopImportSettings = await ExpectLoadIndexPageAsync(Page);
        var productShopSettings = Assert.IsType<ProductShopImportSettings>(startShopImportSettings);

        productShopSettings.ProductUrlFormat = Guid.NewGuid().ToString();
        await Page.Locator("#ProductUrlFormat").FillAsync(productShopSettings.ProductUrlFormat);

        startShopImportSettings.Name = Guid.NewGuid().ToString();
        await Page.Locator("#ShopSettingsName").FillAsync(productShopSettings.Name);

        var confirmationLocator = await GetConfirmationLocatorAsync(Page);

        var nextShop = _shops.FirstOrDefault(s => s.Id != productShopSettings.ShopId);

        await ClickSelectShopAsync(Page, nextShop);

        await ExpectConfirmationAsync(confirmationLocator);
    }

    [Fact]
    public async Task SelectedItemNotChangedAfterConfirmationCancel()
    {
        var startShopImportSettings = await ExpectLoadIndexPageAsync(Page);
        var productShopSettings = Assert.IsType<ProductShopImportSettings>(startShopImportSettings);

        productShopSettings.ProductUrlFormat = Guid.NewGuid().ToString();
        await Page.Locator("#ProductUrlFormat").FillAsync(productShopSettings.ProductUrlFormat);

        productShopSettings.Name = Guid.NewGuid().ToString();
        await Page.Locator("#ShopSettingsName").FillAsync(productShopSettings.Name);

        var confirmationLocator = await GetConfirmationLocatorAsync(Page);

        await ClickSelectTabAsync(Page, TabType.Products);

        await ExpectConfirmationAsync(confirmationLocator);

        await CancelConfirmationAsync(confirmationLocator);

        await Expect(confirmationLocator).Not.ToBeVisibleAsync();

        await ExpectSelectedTabAsync(Page, TabType.Shop);
    }

    [Fact]
    public async Task SelectedItemResetAndChangedAfterConfirmationYes()
    {
        var startShopImportSettings = await ExpectLoadIndexPageAsync(Page);
        var productShopSettings = Assert.IsType<ProductShopImportSettings>(startShopImportSettings);

        var newName = Guid.NewGuid().ToString();
        var newUrl = Guid.NewGuid().ToString();

        productShopSettings.ProductUrlFormat = Guid.NewGuid().ToString();
        await Page.Locator("#ProductUrlFormat").FillAsync(newUrl);

        productShopSettings.Name = Guid.NewGuid().ToString();
        await Page.Locator("#ShopSettingsName").FillAsync(newName);

        var confirmationLocator = await GetConfirmationLocatorAsync(Page);

        var nextShop = _shops.FirstOrDefault(s => s.Id != productShopSettings.ShopId);
        await ClickSelectShopAsync(Page, nextShop);

        await ExpectConfirmationAsync(confirmationLocator);

        await ConfirmAsync(confirmationLocator);

        await Expect(confirmationLocator).Not.ToBeVisibleAsync();

        await ExpectForSelectShopAsync(Page, nextShop);

        var prevShop = _shops.FirstOrDefault(s => s.Id == productShopSettings.ShopId);
        await ClickSelectShopAsync(Page, prevShop);
        await ExpectForSelectShopAsync(Page, prevShop);
    }
}