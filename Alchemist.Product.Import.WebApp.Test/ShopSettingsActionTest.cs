using Alchemist.Import.Settings.Model;
using Json.Extensions;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Playwright;
using System.IO;
using System.Reflection;
using Xunit.Abstractions;

namespace Alchemist.Product.Import.WebApp.Test;

public class ShopSettingsActionTest : ImportWebAppTest
{
    public ShopSettingsActionTest(TestImportWebAppFactory testImportWebAppFactory, ITestOutputHelper testOutputHelper) 
        : base(testImportWebAppFactory, testOutputHelper)
    {
    }

    [Fact]
    public async Task ShopSettingsValidationFailedWhenRequiredFieldsNotFill()
    {
        await ExpectLoadIndexPageAsync();

        var settingsForm = Page.Locator("#settingsForm");

        await settingsForm.Locator("#Name").FillAsync("");
        await settingsForm.Locator("#Url").FillAsync("");

        var saveButton = settingsForm.GetByRole(Microsoft.Playwright.AriaRole.Button).GetByText("Save");
        await saveButton.ClickAsync();

        await Expect(settingsForm.Locator("#Name-error")).ToBeVisibleAsync();
        await Expect(settingsForm.Locator("#Name-error")).ToHaveTextAsync("The Name field is required.");
        
        await Expect(settingsForm.Locator("#Url-error")).ToBeVisibleAsync();
        await Expect(settingsForm.Locator("#Url-error")).ToHaveTextAsync("The Url field is required.");
    }

    private async Task<ILocator> ShowServiceModalFormAsync(string serviceName)
    {
        await ExpectLoadIndexPageAsync();

        var settingsForm = Page.Locator("#settingsForm");
        var importServiceDiv = settingsForm.Locator("div[class='form-group']", new LocatorLocatorOptions { Has = Page.Locator($"#{serviceName}") });
        await Expect(importServiceDiv).ToHaveCountAsync(1);

        var modalButton = importServiceDiv.Locator("button[class='btn btn-primary btn-sm form-button']");
        await Expect(modalButton).ToHaveCountAsync(1);

        var modalForm = Page.Locator("#modalForm");
        await Expect(modalForm).Not.ToBeVisibleAsync();

        await modalButton.ClickAsync();

        await Expect(modalForm).ToBeVisibleAsync();

        return await Task.FromResult(modalForm);
    }

    [Fact]
    public async Task ShopSettingsShowServiceModal()
    {
        await ShowServiceModalFormAsync("ImportService");
    }

    [Fact]
    public async Task ShopSettingsServiceValidationFailedWhenRequiredFieldsNotFill()
    {
        var serviceForm = await ShowServiceModalFormAsync("ImportService");

        var saveButton = serviceForm.Locator("button[class='btn btn-primary']");
        await Expect(saveButton).ToHaveCountAsync(1);

        await saveButton.ClickAsync();

        await Expect(serviceForm).ToBeVisibleAsync();

        await Expect(serviceForm.Locator("#ServiceTypeName-error")).ToBeVisibleAsync();
        await Expect(serviceForm.Locator("#ServiceTypeName-error")).ToContainTextAsync("The Service type field is required.");
    }

    private async Task SetServiceSettingsAsync(string serviceName)
    {
        var serviceForm = await ShowServiceModalFormAsync(serviceName);

        var serviceSettings = new ImportServiceSettings
        {
            ServiceTypeName = Guid.NewGuid().ToString(),
            AssemblyPath = Guid.NewGuid().ToString()
        };

        await serviceForm.Locator("#ServiceTypeName").FillAsync(serviceSettings.ServiceTypeName);
        await serviceForm.Locator("input[name='AssemblyPath']").FillAsync(serviceSettings.AssemblyPath);

        var saveButton = serviceForm.Locator("button[class='btn btn-primary']");
        await Expect(saveButton).ToHaveCountAsync(1);
        await saveButton.ClickAsync();

        await Expect(serviceForm).Not.ToBeVisibleAsync();

        serviceForm = await ShowServiceModalFormAsync(serviceName);

        await serviceForm.Locator("#ServiceTypeName").FillAsync(serviceSettings.ServiceTypeName);
        await serviceForm.Locator("input[name='AssemblyPath']").FillAsync(serviceSettings.AssemblyPath);
    }

    [Fact]
    public async Task ShopSettingsImportServiceSet()
    {
        await SetServiceSettingsAsync("ImportService");
    }
    
    [Fact]
    public async Task ShopSettingsImportServiceUploadFromFile()
    {
        await ExpectLoadIndexPageAsync();

        var settingsForm = Page.Locator("#settingsForm");

        var fileName = "importservice.json";
        var filePath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Content/{fileName}";

        var flexDiv = settingsForm.Locator("div[class='flex control-group']");
        Assert.True((await flexDiv.CountAsync()) >= 1);

        var uploadFileDiv = await UploadFileAsync(flexDiv, "importServiceJson", filePath);

        var fileNameDiv = uploadFileDiv.Locator("div");
        await Expect(fileNameDiv).ToHaveTextAsync(fileName);
    }

    [Fact]
    public async Task ShopSettingsBrowserDataLoaderSet()
    {
        await SetServiceSettingsAsync("BrowserDataLoader");
    }
    [Fact]
    public async Task ShopSettingsBrowserDataLoaderUploadFromFile()
    {
        await ExpectLoadIndexPageAsync();

        var settingsForm = Page.Locator("#settingsForm");

        var fileName = "browserloader.firefox.json";
        var filePath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Content/{fileName}";

        var flexDiv = settingsForm.Locator("div[class='flex control-group']");
        Assert.True((await flexDiv.CountAsync()) >= 1);

        var uploadFileDiv = await UploadFileAsync(flexDiv, "browserDataLoaderJson", filePath);

        var fileNameDiv = uploadFileDiv.Locator("div");
        await Expect(fileNameDiv).ToHaveTextAsync(fileName);
    }


    [Fact]
    public async Task ShopSettingsWebLoaderSet()
    {
        await SetServiceSettingsAsync("WebLoader");
    }

    [Fact]
    public async Task ShopSettingsWebLoaderUploadFromFile()
    {
        await ExpectLoadIndexPageAsync();

        var settingsForm = Page.Locator("#settingsForm");

        var fileName = "webloader.firefox.json";
        var filePath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Content/{fileName}";

        var flexDiv = settingsForm.Locator("div[class='form-group']").Locator("div[class='flex  control-group']");
        await Expect(flexDiv).Not.ToBeEmptyAsync();

        var uploadFileDiv = await UploadFileAsync(flexDiv, "webLoaderJson", filePath);

        var fileNameDiv = uploadFileDiv.Locator("div");
        await Expect(fileNameDiv).ToHaveTextAsync(fileName);
    }

    private async Task<ILocator> UploadFileAsync(ILocator locator, string uploadId, string filePath)
    {       
        var inputFile = Page.Locator($"#{uploadId}");
        await Expect(inputFile).Not.ToBeVisibleAsync();
        await Expect(inputFile).ToHaveCountAsync(1);

        var uploadFileDiv = locator.Locator("div", new LocatorLocatorOptions {  Has = inputFile });
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
        await ExpectLoadIndexPageAsync();

        var settingsForm = Page.Locator("#settingsForm");

        var fileName = "Ozon.Headers.Firefox.json";
        var filePath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Content/{fileName}";        

        var flexDiv = settingsForm.Locator("div[class='form-group']");//.Locator("div[class='flex control-group']");
        var uploadFileDiv = await UploadFileAsync(flexDiv, "requestHeadersJson", filePath);

        var fileNameDiv = uploadFileDiv.Locator("div");
        await Expect(fileNameDiv).ToHaveTextAsync(fileName);
    }

    [Fact]
    public async Task ShopSettingsSave()
    {

    }
}
