using Alchemist.Test.Functional.Playwright;
using Alchemist.Test.ProductWebAppFactory;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using Test.PostresqlTestContainer;
using Xunit.Abstractions;
using TestCommon = Alchemist.Product.WebApp.Test.Infrastructure.Common;

namespace Alchemist.Product.WebApp.Test;

public class TestProductWebAppFactory : ProductAggregatorConfigurationWebAppFactory<PostgresqlTestDbContainer, PostgresDbRespawner>
{
    private static readonly string database = Common.ConfigurationHelper.GetSectionValue("ContainerWebAppTestDb");

    public TestProductWebAppFactory() : base(7102, 7103, 
            8060, 8061, 
            8406, 8407, 
            "ConnectionStrings:DbContext2", database, 
            7088, 7089, 
            7500, 7501,
            "ConnectionStrings:DbContext2", database,
            TestCommon.SignalRTestServer,
            fillSettingsTestData : (dbContext) => TestCommon.FillTestData(dbContext, [1, 2, 3, 4]))
    {
    }
}

public class HomePageTest : PageTest, IClassFixture<TestProductWebAppFactory>
{
    private readonly TestProductWebAppFactory _webAppFactory;
    private readonly ITestOutputHelper _outputHelper;

    public HomePageTest(TestProductWebAppFactory webAppFactory, ITestOutputHelper outputHelper)
    {
        _webAppFactory = webAppFactory;
        
        _outputHelper = outputHelper;

        _webAppFactory.CreateClient();
    }

    private async Task SelectShopClickAsync(Func<ILocator, ILocator> nextShopLocator)
    {
        var nextAction = Page.Locator("a.shop_item:not(.selected)");
        Assert.True(await nextAction.CountAsync() > 0);

        await nextShopLocator(nextAction).ClickAsync();
    }

    private async Task<string> ExpectSelectShopAsync(Func<ILocator, ILocator> nextShopLocator, Func<Task>? expectLoading = null)
    {
        var startShopName = await Page.Locator("a.shop_item.selected").TextContentAsync();

        await SelectShopClickAsync(nextShopLocator);

        if (expectLoading != null) await expectLoading();

        var selectedShopItem = Page.Locator("a.shop_item.selected");

        await Expect(selectedShopItem).Not.ToHaveTextAsync(startShopName);

        return await selectedShopItem.TextContentAsync();
    }

    private async Task ExpectStartPageAsync()
    {
        var url = _webAppFactory.ServerAddress;

        await Page.GotoAsync(url);

        await Expect(Page.Locator("#appDiv")).Not.ToBeEmptyAsync();

        string pattern = @"\d+";
        await Page.WaitForURLAsync(new System.Text.RegularExpressions.Regex($"{url}Shop/{pattern}"));
    }

    private async Task ExpectShopTabLoadedAsync()
    {
        await Expect(Page.Locator("#shopList")).Not.ToBeEmptyAsync();
        await Expect(Page.Locator("#Name")).Not.ToBeEmptyAsync();        
    }

    private async Task ApplicationTabClickAsync(string app = "importsettingsapp")
    {
        var importSettingsTabLocator = Page.Locator($"tab-item[data-app='{app}']").Locator("div");
        await importSettingsTabLocator.ClickAsync();
    }

    private async Task ExpectImportSettingsTabLoadedAsync()
    {
        var url = _webAppFactory.ServerAddress;
        var pattern = @"\d+";
        await Page.WaitForURLAsync(new System.Text.RegularExpressions.Regex($"{url}Import/Settings/{pattern}/.*"));

        await Expect(Page.Locator("#shopListDiv")).Not.ToBeEmptyAsync();

        await Expect(Page.Locator("#ShopSettingsName")).Not.ToHaveValueAsync("");
        await Expect(Page.Locator("#ProductUrlFormat")).Not.ToHaveValueAsync("");
        await Expect(Page.Locator("#CategoryUrlFormat")).Not.ToHaveValueAsync("");
    }    

    [Fact]
    public async Task DefaultIsShopAppPage()
    {
        await ExpectStartPageAsync();

        await ExpectShopTabLoadedAsync();

        var selectedShopLocator = Page.Locator("a[class='shop_item selected']");
        await Expect(selectedShopLocator).ToHaveCountAsync(1);        
    }

    [Fact]
    public async Task ImportSettingsPageOnTabSelection()
    {
        await ExpectStartPageAsync();

        await ExpectShopTabLoadedAsync();

        await ApplicationTabClickAsync();

        await ExpectImportSettingsTabLoadedAsync();
    }

    [Fact]
    public async Task SelectOtherShopInShopAppSuccess()
    {
        await ExpectStartPageAsync();

        await ExpectShopTabLoadedAsync();

        async Task expectShopLoading()
        {
            await Expect(Page.Locator("#shopList")).Not.ToBeEmptyAsync();
            await Expect(Page.Locator("#Name")).Not.ToBeEmptyAsync();
        }

        var shopName = await ExpectSelectShopAsync(locator => locator.First, expectShopLoading);

        await Expect(Page.Locator("#Name")).ToHaveValueAsync(shopName);
    }

    [Fact]
    public async Task SelectOtherShopInImportSettingsAppSuccess()
    {
        await ExpectStartPageAsync();

        await ExpectShopTabLoadedAsync();

        await ApplicationTabClickAsync();
        await ExpectImportSettingsTabLoadedAsync();

        var name = await Page.Locator("#ShopSettingsName").InputValueAsync();

        async Task expectShopLoading()
        {
            await Expect(Page.Locator("#shopListDiv")).Not.ToBeEmptyAsync();
            await Expect(Page.Locator("#ShopSettingsName")).Not.ToBeEmptyAsync();
        }

        await ExpectSelectShopAsync(locator => locator.First, expectShopLoading);
        await Expect(Page.Locator("#ShopSettingsName")).Not.ToHaveValueAsync(name);
    }

    [Fact]
    public async Task DataChangedConfirmationOnSelectAnotherAppTabWhenShopEdited()
    {
        await ExpectStartPageAsync();

        await ExpectShopTabLoadedAsync();

        await Page.Locator("#Url").FillAsync(Guid.NewGuid().ToString());

        await ApplicationTabClickAsync();

        await this.ExpectConfirmationLocatorAsync();
    }

    [Fact]
    public async Task DataChangedConfirmationOnSelectAnotherAppTabWhenImportSettingsEdited()
    {
        await ExpectStartPageAsync();

        await ExpectShopTabLoadedAsync();

        await ApplicationTabClickAsync();
        await ExpectImportSettingsTabLoadedAsync();

        await Page.Locator("#ShopSettingsName").FillAsync(Guid.NewGuid().ToString());

        await ApplicationTabClickAsync("shopapp");

        await this.ExpectConfirmationLocatorAsync();
    }

    [Fact]
    public async Task AnotherAppRedirectedWhenShopChangesResetingConfirmationAccepted()
    {
        await ExpectStartPageAsync();

        await ExpectShopTabLoadedAsync();

        await Page.Locator("#Url").FillAsync(Guid.NewGuid().ToString());

        await ApplicationTabClickAsync();

        var confirmation = await this.ExpectConfirmationLocatorAsync();
        var yesButton = await this.ExpectConfirmationYesButtonAsync(confirmation);
        await yesButton.ClickAsync();

        await ExpectImportSettingsTabLoadedAsync();
    }

    [Fact]
    public async Task AnotherAppRedirectWhenImportSettingsChangesResetingConfirmationAccepted()
    {
        await ExpectStartPageAsync();

        await ExpectShopTabLoadedAsync();

        await ApplicationTabClickAsync();
        await ExpectImportSettingsTabLoadedAsync();

        await Page.Locator("#ShopSettingsName").FillAsync(Guid.NewGuid().ToString());

        await ApplicationTabClickAsync("shopapp");

        var confirmation = await this.ExpectConfirmationLocatorAsync();
        var yesButton = await this.ExpectConfirmationYesButtonAsync(confirmation);
        await yesButton.ClickAsync();

        await ExpectShopTabLoadedAsync();
    }

    [Fact]
    public async Task AnotherAppNotRedirectedWhenShopChangesResetingConfirmationCanceled()
    {
        await ExpectStartPageAsync();

        await ExpectShopTabLoadedAsync();

        await Page.Locator("#Url").FillAsync(Guid.NewGuid().ToString());

        await ApplicationTabClickAsync();

        var confirmation = await this.ExpectConfirmationLocatorAsync();
        var cancelButton = await this.ExpectConfirmationCancelButtonAsync(confirmation);
        await cancelButton.ClickAsync();

        var appUrlRegex = $"{_webAppFactory.ServerAddress}Shop/{@"\d+"}";
        await Expect(Page).Not.ToHaveURLAsync(appUrlRegex);
    }

    [Fact]
    public async Task AnotherAppNotRedirectedWhenImportSettingsChangesResetingConfirmationCanceled()
    {
        await ExpectStartPageAsync();

        await ExpectShopTabLoadedAsync();

        await ApplicationTabClickAsync();
        await ExpectImportSettingsTabLoadedAsync();

        await Page.Locator("#ShopSettingsName").FillAsync(Guid.NewGuid().ToString());

        await ApplicationTabClickAsync("shopapp");

        var confirmation = await this.ExpectConfirmationLocatorAsync();
        var cancelButton = await this.ExpectConfirmationCancelButtonAsync(confirmation);
        await cancelButton.ClickAsync();

        var appUrlRegex = $"{_webAppFactory.ServerAddress}Import/Settings/{@"\d+"}/.*";
        await Expect(Page).Not.ToHaveURLAsync(appUrlRegex);
    }
}