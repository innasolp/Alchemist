using Alchemist.Import.Settings.Model;
using Microsoft.Playwright;
using Alchemist.Import.Settings.Extensions;
using Microsoft.Playwright.Xunit;

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
}
