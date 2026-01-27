using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Category.Service;
using Alchemist.Import.CategoryService.Test.Infrastructure;
using Import.Interfaces;
using Import.Service.Test;
using Import.Service.Test.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using Moq;
using ShopImport.Category.Loader.Interfaces;
using System.Resources;
using Xunit.Abstractions;

namespace Alchemist.Import.CategoryService.Test;

public class ImportCategoryJsonLoadingTest : ImportServiceTest<ShopImportCategoriesTimerServiceTest, ILogger<ShopImportCategoriesTimerService>>
{
    private readonly Mock<ICategoryShopModel> _categoryShopModelMock = new();

    private readonly CategoryImportOptions _loadOptions = new() { SecondsInterval = 30 };

    private readonly Mock<ICategoryItemHandler> _categoryItemHandlerMock = new();

    private readonly Mock<ICategoryLoader> _categoryLoaderMock = new();

    private readonly Mock<ICategoryLoader>[] _categoryLoadersMock;

    protected ResourceManager ImportCategoriesResourceManager { get; }

    public ImportCategoryJsonLoadingTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _categoryLoadersMock =  [_categoryLoaderMock];

        ImportCategoriesResourceManager = new ResourceManager("Alchemist.Import.Category.Service.ImportCategoryLogMessages",
                               typeof(ShopImportCategoriesTimerService).Assembly);

        LoaderMock.SetupLoadCookies();
        _categoryShopModelMock.Setup(s => s.CategorySourceUrl).Returns(Guid.NewGuid().ToString());
        _categoryShopModelMock.Setup(s => s.Name).Returns(Guid.NewGuid().ToString());
        _categoryShopModelMock.Setup(s => s.SourceName).Returns(Guid.NewGuid().ToString());
        _categoryShopModelMock.Setup(s => s.SourceUrl).Returns(Guid.NewGuid().ToString());
    }

    protected override ShopImportCategoriesTimerServiceTest CreateService(string name)
    {
        return new ShopImportCategoriesTimerServiceTest(LoggerMock.Object,
            name,
            LoaderMock.Object,
            _categoryShopModelMock.Object,
            _categoryLoadersMock.Select(m=>m.Object),
            _loadOptions,
            _categoryItemHandlerMock.Object
            );
    }

    private async Task ExecuteServiceAsync(string name, int executionDuration)
    {
        var service = CreateService(name);

        var tokenSource = new CancellationTokenSource();
        tokenSource.CancelAfter(executionDuration);

        await service.Start(new object(), tokenSource.Token); 
    }

    [Fact]
    public async Task LogInfoSuccessWhenCategoriesLoaded()
    {
        LoaderMock.Reset();
        LoaderMock.SetupStartSuccess();

        var category = Helper.CreateCategoryWithChildren();        

        var categoryStream = await TestExtensions.LoadItemAsync(category);

        var requestData = new { id = 2 };
        var loadAutoResetEvent = new AsyncAutoResetEvent();
        LoaderMock.SetupGetRequestData(requestData);
        LoaderMock.Setup(w =>
           w.Load(_categoryShopModelMock.Object.CategorySourceUrl, 
           It.Is<object?>(data => data == requestData), 
           It.IsAny<CancellationToken>()))
           .Returns(async (string url, object? data, CancellationToken cancellationToken) =>
           {
               var stream = categoryStream;
               loadAutoResetEvent.Set();
               return stream;
           });        
        
        var name = Guid.NewGuid().ToString();

        _categoryLoaderMock.Setup(s => s.LoadAsync(null, It.Is<Stream>(s => s == categoryStream), It.IsAny<CancellationToken>())).
            ReturnsAsync([category]);

        await ExecuteServiceAsync(name, 500);

        LoggerMock.VerifyInfo(ImportCategoriesResourceManager.GetString("CategoryNameIdForShopWasLoaded"),
            category.Name, category.Id, _categoryShopModelMock.Object.SourceName); 
    }

    [Fact]
    public async Task LogErrorWhenJsonLoadFromCategorySourceUrlFailed()
    {
        LoaderMock.Reset();
        LoaderMock.SetupStartSuccess();       

        var exception = new LoaderServiceException("Json loading failed");
        var requestData = new {id = 10};
        LoaderMock.SetupGetRequestData(requestData);
        LoaderMock.Setup(w=>
            w.Load(_categoryShopModelMock.Object.CategorySourceUrl,
            It.Is<object?>(d => d == requestData),
            It.IsAny<CancellationToken>()))
            .Throws(exception);

        var name = Guid.NewGuid().ToString();

        await ExecuteServiceAsync(name, 500);

        LoggerMock.VerifyInfo(ImportCategoriesResourceManager.GetString("CategoriesWereNotLoaded"),
            _categoryShopModelMock.Object.SourceName, _categoryShopModelMock.Object.SourceUrl);
    }
}