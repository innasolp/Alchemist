using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.ProductService.Test.Infrastructure;
using Import.Service.Test;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace Alchemist.Import.ProductService.Test;

public class ImportShopProductServiceTest : ImportServiceExecutionTest<TestImportProductService<TestCategory, TestProductItem>, ILogger>
{
    public ImportShopProductServiceTest(ITestOutputHelper outputHelper):base(outputHelper)
    {
        ProductShopModelMock.Setup(s => s.Categories).Returns([]);
    }

    protected override TestImportProductService<TestCategory, TestProductItem> CreateService(string name)
    {
        return new TestImportProductService<TestCategory, TestProductItem>(
             LoggerMock.Object,
             name,
             ProductShopModelMock.Object,
             LoaderMock.Object,
             ProductItemHandlerMock.Object);
    }

    protected Mock<IProductShopModel> ProductShopModelMock { get; } = new Mock<IProductShopModel>();

    protected Mock<IProductItemHandler> ProductItemHandlerMock { get; } = new Mock<IProductItemHandler>();

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
        await ShouldLogImportStoppedWhenCancellationRequestedAsync();
    }

    [Fact]
    public async Task ShouldLogResettingErrorIfLoaderServiceNeedsReseting()
    {
        LoaderMock.Reset();

        ProductShopModelMock.Setup(s => s.CategoryUrlFormat).Returns("Category_{0}_page{1}");        
        var categoryMock = TestHelper.CreateCategoryMock();        
        ProductShopModelMock.Object.Categories.Add(categoryMock.Object);
        
        await ShouldLogResettingErrorIfLoaderServiceNeedsResettingAsync();
    }

    [Fact]
    public async Task ShouldLogServiceFailedErrorWhenUnhandledExceptionThrown()
    {
        LoaderMock.Reset();

        ProductShopModelMock.Setup(s => s.CategoryUrlFormat).Returns("Category_{0}_page{1}");
        var categoryMock = TestHelper.CreateCategoryMock();
        ProductShopModelMock.Object.Categories.Add(categoryMock.Object);

        await ShouldLogServiceFailedErrorWhenUnhandledExceptionThrownAsync();
    }

    [Fact]
    public async Task ShouldLogRequestFailedAndLoaderWillBePausedWarningWhenLoaderNeedsWait()
    {
        LoaderMock.Reset();

        ProductShopModelMock.Setup(s => s.CategoryUrlFormat).Returns("Category_{0}_page{1}");
        var categoryMock = TestHelper.CreateCategoryMock();
        ProductShopModelMock.Object.Categories.Add(categoryMock.Object);

        await ShouldLogRequestFailedAndLoaderWillBePausedWarningWhenLoaderNeedsWaitAsync();
    }
}