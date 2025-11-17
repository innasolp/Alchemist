using Moq;
using Alchemist.Import.Products.Service;
using Alchemist.Import.Product.Test.Infrastructure;
using Alchemist.Test.Import.Service.Infrastructure;
using Xunit.Abstractions;
using Alchemist.Common;
using Alchemist.Import.Products.Interfaces;
using Alchemist.Exceptions;
using Alchemist.Import.Interfaces;

namespace Alchemist.Import.Product.Test;

public class ImportShopProductCategoryProcessTest : ImportProductsTest
{
    public ImportShopProductCategoryProcessTest(ITestOutputHelper outputHelper):base(outputHelper)
    {
        LoaderMock.SetupLoadCookies();
        ProductShopModelMock.Setup(s => s.ProductUrl).Returns("Product_{0}");
        ProductShopModelMock.Setup(s => s.CategoryUrl).Returns("Category_{0}_page{1}");
    }

    private void SetupServiceWithCategoryLoadException(string name, Exception exception, out string categoryUrl )
    {
        Service.SetName(name);

        LoaderMock.Reset();
        LoaderMock.SetupStartSuccess();

        var categoryMock = TestHelper.CreateCategoryMock();
        ProductShopModelMock.Object.Categories.Add(categoryMock.Object);

        ProductShopModelMock.Setup(s => s.CategoryUrl).Returns(Guid.NewGuid().ToString());

        var url = ProductShopModelMock.Object.GetCategoryPageUrl(categoryMock.Object, 1);
        LoaderMock.Setup(w => w.Load(url, It.IsAny<object>())).Throws(exception);

        categoryUrl = url;
    }

    private void SetupServiceWithCategoryProcessException(string name, Func<string, Exception> getItemException, int pageCount, out TestCategory testCategory, out string url)
    {
        Service.SetName(name);
        Service.SetPageProductCount(pageCount);

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

        var productItems = categoryProducts.CategoryProductItems.ToDictionary(Service.GetTestApiUrl, TestHelper.CreateProductItem);
        LoaderMock.SetupLoadItemsThrowsExceptions(productItems.Keys, getItemException, requestData);

        ProductItemHandlerMock.Setup(s => s.HandleItem(It.IsAny<IImportProduct>())).Returns(Task.FromResult(ResultStatus.Success));

        testCategory = categoryProducts;
        url = categoryUrl;
    }

    private void SetupServiceWithCategoryLoadSuccessfull(string name, int pageCount, out TestCategory testCategory, out string url)
    {
        Service.SetName(name);
        Service.SetPageProductCount(pageCount);

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

        var productItems = categoryProducts.CategoryProductItems.ToDictionary(Service.GetTestApiUrl, TestHelper.CreateProductItem);
        LoaderMock.SetupLoadItemsSuccessfull(productItems, requestData);

        ProductItemHandlerMock.Setup(s => s.HandleItem(It.IsAny<IImportProduct>())).Returns(Task.FromResult(ResultStatus.Success));

        testCategory = categoryProducts;
        url = categoryUrl;
    }


    [Fact]
    public async Task ImportLogWarningWhenCategoryLoadThrowsExceptionWithNeedWaiting()
    {
        var innerException = new HttpRequestException(HttpRequestError.InvalidResponse, "request forbidden", statusCode:System.Net.HttpStatusCode.Forbidden);
        var exception = new LoaderServiceException("request forbidden", innerException, LoaderServiceAction.Wait);
        SetupServiceWithCategoryLoadException(Guid.NewGuid().ToString(), exception, out var url);

        var token = new CancellationTokenSource();
        var task = Service.StartServiceInFactoryAsync(token.Token); ;

        await Task.Delay(1000);

        LoaderMock.Verify(l => l.Load(It.Is<string>(v => v == url), 
            It.IsAny<object>()));

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
        SetupServiceWithCategoryLoadException(Guid.NewGuid().ToString(), exception, out var url);
        LoaderMock.Setup(s => s.Reset()).Returns(Task.FromResult(true));
        LoaderMock.Setup(s => s.UpdateData(url)).Returns(Task.FromResult(true));
        LoaderMock.Setup(s => s.GetData(It.IsAny<string>())).Returns(Task.FromResult(new object()));

        var token = new CancellationTokenSource();
        var task = Service.StartServiceInFactoryAsync(token.Token); 

        await Task.Delay(1000);

        LoaderMock.Verify(l => l.Load(It.Is<string>(v => v == url), 
            It.IsAny<object>()));

        LoggerMock.VerifyWarning(exception, LogResourceManager.GetString("LoadFromUrlCompletedWithErrorAndNeedReset"), [url, exception.Message]);

        LoggerMock.VerifyInfo(LogResourceManager.GetString("LoaderIsReseting"));  
        LoggerMock.VerifyInfo(LogResourceManager.GetString("LoaderResetSuccessfully"));
        
        LoggerMock.VerifyWarning(ImportProductsResourceManager.GetString("CategoryNotLoadedFromUrlWarning"), url, exception.Message);

        await token.CancelAsync();
    }   


    [Fact]
    public async Task ImportLogWarningWhenCategoryLoadThrowsWarningException()
    {
        var exception = new WarningException("warning");        
        SetupServiceWithCategoryLoadException(Guid.NewGuid().ToString(), exception, out var url);
        Service.SetPageProductCount(10);        

        var token = new CancellationTokenSource();
        var task = Service.StartServiceInFactoryAsync(token.Token);

        await Task.Delay(1000);       

        await token.CancelAsync();

        LoaderMock.Verify(l => l.Load(It.Is<string>(v => v == url), It.IsAny<object>()));

        LoggerMock.VerifyWarning(exception, LogResourceManager.GetString("ProcessUrlNotCompleteWarning"), url, exception.Message);       

        LoggerMock.VerifyWarning(ImportProductsResourceManager.GetString("CategoryNotLoadedFromUrlWarning"), url, exception.Message);        
    }

    [Fact]
    public async Task ImportLogErrorWhenCategoryLoadThrowsNotWarningException()
    {
        var exception = new InvalidOperationException("error");
        SetupServiceWithCategoryLoadException(Guid.NewGuid().ToString(), exception, out var url);

        var token = new CancellationTokenSource();
        var task = Service.StartServiceInFactoryAsync(token.Token);

        await Task.Delay(1000);

        LoaderMock.Verify(l => l.Load(It.Is<string>(v => v == url), It.IsAny<object>()));

        LoggerMock.VerifyError(exception, LogResourceManager.GetString("ProcessUrlFailedError"), url);

        LoggerMock.VerifyError(ImportProductsResourceManager.GetString("CategoryLoadingFault"), url, exception.Message);
    }

    [Fact]
    public async Task ImportLogErrorWhenAllCategoryProductsNotProcessed()
    {
        var exceptionFormat = "test exception {0}";
        SetupServiceWithCategoryProcessException(Guid.NewGuid().ToString(), 
            item=>new WarningException(string.Format(exceptionFormat, item)), 
            10, 
            out var categoryProducts,
            out var categoryUrl);

        var token = new CancellationTokenSource();
        var task = Service.StartServiceInFactoryAsync(token.Token);

        await Task.Delay(2000);

        await token.CancelAsync();

        LoaderMock.Verify(l => l.Load(It.Is<string>(v => v == categoryUrl), It.IsAny<object>()));

        foreach (var item in categoryProducts.CategoryProductItems)
        {
            LoggerMock.VerifyWarning(ImportProductsResourceManager.GetString("ProductWasNotLoadedFromUrlWithWarningAndWouldBeReloaded"), 
                item.Name, Service.GetTestApiUrl(item), string.Format(exceptionFormat, Service.GetTestApiUrl(item)));
        }

        LoggerMock.VerifyError(ImportProductsResourceManager.GetString("CategoryProductsWereNotLoadedError"),
            categoryUrl);
    }

    [Fact]
    public async Task ImportLogWhenCategoryCompleted()
    {
        SetupServiceWithCategoryLoadSuccessfull(Guid.NewGuid().ToString(), 10, out var categoryProducts, out var categoryUrl);

        var token = new CancellationTokenSource();
        var task = Service.StartServiceInFactoryAsync(token.Token);

        await Task.Delay((categoryProducts.CategoryProductItems.Length + 1)*200);
        
        await token.CancelAsync();
        
        LoaderMock.Verify(l => l.Load(It.Is<string>(v => v == categoryUrl), It.IsAny<object>()));

        foreach (var item in categoryProducts.CategoryProductItems)
        {
            LoggerMock.VerifyInfo(ImportProductsResourceManager.GetString("ProductHasBeenSuccessfullyLoadedFromUrl"), item.Name, Service.GetTestApiUrl(item));
        }

        LoggerMock.VerifyInfo(ImportProductsResourceManager.GetString("CategoryCompletedInfo"),
            ProductShopModelMock.Object.Categories[0].GetCategoryUrl(), categoryProducts.CategoryProductItems.Length, 0);
        
    }
}
