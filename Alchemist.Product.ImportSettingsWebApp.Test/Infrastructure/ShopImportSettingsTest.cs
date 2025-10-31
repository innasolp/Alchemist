using Alchemist.Product.ImportSettingsWebApp.Models;
using Alchemist.Test.ImportSettingsWebApp.Factory;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using Alchemist.Test.Functional.Playwright;

namespace Alchemist.Product.ImportSettingsWebApp.Test.Infrastructure;

public abstract class ShopImportSettingsTest<T> : PageTest, IClassFixture<T>
    where T :  ImportSettingsWebAppFactory
{   
    protected abstract Task ExpectSettingsLoadedAsync();

    protected abstract Task FillInputFieldsAsync();

    protected virtual async Task PopupConfirmationWhenOtherShopSelectWithoutSavingChangesAsync()
    {
        await ExpectSettingsLoadedAsync();       

        await FillInputFieldsAsync();

        await this.SelectNextShopClickAsync();

        await Expect(Page.GetByRole(AriaRole.Dialog, new() { Name = "Input data will be reset. Continue?" })).ToBeVisibleAsync();
    }

    protected abstract Task SelectOtherTabAsync();
    
    protected virtual async Task PopupConfirmationWhenOtherTabSelectWithoutSavingChangesAsync()
    {
        await ExpectSettingsLoadedAsync();

        await FillInputFieldsAsync();

        await SelectOtherTabAsync();

        await Expect(Page.GetByRole(AriaRole.Dialog, new() { Name = "Input data will be reset. Continue?" })).ToBeVisibleAsync();
    }


    protected virtual async Task ValidationErrorWhenShopSettingsNameIsEmptyAsync()
    {
        await ExpectSettingsLoadedAsync();

        await this.ExpectImportSettingsRequiredValidationErrorAsync("ShopSettingsName", "Name");
    }


    protected virtual async Task ValidationErrorWhenRequiredFieldIsEmptyAsync(string inputFieldName, string property)
    {
        await ExpectSettingsLoadedAsync();

        await this.ExpectImportSettingsRequiredValidationErrorAsync(inputFieldName, property);
    }

    protected virtual async Task ValidationErrorWhenImportServiceNotSetAsync()
    {       
        await ExpectSettingsLoadedAsync();

        await this.ExpectSelectShopAsync(locator => locator.Last);

        await FillInputFieldsAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await Expect(Page.GetByText($"The ImportService field is required.")).ToBeVisibleAsync();
    }
    
    protected virtual async Task ValidationErrorWhenWebLoaderNotSetAsync()
    {
        await ExpectSettingsLoadedAsync();

        await this.ExpectSelectShopAsync(locator => locator.Last);

        await FillInputFieldsAsync();

        await this.SetServiceButtonClickAsync("import-service");

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Service type" }).FillAsync(Guid.NewGuid().ToString());
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Service implementation type" }).FillAsync(Guid.NewGuid().ToString());
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Service assembly path", Exact = true }).FillAsync(Guid.NewGuid().ToString());
        await this.SaveServiceBtnClickAsync();

        await Expect(Page.Locator("#serviceSettingsForm > .row")).Not.ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await Expect(Page.GetByText($"The WebLoader field is required.")).ToBeVisibleAsync();
    }

    protected virtual async Task UploadFormJsonAsync(string fileName, string serviceTypeName)
    {
        await ExpectSettingsLoadedAsync();       

        await this.ExpectFileUploadAsync("import-settings", fileName, Path.Combine($"{Directory.GetCurrentDirectory()}/Content", fileName));

        await Expect(Page.Locator($"input[data-name='{nameof(ShopImportSettingsModel.ImportService)}']")).ToHaveValueAsync(serviceTypeName);
    }
}
