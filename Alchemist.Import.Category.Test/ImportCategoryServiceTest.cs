using Alchemist.Import.Category.Interfaces;
using Import.Service.Test;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;
using Import.Service.Test.Infrastructure;
using Alchemist.Import.Category.Service;
using Alchemist.Import.CategoryService.Test.Infrastructure;
using ShopImport.Category.Loader.Interfaces;

namespace Alchemist.Import.CategoryService.Test;

public class ImportCategoryServiceTest : ImportServiceExecutionTest<ShopImportCategoriesTimerServiceTest, ILogger<ShopImportCategoriesTimerService>>
{
    private readonly Mock<ICategoryShopModel> _categoryShopModelMock = new();

    private readonly CategoryImportOptions _importOptions = new() { SecondsInterval = 5 };

    private readonly Mock<ICategoryLoader>[] _categoryLoadersMock = [new Mock<ICategoryLoader>()];    

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
        await ShouldLogImportWasStoppedWhenLoaderNotExecutedAsync();
    }

    [Fact]
    public async Task ShouldLogServiceStartedWhenLoaderExecutesSuccessfully()
    {
        LoaderMock.Reset();
        await ShouldLogServiceStartedWhenLoaderExecutesSuccessfullyAsync();
    }

    [Fact]
    public async Task ShouldLogImportStoppedWhenCancellationRequested()
    {
        LoaderMock.Reset();

        var category = Helper.CreateCategoryWithChildren();
        var requestData = new object();
        LoaderMock.SetupLoadItem(_categoryShopModelMock.Object.CategorySourceUrl, requestData, category);

        await ShouldLogImportStoppedWhenCancellationRequestedAsync();
    }

    [Fact]
    public async Task ShouldLogResettingErrorIfLoaderServiceNeedsReseting()
    {
        LoaderMock.Reset();

        await ShouldLogResettingErrorIfLoaderServiceNeedsResettingAsync();
    }

    [Fact]
    public async Task ShouldLogServiceFailedErrorWhenUnhandledExceptionThrown()
    {
        LoaderMock.Reset();

        await ShouldLogServiceFailedErrorWhenUnhandledExceptionThrownAsync();
    }

    [Fact]
    public async Task ShouldLogRequestFailedAndLoaderWillBePausedWarningWhenLoaderNeedsWait()
    {
        LoaderMock.Reset();

        await ShouldLogRequestFailedAndLoaderWillBePausedWarningWhenLoaderNeedsWaitAsync();
    }
}