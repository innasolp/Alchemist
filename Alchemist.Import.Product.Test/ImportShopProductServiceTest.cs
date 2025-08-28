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
        await ImportWasStoppedWhenWebLoaderNotExecutedAsync();
    }

    [Fact]
    public async Task StartedWhenWebLoaderExecutedSuccessfullAsync()
    {
        LoaderMock.Reset();
        await ImportStartedWhenWebLoaderExecutedSuccessfullAsync();
    }

    [Fact]
    public async Task StoppedWhenCancellationRequestedAsync()
    {
        LoaderMock.Reset();
        await ImportStoppedWhenCancellationRequestedAsync();
    }
}