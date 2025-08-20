using Alchemist.Import.Settings.Interfaces;
using Alchemist.Product.Import.WebApp.Models;
using Alchemist.Product.Import.WebApp.Test.Infrastructure;
using Microsoft.Playwright;
using System.Reflection;
using Xunit.Abstractions;
using Alchemist.Import.Settings.Extensions;

namespace Alchemist.Product.Import.WebApp.Test;

public class ServiceSettingsActionTest(TestImportWebAppFactory webAppFactory, ITestOutputHelper testOutputHelper) 
    : ImportWebAppTest(webAppFactory, testOutputHelper, 8120, 8121)
{  
    private async Task<ILocator> ExpectUploadFileAsync(IPage page, ILocator locator, string uploadId, string filePath)
    {
        var inputFile = page.Locator($"#{uploadId}");
        await Expect(inputFile).Not.ToBeVisibleAsync();
        await Expect(inputFile).ToHaveCountAsync(1);

        var uploadFileDiv = locator.Locator("div", new LocatorLocatorOptions { Has = inputFile });
        await Expect(uploadFileDiv).ToHaveCountAsync(1);

        var uploadButton = uploadFileDiv.GetByRole(AriaRole.Button);
        await Expect(uploadButton).ToBeVisibleAsync();

        await uploadButton.ClickAsync();

        await inputFile.SetInputFilesAsync(filePath);

        return uploadFileDiv;
    }

    [Fact]
    public async Task SetServiceShowServiceFormModal()
    {
        var newPage = await Context.NewPageAsync();
        await ExpectLoadIndexPageAsync(newPage);
        await this.ExpectShowServiceModalFormAsync(newPage, async (page) => await this.ExpectShowServiceSettingsButtonAsync(page, nameof(IShopImportSettings.ImportService)));
    }

    [Fact]
    public async Task ShopSettingsServiceValidationFailedWhenRequiredFieldsNotFill()
    {
        var newPage = await Context.NewPageAsync();

        await ExpectLoadIndexPageAsync(newPage);

        var serviceForm = await this.ExpectShowServiceModalFormAsync(newPage, async (page) => await this.ExpectShowServiceSettingsButtonAsync(page, nameof(IShopImportSettings.ImportService)));

        var saveButton = await this.ExpectSingleElementAsync(serviceForm,"button[class='btn btn-primary']");   
        await saveButton.ClickAsync();

        await Expect(serviceForm).ToBeVisibleAsync();

        await serviceForm.Locator("#ServiceTypeName").FillAsync("");

        await Expect(serviceForm.Locator("#ServiceTypeName-error")).ToBeVisibleAsync();
        await Expect(serviceForm.Locator("#ServiceTypeName-error")).ToContainTextAsync("The Service type field is required.");
    }

    [Fact]
    public async Task ImportServiceSetSuccessWhenDataIsValid()
    {
        await Context.ClearCookiesAsync();

        var newPage = await Context.NewPageAsync();
        var shopImportSettings = await ExpectLoadIndexPageAsync(newPage);
        var importService = await this.ExpectSetServiceSettingsAsync(newPage, _shopSettings, nameof(IShopImportSettings.ImportService), shopImportSettings);
        await this.ExpectShowCheckAndCloseServiceSettingsAsync(newPage, importService);
    }

    [Fact]
    public async Task WebLoaderSet()
    {
        var newPage = await Context.NewPageAsync();
        var shopImportSettings = await ExpectLoadIndexPageAsync(newPage);
        var service = await this.ExpectSetServiceSettingsAsync(newPage, _shopSettings, nameof(IShopImportSettings.WebLoader), shopImportSettings);
        await this.ExpectShowCheckAndCloseServiceSettingsAsync(newPage, service);
    }


    [Fact]
    public async Task ImportServiceUploadFromFile()
    {
        var newPage = await Context.NewPageAsync();

        await ExpectLoadIndexPageAsync(newPage);

        var settingsForm = newPage.Locator("#settingsForm");

        var fileName = "importservice.json";
        var filePath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Content/{fileName}";

        var flexDiv = settingsForm.Locator("div[class='flex control-group']");
        Assert.True((await flexDiv.CountAsync()) >= 1);

        var uploadFileDiv = await ExpectUploadFileAsync(newPage, flexDiv, "importServiceJson", filePath);

        var fileNameDiv = uploadFileDiv.Locator("div");
        await Expect(fileNameDiv).ToHaveTextAsync(fileName);
    }

    [Fact]
    public async Task RequestHeadersUploadFromFile()
    {
        var newPage = await Context.NewPageAsync();

        var shopImportSettings = await ExpectLoadIndexPageAsync(newPage);

        var settingsForm = newPage.Locator("#settingsForm");

        var fileName = "Ozon.Headers.Firefox.json";
        var filePath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Content/{fileName}";

        var flexDiv = settingsForm.Locator("div[class='form-group']");//.Locator("div[class='flex control-group']");
        var uploadFileDiv = await ExpectUploadFileAsync(newPage, flexDiv, "requestHeadersJson", filePath);

        var fileNameDiv = uploadFileDiv.Locator("div");
        await Expect(fileNameDiv).ToHaveTextAsync(fileName);
    }

    [Fact]
    public async Task BrowserDataLoaderSet()
    {
        var newPage = await Context.NewPageAsync();
        var shopImportSettings = await ExpectLoadIndexPageAsync(newPage);
        var service = await this.ExpectSetServiceSettingsAsync(newPage, _shopSettings, nameof(IShopImportSettings.BrowserDataLoader), shopImportSettings);
        await this.ExpectShowCheckAndCloseServiceSettingsAsync(newPage, service);
        await newPage.CloseAsync();
    }

    [Fact]
    public async Task BrowserLauncherSet()
    {
        var newPage = await Context.NewPageAsync();
        var shopImportSettings = await ExpectLoadIndexPageAsync(newPage);
        var service = await this.ExpectSetServiceSettingsAsync(newPage, _shopSettings, nameof(IShopImportSettings.BrowserLauncher), shopImportSettings);
        await this.ExpectShowCheckAndCloseServiceSettingsAsync(newPage, service);
        await newPage.CloseAsync();
    }

    [Fact]
    public async Task WebLoaderUploadFromFile()
    {
        var newPage = await Context.NewPageAsync();

        await ExpectLoadIndexPageAsync(newPage);

        var settingsForm = newPage.Locator("#settingsForm");

        var fileName = "webloader.firefox.json";
        var filePath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Content/{fileName}";

        var flexDiv = settingsForm.Locator("div[class='form-group']").Locator("div[class='flex  control-group']");
        await Expect(flexDiv).Not.ToBeEmptyAsync();

        var uploadFileDiv = await ExpectUploadFileAsync(newPage, flexDiv, "webLoaderJson", filePath);

        var fileNameDiv = uploadFileDiv.Locator("div");
        await Expect(fileNameDiv).ToHaveTextAsync(fileName);
    }
    [Fact]
    public async Task ShopSettingsBrowserDataLoaderUploadFromFile()
    {
        var newPage = await Context.NewPageAsync();

        await ExpectLoadIndexPageAsync(newPage);

        var settingsForm = newPage.Locator("#settingsForm");

        var fileName = "browserloader.firefox.json";
        var filePath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Content/{fileName}";

        var flexDiv = settingsForm.Locator("div[class='flex control-group']");
        Assert.True((await flexDiv.CountAsync()) >= 1);

        var uploadFileDiv = await ExpectUploadFileAsync(newPage, flexDiv, "browserDataLoaderJson", filePath);

        var fileNameDiv = uploadFileDiv.Locator("div");
        await Expect(fileNameDiv).ToHaveTextAsync(fileName);
    }

    [Fact]
    public async Task ServiceSettingsResetedWhenResetConfirmed()
    {
        var newPage = await Context.NewPageAsync();

        var shopImportSettings = await ExpectLoadIndexPageAsync(newPage);

        var serviceForm = await this.ExpectShowServiceModalFormAsync(newPage, async (page) => await this.ExpectShowServiceSettingsButtonAsync(page, nameof(IShopImportSettings.ImportService)));

        await this.ExpectCheckServiceSettingsFieldsAsync(serviceForm, shopImportSettings.ImportService);

        var serviceSettings = _shopSettings.FirstOrDefault(s => s.ParentSettingsId == shopImportSettings.Id && Helper.IsServiceSettingsPrimary(s.Name))?
            .ToImportServiceSettings<ServiceSettingsModel>()
           ?? new ServiceSettingsModel(shopImportSettings.ShopId, 0, shopImportSettings.Id, shopImportSettings.Guid, shopImportSettings.ShopGuid)
           {
               Name = Guid.NewGuid().ToString()
           };
        serviceSettings = await serviceForm.FillServiceSettingsInputsAsync(serviceSettings, nameof(IShopImportSettings.ImportService));
        var resetConfirmationLocator = await GetConfirmationLocatorAsync(newPage);

        await this.ExpectCloseServiceSettingsButtonClickAsync(newPage);

        await ExpectConfirmationAsync(resetConfirmationLocator);

        await ConfirmAsync(resetConfirmationLocator);

        await Expect(resetConfirmationLocator).Not.ToBeVisibleAsync();

        await Expect(serviceForm).Not.ToBeVisibleAsync();

        serviceForm = await this.ExpectShowServiceModalFormAsync(newPage, async (page) => await this.ExpectShowServiceSettingsButtonAsync(page, nameof(IShopImportSettings.ImportService)));

        await this.ExpectWithNullValueAsync(serviceForm.Locator("#ServiceTypeName"), shopImportSettings.ImportService?.ServiceTypeName);
        await this.ExpectWithNullValueAsync(serviceForm.Locator("#AssemblyPath"), shopImportSettings.ImportService?.AssemblyPath);
    }

    [Fact]
    public async Task ServiceSettingsNotCloseWhenResetConfirmationCancel()
    {
        var newPage = await Context.NewPageAsync();

        var shopImportSettings = await ExpectLoadIndexPageAsync(newPage);

        var serviceForm = await this.ExpectShowServiceModalFormAsync(newPage, async (page) => await this.ExpectShowServiceSettingsButtonAsync(page, nameof(IShopImportSettings.ImportService)));

        var serviceSettings = _shopSettings.FirstOrDefault(s => s.ParentSettingsId == shopImportSettings.Id && Helper.IsServiceSettingsPrimary(s.Name))?
            .ToImportServiceSettings<ServiceSettingsModel>()
           ?? new ServiceSettingsModel(shopImportSettings.ShopId, 0, shopImportSettings.Id, shopImportSettings.Guid, shopImportSettings.ShopGuid)
           {
               Name = Guid.NewGuid().ToString()
           };
        serviceSettings = await serviceForm.FillServiceSettingsInputsAsync(serviceSettings, nameof(IShopImportSettings.ImportService));
        var confirmationLocator = await GetConfirmationLocatorAsync(newPage);

        await this.ExpectCloseServiceSettingsButtonClickAsync(newPage);

        await ExpectConfirmationAsync(confirmationLocator);

        await CancelConfirmationAsync(confirmationLocator);

        await Expect(confirmationLocator).Not.ToBeVisibleAsync();

        await Expect(serviceForm.Locator("#ServiceTypeName")).ToHaveValueAsync(serviceSettings.ServiceTypeName);
        await Expect(serviceForm.Locator("#AssemblyPath")).ToHaveValueAsync(serviceSettings.AssemblyPath);
    }
}
