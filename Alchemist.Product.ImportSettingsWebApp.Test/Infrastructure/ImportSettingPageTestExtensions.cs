using Alchemist.Import.Products.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
        var nextSettingsTabDivLocator = pageTest.Page.Locator("div.shop-tab").Filter(new LocatorFilterOptions { HasText = tab });
        await pageTest.Expect(nextSettingsTabDivLocator.Locator("a:not(.selected)")).ToHaveCountAsync(1);

        await nextSettingsTabDivLocator.ClickAsync();

        var selectedShopTabDiv = pageTest.Page.Locator("div.shop-tab").Locator("a.selected");
        await pageTest.Expect(selectedShopTabDiv).ToHaveCountAsync(1);
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

    internal static async Task<(string serviceTypeName, string serviceImplementationType, string serviceAssemblyPath)>
        SetServiceSettingsAsync(this PageTest pageTest, string serviceClass)
    {
        await pageTest.SetServiceButtonClickAsync(serviceClass);

        var newServiceTypeName = Guid.NewGuid().ToString();
        await pageTest.Page.GetByRole(AriaRole.Textbox, new() { Name = "Service type" }).FillAsync(newServiceTypeName);

        var newServiceImplementationType = Guid.NewGuid().ToString();
        await pageTest.Page.GetByRole(AriaRole.Textbox, new() { Name = "Service implementation type" }).FillAsync(newServiceImplementationType);

        var newServiceAssemblyPath = Guid.NewGuid().ToString();
        await pageTest.Page.GetByRole(AriaRole.Textbox, new() { Name = "Service assembly path", Exact = true }).FillAsync(newServiceAssemblyPath);

        await pageTest.SaveServiceBtnClickAsync();

        return (newServiceTypeName, newServiceImplementationType, newServiceAssemblyPath);
    }

    internal static async Task ExpectServiceSettingsFieldsAsync(this PageTest pageTest, string serviceTypeName, string serviceImplementationType, string serviceAssemblyPath)
    {
        await pageTest.Expect(pageTest.Page.GetByRole(AriaRole.Textbox, new() { Name = "Service type" }))
            .ToHaveValueAsync(serviceTypeName);
        await pageTest.Expect(pageTest.Page.GetByRole(AriaRole.Textbox, new() { Name = "Service implementation type" }))
            .ToHaveValueAsync(serviceImplementationType);
        await pageTest.Expect(pageTest.Page.GetByRole(AriaRole.Textbox, new() { Name = "Service assembly path", Exact = true }))
            .ToHaveValueAsync(serviceAssemblyPath);
    }
    internal static async Task ExpectShowServiceAsync(this PageTest pageTest, string serviceClass)
    {
        await pageTest.Expect(pageTest.Page.Locator($"input.form-input.{serviceClass}")).Not.ToHaveValueAsync("");

        await pageTest.SetServiceButtonClickAsync(serviceClass);

        await pageTest.Expect(pageTest.Page.Locator("#serviceSettingsForm > .row")).ToBeVisibleAsync();
    }

    internal static async Task CloseServiceSettingsAsync(this PageTest pageTest)
    {
        await pageTest.Page.Locator(".service-settings-modal").Locator(".close").ClickAsync();
    }

    internal static async Task<(string, string, string, UrlFormatType, UrlFormatType)> FillProductInputFieldsAsync(this PageTest pageTest)
    {
        var name = Guid.NewGuid().ToString();
        await pageTest.Page.Locator($"#ShopSettingsName").FillAsync(name);

        var productUrlFormat = Guid.NewGuid().ToString();
        await pageTest.Page.Locator($"#ProductUrlFormat").FillAsync(productUrlFormat);

        var categoryUrlFormat = Guid.NewGuid().ToString();
        await pageTest.Page.Locator($"#CategoryUrlFormat").FillAsync(categoryUrlFormat);

        var formatTypes = Enum.GetValues<UrlFormatType>();

        int productUrlFormatTypeOption = new Random().Next(0, formatTypes.Length - 1);
        await pageTest.Page.Locator("#ProductUrlFormatType").SelectOptionAsync([productUrlFormatTypeOption.ToString()]);

        int categoryUrlFormatTypeOption = new Random().Next(0, formatTypes.Length - 1);
        await pageTest.Page.Locator("#CategoryUrlFormatType").SelectOptionAsync([categoryUrlFormatTypeOption.ToString()]);

        return (name, productUrlFormat, categoryUrlFormat, formatTypes[productUrlFormatTypeOption], formatTypes[categoryUrlFormatTypeOption]);
    }

    internal static async Task<(string, string)> FillCategoryInputFieldsAsync(this PageTest pageTest)
    {
        var name = Guid.NewGuid().ToString();
        await pageTest.Page.Locator($"#ShopSettingsName").FillAsync(name);

        var categorySourceUrl = Guid.NewGuid().ToString();
        await pageTest.Page.Locator($"#CategorySourceUrl").FillAsync(categorySourceUrl);
        return (name, categorySourceUrl);
    }
}