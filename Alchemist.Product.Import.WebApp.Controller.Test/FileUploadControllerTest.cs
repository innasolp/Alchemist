using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.WebApp.Controllers;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Reflection;
using Alchemist.Product.Import.Model.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Alchemist.Product.Import.WebApp.Controller.Test;

public class FileUploadControllerTest : ControllerTest<FileUploadController>
{
    private FileUploadController CreateFileUploadController()
    {
        return new FileUploadController(_importFacade);
    }

    private IFormFile GetFormFile(string fileName, string? path = null)
    {
        var fileMock = new Mock<IFormFile>();

        var filePath = $"{path ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Content/{fileName}";

        fileMock.Setup(ff => ff.OpenReadStream()).Returns(() => File.OpenRead(filePath));

        fileMock.Setup(ff => ff.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())).
            Returns<Stream, CancellationToken>((stream, token) => CopyStreamFromFileToAsync(filePath, stream));

        fileMock.Setup(ff => ff.FileName).Returns(fileName);
        fileMock.Setup(ff => ff.Length).Returns(1000);

        return fileMock.Object;
    }

    private static async Task CopyStreamFromFileToAsync(string filePath, Stream stream)
    {
        var bytes = await File.ReadAllBytesAsync(filePath);
        var memoryStream = new MemoryStream(bytes);
        await memoryStream.CopyToAsync(stream);
    }

    [Fact]
    public async Task UploadProductShopSettingsActionAsync()
    {
        var indexViewModel = await GetIndexActionViewModelAfterUpdateShopsAsync();

        var shopSettings = Assert.IsType<ProductShopSettingsModel>(indexViewModel.SelectedShopImport.GetSettings(indexViewModel.SelectedTab, true));

        await UploadShopSettingsActionAsync(shopSettings, "OzonProductSettings.json");
    }
    [Fact]
    public async Task UploadCategoryShopSettingsActionAsync()
    {
        var shopSettingsController = new ShopSettingsController(null, _importFacade, _settingsDataAdapterMock.Object);

        var categorySettings = await CommonActions.ChangeShopSettingsAsync<CategoryShopSettingsModel>(CreateHomeController(), shopSettingsController, Interfaces.ShopSettingType.Category);

        await UploadShopSettingsActionAsync(categorySettings, "ozoncategories.json");
    }

    private async Task UploadShopSettingsActionAsync<T>(T shopSettings, string fileName)
        where T:ShopSettingsModel, new()
    {
        var prevShopSettings = shopSettings.GetCopy();
        
        var fileShopSettings = await fileName.ReadFromFileAsync<T>();

        var fileUploadController = CreateFileUploadController();
        var formFile = GetFormFile(fileName);

        var uploadedShopSettingsResult = Assert.IsType<OkObjectResult>(await fileUploadController.UploadShopSettings(shopSettings.ShopGuid, (int)shopSettings.ShopSettingType, formFile));
        var uploadedShopSettings = Assert.IsType<T>(uploadedShopSettingsResult.Value);
        Assert.Equal(fileName, uploadedShopSettings.FileName);

        ModelAssert.EqualFields(fileShopSettings, uploadedShopSettings);
        ModelAssert.NotEqualFields(prevShopSettings, uploadedShopSettings);

        ModelAssert.EqualServices(fileShopSettings, uploadedShopSettings);
        ModelAssert.NotEqualServices(prevShopSettings, uploadedShopSettings);
    }

    [Fact]
    public async Task UploadImportServiceOnUploadServiceSettingsActionAsync()
    {
        await UploadServiceOnUploadServiceSettingsActionAsync(nameof(ShopSettingsModel.ImportService),
            (shopSettings) => shopSettings.ImportService,
            "importservice.json");
    }

    [Fact]
    public async Task UploadBrowserDataLoaderOnUploadServiceSettingsActionAsync()
    {
        await UploadServiceOnUploadServiceSettingsActionAsync(nameof(ShopSettingsModel.BrowserDataLoader),
            (shopSettings) => shopSettings.BrowserDataLoader,
            "browserloader.firefox.json");
    }

    [Fact]
    public async Task UploadWebLoaderOnUploadServiceSettingsActionAsync()
    {
        await UploadServiceOnUploadServiceSettingsActionAsync(nameof(ShopSettingsModel.WebLoader),
            (shopSettings) => shopSettings.WebLoader,
            "webloader.firefox.json");
    }

    [Fact]
    public async Task UploadRequestHeadersOnUploadServiceSettingsActionAsync()
    {
        await UploadServiceOnUploadServiceSettingsActionAsync(nameof(ShopSettingsModel.RequestHeaders),
            (shopSettings) => shopSettings.RequestHeaders,
            "Ozon.Headers.Firefox.json");
    }

    private async Task UploadServiceOnUploadServiceSettingsActionAsync(string serviceName, Func<ShopSettingsModel, ServiceSettingsModel> getSetvice, string fileName)
    {
        var indexViewModel = await GetIndexActionViewModelAfterUpdateShopsAsync();

        var shopSettings = indexViewModel.SelectedShopImport.GetSettings(indexViewModel.SelectedTab, true) as ShopSettingsModel;

        var prevService = shopSettings.CreateServiceSettingsModel(serviceName);
        prevService.Update(getSetvice(shopSettings));

        var fileService = await fileName.ReadFromFileAsync<ServiceSettingsModel>();

        var fileUploadController = CreateFileUploadController();
        var formFile = GetFormFile(fileName);

        var uploadedServiceResult = Assert.IsType<OkObjectResult>(await fileUploadController.UploadServiceSettings(shopSettings.ShopGuid, shopSettings.Guid, serviceName, formFile));
        var uploadedService = Assert.IsType<ServiceSettingsModel>(uploadedServiceResult.Value);
        Assert.NotNull(uploadedService);
        Assert.Equal(fileName, uploadedService.FileName);

        ModelAssert.EqualFields(fileService, getSetvice(shopSettings));
        ModelAssert.NotEqualFields(prevService, getSetvice(shopSettings));
    }
}