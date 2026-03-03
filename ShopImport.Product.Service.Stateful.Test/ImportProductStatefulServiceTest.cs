using Alchemist.Import.Products.Interfaces;
using Import.Service.Test.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using ShopImport.KeyHash;
using ShopImport.Product.Service.Stateful.Test.Infrastructure;
using ShopImport.Product.Service.Test.Infrastructure;
using SmartFormat;
using System.Resources;
using Xunit.Abstractions;

namespace ShopImport.Product.Service.Stateful.Test;

public class ImportProductStatefulServiceTest : ImportProductsTest<TestImportProductStatefulService<TestCategory, TestProductItem>>
{
    private readonly Mock<IKeyHasher> _keyHasherMock = new();

    private readonly TestServiceStateRepository _serviceStarepository = new TestServiceStateRepository();

    protected ResourceManager ImportProductsStatefulResourceManager { get; }

    public ImportProductStatefulServiceTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        SetupKeyHasher();

        ImportProductsStatefulResourceManager = new ResourceManager("ShopImport.Product.Service.Stateful.ImportProductStatefulLogMessages",
                               typeof(ShopImportCategoryProductsStatefulService<TestCategory, TestProductItem>).Assembly);
    }

    private void SetupKeyHasher()
    {
        _keyHasherMock.Setup(s => s.Hash(It.IsAny<It.IsAnyType>())).Returns((object value) =>
        {
            return JsonCanonicalizer.GetCanonicalJson(value);
        });
    }   

    private void SetupCategoriesLoadingWithDelayOnRandomIteration(out int categoryPauseIteration)
    {
        var productCategories = new List<IProductShopCategory>
        {
            TestHelper.CreateProductShopCategoryMock().Object,
            TestHelper.CreateProductShopCategoryMock().Object,
            TestHelper.CreateProductShopCategoryMock().Object,
            TestHelper.CreateProductShopCategoryMock().Object
        };

        ProductShopModelMock.Setup(s => s.Categories).Returns(productCategories);
        ProductShopModelMock.Setup(s => s.CategoryUrlFormat).Returns($"{Guid.NewGuid().ToString()}_{{0}}_{{1}}");
        ProductShopModelMock.Setup(s => s.ProductUrl).Returns(Guid.NewGuid().ToString());

        LoaderMock.Reset();
        LoaderMock.SetupStartSuccess();
        LoaderMock.Setup(s => s.Name).Returns($"{Guid.NewGuid()}");

        var requestData = new object();
        LoaderMock.SetupGetRequestData(requestData);

        categoryPauseIteration = new Random().Next(1, productCategories.Count - 2);

        bool delayed = false;

        async Task delayOnCategoryLoadAsync(CancellationToken token)
        {
            if (delayed)
            {
                _outputHelper.WriteLine("Category loading without delay on next attempt.");
                return;
            }

            try
            {
                await Task.Delay(1000, token);
                _outputHelper.WriteLine("Category loading not canceled on first attempt");
            }
            catch (OperationCanceledException)
            {
                _outputHelper.WriteLine("Category loading was canceled on first attempt correctly.");
                delayed = true;
                throw;
            }
        }

        for (int i = 0; i < productCategories.Count; i++)
        {
            IProductShopCategory? productCategory = productCategories[i];
            var categoryUrl = ProductShopModelMock.Object.GetCategoryPageUrl(productCategory, 1);
            var productCount = new Random().Next(10, 20);
            var categoryProducts = TestHelper.CreateCategoryWithProducts(productCount, productCount, 1);
            
            Func<CancellationToken, Task>? onLoad = (i == categoryPauseIteration) ? delayOnCategoryLoadAsync : null;

            LoaderMock.SetupLoadItem(categoryUrl, requestData, categoryProducts, onLoad);
        }
    }

    private string GetCategoryPageMesageFromat()
    {
        var categoryPageResourceKey = "CategoryPageLoadedSuccessfully";
        return ImportProductsResourceManager.GetString(categoryPageResourceKey)
            ?? throw new InvalidOperationException($"message format {categoryPageResourceKey} is null or not found");
    }

    private void VerifyNotContainsCategoryPageMessage(IProductShopCategory productShopCategory)
    {
        var categoryPageMessageFormat = GetCategoryPageMesageFromat();

        var formatParameters = new
        {
            Category = productShopCategory.Path,
            Page = 1
        };
        var message = Smart.Format(categoryPageMessageFormat, formatParameters);

        LoggerMock.Verify(l => l.Log(
           It.Is<LogLevel>(v => v == LogLevel.Information),
           It.IsAny<EventId>(),
           It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
           It.IsAny<Exception?>(),
           It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never());
    }

    private void VerifyWarningServiceWasCancelled(string serviceName)
    {
        string cancelledMessageResourceKey = "ServiceWasCancelled";
        string cancelledMessageFormat = LogResourceManager.GetString(cancelledMessageResourceKey) 
            ?? throw new InvalidOperationException($"message format {cancelledMessageResourceKey} is null or not found");
        LoggerMock.VerifyWarning(cancelledMessageFormat, serviceName);
    }

    private void VerifyCategoryWasLoadedFromState(IProductShopCategory productShopCategory)
    {
        string messageResourceKey = "CategoryWasLoadedFromState";
        string messageFormat = ImportProductsStatefulResourceManager.GetString(messageResourceKey)
            ?? throw new InvalidOperationException($"message format {messageResourceKey} is null or not found");

        LoggerMock.VerifyInfo(messageFormat, productShopCategory.Path);
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
             _serviceStarepository,
             1000);
    }
    private string VerifyCategoryPageLoadedSuccessfully(IProductShopCategory productShopCategory, int page = 1)
    {
        var categoryPageMessageFormat = GetCategoryPageMesageFromat();

        LoggerMock.VerifyInfo(categoryPageMessageFormat, productShopCategory.Path, page);
        return categoryPageMessageFormat;
    }

    [Fact]
    public async Task FailedCategoryRepeteLoadingAfterServiceRestartIfServiceStateisNotEmpty()
    {
        SetupCategoriesLoadingWithDelayOnRandomIteration(out var pauseIteration);

        var serviceName = Guid.NewGuid().ToString();
        var service = CreateService(serviceName);

        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(500);

        await service.Start(cancellationTokenSource.Token);

        VerifyCategoryPageLoadedSuccessfully(ProductShopModelMock.Object.Categories[pauseIteration - 1]);

        VerifyNotContainsCategoryPageMessage(ProductShopModelMock.Object.Categories[pauseIteration]);

        VerifyWarningServiceWasCancelled(serviceName);

        LoggerMock.Invocations.Clear();

        using var newCancellationTokenSource = new CancellationTokenSource();
        await service.Start(newCancellationTokenSource.Token);

        VerifyCategoryWasLoadedFromState(ProductShopModelMock.Object.Categories[pauseIteration]);

        VerifyCategoryPageLoadedSuccessfully(ProductShopModelMock.Object.Categories[pauseIteration]);
    }

    [Fact]
    public async Task CategoryNotReloadAfterServiceRestartIfFailedOnPreviousAttemptAndStateRepositoryIsEmpty()
    {
        SetupCategoriesLoadingWithDelayOnRandomIteration(out var pauseIteration);

        var serviceName = Guid.NewGuid().ToString();
        await using var service = CreateService(serviceName);

        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(500);

        await service.Start(cancellationTokenSource.Token);

        VerifyCategoryPageLoadedSuccessfully(ProductShopModelMock.Object.Categories[pauseIteration - 1]);

        VerifyNotContainsCategoryPageMessage(ProductShopModelMock.Object.Categories[pauseIteration]);

        VerifyWarningServiceWasCancelled(serviceName);

        LoggerMock.Invocations.Clear();
        _serviceStarepository.LoadMock = (key) => null;

        using var newCancellationTokenSource = new CancellationTokenSource();
        await service.Start(newCancellationTokenSource.Token);

        VerifyNotContainsCategoryPageMessage(ProductShopModelMock.Object.Categories[pauseIteration]);

        if(pauseIteration < ProductShopModelMock.Object.Categories.Count - 1)
            VerifyCategoryPageLoadedSuccessfully(ProductShopModelMock.Object.Categories[pauseIteration + 1]);
    }

    [Fact]
    public async Task HandledCategoriesBeforeCanceledItemRemovedAfterServiceRestartIfServiceStateNotEmpty()
    {
        SetupCategoriesLoadingWithDelayOnRandomIteration(out var pauseIteration);

        var serviceName = Guid.NewGuid().ToString();
        await using var service = CreateService(serviceName);

        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(500);

        await service.Start(cancellationTokenSource.Token);

        VerifyCategoryPageLoadedSuccessfully(ProductShopModelMock.Object.Categories[pauseIteration - 1]);

        VerifyNotContainsCategoryPageMessage(ProductShopModelMock.Object.Categories[pauseIteration]);

        VerifyWarningServiceWasCancelled(serviceName);

        LoggerMock.Invocations.Clear();

        using var newCancellationTokenSource = new CancellationTokenSource();
        await using var newService = CreateService(serviceName);
        await newService.Start(newCancellationTokenSource.Token);

        VerifyNotContainsCategoryPageMessage(ProductShopModelMock.Object.Categories[pauseIteration - 1 ]);
    }
}