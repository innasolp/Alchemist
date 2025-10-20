using Alchemist.Product.ImportSettingsWebApp.Test.Infrastructure;
using Alchemist.Test.ImportSettingsWebApp.Factory;
using Microsoft.Playwright;
using Xunit.Abstractions;
using TestCommon = Alchemist.Product.ImportSettingsWebApp.Test.Infrastructure.Common;

namespace Alchemist.Product.ImportSettingsWebApp.Test;

public class CategoryShopSettingsTestImportSettingsWebAppFactory()
    : ImportSettingsWebAppFactory(false,
        TestCommon.CreateShopWebAppApiFactory("CategorySettingsTestDb", 8424, 8425, 8076, 8077).ServerAddress,
        8094, 8095,
        TestCommon.CreateSettingsApiHttpClient("CategorySettingsTestDb", 8224, 8225))
{
}

public class CategoryShopSettingsTest : ShopImportSettingsTest<CategoryShopSettingsTestImportSettingsWebAppFactory>
{
    internal record Service(string Name, string ServiceTypeName, string ImplementationTypeName, string AssemblyPath);

    private readonly CategoryShopSettingsTestImportSettingsWebAppFactory _webAppFactory;
    private readonly ITestOutputHelper _outputHelper;

    public CategoryShopSettingsTest(CategoryShopSettingsTestImportSettingsWebAppFactory webAppFactory, ITestOutputHelper outputHelper)
    {
        _webAppFactory = webAppFactory;
        _outputHelper = outputHelper;

        _webAppFactory.CreateClient();
    }

    protected override async Task ExpectSettingsLoadedAsync()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectProductShopSettingsLoadedAsync();

        await this.SettingsTabClickAsync("Category");

        await this.ExpectCategoryShopSettingsLoadedAsync();
    }

    protected override async Task FillInputFieldsAsync()
    {
        await Page.Locator($"#CategorySourceUrl").FillAsync(Guid.NewGuid().ToString());
    }

    protected override async Task SelectOtherTabAsync()
    {
        await this.SettingsTabClickAsync("Product");
    }

    private async Task<Service> ServiceSaveAsync()
    {
        var service = new Service(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
        );

        await Page.Locator("#ServiceName").FillAsync(service.Name);
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Service type" }).FillAsync(service.ServiceTypeName);
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Service implementation type" }).FillAsync(service.ImplementationTypeName);
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Service assembly path", Exact = true }).FillAsync(service.AssemblyPath);

        await Page.Locator("#saveServiceSettingsBtn").ClickAsync();

        await Expect(Page.Locator("#serviceSettingsForm")).Not.ToBeVisibleAsync();

        return service;
    }

    private ILocator FindServiceRow(Service service)
    {
        var li = Page.Locator("li.table-ul.service_li");
        return li.Filter(new LocatorFilterOptions { HasText = $"{service.Name}" })
          .And(li.Filter(new LocatorFilterOptions { HasText = service.ServiceTypeName }));
    }

    [Fact]
    public async Task PopupConfirmationWhenSelectOtherShopAfterMakingChanges()
    {
        await PopupConfirmationWhenSelectOtherShopAfterMakingChangesAsync();
    }

    [Fact]
    public async Task PopupConfirmationWhenSelectOtherTabAfterMakingChanges()
    {
        await PopupConfirmationWhenSelectOtherTabAfterMakingChangesAsync();
    }

    [Fact]
    public async Task ValidationErrorWhenShopSettingsNameIsEmpty()
    {
        await ValidationErrorWhenShopSettingsNameIsEmptyAsync();
    }

    [Fact]
    public async Task ValidationErrorWhenCategorySourceUrlIsEmpty()
    {
        await ValidationErrorWhenRequiredFieldIsEmptyAsync("CategorySourceUrl", "CategorySourceUrl");
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
    public async Task ServiceFormShowWhenAddServiceButtonClick()
    {
        await ExpectSettingsLoadedAsync();

        await Page.GetByText("Add service").ClickAsync();
        await Expect(Page.Locator("#serviceSettingsForm")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task NewRowInServiceTableWhenServiceSaved()
    {
        await ExpectSettingsLoadedAsync();

        await Page.GetByText("Add service").ClickAsync();

        var service = await ServiceSaveAsync();

        var li = Page.Locator("li.table-ul.service_li");

        await Expect(FindServiceRow(service)).ToHaveCountAsync(1);
    }

    [Fact] 
    public async Task ServiceRowUpdatedWhenServiceEditedAndSaved()
    {
        await ExpectSettingsLoadedAsync();

        await Page.GetByText("Add service").ClickAsync();

        var service = await ServiceSaveAsync();

        var row = FindServiceRow(service);
        await Expect(row).ToHaveCountAsync(1);

        var editServiceBtn = row.Locator("i.editService");
        await Expect(editServiceBtn).ToHaveCountAsync(1);

        await editServiceBtn.ClickAsync();

        await Expect(Page.Locator("#serviceSettingsForm")).ToBeVisibleAsync();

        await Expect(Page.Locator("#ServiceName")).ToHaveValueAsync(service.Name);
        await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "Service type" })).ToHaveValueAsync(service.ServiceTypeName);
        await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "Service implementation type" })).ToHaveValueAsync(service.ImplementationTypeName);
        await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "Service assembly path", Exact = true })).ToHaveValueAsync(service.AssemblyPath);

        var editedService = await ServiceSaveAsync();

        await Expect(Page.Locator("li.table-ul.service_li")).ToHaveCountAsync(1);
        var editedRow = FindServiceRow(editedService);
        await Expect(editedRow).ToHaveCountAsync(1);
    }
}