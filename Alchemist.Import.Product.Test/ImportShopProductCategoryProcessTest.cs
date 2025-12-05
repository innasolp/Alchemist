using Alchemist.Import.Product.Test.Infrastructure;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Import.Products.Service;
using Import.Interfaces;
using Import.Service;
using Import.Service.Test.Infrastructure;
using Moq;
using Xunit.Abstractions;

namespace Alchemist.Import.Product.Test;

public class ImportShopProductCategoryProcessTest : ImportProductsTest
{
    public ImportShopProductCategoryProcessTest(ITestOutputHelper outputHelper):base(outputHelper)
    {
        LoaderMock.SetupLoadCookies();
        ProductShopModelMock.Setup(s => s.ProductUrl).Returns("Product_{0}");
        ProductShopModelMock.Setup(s => s.CategoryUrlFormat).Returns("Category_{0}_page{1}");
    }

    private void SetupLoaderWithCategoryLoadException(Exception exception, out string categoryUrl )
    {
        LoaderMock.Reset();
        LoaderMock.SetupStartSuccess();

        var categoryMock = TestHelper.CreateCategoryMock();
        ProductShopModelMock.Object.Categories.Add(categoryMock.Object);

        ProductShopModelMock.Setup(s => s.CategoryUrlFormat).Returns($"{Guid.NewGuid()}_{{0}}");

        var url = ProductShopModelMock.Object.GetCategoryPageUrl(categoryMock.Object, 1);
        LoaderMock.Setup(w => w.Load(url, It.IsAny<object>(), It.IsAny<CancellationToken>())).Throws(exception);

        categoryUrl = url;        
    }

    private TestImportProductService<TestCategory, TestProductItem> SetupServiceWithCategoryProcessException(string name, Func<string, Exception> getItemException, int pageCount, out TestCategory testCategory, out string url)
    {
        LoaderMock.Reset();
        LoaderMock.SetupStartSuccess();

        var categoryMock = TestHelper.CreateCategoryMock();
        ProductShopModelMock.Object.Categories.Add(categoryMock.Object);

        ProductShopModelMock.Setup(s => s.CategoryUrlFormat).Returns(Guid.NewGuid().ToString());

        var categoryUrl = ProductShopModelMock.Object.GetCategoryPageUrl(categoryMock.Object, 1);
        var categoryProducts = TestHelper.CreateCategoryWithProducts();
        
        var requestData = new object();
        LoaderMock.SetupGetRequestData(requestData);

        LoaderMock.SetupLoadItem(categoryUrl, requestData, categoryProducts);

        var service = CreateService(name);
        service.SetPageProductCount(pageCount); 

        var productItems = categoryProducts.CategoryProductItems.ToDictionary(service.GetTestApiUrl, TestHelper.CreateProductItem);
        LoaderMock.SetupLoadItemsThrowsExceptions(productItems.Keys, getItemException, requestData);

        ProductItemHandlerMock.Setup(s => s.HandleItem(It.IsAny<IImportProduct>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(ResultStatus.Success));

        testCategory = categoryProducts;
        url = categoryUrl;        
        
        return service;
    }

    private TestImportProductService<TestCategory, TestProductItem> SetupServiceWithCategoryLoadSuccessfull(string name, int pageCount, out TestCategory testCategory, out string url)
    {
        LoaderMock.Reset();
        LoaderMock.SetupStartSuccess();

        var categoryMock = TestHelper.CreateCategoryMock();
        ProductShopModelMock.Object.Categories.Add(categoryMock.Object);

        ProductShopModelMock.Setup(s => s.CategoryUrlFormat).Returns($"{Guid.NewGuid()}_{{0}}_{{1}}");

        var categoryUrl = ProductShopModelMock.Object.GetCategoryPageUrl(categoryMock.Object, 1);
        var categoryProducts = TestHelper.CreateCategoryWithProducts();

        var requestData = new object();
        LoaderMock.SetupGetRequestData(requestData);
        
        LoaderMock.SetupLoadItem(categoryUrl, requestData, categoryProducts);
        LoaderMock.Setup(w => w.Load(It.IsNotIn(categoryUrl), requestData, It.IsAny<CancellationToken>())).Returns(
            (string url, object requestData, CancellationToken token) => TestExtensions.LoadItemAsync(new TestCategory() { CategoryProductItems = [] }));

        var service = CreateService(name);
        service.SetPageProductCount(pageCount);

        var productItems = categoryProducts.CategoryProductItems.ToDictionary(service.GetTestApiUrl, TestHelper.CreateProductItem);
        LoaderMock.SetupLoadItemsSuccessfull(productItems, requestData);

        ProductItemHandlerMock.Setup(s => s.HandleItem(It.IsAny<IImportProduct>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(ResultStatus.Success));

        testCategory = categoryProducts;
        url = categoryUrl;

        return service;
    }

  [Fact]
    public async Task ImportLogErrorWhenAllCategoryProductsNotProcessed()
    {
        var exceptionFormat = "test exception {0}";
        var service = SetupServiceWithCategoryProcessException(Guid.NewGuid().ToString(), 
            item=>new LoaderServiceException(string.Format(exceptionFormat, item)), 
            10, 
            out var categoryProducts,
            out var categoryUrl);

        var token = new CancellationTokenSource();
        var task = service.StartServiceInFactoryAsync(token.Token);

        await Task.Delay(500);

        await token.CancelAsync();

        LoaderMock.Verify(l => l.Load(It.Is<string>(v => v == categoryUrl), It.IsAny<object>(), It.IsAny<CancellationToken>()));

        foreach (var item in categoryProducts.CategoryProductItems)
        {
            LoggerMock.VerifyInfo(ImportProductsResourceManager.GetString("ProductWasNotLoadedFromUrlWithError"), service.GetTestApiUrl(item));
        }

        LoggerMock.VerifyInfo(ImportProductsResourceManager.GetString("CategoryProductsWereNotLoadedError"), categoryUrl);
    }

    [Fact]
    public async Task ImportLogWhenCategoryCompleted()
    {
        var service = SetupServiceWithCategoryLoadSuccessfull(Guid.NewGuid().ToString(), 10, out var categoryProducts, out var categoryUrl);

        var token = new CancellationTokenSource();
        var task = service.StartServiceInFactoryAsync(token.Token);

        await Task.Delay(500);
        
        await token.CancelAsync();
        
        LoaderMock.Verify(l => l.Load(It.Is<string>(v => v == categoryUrl), It.IsAny<object>(), It.IsAny<CancellationToken>()));

        foreach (var item in categoryProducts.CategoryProductItems)
        {
            LoggerMock.VerifyInfo(ImportProductsResourceManager.GetString("ProductHasBeenSuccessfullyLoadedFromUrl"), item.Name, service.GetTestApiUrl(item));
        }

        LoggerMock.VerifyInfo(ImportProductsResourceManager.GetString("CategoryCompletedInfo"),
            ProductShopModelMock.Object.Categories[0].GetCategoryUrl(), categoryProducts.CategoryProductItems.Length, 0);
        
    }
}