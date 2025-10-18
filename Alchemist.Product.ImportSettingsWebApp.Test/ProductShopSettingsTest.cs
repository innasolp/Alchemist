using Alchemist.Product.ImportSettingsWebApp.Test.Infrastructure;
using Alchemist.Test.ImportSettingsWebApp.Factory;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using Xunit.Abstractions;
using TestCommon = Alchemist.Product.ImportSettingsWebApp.Test.Infrastructure.Common;

namespace Alchemist.Product.ImportSettingsWebApp.Test;

public class ProductShopSettingsTestImportSettingsWebAppFactory()
    : ImportSettingsWebAppFactory(false,
        TestCommon.CreateShopWebAppApiFactory("ProductSettingsTestDb", 8424, 8425, 8076, 8077).ServerAddress,
        8094, 8095,
        TestCommon.CreateSettingsApiHttpClient("ProductSettingsTestDb", 8224, 8225))
{
}

public class ProductShopSettingsTest : PageTest, IClassFixture<ProductShopSettingsTestImportSettingsWebAppFactory>
{
    private readonly ProductShopSettingsTestImportSettingsWebAppFactory _webAppFactory;
    private readonly ITestOutputHelper _outputHelper;

    public ProductShopSettingsTest(ProductShopSettingsTestImportSettingsWebAppFactory webAppFactory, ITestOutputHelper outputHelper)
    {
        _webAppFactory = webAppFactory;
        _outputHelper = outputHelper;

        _webAppFactory.CreateClient();
    }

    [Fact]
    public async Task PopupConfirmationWhenSelectOtherShopAfterMakingChanges()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectProductShopSettingsLoadedAsync();       

        await Page.Locator("#ProductUrlFormat" ).FillAsync(Guid.NewGuid().ToString());

        await this.SelectNextShopClickAsync();

        await Expect(Page.GetByRole(AriaRole.Dialog, new() { Name = "Input data will be reset. Continue?" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task PopupConfirmationWhenSelectCategoryTabAfterMakingChanges()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectProductShopSettingsLoadedAsync();

        await Page.Locator("#ProductUrlFormat").FillAsync(Guid.NewGuid().ToString());

        await this.SettingsTabClickAsync("Category");

        await Expect(Page.GetByRole(AriaRole.Dialog, new() { Name = "Input data will be reset. Continue?" })).ToBeVisibleAsync();
    }

    

    [Fact]
    public async Task ValidationErrorWhenShopSettingsNameIsEmpty()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectProductShopSettingsLoadedAsync();

        await this.ExpectImportSettingsRequiredValidationErrorAsync("ShopSettingsName", "Name");
    }

    [Fact]
    public async Task ValidationErrorWhenProductUrlFormatIsEmpty()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectProductShopSettingsLoadedAsync();

        await this.ExpectImportSettingsRequiredValidationErrorAsync("ProductUrlFormat", "ProductUrlFormat");
    }

    [Fact]
    public async Task ValidationErrorWhenCategoryUrlFormatIsEmpty()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectProductShopSettingsLoadedAsync();

        await this.ExpectImportSettingsRequiredValidationErrorAsync("CategoryUrlFormat", "CategoryUrlFormat");
    }

    [Fact]
    public async Task ValidationErrorWhenImportServiceNotSet()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectProductShopSettingsLoadedAsync();

        await this.ExpectSelectShopAsync(locator => locator.Last);

        await Page.Locator($"#ShopSettingsName").FillAsync(Guid.NewGuid().ToString());
        await Page.Locator($"#ProductUrlFormat").FillAsync(Guid.NewGuid().ToString());
        await Page.Locator($"#CategoryUrlFormat").FillAsync(Guid.NewGuid().ToString());

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await Expect(Page.GetByText($"The ImportService field is required.")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ValidationErrorWhenWebLoaderNotSet()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectProductShopSettingsLoadedAsync();

        await this.ExpectSelectShopAsync(locator => locator.Last);

        await Page.Locator($"#ShopSettingsName").FillAsync(Guid.NewGuid().ToString());
        await Page.Locator($"#ProductUrlFormat").FillAsync(Guid.NewGuid().ToString());
        await Page.Locator($"#CategoryUrlFormat").FillAsync(Guid.NewGuid().ToString());

        await this.SetServiceButtonClickAsync("import-service");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Service type" }).FillAsync(Guid.NewGuid().ToString());
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Service implementation type" }).FillAsync(Guid.NewGuid().ToString());
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Service assembly path", Exact = true }).FillAsync(Guid.NewGuid().ToString());
        await this.SaveServiceBtnClickAsync();

        await Expect(Page.Locator("#serviceSettingsForm > .row")).Not.ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await Expect(Page.GetByText($"The WebLoader field is required.")).ToBeVisibleAsync();
    }
}

