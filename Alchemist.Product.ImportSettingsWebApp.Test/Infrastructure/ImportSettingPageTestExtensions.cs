using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace Alchemist.Product.ImportSettingsWebApp.Test.Infrastructure;

internal static class ImportSettingPageTestExtensions
{ 
    internal static async Task ExpectProductShopSettingsLoadedAsync(this PageTest pageTest)
    {
        await pageTest.Expect(pageTest.Page.Locator("#shopListDiv")).Not.ToBeEmptyAsync();

        await pageTest.Expect(pageTest.Page.Locator("#ShopSettingsName")).Not.ToHaveValueAsync("");
        await pageTest.Expect(pageTest.Page.Locator("#ProductUrlFormat")).Not.ToHaveValueAsync("");
        await pageTest.Expect(pageTest.Page.Locator("#CategoryUrlFormat")).Not.ToHaveValueAsync("");
    }

    internal static async Task ExpectCategoryShopSettingsLoadedAsync(this PageTest pageTest)
    {
        await pageTest.Expect(pageTest.Page.Locator("#shopListDiv")).Not.ToBeEmptyAsync();
        
        await pageTest.Expect(pageTest.Page.Locator("#CategorySourceUrl")).Not.ToHaveValueAsync("");
    }

    internal static async Task SelectShopClickAsync(this PageTest pageTest, Func<ILocator, ILocator> nextShopLocator)
    {
        var nextAction = pageTest.Page.Locator("a.shop_item:not(.selected)");
        Assert.True(await nextAction.CountAsync() > 0);

        await nextShopLocator(nextAction).ClickAsync();
    }

    internal static async Task SelectNextShopClickAsync(this PageTest pageTest)
    {
        await pageTest.SelectShopClickAsync(locator=>locator.First);
    }

    internal static async Task<string> ExpectSelectShopAsync(this PageTest pageTest, 
        Func<ILocator, ILocator> nextShopLocator,
        Func<Task>? expectLoading = null)    
    {
        var startShopName = await pageTest.Page.Locator("a.shop_item.selected").TextContentAsync();
        
        await pageTest.SelectShopClickAsync(nextShopLocator);

        if(expectLoading != null) await expectLoading();

        var selectedShopItem = pageTest.Page.Locator("a.shop_item.selected");

        await pageTest.Expect(selectedShopItem).Not.ToHaveTextAsync(startShopName);

        return await selectedShopItem.TextContentAsync();
    }

    internal static async Task<string> ExpectSelectNextShopAsync(this PageTest pageTest, Func<Task>? expectLoading = null)
    {
        return await pageTest.ExpectSelectShopAsync(locator => locator.First, expectLoading);
    }

    internal static async Task SettingsTabClickAsync(this PageTest pageTest, string tab)
    {
        var categoryShopTabDivLocator = pageTest.Page.Locator("div.shop-tab").Locator("a:not(.selected)").GetByText(tab);

        await pageTest.Expect(categoryShopTabDivLocator).ToHaveCountAsync(1);
        
        await categoryShopTabDivLocator.ClickAsync();
    }

    internal static async Task ExpectNextSettingsTabAsync(this PageTest pageTest, string tab)
    {
        var categoryShopTabDivLocator = pageTest.Page.Locator("div.shop-tab").Locator("a:not(.selected)");
        await pageTest.Expect(categoryShopTabDivLocator).ToHaveTextAsync(tab);

        await categoryShopTabDivLocator.ClickAsync();

        var selectedShopTabDiv = pageTest.Page.Locator("div.shop-tab").Locator("a.selected");
        await pageTest.Expect(selectedShopTabDiv).ToHaveTextAsync(tab);
    }

    internal static async Task SetServiceButtonClickAsync(this PageTest pageTest, string serviceClass)
    {
        var setButton = pageTest.Page.Locator($"button.set-service.{serviceClass}");
        await pageTest.Expect(setButton).ToHaveCountAsync(1);
        await setButton.ClickAsync();
    }

    internal static async Task SaveServiceBtnClickAsync(this PageTest pageTest)
    {
        await pageTest.Page.Locator("#saveServiceSettingsBtn").ClickAsync();
    }

    internal static async Task ExpectImportSettingsRequiredValidationErrorAsync(this PageTest pageTest, string fieldName, string property)
    {
        await pageTest.Page.Locator($"#{fieldName}").FillAsync("");
        await pageTest.Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await pageTest.Expect(pageTest.Page.GetByText($"The {property} field is required.")).ToBeVisibleAsync();
    }
}
