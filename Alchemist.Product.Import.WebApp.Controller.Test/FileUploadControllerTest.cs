using Alchemist.Import.Settings.Interfaces;
using Alchemist.Import.Settings.Extensions;
using Alchemist.Product.Import.Model;
using Alchemist.Product.Import.WebApp.Controllers;
using Alchemist.Product.Import.WebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alchemist.Product.Interfaces;

namespace Alchemist.Product.Import.WebApp.Controller.Test;

public class FileUploadControllerTest : ControllerTest<FileUploadController>
{
    private readonly ModelJsonConverter<IServiceSettingsModel> _serviceSettingsModelConverter;
    public FileUploadControllerTest()
    {
        var defaultServiceProperties = new Dictionary<string, object>()
        {
            { nameof(IServiceSettingsModel.Id), 0 },
            { nameof(IServiceSettingsModel.ParentSettingsId), 0 },
            { nameof(IServiceSettingsModel.ShopId), 0 },
            { nameof(IServiceSettingsModel.ShopGuid), Guid.Empty },
            { nameof(IServiceSettingsModel.ShopSettingsGuid), Guid.Empty },
        };
        _serviceSettingsModelConverter = new ModelJsonConverter<IServiceSettingsModel>(defaultServiceProperties);

        var defaultShopSettingsProperties = new Dictionary<string, object>()
        {
            { nameof(IShopImportSettingsModel.Id), 0 },
            { nameof(IShopImportSettingsModel.ShopId), 0 },
            { nameof(IShopImportSettingsModel.ShopGuid), Guid.Empty }
        };        
    }

    private FileUploadController CreateFileUploadController()
    {
        return new FileUploadController(_importFacade, ModelFactory);
    }

    private static async Task<T> ReadFromFileAsync<T>(string fileName, JsonSerializerOptions options )
        where T : class
    {
        var filePath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Content/{fileName}";
        using FileStream s = File.OpenRead(filePath);
        T result = await JsonSerializer.DeserializeAsync<T>(s, options);
        s.Close();
        return await Task.FromResult(result);
    }


    [Fact]
    public async Task UploadShopSettingsBadRequestWhenFileIsNull()
    {
        var fileUploadController = CreateFileUploadController();
        var actionResult = Assert.IsType<BadRequestObjectResult>(await fileUploadController.UploadShopSettingsAsync(Guid.Empty, (int)ShopSettingType.Product, null));
        Assert.Equal("file", Assert.IsType<string>(actionResult.Value));
    }
    

    [Fact]
    public async Task UploadServiceSettingsBadRequestWhenFileIsNull()
    {
        var fileUploadController = CreateFileUploadController();
        var actionResult = Assert.IsType<BadRequestObjectResult>(await fileUploadController.UploadServiceSettingsAsync(Guid.Empty, Guid.Empty, "", null));
        Assert.Equal("file", Assert.IsType<string>(actionResult.Value));
    }

    [Fact]
    public async Task UploadServiceSettingsBadRequestWhenShopGuidEmpty()
    {
        var fileName = "importservice.json";
        var formFile = GetFormFile(fileName);
        var fileUploadController = CreateFileUploadController();
        var actionResult = Assert.IsType<BadRequestObjectResult>(await fileUploadController.UploadServiceSettingsAsync(Guid.Empty, Guid.Empty, "", formFile));
        Assert.Equal("shopGuid", Assert.IsType<string>(actionResult.Value));
    }

    [Fact]
    public async Task UploadShopSettingsBadRequestWhenShopGuidEmpty()
    {
        var fileName = "importservice.json";
        var formFile = GetFormFile(fileName);
        var fileUploadController = CreateFileUploadController();
        var actionResult = Assert.IsType<BadRequestObjectResult>(await fileUploadController.UploadShopSettingsAsync(Guid.Empty, (int)ShopSettingType.Product, formFile));
        Assert.Equal("shopGuid", Assert.IsType<string>(actionResult.Value));
    }

    [Fact]
    public async Task UploadServiceSettingsBadRequestWhenShopSettingsGuidEmpty()
    {
        var fileName = "importservice.json";
        var formFile = GetFormFile(fileName);
        var fileUploadController = CreateFileUploadController();
        var actionResult = Assert.IsType<BadRequestObjectResult>(await fileUploadController.UploadServiceSettingsAsync(Guid.NewGuid(), Guid.Empty, "", formFile));
        Assert.Equal("shopSettingsGuid", Assert.IsType<string>(actionResult.Value));
    }

    [Fact]
    public async Task UploadServiceSettingsBadRequestWhenServiceSettingsEmpty()
    {
        var fileName = "importservice.json";
        var formFile = GetFormFile(fileName);
        var fileUploadController = CreateFileUploadController();
        var actionResult = Assert.IsType<BadRequestObjectResult>(await fileUploadController.UploadServiceSettingsAsync(Guid.NewGuid(), Guid.NewGuid(), "", formFile));
        Assert.Equal("serviceSettingsName", Assert.IsType<string>(actionResult.Value));
    }

    [Fact]
    public async Task UploadServiceSettingsNotFoundWhenShopNotExists()
    {
        var fileName = "importservice.json";
        var formFile = GetFormFile(fileName);
        var fileUploadController = CreateFileUploadController();
        var shopGuid = Guid.NewGuid();
        var actionResult = Assert.IsType<NotFoundObjectResult>(await fileUploadController.UploadServiceSettingsAsync(shopGuid, Guid.NewGuid(), "importservice", formFile));
        Assert.Equal(shopGuid, Assert.IsType<Guid>(actionResult.Value));
    }

    [Fact]
    public async Task UploadShopSettingsNotFoundWhenShopNotExists()
    {
        var fileName = "importservice.json";
        var formFile = GetFormFile(fileName);
        var shopGuid = Guid.NewGuid();
        var fileUploadController = CreateFileUploadController();
        var actionResult = Assert.IsType<NotFoundObjectResult>(await fileUploadController.UploadShopSettingsAsync(shopGuid, (int)ShopSettingType.Product, formFile));
        Assert.Equal(shopGuid, Assert.IsType<Guid>(actionResult.Value));
    }


    private static IFormFile GetFormFile(string fileName, string? path = null)
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
        await LoadShopsAsync();

        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        Assert.True(_importFacade.TryGetShopImport(shopGuid, out var shopImport));

        Assert.True(_importFacade.TryGetShopSettings(shopGuid, ShopSettingType.Product, out var shopSettings));

        FillShopSettingsFields(shopSettings);

        await AssertUploadShopSettingsActionAsync(shopSettings as ProductShopSettingsModel,
            "OzonProductSettings.json");
    }

    [Fact]
    public async Task UploadCategoryShopSettingsActionAsync()
    {
        await LoadShopsAsync();

        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        Assert.True(_importFacade.TryGetShopImport(shopGuid, out var shopImport));

        Assert.True(_importFacade.TryGetShopSettings(shopGuid, ShopSettingType.Category, out var categorySettings));

        FillShopSettingsFields(categorySettings);

        await AssertUploadShopSettingsActionAsync(categorySettings as CategoryShopSettingsModel, "ozoncategories.json");
    }    

    private async Task AssertUploadShopSettingsActionAsync<T>(T shopSettings, string fileName)
        where T: class, IShopImportSettingsModel
    {
        var prevShopSettings = ModelFactory.GetCopy(shopSettings);

        var option = new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        var fileShopSettings = await ReadFromFileAsync<T>(fileName, option);

        var fileUploadController = CreateFileUploadController();
        var formFile = GetFormFile(fileName);

        var uploadedShopSettingsResult = Assert.IsType<OkObjectResult>(await fileUploadController.UploadShopSettingsAsync(shopSettings.ShopGuid, (int)shopSettings.Type, formFile));
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
            (shopSettings) => shopSettings.GetImportService() as  ServiceSettingsModel,
            "importservice.json");
    }

    [Fact]
    public async Task UploadBrowserDataLoaderOnUploadServiceSettingsActionAsync()
    {
        await UploadServiceOnUploadServiceSettingsActionAsync(nameof(ShopSettingsModel.BrowserDataLoader),
            (shopSettings) => shopSettings.GetBrowserDataLoader() as IServiceSettingsModel,
            "browserloader.firefox.json");
    }

    [Fact]
    public async Task UploadBrowserLauncherOnUploadServiceSettingsActionAsync()
    {
        await UploadServiceOnUploadServiceSettingsActionAsync(nameof(ShopSettingsModel.BrowserLauncher),
            (shopSettings) => shopSettings.GetBrowserLauncher() as ServiceSettingsModel,
            "browserlauncher.firefox.json");
    }

    [Fact]
    public async Task UploadWebLoaderOnUploadServiceSettingsActionAsync()
    {
        await UploadServiceOnUploadServiceSettingsActionAsync(nameof(ShopSettingsModel.WebLoader),
            (shopSettings) => shopSettings.GetWebLoader() as ServiceSettingsModel,
            "webloader.firefox.json");
    }

    [Fact]
    public async Task UploadRequestHeadersOnUploadServiceSettingsActionAsync()
    {
        await UploadServiceOnUploadServiceSettingsActionAsync(nameof(ShopSettingsModel.RequestHeaders),
            (shopSettings) => shopSettings.GetRequestHeaders() as ServiceSettingsModel,
            "Ozon.Headers.Firefox.json");
    }

    private async Task UploadServiceOnUploadServiceSettingsActionAsync(string serviceName,
        Func<IShopImportSettingsModel, IServiceSettingsModel> getSetvice,
        string fileName)
    {
        await LoadShopsAsync();        

        var shopGuid = _importFacade.GetShops().Last().ShopGuid;
        Assert.True(_importFacade.TryGetShopImport(shopGuid, out var shopImport));

        Assert.True(_importFacade.TryGetShopSettings(shopGuid, ShopSettingType.Product, out var shopSettings));

        FillShopSettingsFields(shopSettings);

        Assert.True(_importFacade.TryGetServiceSettings(shopGuid, shopSettings.Guid, serviceName, out var prevService));
        var copy = ModelFactory.GetCopy(prevService);

        var fileService = await fileName.ReadFromFileAsync<ServiceSettingsModel>([_serviceSettingsModelConverter]);

        var formFile = GetFormFile(fileName);

        var fileUploadController = CreateFileUploadController();

        var uploadedServiceResult = Assert.IsType<OkObjectResult>(await fileUploadController.UploadServiceSettingsAsync(shopSettings.ShopGuid, shopSettings.Guid, serviceName, formFile));
        var uploadedService = Assert.IsType<ServiceSettingsModel>(uploadedServiceResult.Value);
        Assert.NotNull(uploadedService);
        Assert.Equal(fileName, uploadedService.FileName);

        var service = getSetvice(shopSettings);

        ModelAssert.EqualFields(fileService, service);
        ModelAssert.NotEqualFields(copy, service);
    }
}