using Microsoft.Playwright;
using Alchemist.Import.Settings.Extensions;
using Microsoft.Playwright.Xunit;
using Alchemist.Product.Interfaces;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Import.WebApp.Models;

namespace Alchemist.Product.Import.WebApp.Test.Infrastructure;

internal static class PageTestExtensions
{

    public static async Task<ILocator> ExpectSingleElementAsync(this PageTest pageTest, ILocator parent, string expression)
    {
        var element = parent.Locator(expression);
        await pageTest.Expect(element).ToHaveCountAsync(1);
        await pageTest.Expect(element).ToBeVisibleAsync();

        return element;
    }

    public static async Task<ILocator> ExpectSingleElementAsync(this PageTest pageTest, IPage page, string expression)
    {
        var element = page.Locator(expression);
        await pageTest.Expect(element).ToHaveCountAsync(1);
        await pageTest.Expect(element).ToBeVisibleAsync();

        return element;
    }

    public static async Task<ServiceSettingsModel> FillServiceSettingsInputsAsync(this ILocator serviceForm, ServiceSettingsModel serviceSettings,  string serviceName)
    {   
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

    public static async Task ExpectShowCheckAndCloseServiceSettingsAsync(this PageTest pageTest, IPage page, ServiceSettingsModel serviceSettings)
    {
        var serviceForm = await pageTest.ExpectShowServiceModalFormAsync(page, async (page) => await pageTest.ExpectShowServiceSettingsButtonAsync(page, serviceSettings.Name));

        await pageTest.ExpectCheckServiceSettingsFieldsAsync(serviceForm, serviceSettings);

        var closeModal = page.Locator("#divModal").Locator("a[class='close']");
        await closeModal.ClickAsync();
        await pageTest.Expect(serviceForm).Not.ToBeVisibleAsync();
    }

    public static async Task ExpectCheckServiceSettingsFieldsAsync(this PageTest pageTest, ILocator serviceForm, ServiceSettingsModel serviceSettings)
    {
        await pageTest.ExpectWithNullValueAsync(serviceForm.Locator("#ServiceTypeName"), serviceSettings.ServiceTypeName);
        await pageTest.ExpectWithNullValueAsync(serviceForm.Locator("#AssemblyPath"), serviceSettings.AssemblyPath);
        await pageTest.ExpectWithNullValueAsync(serviceForm.Locator("#ServiceProviderPath"), serviceSettings.ServiceProviderPath);
    }

    public static async Task<ServiceSettingsModel> ExpectSetServiceSettingsAsync(this PageTest pageTest, IPage page, List<IShopSettings> shopSettings,  string serviceName, IShopImportSettings shopImportSettings)
    {
        var serviceForm = await pageTest.ExpectShowServiceModalFormAsync(page, async (page) => await pageTest.ExpectShowServiceSettingsButtonAsync(page, serviceName));

        var serviceSettings = shopSettings.FirstOrDefault(s => s.Name == serviceName && s.ParentSettingsId == shopImportSettings.Id)?.ToImportServiceSettings<ServiceSettingsModel>()
           ?? new ServiceSettingsModel(shopImportSettings.ShopId, 0, shopImportSettings.Id, Guid.NewGuid(), Guid.NewGuid())
           {
               Name = serviceName
           };
        serviceSettings = await serviceForm.FillServiceSettingsInputsAsync(serviceSettings, serviceName);

        var saveButton = await pageTest.ExpectSingleElementAsync(serviceForm, "button[class='btn btn-primary']");        
        await saveButton.ClickAsync();

        await pageTest.Expect(serviceForm.Locator("#AssemblyPath-error")).Not.ToBeVisibleAsync();

        await Task.Delay(500);

        await pageTest.Expect(serviceForm).Not.ToBeVisibleAsync();

        return serviceSettings;
    }
}
