using Moq;
using System.ComponentModel;
using WebLoader.Common;
using Alchemist.Import.Products.Service;
using Alchemist.Import.Interfaces;
using Alchemist.Import.Product.Test.Infrastructure;
using Alchemist.Test.Import.Service.Infrastructure;
using Xunit.Abstractions;

namespace Alchemist.Import.Product.Test;

public class ImportShopProductCategoryProcessTest : ImportProductsTest
{
    public ImportShopProductCategoryProcessTest(ITestOutputHelper outputHelper):base(outputHelper)
    {
        ProductShopModelMock.Setup(s => s.ProductUrl).Returns("Product_{0}");
        ProductShopModelMock.Setup(s => s.CategoryUrl).Returns("Category_{0}_page{1}");
    }

    private void SetupServiceWithCategoryException(string name, Exception exception, out string categoryUrl )
    {
        Service.SetName(name);

        WebLoaderMock.Reset();
        WebLoaderMock.SetupStartSuccess();

        var categoryMock = TestHelper.CreateCategoryMock();
        ProductShopModelMock.Object.Categories.Add(categoryMock.Object);

        ProductShopModelMock.Setup(s => s.CategoryUrl).Returns(Guid.NewGuid().ToString());

        var url = ProductShopModelMock.Object.GetCategoryPageUrl(categoryMock.Object, 1);
        WebLoaderMock.Setup(w => w.LoadFromUrl(url, RequestHeaders)).Throws(exception);

        categoryUrl = url;
    }

    private void SetupServiceWithCategoryLoadSuccessfull(string name, int pageCount, out TestCategory testCategory, out string url)
    {
        Service.SetName(name);
        Service.SetPageProductCount(pageCount);

        WebLoaderMock.Reset();
        WebLoaderMock.SetupStartSuccess();

        var categoryMock = TestHelper.CreateCategoryMock();
        ProductShopModelMock.Object.Categories.Add(categoryMock.Object);

        ProductShopModelMock.Setup(s => s.CategoryUrl).Returns(Guid.NewGuid().ToString());

        var categoryUrl = ProductShopModelMock.Object.GetCategoryPageUrl(categoryMock.Object, 1);
        var categoryProducts = TestHelper.CreateCategoryWithProducts();

        WebLoaderMock.SetupLoadItem(categoryUrl, RequestHeaders, categoryProducts);

        var productItems = categoryProducts.CategoryProductItems.ToDictionary(Service.GetTestApiUrl, TestHelper.CreateProductItem);
        WebLoaderMock.SetupLoadItems(productItems, RequestHeaders);

        ProductItemHandlerMock.Setup(s => s.HandleItem(It.IsAny<It.IsAnyType>(), It.IsAny<IShopModel>())).Returns(Task.FromResult(Common.ItemProcessStatus.AlreadyExists));

        testCategory = categoryProducts;
        url = categoryUrl;
    }


    [Fact]
    public async Task ImportLogWarningWhenCategoryLoadThrowsHttpException()
    {
        var exception = new HttpRequestException(HttpRequestError.InvalidResponse, "request forbidden", statusCode:System.Net.HttpStatusCode.Forbidden);
        SetupServiceWithCategoryException(Guid.NewGuid().ToString(), exception, out var url);

        var token = new CancellationTokenSource();
        var task = Service.StartServiceInFactoryAsync(token.Token); ;

        await Task.Delay(1000);

        WebLoaderMock.Verify(l => l.LoadFromUrl(It.Is<string>(v => v == url), It.IsAny<RequestHeaders>()));

        LoggerMock.VerifyWarning(ImportProductsResourceManager.GetString("CategoryProcessWarning"), url, exception.Message);        

        LoggerMock.VerifyWarning(exception, ServiceResourceManager.GetString("HttpRequestErrorAndWebLoaderRestart"), 
            System.Net.HttpStatusCode.Forbidden, url, WebLoaderMock.Object.GetType().Name);

        await token.CancelAsync();
    }

    [Fact]
    public async Task ImportLogWarningWhenCategoryLoadThrowsWebLoaderException()
    {
        var exception = new WebLoaderException(NsError.NS_ERROR_REDIRECT_LOOP, "error redirect loop");
        SetupServiceWithCategoryException(Guid.NewGuid().ToString(), exception, out var url);

        var token = new CancellationTokenSource();
        var task = Service.StartServiceInFactoryAsync(token.Token); 

        await Task.Delay(3000);

        WebLoaderMock.Verify(l => l.LoadFromUrl(It.Is<string>(v => v == url), It.IsAny<RequestHeaders>()));

        LoggerMock.VerifyWarning(exception, ServiceResourceManager.GetString("WebLoaderThrowsNsRedirectLoopAndWillBeReseted"), url);

        LoggerMock.VerifyInfo(ServiceResourceManager.GetString("WebLoaderIsReseting"));             

        LoggerMock.VerifyInfo(ServiceResourceManager.GetString("WebLoaderResetSuccessfully"));
        
        LoggerMock.VerifyWarning(ImportProductsResourceManager.GetString("CategoryProcessWarning"), url, exception.Message);

        await token.CancelAsync();
    }

    [Fact]
    public async Task ImportLogWarningWhenCategoryLoadThrowsWarningException()
    {
        var exception = new WarningException("warning");        
        SetupServiceWithCategoryException(Guid.NewGuid().ToString(), exception, out var url);
        Service.SetPageProductCount(10);

        var token = new CancellationTokenSource();
        var task = Service.StartServiceInFactoryAsync(token.Token);

        await Task.Delay(500);       

        await token.CancelAsync();

        WebLoaderMock.Verify(l => l.LoadFromUrl(It.Is<string>(v => v == url), It.IsAny<RequestHeaders>()));

        LoggerMock.VerifyWarning(exception, ServiceResourceManager.GetString("ProcessUrlNotCompleteWarning"), url, exception.Message);       

        LoggerMock.VerifyWarning(ImportProductsResourceManager.GetString("CategoryProcessWarning"), url, exception.Message);        
    }

    [Fact]
    public async Task ImportLogErrorWhenCategoryLoadThrowsNotWarningException()
    {
        var exception = new InvalidOperationException("error");
        SetupServiceWithCategoryException(Guid.NewGuid().ToString(), exception, out var url);

        var token = new CancellationTokenSource();
        var task = Service.StartServiceInFactoryAsync(token.Token);

        await Task.Delay(1000);

        WebLoaderMock.Verify(l => l.LoadFromUrl(It.Is<string>(v => v == url), It.IsAny<RequestHeaders>()));

        LoggerMock.VerifyError(exception, ServiceResourceManager.GetString("ProcessUrlFailedError"), url);

        LoggerMock.VerifyError(ImportProductsResourceManager.GetString("CategoryProcessFault"), url, exception.Message);
    }

    [Fact]
    public async Task ImportLogWhenCategoryCompleted()
    {
        SetupServiceWithCategoryLoadSuccessfull(Guid.NewGuid().ToString(), 10, out var categoryProducts, out var categoryUrl);

        var token = new CancellationTokenSource();
        var task = Service.StartServiceInFactoryAsync(token.Token);

        await Task.Delay(3000);
        
        await token.CancelAsync();

        WebLoaderMock.Verify(l => l.LoadFromUrl(It.Is<string>(v => v == categoryUrl), It.IsAny<RequestHeaders>()));

        foreach (var item in categoryProducts.CategoryProductItems)
        {
            LoggerMock.VerifyInfo(ImportProductsResourceManager.GetString("ProductHasBeenSuccessfullyLoadedFromUrl"), item.Name, Service.GetTestApiUrl(item));
        }

        LoggerMock.VerifyInfo(ImportProductsResourceManager.GetString("CategoryCompletedInfo"),
            ProductShopModelMock.Object.Categories[0].GetCategoryUrl(), categoryProducts.CategoryProductItems.Length, 0);
        
    }
}
