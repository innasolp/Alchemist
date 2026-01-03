using Alchemist.Import.Products.Interfaces;
using Alchemist.Product.ImportSettingsWebApp.Test.Infrastructure;
using Alchemist.Test.ImportSettingsWebApp.Factory;
using Alchemist.Test.SettingsAPIFactory;
using Alchemist.Test.ShopWebAppFactory;
using Microsoft.Playwright;
using Xunit.Abstractions;
using TestCommon = Alchemist.Product.ImportSettingsWebApp.Test.Infrastructure.Common;

namespace Alchemist.Product.ImportSettingsWebApp.Test;

public class ProductShopSettingsTestImportSettingsWebAppFactory()
    : ImportSettingsWebAppFactory(false,
        ShopWebAppHelper.CreateShopWebAppApiFactory(Common.ConfigurationHelper.GetConnectionString("ProductSettingsTestDb"),
            8426, 8427, 8078, 8079, TestCommon.SignalRTestServer).ServerAddress,
        8096, 8097,
        SettingsApiHelper.CreateSettingsApiHttpClient(Common.ConfigurationHelper.GetConnectionString("ProductSettingsTestDb"),
            8226, 8227, TestCommon.SignalRTestServer, (dbContext) => TestCommon.FillTestData(dbContext, [1, 2, 3, 4])))
{
}

public class ProductShopSettingsTest
    : ShopImportSettingsTest<ProductShopSettingsTestImportSettingsWebAppFactory, (string, string, string, UrlFormatType, UrlFormatType)>
{
    private record CategoryUrl(int item, string url);

    private readonly ProductShopSettingsTestImportSettingsWebAppFactory _webAppFactory;
    private readonly ITestOutputHelper _outputHelper;

    public ProductShopSettingsTest(ProductShopSettingsTestImportSettingsWebAppFactory webAppFactory, ITestOutputHelper outputHelper)
    {
        _webAppFactory = webAppFactory;
        _outputHelper = outputHelper;

        _webAppFactory.CreateClient();
    }

    protected override async Task ExpectPageLoadedAsync()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectProductShopSettingsLoadedAsync();
    }

    protected override async Task<(string, string, string, UrlFormatType, UrlFormatType)> FillInputFieldsAsync()
    {
        return await this.FillProductInputFieldsAsync();
    }

    protected override async Task SelectOtherTabAsync()
    {
        await this.SettingsTabClickAsync("Category");
    }

    private async Task<CategoryUrl> ExpectAddCategoryUrlAsync()
    {
        await Page.GetByText("Add category").ClickAsync();

        await Expect(Page.Locator("#rootCategoryFormDiv")).ToBeVisibleAsync();

        return await ExpectCategoryUrlSaveAsync();
    }

    private async Task<CategoryUrl> ExpectCategoryUrlSaveAsync()
    {
        var categoryUrl = new CategoryUrl(new Random().Next(0, int.MaxValue), Guid.NewGuid().ToString());

        await Page.GetByRole(AriaRole.Spinbutton, new() { Name = "Item" }).FillAsync($"{categoryUrl.item}");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Url", Exact = true }).FillAsync(categoryUrl.url);

        await Page.Locator("button.save-root-category").ClickAsync();

        await Expect(Page.Locator("#rootCategoryFormDiv")).Not.ToBeVisibleAsync();

        return categoryUrl;
    }

    private async Task<(ILocator li, ILocator row)> ExpectAddRootCategoryAndEditAsync()
    {
        var newCategoryUrl = await ExpectAddCategoryUrlAsync();

        var li = Page.Locator("li.rootcategory_li");
        var row = li.Filter(new LocatorFilterOptions { HasText = $"{newCategoryUrl.item}" })
          .And(li.Filter(new LocatorFilterOptions { HasText = newCategoryUrl.url }));

        var editCategoryBtn = row.Locator("button.edit-root-category");
        await Expect(editCategoryBtn).ToHaveCountAsync(1);

        await editCategoryBtn.ClickAsync();
        return (li, row);
    }

    [Fact]
    public async Task PopupConfirmationWhenOtherShopSelectWithoutSavingChanges()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await PopupConfirmationWhenOtherShopSelectWithoutSavingChangesAsync();
    }

    [Fact]
    public async Task PopupConfirmationWhenOtherTabSelectWithoutSavingChanges()
    {
        await PopupConfirmationWhenOtherTabSelectWithoutSavingChangesAsync();
    }

    [Fact]
    public async Task ValidationErrorWhenShopSettingsNameIsEmpty()
    {
        await ValidationErrorWhenShopSettingsNameIsEmptyAsync();
    }

    [Fact]
    public async Task ValidationErrorWhenProductUrlFormatIsEmpty()
    {
        await ValidationErrorWhenRequiredFieldIsEmptyAsync("ProductUrlFormat", "ProductUrlFormat");
    }

    [Fact]
    public async Task ValidationErrorWhenCategoryUrlFormatIsEmpty()
    {
        await ValidationErrorWhenRequiredFieldIsEmptyAsync("CategoryUrlFormat", "CategoryUrlFormat");
    }

    [Fact]
    public async Task ValidationErrorWhenImportServiceNotSet()
    {
        await ValidationErrorWhenImportServiceNotSetAsync();
    }

    [Fact]
    public async Task ValidationErrorWhenWebLoaderNotSet()
    {
        await ValidationErrorWhenWebLoaderNotSetAsync();
    }

    [Fact]
    public async Task CategoryUrlDialogWhenAddRootCategoryClick()
    {
        await ExpectPageLoadedAsync();

        await Page.GetByText("Add category").ClickAsync();

        await Expect(Page.Locator("#rootCategoryFormDiv")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task CategoryUrlValidationErrorWhenRequiredFieldsAreEmpty()
    {
        await ExpectPageLoadedAsync();

        await Page.GetByText("Add category").ClickAsync();

        await Expect(Page.Locator("#rootCategoryFormDiv")).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Spinbutton, new() { Name = "Item" }).FillAsync("");

        await Page.Locator("button.save-root-category").ClickAsync();

        await Expect(Page.GetByText("The Item field is required.")).ToBeVisibleAsync();
        await Expect(Page.GetByText("The Url field is required.")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task NewRootCategoryCloseWithoutChangesSuccess()
    {
        await ExpectPageLoadedAsync();

        await Page.GetByText("Add category").ClickAsync();

        await Expect(Page.Locator("#rootCategoryFormDiv")).ToBeVisibleAsync();

        var form = Page.Locator("#rootCategoryForm");

        await Expect(form).ToBeVisibleAsync();

        await Page.Locator(".root-category-modal").Locator(".close").ClickAsync();

        await Expect(form).Not.ToBeVisibleAsync();
    }

    [Fact]
    public async Task PopupConfirmationWhenNewRootCategoryCloseWithoutSavingChanges()
    {
        await ExpectPageLoadedAsync();

        await Page.GetByText("Add category").ClickAsync();

        await Expect(Page.Locator("#rootCategoryFormDiv")).ToBeVisibleAsync();

        var form = Page.Locator("#rootCategoryForm");

        await form.GetByRole(AriaRole.Textbox, new() { Name = "Url" }).FillAsync(Guid.NewGuid().ToString());

        await Page.Locator(".root-category-modal").Locator(".close").ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Dialog, new() { Name = "Input values will be reset. Are you sure?" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task PopupConfirmationWhenEditRootCategoryCloseWithoutSavingChanges()
    {
        await ExpectPageLoadedAsync();

        await ExpectAddRootCategoryAndEditAsync();

        await Expect(Page.Locator("#rootCategoryFormDiv")).ToBeVisibleAsync();

        var form = Page.Locator("#rootCategoryForm");

        await form.GetByRole(AriaRole.Textbox, new() { Name = "Url" }).FillAsync(Guid.NewGuid().ToString());

        await Page.Locator(".root-category-modal").Locator(".close").ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Dialog, new() { Name = "Input values will be reset. Are you sure?" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task NewRowInCategoryUrlTableWhenSaved()
    {
        await ExpectPageLoadedAsync();

        var categoryUrl = await ExpectAddCategoryUrlAsync();

        var li = Page.Locator("li.rootcategory_li");

        await Expect(li.Filter(new LocatorFilterOptions { HasText = $"{categoryUrl.item}" })
          .And(li.Filter(new LocatorFilterOptions { HasText = categoryUrl.url }))).ToHaveCountAsync(1);
    }

    [Fact]
    public async Task RowInCategoryUrlTableChangedWhenSaved()
    {
        await ExpectPageLoadedAsync();
        (ILocator li, ILocator row) = await ExpectAddRootCategoryAndEditAsync();

        var savedCategoryUrl = await ExpectCategoryUrlSaveAsync();
        var savedRow = li.Filter(new LocatorFilterOptions { HasText = $"{savedCategoryUrl.item}" })
          .And(li.Filter(new LocatorFilterOptions { HasText = savedCategoryUrl.url }));

        await Expect(savedRow).ToHaveCountAsync(1);
        await Expect(row).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task UploadFromJson()
    {
        var fileName = "productsettings.json";
        var serviceTypeName = "IImportService";

        await ExpectUploadFormJsonAsync(fileName, serviceTypeName);
    }
}