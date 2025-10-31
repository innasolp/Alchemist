using Alchemist.Product.ImportSettingsWebApp.Models;
using Alchemist.Product.ImportSettingsWebApp.Test.Infrastructure;
using Alchemist.Test.Functional.Playwright;
using Alchemist.Test.ImportSettingsWebApp.Factory;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using System.Collections.ObjectModel;
using Xunit.Abstractions;
using TestCommon = Alchemist.Product.ImportSettingsWebApp.Test.Infrastructure.Common;

namespace Alchemist.Product.ImportSettingsWebApp.Test;

public class ServiceSettingsTestImportSettingsWebAppFactory()
    : ImportSettingsWebAppFactory(false,
        TestCommon.CreateShopWebAppApiFactory("ServiceSettingsTestDb", 8422, 8423, 8074, 8075).ServerAddress,
        8092, 8093,
        TestCommon.CreateSettingsApiHttpClient("ServiceSettingsTestDb", 8222, 8223))
{
}

public class ServiceSettingsTest : PageTest, IClassFixture<ServiceSettingsTestImportSettingsWebAppFactory>
{
    private readonly ServiceSettingsTestImportSettingsWebAppFactory _webAppFactory;
    private readonly ITestOutputHelper _outputHelper;
    private readonly ObservableCollection<IConsoleMessage> _messages = [];

    public ServiceSettingsTest(ServiceSettingsTestImportSettingsWebAppFactory webAppFactory, ITestOutputHelper outputHelper)
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

    private async Task ExpectShowServiceAsync(string serviceClass)
    {
        await Expect(Page.Locator($"input.form-input.{serviceClass}")).Not.ToHaveValueAsync("");

        await this.SetServiceButtonClickAsync(serviceClass);

        await Expect(Page.Locator("#serviceSettingsForm > .row")).ToBeVisibleAsync();
    }    

    private async Task CloseServiceSettingsAsync()
    {
        await Page.Locator(".service-settings-modal").Locator(".close").ClickAsync();
    }

    [Fact]
    public async Task ShowImportService()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectProductShopSettingsLoadedAsync();

        await ExpectShowServiceAsync("import-service");
    }

    [Fact]
    public async Task ShowBrowserDataLoader()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectProductShopSettingsLoadedAsync();

        await ExpectShowServiceAsync("browser-data-loader");
    }

    [Fact]
    public async Task ShowBrowserLauncher()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectProductShopSettingsLoadedAsync();

        await ExpectShowServiceAsync("browser-launcher");
    }

    [Fact]
    public async Task ShowWebLoader()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectProductShopSettingsLoadedAsync();

        await ExpectShowServiceAsync("web-loader");
    }

    [Fact]
    public async Task CloseServiceWithoutChanges()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectProductShopSettingsLoadedAsync();

        await ExpectShowServiceAsync("import-service");

        await CloseServiceSettingsAsync();

        await Expect(Page.Locator("#serviceSettingsForm > .row")).Not.ToBeVisibleAsync();
    }

    [Fact]
    public async Task PopupConfirmationWhenServiceClosingWithoutSavingChanges()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectProductShopSettingsLoadedAsync();

        await ExpectShowServiceAsync("import-service");

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Service type" }).FillAsync(Guid.NewGuid().ToString());

        await CloseServiceSettingsAsync();

        await Expect(Page.Locator("#serviceSettingsForm > .row")).ToBeVisibleAsync();

        await Expect(Page.GetByRole(AriaRole.Dialog, new() { Name = "Input values will be reset." })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task PopupConfirmationClosedAndChangesResetedWhenYesPress()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectProductShopSettingsLoadedAsync();

        await this.SetServiceButtonClickAsync("import-service");

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Service type" }).FillAsync(Guid.NewGuid().ToString());

        await CloseServiceSettingsAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Yes" }).ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Dialog, new() { Name = "Input values will be reset." })).Not.ToBeVisibleAsync();
    }

    [Fact]
    public async Task ServiceFormIsVisibleWhenConfirmationCancelResetingPress()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectProductShopSettingsLoadedAsync();

        await this.SetServiceButtonClickAsync("import-service");

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Service type" }).FillAsync(Guid.NewGuid().ToString());

        await CloseServiceSettingsAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Cancel" }).ClickAsync();

        await Expect(Page.Locator("#serviceSettingsForm > .row")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ServiceValidationErrorWhenServiceTypeIsEmpty()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectProductShopSettingsLoadedAsync();

        await this.SetServiceButtonClickAsync("import-service");

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Service type" }).FillAsync("");

        await this.SaveServiceBtnClickAsync();

        await Expect(Page.Locator("#serviceSettingsForm > .row")).ToBeVisibleAsync();

        await Expect(Page.GetByText("The Service type field is required")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ServiceValidationErrorWhenServiceImplementationTypeAndAssemblyPathAreEmpty()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectProductShopSettingsLoadedAsync();

        await this.SetServiceButtonClickAsync("import-service");

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Service implementation type" }).FillAsync("");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Service assembly path", Exact = true }).FillAsync("");

        await this.SaveServiceBtnClickAsync();

        await Expect(Page.GetByText("Assembly path for implementation type or implementation factory is required")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ServiceFieldChangeWhenServiceTypeChangedAndSaved()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectProductShopSettingsLoadedAsync();

        var previousServiceType = await Page.GetByRole(AriaRole.Textbox, new() { Name = "ImportService" }).InputValueAsync();

        await this.SetServiceButtonClickAsync("import-service");

        var newServiceTypeName = Guid.NewGuid().ToString();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Service type" }).FillAsync(newServiceTypeName);

        await this.SaveServiceBtnClickAsync();

        await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "ImportService" })).ToHaveValueAsync(newServiceTypeName);
    }

    [Fact]
    public async Task ServiceFieldsAfterServiceSaving()
    {
        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectProductShopSettingsLoadedAsync();

        await this.SetServiceButtonClickAsync("import-service");

        var newServiceImplementationType = Guid.NewGuid().ToString();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Service implementation type" }).FillAsync(newServiceImplementationType);

        var newServiceAssemblyPath = Guid.NewGuid().ToString();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Service assembly path", Exact = true }).FillAsync(newServiceAssemblyPath);

        await this.SaveServiceBtnClickAsync();

        await this.SetServiceButtonClickAsync("import-service");

        await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "Service implementation type" })).ToHaveValueAsync(newServiceImplementationType);
        await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "Service assembly path", Exact = true })).ToHaveValueAsync(newServiceAssemblyPath);
    }

    [Fact]
    public async Task ServiceSetWhenUploadFromFile()
    {
        var fileName = "importservice.json";
        var serviceTypeName = "IImportService";

        var url = _webAppFactory.ServerAddress;
        await Page.GotoAsync(url);

        await this.ExpectProductShopSettingsLoadedAsync();

        await this.ExpectFileUploadAsync("import-service", fileName, Path.Combine($"{Directory.GetCurrentDirectory()}/Content", fileName));

        await Expect(Page.Locator($"input[data-name='{nameof(ShopImportSettingsModel.ImportService)}']")).ToHaveValueAsync(serviceTypeName);

        await ExpectShowServiceAsync("import-service");
        await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "Service type" })).ToHaveValueAsync(serviceTypeName);
    }
}
