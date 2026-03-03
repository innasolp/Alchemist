using Alchemist.Import.Products.Interfaces;
using Import.Service.Test;
using Import.Service.Test.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using ShopImport.KeyHash;
using ShopImport.Product.Service.Stateful.Test.Infrastructure;
using ShopImport.Product.Service.Test.Infrastructure;
using ShopImport.ServiceState;
using Xunit.Abstractions;

namespace ShopImport.Product.Service.Stateful.Test;

public class ImportShopProductServiceTest : ImportServiceExecutionTest<TestImportProductStatefulService<TestCategory, TestProductItem>, ILogger>
{
    private readonly Mock<IKeyHasher> _keyHasherMock = new();

    private readonly Mock<IServiceStateRepository> _serviceStarepositoryMock = new();

    public ImportShopProductServiceTest(ITestOutputHelper outputHelper):base(outputHelper)
    {
        ProductShopModelMock.Setup(s => s.Categories).Returns([]);
    }

    protected override TestImportProductStatefulService<TestCategory, TestProductItem> CreateService(string name)
    {
        return new TestImportProductStatefulService<TestCategory, TestProductItem>(
             LoggerMock.Object,
             name,
             ProductShopModelMock.Object,
             LoaderMock.Object,
             ProductItemHandlerMock.Object,
             _keyHasherMock.Object,
             _serviceStarepositoryMock.Object);
    }

    protected Mock<IProductShopModel> ProductShopModelMock { get; } = new Mock<IProductShopModel>();

    protected Mock<IProductItemHandler> ProductItemHandlerMock { get; } = new Mock<IProductItemHandler>();

    [Fact]
    public async Task ShouldLogImportWasStoppedWithErrorWhenLoaderNotExecuted()
    {
        LoaderMock.Reset();        
        await ShouldLogImportWasStoppedWithErrorWhenLoaderNotExecutedAsync(3000);
    }

    [Fact]
    public async Task ShouldLogServiceStartedWhenLoaderExecutesSuccessfully()
    {
        LoaderMock.Reset();
        await ShouldLogServiceStartedWhenLoaderExecutesSuccessfullyAsync(1000);
    }

    [Fact]
    public async Task ShouldLogImportStoppedWhenCancellationRequested()
    {
        LoaderMock.Reset();
        LoaderMock.SetupStartSuccess();
        LoaderMock.Setup(s => s.Name).Returns($"{Guid.NewGuid()}");

        var categoryMock = TestHelper.CreateProductShopCategoryMock();
        ProductShopModelMock.Object.Categories.Add(categoryMock.Object);

        ProductShopModelMock.Setup(s => s.CategoryUrlFormat).Returns($"{Guid.NewGuid()}_{{0}}_{{1}}");

        var requestData = new object();
        LoaderMock.SetupGetRequestData(requestData);

        LoaderMock.Setup(s => s.Load(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>())).
            Returns(async (string path, object? data, CancellationToken cancellationToken) =>
            {
                await Task.Delay(100, cancellationToken);
                return default;
            });

        await ShouldLogImportStoppedWhenCancellationRequestedAsync(500);
    }

    [Fact]
    public async Task ShouldLogResettingErrorIfLoaderServiceNeedsReseting()
    {
        LoaderMock.Reset();

        ProductShopModelMock.Setup(s => s.CategoryUrlFormat).Returns("Category_{0}_page{1}");        
        var categoryMock = TestHelper.CreateProductShopCategoryMock();        
        ProductShopModelMock.Object.Categories.Add(categoryMock.Object);
        
        await ShouldLogResettingErrorIfLoaderServiceNeedsResettingAsync(15000);
    }

    [Fact]
    public async Task ShouldLogServiceFailedErrorWhenUnhandledExceptionThrown()
    {
        LoaderMock.Reset();

        ProductShopModelMock.Setup(s => s.CategoryUrlFormat).Returns("Category_{0}_page{1}");
        var categoryMock = TestHelper.CreateProductShopCategoryMock();
        ProductShopModelMock.Object.Categories.Add(categoryMock.Object);

        await ShouldLogServiceFailedErrorWhenUnhandledExceptionThrownAsync(1000);
    }

    [Fact]
    public async Task ShouldLogRequestFailedAndLoaderWillBePausedWarningWhenLoaderNeedsWait()
    {
        LoaderMock.Reset();

        ProductShopModelMock.Setup(s => s.CategoryUrlFormat).Returns("Category_{0}_page{1}");
        var categoryMock = TestHelper.CreateProductShopCategoryMock();
        ProductShopModelMock.Object.Categories.Add(categoryMock.Object);

        await ShouldLogRequestFailedAndLoaderWillBePausedWarningWhenLoaderNeedsWaitAsync(1000);
    }
}