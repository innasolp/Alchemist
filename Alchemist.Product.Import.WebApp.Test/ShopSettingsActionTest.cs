using Alchemist.Import.Settings.Extensions;
using Alchemist.Import.Settings.Interfaces;
using Alchemist.Import.Settings.Model;
using Alchemist.Product.Interfaces;
using Microsoft.Playwright;
using Moq;
using System.Reflection;
using Xunit.Abstractions;

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
            if (existingSettings != null) existingSettings.JsonValue = shopSettings.JsonValue;

            var savingSettings = existingSettings ?? shopSettings;

            ShopImportSettings shopSettingsModel = savingSettings.Type == Interfaces.ShopSettingType.Product
                    ? await savingSettings.GetShopImportSettings<ProductShopImportSettings, ImportServiceSettings>((parentSettingsId) => Task.FromResult(_shopSettings.Where(s => s.ParentSettingsId == parentSettingsId).ToList()))
                    : await savingSettings.GetShopImportSettings<CategoryShopImportSettings, ImportServiceSettings>((parentSettingsId) => Task.FromResult(_shopSettings.Where(s => s.ParentSettingsId == parentSettingsId).ToList()));


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
                SaveService(service);
                result.Add(service);
            }

            return await Task.FromResult(result);
        }
        catch(Exception e)
        {
            throw;
        }
    }

    private void SaveService(IShopSettings serviceSettings)
    {
        if (serviceSettings.Id == 0)
        {
            serviceSettings.Id = _shopSettings.Count + 1;
            _shopSettings.Add(serviceSettings);
        }
        else
        {
            var existingItem = _shopSettings.FirstOrDefault(s => s.Id == serviceSettings.Id);
            var index = _shopSettings.IndexOf(existingItem);
            _shopSettings[index] = serviceSettings;
        }        
    }

    [Fact]
    public async Task ShopSettingsValidationFailedWhenRequiredFieldsNotFill()
    {
        var newPage = await Context.NewPageAsync();

        await ExpectLoadIndexPageAsync(newPage);

        var settingsForm = newPage.Locator("#settingsForm");

        await settingsForm.Locator("#Name").FillAsync("");
        await settingsForm.Locator("#SettingsUrl").FillAsync("");

        var saveButton = settingsForm.GetByRole(AriaRole.Button).GetByText("Save");
        await saveButton.ClickAsync();

        await Expect(settingsForm.Locator("#Name-error")).ToBeVisibleAsync();
        await Expect(settingsForm.Locator("#Name-error")).ToHaveTextAsync("The Name field is required.");

        await Expect(settingsForm.Locator("#SettingsUrl-error")).ToBeVisibleAsync();
        await Expect(settingsForm.Locator("#SettingsUrl-error")).ToHaveTextAsync("The SettingsUrl field is required.");
    }

    private async Task<ILocator> ShowServiceModalFormAsync(IPage page, string serviceName)
    {
        var settingsForm = page.Locator("#settingsForm");
        var serviceDiv = settingsForm.Locator("div[class='form-group']", new LocatorLocatorOptions { Has = page.Locator($"#{serviceName}") });
        await Expect(serviceDiv).ToHaveCountAsync(1);

        var modalButton = serviceDiv.Locator("button[class='btn btn-primary btn-sm form-button']");
        await Expect(modalButton).ToHaveCountAsync(1);

        var serviceSettingsForm = page.Locator("#serviceSettingsForm");
        await Expect(serviceSettingsForm).Not.ToBeVisibleAsync();

        await modalButton.ClickAsync(new LocatorClickOptions { Delay = 500});        

        await Expect(serviceSettingsForm).ToBeVisibleAsync();        

        return await Task.FromResult(serviceSettingsForm);
    }


    [Fact]
    public async Task ShopSettingsShowServiceModal()
    {
        var newPage = await Context.NewPageAsync();
        await ExpectLoadIndexPageAsync(newPage);
        await ShowServiceModalFormAsync(newPage, nameof(IShopImportSettings.ImportService));
    }

    [Fact]
    public async Task ShopSettingsServiceValidationFailedWhenRequiredFieldsNotFill()
    {
        var newPage = await Context.NewPageAsync();

        await ExpectLoadIndexPageAsync(newPage);

        var serviceForm = await ShowServiceModalFormAsync(newPage, nameof(IShopImportSettings.ImportService));

        var saveButton = serviceForm.Locator("button[class='btn btn-primary']");
        await Expect(saveButton).ToHaveCountAsync(1);

        await saveButton.ClickAsync();

        await Expect(serviceForm).ToBeVisibleAsync();

        await Expect(serviceForm.Locator("#ServiceTypeName-error")).ToBeVisibleAsync();
        await Expect(serviceForm.Locator("#ServiceTypeName-error")).ToContainTextAsync("The Service type field is required.");
    }

    private async Task<ImportServiceSettings> FillServiceSettingsInputsAsync(ILocator serviceForm, string serviceName, ShopImportSettings shopImportSettings)
    {
        var serviceSettings = _shopSettings.FirstOrDefault(s => s.Name == serviceName && s.ParentSettingsId == shopImportSettings.Id)?.ToImportServiceSettings<ImportServiceSettings>()
            ?? new ImportServiceSettings
            {
                Name = serviceName
            };

        serviceSettings.ServiceTypeName = Guid.NewGuid().ToString();
        serviceSettings.AssemblyPath = Guid.NewGuid().ToString();

        await serviceForm.Locator("#ServiceTypeName").FillAsync(serviceSettings.ServiceTypeName);
        await serviceForm.Locator("#AssemblyPath").FillAsync(serviceSettings.AssemblyPath);

        return await Task.FromResult(serviceSettings);
    }

    private async Task<ImportServiceSettings> SetServiceSettingsAsync(IPage page, string serviceName, ShopImportSettings shopImportSettings)
    {
        var serviceForm = await ShowServiceModalFormAsync(page, serviceName);

        var serviceSettings = await FillServiceSettingsInputsAsync(serviceForm, serviceName, shopImportSettings);

        var saveButton = serviceForm.Locator("button[class='btn btn-primary']");
        await Expect(saveButton).ToHaveCountAsync(1);
        await saveButton.ClickAsync();

        await Expect(serviceForm).Not.ToBeVisibleAsync();

        return serviceSettings;
    }

    private async Task CheckServiceSettingsAsync(IPage page, ImportServiceSettings serviceSettings)
    {
        var serviceForm = await ShowServiceModalFormAsync(page, serviceSettings.Name);      

//        await ExpectWithNullValueAsync(serviceForm.Locator("#ImplementationTypeName"), serviceSettings.ImplementationTypeName); 
        await ExpectWithNullValueAsync(serviceForm.Locator("#ServiceTypeName"), serviceSettings.ServiceTypeName);
        await ExpectWithNullValueAsync(serviceForm.Locator("#AssemblyPath"), serviceSettings.AssemblyPath);
        await ExpectWithNullValueAsync(serviceForm.Locator("#ServiceProviderPath"), serviceSettings.ServiceProviderPath);

        var closeModal = page.Locator("#divModal").Locator("a[class='close']");
        await closeModal.ClickAsync();
        await Expect(serviceForm).Not.ToBeVisibleAsync();
    }


    [Fact]
    public async Task ShopSettingsImportServiceSet()
    {
        await Context.ClearCookiesAsync();

        var newPage = await Context.NewPageAsync();
        var shopImportSettings = await ExpectLoadIndexPageAsync(newPage);
        var importService = await SetServiceSettingsAsync(newPage, nameof(IShopImportSettings.ImportService), shopImportSettings);
        await CheckServiceSettingsAsync(newPage, importService);
    }

    [Fact]
    public async Task ShopSettingsImportServiceUploadFromFile()
    {
        var newPage = await Context.NewPageAsync();

        await ExpectLoadIndexPageAsync(newPage);

        var settingsForm = newPage.Locator("#settingsForm");

        var fileName = "importservice.json";
        var filePath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Content/{fileName}";

        var flexDiv = settingsForm.Locator("div[class='flex control-group']");
        Assert.True((await flexDiv.CountAsync()) >= 1);

        var uploadFileDiv = await UploadFileAsync(newPage, flexDiv, "importServiceJson", filePath);

        var fileNameDiv = uploadFileDiv.Locator("div");
        await Expect(fileNameDiv).ToHaveTextAsync(fileName);
    }

    [Fact]
    public async Task ShopSettingsBrowserDataLoaderSet()
    {
        var newPage = await Context.NewPageAsync();
        var shopImportSettings = await ExpectLoadIndexPageAsync(newPage);
        var service = await SetServiceSettingsAsync(newPage, nameof(IShopImportSettings.BrowserDataLoader), shopImportSettings);        
        await CheckServiceSettingsAsync(newPage, service);
        await newPage.CloseAsync();
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

        var uploadFileDiv = await UploadFileAsync(newPage, flexDiv, "browserDataLoaderJson", filePath);

        var fileNameDiv = uploadFileDiv.Locator("div");
        await Expect(fileNameDiv).ToHaveTextAsync(fileName);
    }


    [Fact]
    public async Task ShopSettingsWebLoaderSet()
    {
        var newPage = await Context.NewPageAsync();
        var shopImportSettings = await ExpectLoadIndexPageAsync(newPage);
        var service = await SetServiceSettingsAsync(newPage, nameof(IShopImportSettings.WebLoader), shopImportSettings);
        await CheckServiceSettingsAsync(newPage, service);
    }

    [Fact]
    public async Task ShopSettingsWebLoaderUploadFromFile()
    {
        var newPage = await Context.NewPageAsync();

        await ExpectLoadIndexPageAsync(newPage);

        var settingsForm = newPage.Locator("#settingsForm");

        var fileName = "webloader.firefox.json";
        var filePath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Content/{fileName}";

        var flexDiv = settingsForm.Locator("div[class='form-group']").Locator("div[class='flex  control-group']");
        await Expect(flexDiv).Not.ToBeEmptyAsync();

        var uploadFileDiv = await UploadFileAsync(newPage, flexDiv, "webLoaderJson", filePath);

        var fileNameDiv = uploadFileDiv.Locator("div");
        await Expect(fileNameDiv).ToHaveTextAsync(fileName);
    }

    private async Task<ILocator> UploadFileAsync(IPage page, ILocator locator, string uploadId, string filePath)
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
    public async Task ShopSettingsRequestHeadersUploadFromFile()
    {
        var newPage = await Context.NewPageAsync();

        var shopImportSettings = await ExpectLoadIndexPageAsync(newPage);

        var settingsForm = newPage.Locator("#settingsForm");

        var fileName = "Ozon.Headers.Firefox.json";
        var filePath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Content/{fileName}";

        var flexDiv = settingsForm.Locator("div[class='form-group']");//.Locator("div[class='flex control-group']");
        var uploadFileDiv = await UploadFileAsync(newPage, flexDiv, "requestHeadersJson", filePath);

        var fileNameDiv = uploadFileDiv.Locator("div");
        await Expect(fileNameDiv).ToHaveTextAsync(fileName);
    }

    [Fact]
    public async Task ShopSettingsSave()
    {
        var page = await Context.NewPageAsync();

        var shopImportSettings = await ExpectLoadIndexPageAsync(page) as ProductShopImportSettings;

        var importService = await SetServiceSettingsAsync(page, nameof(IShopImportSettings.ImportService), shopImportSettings);
        var webLoader = await SetServiceSettingsAsync(page, nameof(IShopImportSettings.WebLoader), shopImportSettings);
        var browserDataLoader = await SetServiceSettingsAsync(page, nameof(IShopImportSettings.BrowserDataLoader), shopImportSettings);
        
        var settingsForm = page.Locator("#settingsForm");

        var shopProductSettings = new ProductShopImportSettings()
        {
            ImportService = importService,
            BrowserDataLoader = browserDataLoader,
            WebLoader = webLoader,
            Name = Guid.NewGuid().ToString(),
            Url = Guid.NewGuid().ToString()
        };

        await settingsForm.Locator("#Name").FillAsync(shopProductSettings.Name);
        await settingsForm.Locator("#SettingsUrl").FillAsync(shopProductSettings.Url);

        var saveButton = settingsForm.GetByRole(AriaRole.Button).GetByText("Save");
        await saveButton.ClickAsync();
        await Expect(saveButton).ToBeEnabledAsync();

        await page.CloseAsync();

        await Context.ClearCookiesAsync();

        var newPage = await Context.NewPageAsync();

        await newPage.WaitForTimeoutAsync(1000);

        await ExpectLoadIndexPageAsync(newPage);

        var newSettingsForm = newPage.Locator("#settingsForm");

        await Expect(newSettingsForm.Locator("#Name")).ToHaveValueAsync(shopProductSettings.Name);
        await Expect(newSettingsForm.Locator("#SettingsUrl")).ToHaveValueAsync(shopProductSettings.Url);

        await CheckServiceSettingsAsync(newPage, importService);
        await CheckServiceSettingsAsync(newPage, browserDataLoader);
        await CheckServiceSettingsAsync(newPage, webLoader);

        await newPage.CloseAsync();

        await Context.CloseAsync();        
    }

    private async Task<ILocator> ExpectCloseServiceSettingsButtonClickAsync(IPage page)
    {
        var closeServiceSettingsBtn = page.Locator("#serviceSettingsCloseBtn");
        await Expect(closeServiceSettingsBtn).ToHaveCountAsync(1);
        await Expect(closeServiceSettingsBtn).ToBeVisibleAsync();

        await closeServiceSettingsBtn.ClickAsync();

        return closeServiceSettingsBtn;
    }

    [Fact]
    public async Task ShowConfirmationWindowWhenCloseWithoutSaving()
    {
        var newPage = await Context.NewPageAsync();

        var shopImportSettings = await ExpectLoadIndexPageAsync(newPage);

        var serviceForm = await ShowServiceModalFormAsync(newPage, nameof(IShopImportSettings.ImportService));

        var serviceSettings  = await FillServiceSettingsInputsAsync(serviceForm, nameof(IShopImportSettings.ImportService), shopImportSettings);

        var confirmationLocator = await GetConfirmationLocatorAsync(newPage);

        await ExpectCloseServiceSettingsButtonClickAsync(newPage);

        await ExpectConfirmationAsync(confirmationLocator);
    }

    [Fact]
    public async Task ServiceSettingsNotCloseWhenResetConfirmationCancel()
    {
        var newPage = await Context.NewPageAsync();

        var shopImportSettings = await ExpectLoadIndexPageAsync(newPage);

        var serviceForm = await ShowServiceModalFormAsync(newPage, nameof(IShopImportSettings.ImportService));

        var serviceSettings = await FillServiceSettingsInputsAsync(serviceForm, nameof(IShopImportSettings.ImportService), shopImportSettings);
        var confirmationLocator = await GetConfirmationLocatorAsync(newPage);

        await ExpectCloseServiceSettingsButtonClickAsync(newPage);

        await ExpectConfirmationAsync(confirmationLocator);

        await CancelConfirmationAsync(confirmationLocator);

        await Expect(confirmationLocator).Not.ToBeVisibleAsync();

        await Expect(serviceForm.Locator("#ServiceTypeName")).ToHaveValueAsync(serviceSettings.ServiceTypeName);
        await Expect(serviceForm.Locator("#AssemblyPath")).ToHaveValueAsync(serviceSettings.AssemblyPath);
    }

    [Fact]
    public async Task ServiceSettingsResetedWhenResetConfirmed()
    {
        var newPage = await Context.NewPageAsync();

        var shopImportSettings = await ExpectLoadIndexPageAsync(newPage);

        var serviceForm = await ShowServiceModalFormAsync(newPage, nameof(IShopImportSettings.ImportService));

        await ExpectWithNullValueAsync(serviceForm.Locator("#ServiceTypeName"), shopImportSettings.ImportService?.ServiceTypeName);
        await ExpectWithNullValueAsync(serviceForm.Locator("#AssemblyPath"), shopImportSettings.ImportService?.AssemblyPath);

        var serviceSettings = await FillServiceSettingsInputsAsync(serviceForm, nameof(IShopImportSettings.ImportService), shopImportSettings);
        var resetConfirmationLocator = await GetConfirmationLocatorAsync(newPage);

        await ExpectCloseServiceSettingsButtonClickAsync(newPage);

        await ExpectConfirmationAsync(resetConfirmationLocator);

        await ConfirmAsync(resetConfirmationLocator);

        await Expect(resetConfirmationLocator).Not.ToBeVisibleAsync();

        await Expect(serviceForm).Not.ToBeVisibleAsync();

        serviceForm = await ShowServiceModalFormAsync(newPage, nameof(IShopImportSettings.ImportService));        

        await ExpectWithNullValueAsync(serviceForm.Locator("#ServiceTypeName"), shopImportSettings.ImportService?.ServiceTypeName);
        await ExpectWithNullValueAsync(serviceForm.Locator("#AssemblyPath"),shopImportSettings.ImportService?.AssemblyPath);
    }

}
