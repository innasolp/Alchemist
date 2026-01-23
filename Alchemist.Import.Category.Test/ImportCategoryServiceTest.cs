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
    public async Task StoppedWhenWebLoaderNotExecutedAsync()
    {
        LoaderMock.Reset();
        await ImportWasStoppedWhenLoaderNotExecutedAsync();
    }

    [Fact]
    public async Task StartedWhenWebLoaderExecutedSuccessfullAsync()
    {
        LoaderMock.Reset();
        await ImportStartedWhenLoaderExecutedSuccessfullAsync();
    }

    [Fact]
    public async Task StoppedWhenCancellationRequestedAsync()
    {
        LoaderMock.Reset();

        var category = Helper.CreateCategoryWithChildren();
        var requestData = new object();
        LoaderMock.SetupLoadItem(_categoryShopModelMock.Object.CategorySourceUrl, requestData, category);

        await ImportStoppedWhenCancellationRequestedAsync();
    }

    [Fact]
    public async Task LogResetingWarningIfLoaderServiceNeedReseting()
    {
        LoaderMock.Reset();

        await LogResetingWarningIfLoaderServiceNeedResetingAsync();
    }

    [Fact]
    public async Task LogServiceFailedErrorWhenUnhandledExceptionThrown()
    {
        LoaderMock.Reset();

        await LogServiceFailedErrorWhenUnhandledExceptionThrownAsync();
    }

    [Fact]
    public async Task LogRequestFailedAndLoaderWillBePausedWarningWhenForbiddenRequest()
    {
        LoaderMock.Reset();

        await LogRequestFailedAndLoaderWillBePausedWarningWhenForbiddenRequestAsync();
    }
}