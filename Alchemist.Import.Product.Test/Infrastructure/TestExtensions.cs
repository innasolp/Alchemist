using Alchemist.Import.Products.Interfaces;
using Moq;
using WebLoader.Interfaces;
using Alchemist.Import.Products.Service;
using WebLoader.Common;
using System.Text.Json;
using Alchemist.Import.Interfaces;

namespace Alchemist.Import.Product.Test.Infrastructure;

internal static class TestExtensions
{
    public static void SetupStartSuccess(this Mock<IWebLoader> webLoaderMock)
    {
        webLoaderMock.Setup(w => w.Start())
            .Returns(Task.FromResult(true))
            .Callback(() => webLoaderMock.Setup(w => w.IsStarted).Returns(true));
    }
    
    public static string GetCategoryPageUrl(this IProductShopModel productShopModel, IProductShopCategoryModel category, int page)
    {
        return string.Format(productShopModel.CategoryUrl, category.GetCategoryForUrl(), page);
    }    

    public static void SetupLoadItem<T>(this Mock<IWebLoader> webLoaderMock, string itemUrl, RequestHeaders requestHeaders, T item)
        where T:class
    {
        webLoaderMock.Setup(w => w.LoadFromUrl(itemUrl, requestHeaders)).Returns(LoadItemAsync(item));
    }

    public static void SetupLoadItems<T>(this Mock<IWebLoader> webLoaderMock, Dictionary<string,T> itemUrls, RequestHeaders requestHeaders)
        where T : class
    {
        foreach(var itemUrl in itemUrls)
        webLoaderMock.Setup(w => w.LoadFromUrl(itemUrl.Key, requestHeaders)).Returns(LoadItemAsync(itemUrl.Value));
    }

    private static async Task<Stream> LoadItemAsync<T>(T item)
        where T : class
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(item);
        var fs = new MemoryStream(bytes);
        return await Task.FromResult(fs);
    }

    public static Task StartServiceInFactoryAsync(this IImportService service, CancellationTokenSource token)
    {
        return Task.Factory.StartNew(async () => await service.Start(token),
           token.Token,
           TaskCreationOptions.None,
           TaskScheduler.Default);
    }
}
