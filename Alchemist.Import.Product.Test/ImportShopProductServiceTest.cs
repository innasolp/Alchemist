using Alchemist.Import.Product.Test.Infrastructure;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Test.Import.Service;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace Alchemist.Import.Product.Test;

public class ImportShopProductServiceTest : ImportServiceExecutionTest<TestImportProductService<TestCategory, TestProductItem>, ILogger>
{
    public ImportShopProductServiceTest(ITestOutputHelper outputHelper):base(outputHelper)
    {
        ProductShopModelMock.Setup(s => s.Categories).Returns(new System.Collections.ObjectModel.ObservableCollection<IProductShopCategory>());

        Service = new TestImportProductService<TestCategory, TestProductItem>(
             LoggerMock.Object,
             ProductShopModelMock.Object,
             LoaderMock.Object,
             ProductItemHandlerMock.Object);
    }

    protected override TestImportProductService<TestCategory, TestProductItem> Service { get; }

    protected Mock<IProductShopModel> ProductShopModelMock { get; } = new Mock<IProductShopModel>();

    protected Mock<IProductItemHandler> ProductItemHandlerMock { get; } = new Mock<IProductItemHandler>();

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
        await ImportStoppedWhenCancellationRequestedAsync();
    }

    [Fact]
    public async Task ImportFailedWhenLoaderAlwaysNeedReseting()
    {
        LoaderMock.Reset();

        ProductShopModelMock.Setup(s => s.CategoryUrl).Returns("Category_{0}_page{1}");

        var categoryMock = TestHelper.CreateCategoryMock();
        ProductShopModelMock.Object.Categories.Add(categoryMock.Object);

        await ImportFailedWhenLoaderAlwaysNeedResetingAsync();
    }
}