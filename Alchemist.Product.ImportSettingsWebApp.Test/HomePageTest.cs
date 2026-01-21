using Alchemist.Product.ImportSettingsWebApp.Test.Infrastructure;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using System.Collections.ObjectModel;
using Xunit.Abstractions;

namespace Alchemist.Product.ImportSettingsWebApp.Test;

public class HomePageTestImportSettingsWebAppFactory()
    : ImportSettingsWebAppTestContainerLifetimeFactory(false,
        8090, 8091, 
        8220, 8221,
        Common.ConfigurationHelper.GetSectionValue("HomePageTestDb"),
        8420,8421, 8072,8073)
{
}

public class HomePageTest : PageTest, IClassFixture<HomePageTestImportSettingsWebAppFactory>
{
    private readonly HomePageTestImportSettingsWebAppFactory _webAppFactory;
    private readonly ITestOutputHelper _outputHelper;
    private readonly ObservableCollection<IConsoleMessage> _messages = [];

    public HomePageTest(HomePageTestImportSettingsWebAppFactory webAppFactory, ITestOutputHelper outputHelper)
    {
        _webAppFactory = webAppFactory;
        _outputHelper = outputHelper;

        _webAppFactory.CreateClient();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        Page.Console += OnPageConsole;
    }
    private void OnPageConsole(object? sender, IConsoleMessage e)
    {
        _messages.Add(e);
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

        try
        {
            await this.ExpectProductShopSettingsLoadedAsync();

            await this.ExpectNextSettingsTabAsync("Category");

            await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "CategorySourceUrl" })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "ProductUrlFormat" })).Not.ToBeVisibleAsync();
        }
        catch
        {
            foreach (var error in _messages.Where(m => m.Type == "error"))
                _outputHelper.WriteLine(error.Text);
            throw;
        }
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
