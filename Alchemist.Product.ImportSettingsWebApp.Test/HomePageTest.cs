using Alchemist.Product.ImportSettingsWebApp.Test.Infrastructure;
using Alchemist.Test.ImportSettingsWebApp.Factory;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using Xunit.Abstractions;
using TestCommon = Alchemist.Product.ImportSettingsWebApp.Test.Infrastructure.Common;

namespace Alchemist.Product.ImportSettingsWebApp.Test;

public class HomePageTestImportSettingsWebAppFactory()
    : ImportSettingsWebAppFactory(false,
        TestCommon.CreateShopWebAppApiFactory("HomePageTestDb", 8420,8421, 8072,8073).ServerAddress,
        8090, 8091, 
        TestCommon.CreateSettingsApiHttpClient("HomePageTestDb", 8220, 8221))
{
}

public class HomePageTest : PageTest, IClassFixture<HomePageTestImportSettingsWebAppFactory>
{
    private readonly HomePageTestImportSettingsWebAppFactory _webAppFactory;
    private readonly ITestOutputHelper _outputHelper;

    public HomePageTest(HomePageTestImportSettingsWebAppFactory webAppFactory, ITestOutputHelper outputHelper)
    {
        _webAppFactory = webAppFactory;
        _outputHelper = outputHelper;

        _webAppFactory.CreateClient();
    }

    [Fact]
    public async Task StartPageTest()
    {
        var url = _webAppFactory.ServerAddress;

        await Page.GotoAsync(url);
        
        await Expect(Page.Locator("#ShopSettingsName")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "ProductUrlFormat" })).ToBeVisibleAsync();

        var shopTabDivLocator = Page.Locator("div.shop-tab");
        await Expect(shopTabDivLocator).ToHaveCountAsync(2);

        var selectedTab = shopTabDivLocator.Locator("a.selected");
        await Expect(selectedTab).ToHaveCountAsync(1);
        await Expect(selectedTab).ToHaveTextAsync("Product");
    }

    [Fact]
    public async Task UploadShopListTest()
    {
        var url = _webAppFactory.ServerAddress;

        await Page.GotoAsync(url);      

        await Expect(Page.Locator("#shopListDiv")).Not.ToBeEmptyAsync();

        var shopLocator = Page.Locator(".shop_item");
        Assert.True(await shopLocator.CountAsync() > 0);

        var selectedShopLocator = Page.Locator("a[class='shop_item selected']");
        await Expect(selectedShopLocator).ToHaveCountAsync(1);

        var currentShopName = await selectedShopLocator.TextContentAsync();
    }

    [Fact]
    public async Task SelectShopTest()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectProductShopSettingsLoadedAsync();

        await this.ExpectSelectNextShopAsync();
    }

    [Fact]
    public async Task SelectCategorySettingsTest()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectNextSettingsTabAsync("Category");

        await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "CategorySourceUrl" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "ProductUrlFormat" })).Not.ToBeVisibleAsync();
    }

    [Fact]
    public async Task SelectShopWithoutSettings()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectProductShopSettingsLoadedAsync();

        await this.ExpectSelectShopAsync(locator => locator.Last);

        await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "ProductUrlFormat" })).ToBeEmptyAsync();
        await Expect(Page.Locator("#ShopSettingsName")).ToBeEmptyAsync();
    }
}
