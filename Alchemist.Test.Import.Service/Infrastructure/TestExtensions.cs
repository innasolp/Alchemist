using Moq;
using WebLoader.Interfaces;
using WebLoader.Common;
using System.Text.Json;
using Alchemist.Import.Interfaces;
using ICookieData = WebLoader.Interfaces.ICookieData;

namespace Alchemist.Test.Import.Service.Infrastructure;

public static class TestExtensions
{
    public static void SetupLoadCookies(this Mock<IBrowserService> browserDataLoaderMock)
    {
        browserDataLoaderMock.Setup(w => w.LoadCookies(It.IsAny<string>()))
            .Returns(async (string host) => await Task.FromResult(new List<Alchemist.Import.Interfaces.ICookieData>()));
    }

    public static void SetupStartSuccess(this Mock<IWebLoader> webLoaderMock)
    {
        webLoaderMock.Setup(w => w.Start())
            .Returns(Task.FromResult(true))
            .Callback(() => webLoaderMock.Setup(w => w.IsStarted).Returns(true));
    }
      

    public static void SetupLoadItem<T>(this Mock<IWebLoader> webLoaderMock,
        string itemUrl, 
        RequestHeaders requestHeaders,
        IEnumerable<ICookieData> cookies,
        T item)
        where T:class
    {
        webLoaderMock.Setup(w => w.LoadFromUrl(itemUrl, requestHeaders, cookies)).Returns(
            (string url, RequestHeaders headers, IEnumerable<ICookieData> cookieData) => LoadItemAsync(item));
    }

    public static void SetupLoadItemsSuccessfull<T>(this Mock<IWebLoader> webLoaderMock,
        Dictionary<string,T> itemUrls,
        RequestHeaders requestHeaders,
        IEnumerable<ICookieData> cookies)
        where T : class
    {
        foreach(var itemUrl in itemUrls)
        webLoaderMock.Setup(w => w.LoadFromUrl(itemUrl.Key, requestHeaders, cookies)).Returns(LoadItemAsync(itemUrl.Value));
    }

    public static void SetupLoadItemsThrowsExceptions<T>(this Mock<IWebLoader> webLoaderMock,
        Dictionary<string, T> itemUrls, 
        Func<string, Exception> getItemException,
        RequestHeaders requestHeaders,
        IEnumerable<ICookieData> cookies)
        where T : class
    {
        foreach (var itemUrl in itemUrls)
            webLoaderMock.Setup(w => w.LoadFromUrl(itemUrl.Key, requestHeaders, cookies))
                .Throws(getItemException(itemUrl.Key));
    }

    private static async Task<Stream> LoadItemAsync<T>(T item)
        where T : class
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(item);
        var fs = new MemoryStream(bytes);
        return await Task.FromResult(fs);
    }

    public static Task StartServiceInFactoryAsync(this IImportService service, CancellationToken token)
    {
        return Task.Factory.StartNew(async () => await service.Start(token),token,
            TaskCreationOptions.RunContinuationsAsynchronously,            
           TaskScheduler.Current);
    }

    public static void VerifyLoadUrlAndRequestHeaders(this Mock<IWebLoader> webLoaderMock, 
        string url, 
        RequestHeaders requestHeaders,
        IEnumerable<ICookieData> cookies)
    {
        webLoaderMock.Verify(l => l.LoadFromUrl(It.Is<string>(v => v == url), 
            It.Is<RequestHeaders>(r=>r == requestHeaders),
            It.Is<IEnumerable<ICookieData>>(c=>c == cookies)));
    }
}
