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
        ProductShopModelMock.Setup(s => s.CategoryUrl).Returns("Category_{0}_page{1}");
    }

    private void SetupLoaderWithCategoryLoadException(Exception exception, out string categoryUrl )
    {
        LoaderMock.Reset();
        LoaderMock.SetupStartSuccess();

        var categoryMock = TestHelper.CreateCategoryMock();
        ProductShopModelMock.Object.Categories.Add(categoryMock.Object);

        ProductShopModelMock.Setup(s => s.CategoryUrl).Returns(Guid.NewGuid().ToString());

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

        ProductShopModelMock.Setup(s => s.CategoryUrl).Returns(Guid.NewGuid().ToString());

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

        ProductShopModelMock.Setup(s => s.CategoryUrl).Returns(Guid.NewGuid().ToString());

        var categoryUrl = ProductShopModelMock.Object.GetCategoryPageUrl(categoryMock.Object, 1);
        var categoryProducts = TestHelper.CreateCategoryWithProducts();

        var requestData = new object();
        LoaderMock.SetupGetRequestData(requestData);
        LoaderMock.SetupLoadItem(categoryUrl, requestData, categoryProducts);

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
    public async Task ImportLogWarningWhenCategoryLoadThrowsExceptionWithNeedWaiting()
    {
        var innerException = new HttpRequestException(HttpRequestError.InvalidResponse, "request forbidden", statusCode:System.Net.HttpStatusCode.Forbidden);
        var exception = new LoaderServiceException("request forbidden", innerException, LoaderServiceAction.Wait);
        SetupLoaderWithCategoryLoadException(exception, out var url);
        var service = CreateService(Guid.NewGuid().ToString());

        var token = new CancellationTokenSource();
        var task = service.StartServiceInFactoryAsync(token.Token); ;

        await Task.Delay(1000);

        LoaderMock.Verify(l => l.Load(It.Is<string>(v => v == url), 
            It.IsAny<object>(), It.IsAny<CancellationToken>()));

        LoggerMock.VerifyWarning(ImportProductsResourceManager.GetString("CategoryNotLoadedFromUrlWarning"), url, exception.Message);        

        LoggerMock.VerifyWarning(exception, LogResourceManager.GetString("RequestFailedAndLoaderWillBePaused"), 
            url, exception.Message, 500);

        await token.CancelAsync();
    }

    //todo 
    [Fact]
    public async Task ImportLogWarningWhenCategoryLoadThrowsExceptionWithNeedReseting()
    {
        var exception = new LoaderServiceException( "error redirect loop", LoaderServiceAction.Reset);
        SetupLoaderWithCategoryLoadException(exception, out var url);
        LoaderMock.Setup(s => s.Reset(It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));
        LoaderMock.Setup(s => s.UpdateData(url, It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));
        LoaderMock.Setup(s => s.GetData(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new object()));

        var service = CreateService(Guid.NewGuid().ToString());

        var token = new CancellationTokenSource();
        var task = service.StartServiceInFactoryAsync(token.Token); 

        await Task.Delay(1000);

        LoaderMock.Verify(l => l.Load(It.Is<string>(v => v == url), 
            It.IsAny<object>(), It.IsAny<CancellationToken>()));

        LoggerMock.VerifyWarning(exception, LogResourceManager.GetString("LoadFromUrlCompletedWithErrorAndNeedReset"), [url, exception.Message]);

        LoggerMock.VerifyInfo(LogResourceManager.GetString("LoaderIsReseting"));  
        LoggerMock.VerifyInfo(LogResourceManager.GetString("LoaderResetSuccessfully"));
        
        LoggerMock.VerifyWarning(ImportProductsResourceManager.GetString("CategoryNotLoadedFromUrlWarning"), url, exception.Message);

        await token.CancelAsync();
    }   


    [Fact]
    public async Task ImportLogWarningWhenCategoryLoadThrowsWarningException()
    {
        var exception = new ImportWarningException("warning");        
        SetupLoaderWithCategoryLoadException(exception, out var url);

        var service = CreateService(Guid.NewGuid().ToString());
        service.SetPageProductCount(10);        

        var token = new CancellationTokenSource();
        var task = service.StartServiceInFactoryAsync(token.Token);

        await Task.Delay(1000);       

        await token.CancelAsync();

        LoaderMock.Verify(l => l.Load(It.Is<string>(v => v == url), It.IsAny<object>(), It.IsAny<CancellationToken>()));

        LoggerMock.VerifyWarning(exception, LogResourceManager.GetString("ProcessUrlNotCompleteWarning"), url, exception.Message);       

        LoggerMock.VerifyWarning(ImportProductsResourceManager.GetString("CategoryNotLoadedFromUrlWarning"), url, exception.Message);        
    }

    [Fact]
    public async Task ImportLogErrorWhenCategoryLoadThrowsNotWarningException()
    {
        var exception = new InvalidOperationException("error");
        SetupLoaderWithCategoryLoadException(exception, out var url);

        var token = new CancellationTokenSource();
        var service = CreateService(Guid.NewGuid().ToString());
        var task = service.StartServiceInFactoryAsync(token.Token);

        await Task.Delay(1000);

        LoaderMock.Verify(l => l.Load(It.Is<string>(v => v == url), It.IsAny<object>(), It.IsAny<CancellationToken>()));

        LoggerMock.VerifyError(exception, LogResourceManager.GetString("ProcessUrlFailedError"), url);

        LoggerMock.VerifyError(ImportProductsResourceManager.GetString("CategoryLoadingFault"), url, exception.Message);
    }

    [Fact]
    public async Task ImportLogErrorWhenAllCategoryProductsNotProcessed()
    {
        var exceptionFormat = "test exception {0}";
        var service = SetupServiceWithCategoryProcessException(Guid.NewGuid().ToString(), 
            item=>new ImportWarningException(string.Format(exceptionFormat, item)), 
            10, 
            out var categoryProducts,
            out var categoryUrl);

        var token = new CancellationTokenSource();
        var task = service.StartServiceInFactoryAsync(token.Token);

        await Task.Delay(2000);

        await token.CancelAsync();

        LoaderMock.Verify(l => l.Load(It.Is<string>(v => v == categoryUrl), It.IsAny<object>(), It.IsAny<CancellationToken>()));

        foreach (var item in categoryProducts.CategoryProductItems)
        {
            LoggerMock.VerifyWarning(ImportProductsResourceManager.GetString("ProductWasNotLoadedFromUrlWithWarningAndWouldBeReloaded"), 
                item.Name, service.GetTestApiUrl(item), string.Format(exceptionFormat, service.GetTestApiUrl(item)));
        }

        LoggerMock.VerifyError(ImportProductsResourceManager.GetString("CategoryProductsWereNotLoadedError"),
            categoryUrl);
    }

    [Fact]
    public async Task ImportLogWhenCategoryCompleted()
    {
        var service = SetupServiceWithCategoryLoadSuccessfull(Guid.NewGuid().ToString(), 10, out var categoryProducts, out var categoryUrl);

        var token = new CancellationTokenSource();
        var task = service.StartServiceInFactoryAsync(token.Token);

        await Task.Delay((categoryProducts.CategoryProductItems.Length + 1)*300);
        
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