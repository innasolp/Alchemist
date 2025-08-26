using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.Import.WebApp.Models;
using Alchemist.Product.Import.WebApp.Test.Infrastructure;
using Alchemist.Product.Interfaces;
using Microsoft.Playwright;
using Moq;
using Xunit.Abstractions;

using SettingsCommon = Alchemist.Import.Settings.Extensions.Common;

namespace Alchemist.Product.Import.WebApp.Test;

public class ShopSettingsActionTest : ImportWebAppTest
{
    public ShopSettingsActionTest(TestImportWebAppFactory testImportWebAppFactory, ITestOutputHelper testOutputHelper)
        : base(testImportWebAppFactory, testOutputHelper, httpPort:8114, httpsPort:8115)
    {
        _webAppFactory.SettingsAPIClient.Setup(s => s.SaveShopSettings(It.IsAny<IShopSettings>(), It.IsAny<IEnumerable<IShopSettings>>()))
            .Returns(SaveShopSettingsWithServicesAsync);
    }

    private async Task<List<IShopSettings>> SaveShopSettingsWithServicesAsync(IShopSettings shopSettings, IEnumerable<IShopSettings> services)
    {
        try
        {
            var existingSettings = _shopSettings.FirstOrDefault(s => s.ShopId == shopSettings.ShopId && s.Type == shopSettings.Type);
            if (existingSettings != null)
            {
                existingSettings.Name = shopSettings.Name;
                existingSettings.JsonValue = shopSettings.JsonValue;
            }

            var savingSettings = existingSettings ?? shopSettings;
            var children = _shopSettings.Where(s => s.ParentSettingsId == savingSettings.Id);

            var shopSettingsModel = savingSettings.GetShopImportSettings(children);  

            if (shopSettingsModel.Id == 0)
            {
                shopSettingsModel.Id = _shopSettings.Count + 1;
                _shopSettings.Add(shopSettingsModel.ToEntity());
            }
            else
            {
                var index = _shopSettings.IndexOf(savingSettings);
                _shopSettings[index] = shopSettingsModel.ToEntity();
            }

            var result = new List<IShopSettings>() { shopSettingsModel.ToEntity() };

            foreach (var service in services)
            {
                service.ParentSettingsId = shopSettingsModel.Id;
                _shopSettings.SaveService(service);
                result.Add(service);
            }

            return await Task.FromResult(result);
        }
        catch(Exception e)
        {
            throw;
        }
    }

    [Fact]
    public async Task ShopSettingsValidationFailedWhenRequiredFieldsNotFill()
    {
        var newPage = await Context.NewPageAsync();

        await ExpectLoadIndexPageAsync(newPage);

        var settingsForm = newPage.Locator("#settingsForm");

        await settingsForm.Locator("#ShopSettingsName").FillAsync("");       

        var saveButton = settingsForm.GetByRole(AriaRole.Button).GetByText("Save");
        await saveButton.ClickAsync();

        await Expect(settingsForm.Locator("#ShopSettingsName-error")).ToBeVisibleAsync();
        await Expect(settingsForm.Locator("#ShopSettingsName-error")).ToHaveTextAsync("The Name field is required.");
    }   
    

    [Fact]
    public async Task ShopSettingsSave()
    {
        var page = await Context.NewPageAsync();

        var startShopImportSettings = await ExpectLoadIndexPageAsync(page) as ProductShopSettingsModel;

        var serviceNames = SettingsCommon.GetPrimaryServiceNames().ToList();
        serviceNames.RemoveAll(s => s == nameof(PrimaryServiceName.RequestHeaders));

        var servicesByName = new Dictionary<string, ServiceSettingsModel>();
        foreach(var serviceName in serviceNames)
        {
            var service = await this.ExpectSetServiceSettingsAsync(page, _shopSettings, serviceName, startShopImportSettings);
            servicesByName.Add(serviceName, service);
        }

        var settingsForm = page.Locator("#settingsForm");

        var shopProductSettings = new ProductShopSettingsModel(startShopImportSettings.ShopId, startShopImportSettings.Id, startShopImportSettings.ShopGuid)
        {
            Name = Guid.NewGuid().ToString(),
            ProductUrlFormat = Guid.NewGuid().ToString(),
            CategoryUrlFormat = Guid.NewGuid().ToString()
        };

        foreach(var serviceName in serviceNames)
            shopProductSettings.UpdateServiceSettings(serviceName, servicesByName[serviceName]);       

        await settingsForm.Locator("#ShopSettingsName").FillAsync(shopProductSettings.Name);
        await settingsForm.Locator("#ProductUrlFormat").FillAsync(shopProductSettings.ProductUrlFormat);
        await settingsForm.Locator("#CategoryUrlFormat").FillAsync(shopProductSettings.CategoryUrlFormat);

        var saveButton = settingsForm.GetByRole(AriaRole.Button).GetByText("Save");
        await saveButton.ClickAsync();

        await Task.Delay(500);

        await Expect(saveButton).ToBeEnabledAsync();

        await page.CloseAsync();

        await Context.ClearCookiesAsync();

        var newPage = await Context.NewPageAsync();

        await newPage.WaitForTimeoutAsync(1000);

        await ExpectLoadIndexPageAsync(newPage);

        var newSettingsForm = newPage.Locator("#settingsForm");

        await Expect(newSettingsForm.Locator("#ShopSettingsName")).ToHaveValueAsync(shopProductSettings.Name);
        await Expect(newSettingsForm.Locator("#ProductUrlFormat")).ToHaveValueAsync(shopProductSettings.ProductUrlFormat);
        await Expect(newSettingsForm.Locator("#CategoryUrlFormat")).ToHaveValueAsync(shopProductSettings.CategoryUrlFormat);

        foreach(var service in servicesByName)
            await this.ExpectShowCheckAndCloseServiceSettingsAsync(newPage, service.Value);

        await newPage.CloseAsync();

        await Context.CloseAsync();        
    }    

    [Fact]
    public async Task ShowConfirmationWindowWhenCloseWithoutSaving()
    {
        var newPage = await Context.NewPageAsync();

        var shopImportSettings = await ExpectLoadIndexPageAsync(newPage);

        var serviceForm = await this.ExpectShowServiceModalFormAsync(newPage, 
            async (page) => await this.ExpectShowServiceSettingsButtonAsync(page, nameof(PrimaryServiceName.ImportService)));

        var serviceSettings = _shopSettings.FirstOrDefault(s => s.ParentSettingsId == shopImportSettings.Id && s.Name.IsPrimaryServiceName())?
            .ToImportServiceSettings<ServiceSettingsModel>()
           ?? new ServiceSettingsModel(shopImportSettings.ShopId, 0, shopImportSettings.Id, shopImportSettings.Guid, shopImportSettings.ShopGuid)
           {
               Name = Guid.NewGuid().ToString()
           };
        serviceSettings  = await serviceForm.FillServiceSettingsInputsAsync(serviceSettings, nameof(PrimaryServiceName.ImportService));

        var confirmationLocator = await GetConfirmationLocatorAsync(newPage);

        await this.ExpectCloseServiceSettingsButtonClickAsync(newPage);

        await ExpectConfirmationAsync(confirmationLocator);
    }

}
