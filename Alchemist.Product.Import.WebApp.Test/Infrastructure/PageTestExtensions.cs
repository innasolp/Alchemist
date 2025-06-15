using Alchemist.Import.Settings.Model;
using Microsoft.Playwright;
using Alchemist.Import.Settings.Extensions;
using Microsoft.Playwright.Xunit;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Import.WebApp.Test.Infrastructure;

internal static class PageTestExtensions
{
    public static async Task<ImportServiceSettings> FillServiceSettingsInputsAsync(this ILocator serviceForm, List<Interfaces.IShopSettings> shopSettings,  string serviceName, ShopImportSettings shopImportSettings)
    {
        var serviceSettings = shopSettings.FirstOrDefault(s => s.Name == serviceName && s.ParentSettingsId == shopImportSettings.Id)?.ToImportServiceSettings<ImportServiceSettings>()
            ?? new ImportServiceSettings
            {
                Name = serviceName
            };

        serviceSettings.ServiceTypeName = Guid.NewGuid().ToString();
        serviceSettings.AssemblyPath = Guid.NewGuid().ToString();

        if(string.IsNullOrEmpty(serviceName))
        {
            var serviceNameInput = serviceForm.Locator("#ServiceName");
            serviceSettings.Name = Guid.NewGuid().ToString();
            await serviceNameInput.FillAsync(serviceSettings.Name);
        }

        await serviceForm.Locator("#ServiceTypeName").FillAsync(serviceSettings.ServiceTypeName);
        await serviceForm.Locator("#AssemblyPath").FillAsync(serviceSettings.AssemblyPath);

        return await Task.FromResult(serviceSettings);
    }

    public static async Task<ILocator> ExpectCloseServiceSettingsButtonClickAsync(this PageTest pageTest, IPage page)
    {
        var closeServiceSettingsBtn = page.Locator("#serviceSettingsCloseBtn");
        await pageTest.Expect(closeServiceSettingsBtn).ToHaveCountAsync(1);
        await pageTest.Expect(closeServiceSettingsBtn).ToBeVisibleAsync();

        await closeServiceSettingsBtn.ClickAsync();

        return closeServiceSettingsBtn;
    }

    public static async Task ExpectWithNullValueAsync(this PageTest pageTest, ILocator locator, string? value)
    {
        await pageTest.Expect(locator).ToBeVisibleAsync();

        if (!string.IsNullOrEmpty(value))
            await pageTest.Expect(locator).ToHaveValueAsync(value);
        else
            await pageTest.Expect(locator).ToHaveValueAsync("");
    }


    public static async Task<ILocator> ExpectShowServiceModalFormAsync(this PageTest pageTest, IPage page, Func<IPage, Task<ILocator>> expectShowModalButton)
    {
        var serviceSettingsForm = page.Locator("#serviceSettingsForm");
        await pageTest.Expect(serviceSettingsForm).Not.ToBeVisibleAsync();

        var modalButton = await expectShowModalButton(page);
        await modalButton.ClickAsync();

        await Task.Delay(500);

        await pageTest.Expect(serviceSettingsForm).ToBeVisibleAsync();

        return await Task.FromResult(serviceSettingsForm);
    }

    public static async Task<ILocator> ExpectShowServiceSettingsButtonAsync(this PageTest pageTest, IPage page, string serviceName)
    {
        var settingsForm = page.Locator("#settingsForm");
        var serviceDiv = settingsForm.Locator("div[class='form-group']", new LocatorLocatorOptions { Has = page.Locator($"#{serviceName}") });
        await pageTest.Expect(serviceDiv).ToHaveCountAsync(1);

        var modalButton = serviceDiv.Locator("button[class='btn btn-primary btn-sm form-button']");
        await pageTest.Expect(modalButton).ToHaveCountAsync(1);

        return modalButton;
    }

    public static async Task ExpectCheckServiceSettingsAsync(this PageTest pageTest, IPage page, ImportServiceSettings serviceSettings)
    {
        var serviceForm = await pageTest.ExpectShowServiceModalFormAsync(page, async (page) => await pageTest.ExpectShowServiceSettingsButtonAsync(page, serviceSettings.Name));

        await pageTest.ExpectWithNullValueAsync(serviceForm.Locator("#ServiceTypeName"), serviceSettings.ServiceTypeName);
        await pageTest.ExpectWithNullValueAsync(serviceForm.Locator("#AssemblyPath"), serviceSettings.AssemblyPath);
        await pageTest.ExpectWithNullValueAsync(serviceForm.Locator("#ServiceProviderPath"), serviceSettings.ServiceProviderPath);

        var closeModal = page.Locator("#divModal").Locator("a[class='close']");
        await closeModal.ClickAsync();
        await pageTest.Expect(serviceForm).Not.ToBeVisibleAsync();
    }

    public static async Task<ImportServiceSettings> ExpectSetServiceSettingsAsync(this PageTest pageTest, IPage page, List<IShopSettings> shopSettings,  string serviceName, ShopImportSettings shopImportSettings)
    {
        var serviceForm = await pageTest.ExpectShowServiceModalFormAsync(page, async (page) => await pageTest.ExpectShowServiceSettingsButtonAsync(page, serviceName));

        var serviceSettings = await serviceForm.FillServiceSettingsInputsAsync(shopSettings, serviceName, shopImportSettings);

        var saveButton = serviceForm.Locator("button[class='btn btn-primary']");
        await pageTest.Expect(saveButton).ToHaveCountAsync(1);
        await saveButton.ClickAsync();

        await pageTest.Expect(serviceForm).Not.ToBeVisibleAsync();

        return serviceSettings;
    }
}
