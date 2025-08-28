using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Category.Json;
using Alchemist.Import.Category.Test.Infrastructure;
using Alchemist.Test.Import.Service;
using Alchemist.Test.Import.Service.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using System.Resources;
using Xunit.Abstractions;

namespace Alchemist.Import.Category.Test;

public class ImportCategoryJsonLoadingTest : ImportServiceTest<ShopImportCategoriesTimerServiceTest, ILogger<ShopImportCategoriesTimerService>>
{
    protected override ShopImportCategoriesTimerServiceTest Service { get; }
    
    private readonly Mock<ICategoryShopModel> _categoryShopModelMock = new();

    private readonly CategoryLoadOptions _loadOptions = new() { SecondsInterval = 30 };

    private readonly Mock<ICategoryItemHandler> _categoryItemHandlerMock = new();

    protected ResourceManager ImportCategoriesResourceManager { get; }

    public ImportCategoryJsonLoadingTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        ImportCategoriesResourceManager = new ResourceManager("Alchemist.Import.Category.Json.ImportCategoryLogMessages",
                               typeof(ShopImportCategoriesTimerService).Assembly);

        LoaderMock.SetupLoadCookies();
        _categoryShopModelMock.Setup(s => s.CategorySourceUrl).Returns(Guid.NewGuid().ToString());
        _categoryShopModelMock.Setup(s => s.ShopName).Returns(Guid.NewGuid().ToString());

        Service = new ShopImportCategoriesTimerServiceTest(LoggerMock.Object,
            null,
            LoaderMock.Object,
            _categoryShopModelMock.Object,            
            _loadOptions,
            _categoryItemHandlerMock.Object
            );
    }

    private async Task ExecuteServiceAsync(string name, int executionDuration, int completeDuration)
    {
        Service.SetName(name);

        var token = new CancellationTokenSource();
        var task = Service.Start(token.Token);

        await Task.Delay(executionDuration);

        await token.CancelAsync();

        await Task.Delay(completeDuration);

        await task.WaitAsync(token.Token);
    }

    [Fact]
    public async Task LogInfoSuccessWhenCategoriesLoaded()
    {
        LoaderMock.Reset();
        LoaderMock.SetupStartSuccess();

        var category = Helper.CreateCategoryWithChildren();
        _loadOptions.CategoryPropertyPaths = new Dictionary<string, PropertyPath>() { { "Url", new PropertyPath("Url", "Url") },
            { "Description",new PropertyPath("Description", "Description") },
            { "Children",new PropertyPath("Children", "Children") },
            { "Name",new PropertyPath("Name", "Name") },
            { "Id",new PropertyPath("Id", "Id") } };

        var requestData = new object();
        LoaderMock.SetupGetRequestData(requestData);
        LoaderMock.SetupLoadItem(_categoryShopModelMock.Object.CategorySourceUrl, requestData, category);        
        
        var name = Guid.NewGuid().ToString();

        await ExecuteServiceAsync(name, 5000, 1000);       

        LoggerMock.VerifyInfo(ImportCategoriesResourceManager.GetString("CategoryNameIdForShopWasLoaded"),
            category.Name, category.Id, _categoryShopModelMock.Object.ShopName); 
    }

    [Fact]
    public async Task LogErrorWhenJsonLoadFromCategorySourceUrlFailed()
    {
        LoaderMock.Reset();
        LoaderMock.SetupStartSuccess();       

        var exception = new Exception("Json loading failed");
        var requestData = new object();
        LoaderMock.SetupGetRequestData(requestData);
        LoaderMock.Setup(w=>w.Load(_categoryShopModelMock.Object.CategorySourceUrl, requestData)).Throws(exception);

        var name = Guid.NewGuid().ToString();

        await ExecuteServiceAsync(name, 1000, 1000);

        LoggerMock.VerifyError(exception, ServiceResourceManager.GetString("ProcessUrlFailedError"),
            _categoryShopModelMock.Object.CategorySourceUrl);

        LoggerMock.VerifyError(ImportCategoriesResourceManager.GetString("JsonLoadFromUrlFailed"),
            _categoryShopModelMock.Object.CategorySourceUrl, exception.Message);
    }
}
