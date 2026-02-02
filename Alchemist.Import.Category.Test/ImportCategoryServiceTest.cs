using Alchemist.Import.Category.Interfaces;
using Alchemist.Import.Category.Service;
using Alchemist.Import.CategoryService.Test.Infrastructure;
using Import.Service.Test;
using Import.Service.Test.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;
using Moq;
using ShopImport.Category.Loader.Interfaces;
using Xunit.Abstractions;

namespace Alchemist.Import.CategoryService.Test;

public class ImportCategoryServiceTest : ImportServiceExecutionTest<ShopImportCategoriesTimerServiceTest, ILogger<ShopImportCategoriesTimerService>>
{
    private readonly Mock<ICategoryShopModel> _categoryShopModelMock = new();

    private readonly CategoryImportOptions _importOptions = new() { SecondsInterval = 1 };

    private readonly Mock<ICategoryLoader>[] _categoryLoadersMock = [new Mock<ICategoryLoader>()];

    private readonly Mock<ICategoryLoader> _categoryLoaderMock = new();

    private readonly Mock<ICategoryItemHandler> _categoryItemHandlerMock = new();


    public ImportCategoryServiceTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _categoryShopModelMock.Setup(s => s.CategorySourceUrl).Returns(Guid.NewGuid().ToString());
    }

    protected override ShopImportCategoriesTimerServiceTest CreateService(string name)
    {
        return new ShopImportCategoriesTimerServiceTest(LoggerMock.Object,
             name,
             LoaderMock.Object,
             _categoryShopModelMock.Object,
             _categoryLoadersMock.Select(m=>m.Object),
             _importOptions,
             _categoryItemHandlerMock.Object
             );
    }

    [Fact]
    public async Task ShouldLogImportWasStoppedWhenLoaderNotExecuted()
    {
        LoaderMock.Reset();
        await ShouldLogImportWasStoppedWithErrorWhenLoaderNotExecutedAsync(2000);
    }

    [Fact]
    public async Task ShouldLogServiceStartedWhenLoaderExecutesSuccessfully()
    {
        LoaderMock.Reset();
        
        var requestData = new { id = 2 };
        var category = Helper.CreateCategoryWithChildren();
        LoaderMock.SetupLoadItem(_categoryShopModelMock.Object.CategorySourceUrl, requestData, category);

        var categoryStream = await TestExtensions.LoadItemAsync(category);

        LoaderMock.SetupGetRequestData(requestData);
        LoaderMock.Setup(w =>
           w.Load(_categoryShopModelMock.Object.CategorySourceUrl,
           It.Is<object?>(data => data == requestData),
           It.IsAny<CancellationToken>()))
           .ReturnsAsync(categoryStream);

        _categoryLoaderMock.Setup(s => s.LoadAsync(null, It.Is<Stream>(s => s == categoryStream), It.IsAny<CancellationToken>())).
            ReturnsAsync([category]);

        await ShouldLogServiceStartedWhenLoaderExecutesSuccessfullyAsync(1000);
    }

    [Fact]
    public async Task ShouldLogImportStoppedWhenCancellationRequested()
    {
        LoaderMock.Reset();
        
        var requestData = new { id = 2 };
        var category = Helper.CreateCategoryWithChildren();
        LoaderMock.SetupLoadItem(_categoryShopModelMock.Object.CategorySourceUrl, requestData, category);

        var categoryStream = await TestExtensions.LoadItemAsync(category);

        LoaderMock.SetupGetRequestData(requestData);       

        _categoryLoaderMock.Setup(s => s.LoadAsync(null, It.Is<Stream>(s => s == categoryStream), It.IsAny<CancellationToken>())).
            ReturnsAsync([category]);

        await ShouldLogImportStoppedWhenCancellationRequestedAsync(500);
    }

    [Fact]
    public async Task ShouldLogResettingErrorIfLoaderServiceNeedsReseting()
    {
        LoaderMock.Reset();

        await ShouldLogResettingErrorIfLoaderServiceNeedsResettingAsync(15000);
    }

    [Fact]
    public async Task ShouldLogServiceFailedErrorWhenUnhandledExceptionThrown()
    {
        LoaderMock.Reset();

        await ShouldLogServiceFailedErrorWhenUnhandledExceptionThrownAsync(1000);
    }

    [Fact]
    public async Task ShouldLogRequestFailedAndLoaderWillBePausedWarningWhenLoaderNeedsWait()
    {
        LoaderMock.Reset();

        await ShouldLogRequestFailedAndLoaderWillBePausedWarningWhenLoaderNeedsWaitAsync(1000);
    }
}