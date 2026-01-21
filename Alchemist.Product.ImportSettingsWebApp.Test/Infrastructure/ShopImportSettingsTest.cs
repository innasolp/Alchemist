using Alchemist.Product.ImportSettingsWebApp.Models;
using Alchemist.Test.Functional.Playwright;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace Alchemist.Product.ImportSettingsWebApp.Test.Infrastructure;

public abstract class ShopImportSettingsTest<TWebAppFactory, TInput> : PageTest, IClassFixture<TWebAppFactory>
    where TWebAppFactory :  WebApplicationFactory<ImportSettingsWebAppProgramm>
{    

    protected List<IConsoleMessage> _consoleMessages = [];

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        Page.Console += (sender, args) =>
        {
            _consoleMessages.Add(args);
        };
    }

    protected abstract Task ExpectPageLoadedAsync();

    protected abstract Task<TInput> FillInputFieldsAsync();

    protected virtual async Task PopupConfirmationWhenOtherShopSelectWithoutSavingChangesAsync()
    {
        await ExpectPageLoadedAsync();       

        await FillInputFieldsAsync();

        await this.SelectNextShopClickAsync();

        await Expect(Page.GetByRole(AriaRole.Dialog, new() { Name = "Input data will be reset. Continue?" })).ToBeVisibleAsync();
    }

    protected abstract Task SelectOtherTabAsync();
    
    protected virtual async Task PopupConfirmationWhenOtherTabSelectWithoutSavingChangesAsync()
    {
        await ExpectPageLoadedAsync();

        await FillInputFieldsAsync();

        await SelectOtherTabAsync();

        await Expect(Page.GetByRole(AriaRole.Dialog, new() { Name = "Input data will be reset. Continue?" })).ToBeVisibleAsync();
    }


    protected virtual async Task ValidationErrorWhenShopSettingsNameIsEmptyAsync()
    {
        await ExpectPageLoadedAsync();

        await this.ExpectImportSettingsRequiredValidationErrorAsync("ShopSettingsName", "Name");
    }


    protected virtual async Task ValidationErrorWhenRequiredFieldIsEmptyAsync(string inputFieldName, string property)
    {
        await ExpectPageLoadedAsync();

        await this.ExpectImportSettingsRequiredValidationErrorAsync(inputFieldName, property);
    }

    protected virtual async Task ValidationErrorWhenImportServiceNotSetAsync()
    {       
        await ExpectPageLoadedAsync();

        await this.ExpectSelectShopAsync(locator => locator.Last);

        await FillInputFieldsAsync();

        await Expect(Page.Locator("input.import-service")).ToBeEmptyAsync();

        await Page.Locator("#saveImportSettingsBtn").ClickAsync();
        await Expect(Page.Locator("#saveImportSettingsBtn")).ToBeEnabledAsync();
        await Expect(Page.GetByText($"The ImportService field is required.")).ToBeVisibleAsync();
    }
    
    protected virtual async Task ValidationErrorWhenWebLoaderNotSetAsync()
    {
        await ExpectPageLoadedAsync();

        await this.ExpectSelectShopAsync(locator => locator.Last);

        await FillInputFieldsAsync();

        await this.SetServiceButtonClickAsync("import-service");

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Service type" }).FillAsync(Guid.NewGuid().ToString());
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Service implementation type" }).FillAsync(Guid.NewGuid().ToString());
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Service assembly path", Exact = true }).FillAsync(Guid.NewGuid().ToString());
        await this.SaveServiceBtnClickAsync();

        await Expect(Page.Locator("#serviceSettingsForm > .row")).Not.ToBeVisibleAsync();

        await Expect(Page.Locator("input.web-loader")).ToBeEmptyAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await Expect(Page.GetByText($"The WebLoader field is required.")).ToBeVisibleAsync();
    }

    protected virtual async Task ExpectUploadFormJsonAsync(string fileName, string serviceTypeName)
    {
        await ExpectPageLoadedAsync();       

        await this.ExpectFileUploadAsync("import-settings", fileName, Path.Combine($"{Directory.GetCurrentDirectory()}/Content", fileName));

        await Expect(Page.Locator($"input[data-name='{nameof(ShopImportSettingsModel.ImportService)}']")).ToHaveValueAsync(serviceTypeName);
    }    
}