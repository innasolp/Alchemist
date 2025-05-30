using Moq;
using WebLoader.Interfaces;
using WebLoader.Common;
using System.Text.Json;
using Alchemist.Import.Interfaces;

namespace Alchemist.Test.Import.Service.Infrastructure;

public static class TestExtensions
{
    public static void SetupStartSuccess(this Mock<IWebLoader> webLoaderMock)
    {
        webLoaderMock.Setup(w => w.Start())
            .Returns(Task.FromResult(true))
            .Callback(() => webLoaderMock.Setup(w => w.IsStarted).Returns(true));
    }
      

    public static void SetupLoadItem<T>(this Mock<IWebLoader> webLoaderMock, string itemUrl, RequestHeaders requestHeaders, T item)
        where T:class
    {
        webLoaderMock.Setup(w => w.LoadFromUrl(itemUrl, requestHeaders)).Returns((string url, RequestHeaders headers) => LoadItemAsync(item));
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

    public static Task StartServiceInFactoryAsync(this IImportService service, CancellationToken token)
    {
        return Task.Factory.StartNew(async () => await service.Start(token),token,
            TaskCreationOptions.RunContinuationsAsynchronously,            
           TaskScheduler.Current);
    }

    public static void VerifyLoadUrlAndRequestHeaders(this Mock<IWebLoader> webLoaderMock, string url, RequestHeaders requestHeaders)
    {
        webLoaderMock.Verify(l => l.LoadFromUrl(It.Is<string>(v => v == url), It.Is<RequestHeaders>(r=>r == requestHeaders)));
    }
}
