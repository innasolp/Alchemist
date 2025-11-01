using Alchemist.Test.ImportSettingsWebApp.Factory;

namespace Alchemist.Product.ImportSettingsWebApp.Test.Infrastructure;

public abstract class SaveShopImportSettingsTest<TWebAppFactory, TInput> : ShopImportSettingsTest<TWebAppFactory, TInput>
    where TWebAppFactory : ImportSettingsWebAppFactory
{
    protected abstract Task ExpectSettingsLoadedAsync();

    protected abstract Task ExpectInputFieldsAsync(TInput inputs);

    protected virtual async Task FieldsEqualsInputValuesWhenSettingsSavedAsync()
    {
        await ExpectPageLoadedAsync();

        await this.ExpectSelectShopAsync(locator => locator.Last);

        var (importServiceType, importServiceImplementationType, importServiceAssemblyPath) = await this.SetServiceSettingsAsync("import-service");
        var (webLoaderServiceType, webLoaderImplementationType, webLoaderAssemblyPath) = await this.SetServiceSettingsAsync("web-loader");

        var inputs = await FillInputFieldsAsync();

        await Page.Locator("#saveImportSettingsBtn").ClickAsync();

        await Expect(Page.Locator("#saveImportSettingsBtn")).ToBeEnabledAsync();

        var url = Page.Url;

        await this.ExpectSelectNextShopAsync();

        await Page.GotoAsync(url);

        await ExpectSettingsLoadedAsync();

        await ExpectInputFieldsAsync(inputs);

        await this.SetServiceButtonClickAsync("import-service");
        await this.ExpectServiceSettingsFieldsAsync(importServiceType, importServiceImplementationType, importServiceAssemblyPath);
        await this.CloseServiceSettingsAsync();

        await this.SetServiceButtonClickAsync("web-loader");
        await this.ExpectServiceSettingsFieldsAsync(webLoaderServiceType, webLoaderImplementationType, webLoaderAssemblyPath);
    }

}
