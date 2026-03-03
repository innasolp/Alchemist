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

    private void SetupCategoriesLoadingWithDelayOnIteration(object requestData, List<IProductShopCategory> productCategories, int categoryPauseIteration)
    {
        bool delayed = false;

        async Task delayOnCategoryLoadAsync(TestCategory category, CancellationToken token)
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

            Func<TestCategory, CancellationToken, Task>? onLoad = (i == categoryPauseIteration) ? delayOnCategoryLoadAsync : null;

            LoaderMock.SetupLoadItem(categoryUrl, requestData, categoryProducts, onLoad);
        }
    }

    private object SetupLoader()
    {
        LoaderMock.Reset();
        LoaderMock.SetupStartSuccess();
        LoaderMock.Setup(s => s.Name).Returns($"{Guid.NewGuid()}");

        var requestData = new object();
        LoaderMock.SetupGetRequestData(requestData);
        return requestData;
    }

    private List<Mock<IProductShopCategory>> SetupProductCategories()
    {
        var productCategoriesMock = new List<Mock<IProductShopCategory>>
        {
            TestHelper.CreateProductShopCategoryMock(),
            TestHelper.CreateProductShopCategoryMock(),
            TestHelper.CreateProductShopCategoryMock(),
            TestHelper.CreateProductShopCategoryMock()
        };

        ProductShopModelMock.Setup(s => s.Categories).Returns([.. productCategoriesMock.Select(c=>c.Object)]);
        ProductShopModelMock.Setup(s => s.CategoryUrlFormat).Returns($"{Guid.NewGuid()}_{{0}}_{{1}}");
        ProductShopModelMock.Setup(s => s.ProductUrl).Returns($"{Guid.NewGuid()}_{{0}}");
        return productCategoriesMock;
    }

    private void SetupCategories(object requestData, List<IProductShopCategory> productCategories, out TestCategory[] testCategories)
    {       
        testCategories = new TestCategory[productCategories.Count];

        for (int i = 0; i < productCategories.Count; i++)
        {
            IProductShopCategory? productCategory = productCategories[i];
            var categoryUrl = ProductShopModelMock.Object.GetCategoryPageUrl(productCategory, 1);
            var productCount = new Random().Next(10, 20);
            var categoryProducts = TestHelper.CreateCategoryWithProducts(productCount, productCount, 1);
            testCategories[i] = categoryProducts;

            LoaderMock.SetupLoadItem(categoryUrl, requestData, categoryProducts);                    
        }
    }

    private void SetupProductItemsWithDelayOnIteration(TestCategory[] testCategories, object requestData, int categoryNumber, int productNumber)
    {
        bool delayed = false;

        TestCategoryProduct? failedProduct = null;

        async Task delayOnProductLoadAsync(TestProductItem product, CancellationToken token)
        {
            if (product.ItemId != failedProduct?.Id) return;

            if (delayed)
            {
                _outputHelper.WriteLine("Product loading without delay on next attempt.");
                return;
            }

            try
            {
                await Task.Delay(1000, token);
                _outputHelper.WriteLine("Product loading not canceled on first attempt");
            }
            catch (OperationCanceledException)
            {
                _outputHelper.WriteLine("Product loading was canceled on first attempt correctly.");
                delayed = true;
                throw;
            }
        }

        for (int i = 0; i < testCategories.Length; i++)
        {
            if (i == categoryNumber)
                failedProduct = testCategories[i].CategoryProductItems[productNumber];

            var productItems = testCategories[i].CategoryProductItems.ToDictionary(
                p => GetProductItemApiUrl(ProductShopModelMock.Object.ProductUrl, p), TestHelper.CreateProductItem);
            LoaderMock.SetupLoadItemsSuccessfull(productItems, requestData, delayOnProductLoadAsync);
        }
    }

    private static string GetProductItemApiUrl(string productUrlFormat,ICategoryProductItem productItem)
    {
        return string.Format(productUrlFormat, productItem.Name);
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

    private void VerifyCategoryCompletedSuccessfully(IProductShopCategory productShopCategory, TestCategory[] categoryPages)
    {
        string messageResourceKey = "CategoryCompletedInfo";
        string messageFormat = ImportProductsResourceManager.GetString(messageResourceKey)
            ?? throw new InvalidOperationException($"message format {messageResourceKey} is null or not found");

        LoggerMock.VerifyInfo(messageFormat, productShopCategory.Category, categoryPages.Sum(c=>c.CategoryProductItems.Length), 0);
    }

    private void VerifyNotContainsCategoryCompleted(IProductShopCategory productShopCategory)
    {
        string messageResourceKey = "CategoryCompletedInfo";
        string messageFormat = ImportProductsResourceManager.GetString(messageResourceKey)
            ?? throw new InvalidOperationException($"message format {messageResourceKey} is null or not found");

        var formatParameters = new
        {
            Category = productShopCategory.Path,
            Page = 1
        };

        LoggerMock.Verify(l => l.Log(
           It.Is<LogLevel>(v => v == LogLevel.Information),
           It.IsAny<EventId>(),
           It.Is<It.IsAnyType>((v, t) => LoggerMockExtensions.LogStateComparer(v, messageFormat, productShopCategory.Category, It.IsAny<int>(), It.IsAny<int>())),
           It.IsAny<Exception?>(),
           It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never());
    }

    private void VerifyProductLoadedSuccessfully(ICategoryProductItem categoryProductItem)
    {
        string messageResourceKey = "ProductHasBeenSuccessfullyLoadedFromUrl";
        string messageFormat = ImportProductsResourceManager.GetString(messageResourceKey)
            ?? throw new InvalidOperationException($"message format {messageResourceKey} is null or not found");

        LoggerMock.VerifyInfo(messageFormat, categoryProductItem.Name, GetProductItemApiUrl(ProductShopModelMock.Object.ProductUrl, categoryProductItem));
    }

    private void VerifyNotContainsProductLoadedSuccessfully(ICategoryProductItem categoryProductItem)
    {
        string messageResourceKey = "ProductHasBeenSuccessfullyLoadedFromUrl";
        string messageFormat = ImportProductsResourceManager.GetString(messageResourceKey)
            ?? throw new InvalidOperationException($"message format {messageResourceKey} is null or not found");

        LoggerMock.Verify(l => l.Log(
           It.Is<LogLevel>(v => v == LogLevel.Information),
           It.IsAny<EventId>(),
           It.Is<It.IsAnyType>((v, t) => LoggerMockExtensions.LogStateComparer(v, messageFormat, categoryProductItem.Name, GetProductItemApiUrl(ProductShopModelMock.Object.ProductUrl, categoryProductItem))),
           It.IsAny<Exception?>(),
           It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never());
    }

    [Fact]
    public async Task FailedCategoryRepeateLoadingAfterServiceRestartIfServiceStateisNotEmpty()
    {
        var productCategoryMocks = SetupProductCategories();

        object requestData = SetupLoader();

        var pauseIteration = new Random().Next(1, productCategoryMocks.Count - 2);
        SetupCategoriesLoadingWithDelayOnIteration(requestData, [.. productCategoryMocks.Select(c=>c.Object)], pauseIteration);

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
    public async Task FailedCategoryProductRepeateLoadingAfterServiceRestartIfServiceStateisNotEmpty()
    {
        var productCategoryMocks = SetupProductCategories();

        object requestData = SetupLoader();

        var categoryNumber = new Random().Next(1, productCategoryMocks.Count - 1);
        SetupCategories(requestData, [.. productCategoryMocks.Select(c => c.Object)], out var testCategories);
        var productNumber = new Random().Next(0, testCategories[categoryNumber].CategoryProductItems.Length - 1);
        SetupProductItemsWithDelayOnIteration(testCategories, requestData, categoryNumber, productNumber);

        var serviceName = Guid.NewGuid().ToString();
        var service = CreateService(serviceName);

        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(500);

        await service.Start(cancellationTokenSource.Token);

        VerifyCategoryCompletedSuccessfully(ProductShopModelMock.Object.Categories[categoryNumber - 1], [testCategories[categoryNumber - 1]]);

        VerifyNotContainsCategoryCompleted(ProductShopModelMock.Object.Categories[categoryNumber]);

        VerifyProductLoadedSuccessfully(testCategories[categoryNumber].CategoryProductItems[productNumber - 1]);

        VerifyNotContainsProductLoadedSuccessfully(testCategories[categoryNumber].CategoryProductItems[productNumber]);

        VerifyWarningServiceWasCancelled(serviceName);

        LoggerMock.Invocations.Clear();

        using var newCancellationTokenSource = new CancellationTokenSource();
        await service.Start(newCancellationTokenSource.Token);

        VerifyCategoryWasLoadedFromState(ProductShopModelMock.Object.Categories[categoryNumber]);

        VerifyCategoryCompletedSuccessfully(ProductShopModelMock.Object.Categories[categoryNumber], [testCategories[categoryNumber]]);

        VerifyProductLoadedSuccessfully(testCategories[categoryNumber].CategoryProductItems[productNumber]);
    }

    [Fact]
    public async Task CategoryNotReloadAfterServiceRestartIfFailedOnPreviousAttemptAndStateRepositoryIsEmpty()
    {
        var productCategoryMocks = SetupProductCategories();

        object requestData = SetupLoader();

        var pauseIteration = new Random().Next(1, productCategoryMocks.Count - 2);
        SetupCategoriesLoadingWithDelayOnIteration(requestData, [.. productCategoryMocks.Select(c => c.Object)], pauseIteration);

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
        var productCategoryMocks = SetupProductCategories();
        object requestData = SetupLoader();
        var pauseIteration = new Random().Next(1, productCategoryMocks.Count - 2);
        SetupCategoriesLoadingWithDelayOnIteration(requestData, [.. productCategoryMocks.Select(c => c.Object)], pauseIteration);

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