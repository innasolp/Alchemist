using Alchemist.Import.Category.Interfaces;
using Import.Service.Test;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;
using Import.Service.Test.Infrastructure;
using Alchemist.Import.Category.Service;
using Alchemist.Import.Category.Service.Json;
using Alchemist.Import.CategoryService.Test.Infrastructure;

namespace Alchemist.Import.CategoryService.Test;

public class ImportCategoryServiceTest : ImportServiceExecutionTest<ShopImportCategoriesTimerServiceTest, ILogger<ShopImportCategoriesJsonTimerService>>
{
    private readonly Mock<ICategoryShopModel> _categoryShopModelMock = new();

    private readonly CategoryLoadOptions _loadOptions = new() { SecondsInterval = 5 };

    private readonly Mock<ICategoryItemHandler> _categoryItemHandlerMock = new();

    public ImportCategoryServiceTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _categoryShopModelMock.Setup(s => s.CategorySourceUrl).Returns(Guid.NewGuid().ToString());
    }

    protected override ShopImportCategoriesTimerServiceTest CreateService(string name)
    {
        return new ShopImportCategoriesTimerServiceTest(LoggerMock.Object,
             name,
             null,
             LoaderMock.Object,
             _categoryShopModelMock.Object,
             _loadOptions,
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
        _loadOptions.CategoryPropertyPaths = new Dictionary<string, PropertyPath>() { { "Url", new PropertyPath("Url", "Url") },
            { "Description",new PropertyPath("Description", "Description") },
            { "Children",new PropertyPath("Children", "Children") },
            { "Id",new PropertyPath("Id", "Id") } };

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