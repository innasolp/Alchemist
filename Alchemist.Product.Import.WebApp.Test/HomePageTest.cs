using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Import.Model.Infrastructure;
using Alchemist.Product.Import.WebApp.Models;
using Alchemist.Product.Import.WebApp.Test.Infrastructure;
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

        var menudiv = await this.ExpectSingleElementAsync(Page, ".menuDiv");       

        var shopListPartialDiv = await this.ExpectSingleElementAsync(Page, ".shopListPartialDiv");        

        var shopTabsLocator = await this.ExpectSingleElementAsync(Page, ".tabsMenuDiv");        
    }

    [Fact]
    public async Task IndexPageUploadShops()
    {
        var response = await Page.GotoAsync(_webAppFactory.ServerAddress);
        Assert.True(response?.Ok);

        var shop = _shops.First();
        var shopSetting = _shopSettings.First(s => s.ShopId == shop.Id && s.Type == Interfaces.ShopSettingType.Product);
        var productShopSettings = JsonSerializer.Deserialize<ProductShopSettingsModel>(shopSetting.JsonValue);

        await Expect(Page.Locator(".menuDiv").Locator("div[class = 'menu_item selected']").GetByText(TabHelper.TabNames[TabType.Shop]))
           .ToHaveCountAsync(1);

        var tabsLocator = Page.Locator(".tabsMenuDiv");
        await Expect(tabsLocator.Locator("div[class = 'shopTab menu_item-a selected-a']").GetByText(TabHelper.ShopSettingTypeNames[ShopSettingType.Product]))
            .ToHaveCountAsync(1);

        await Expect(Page.Locator("form[name='itemShopForm']")).ToHaveCountAsync(_shops.Count);

        await Expect(Page.Locator("li[class='left-menu-ul selected']").GetByText(shop.Name)).ToHaveCountAsync(1);

        await Task.Delay(1000);

        var shopSettingsNameLocator = await this.ExpectSingleElementAsync(Page, "#ShopSettingsName");        
        await Expect(shopSettingsNameLocator).ToHaveValueAsync(productShopSettings.Name);

        var urlLocator = await this.ExpectSingleElementAsync(Page, "#ProductUrlFormat");       
        await Expect(urlLocator).ToHaveValueAsync(productShopSettings.ProductUrlFormat);
    }

    [Fact]
    public async Task SelectShop()
    {
        var startShopSettings =  await ExpectLoadIndexPageAsync(Page);
        var productShopSettings = Assert.IsType<ProductShopSettingsModel>(startShopSettings);

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
        var productShopSettings = Assert.IsType<ProductShopSettingsModel>(startShopImportSettings);

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
        var productShopSettings = Assert.IsType<ProductShopSettingsModel>(startShopImportSettings);

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
        var productShopSettings = Assert.IsType<ProductShopSettingsModel>(startShopImportSettings);

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
        var productShopSettings = Assert.IsType<ProductShopSettingsModel>(startShopImportSettings);

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