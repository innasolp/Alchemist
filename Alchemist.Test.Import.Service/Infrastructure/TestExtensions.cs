using Moq;
using System.Text.Json;
using Alchemist.Import.Interfaces;

namespace Alchemist.Test.Import.Service.Infrastructure;

public static class TestExtensions
{
    public static void SetupLoadCookies(this Mock<ILoaderService> loaderMock)
    {
        loaderMock.Setup(w => w.GetData(It.IsAny<string>()))
            .Returns(async (string host) => await Task.FromResult(new object()));
    }

    public static void SetupStartSuccess(this Mock<ILoaderService> loaderMock)
    {
        loaderMock.Setup(w => w.Start())
            .Returns(Task.FromResult(true))
            .Callback(() => loaderMock.Setup(w => w.IsStarted).Returns(true));
    }

    public static void SetupGetRequestData(this Mock<ILoaderService> loaderMock,
        object requestData)
    {
        loaderMock.Setup(l => l.GetData(It.IsAny<string>())).Returns(Task.FromResult(requestData));
    }

    public static void SetupLoadItem<T>(this Mock<ILoaderService> loaderMock,
        string itemUrl, 
        object requestData,
        T item)
        where T:class
    {
        loaderMock.Setup(w => w.Load(itemUrl, requestData)).Returns(
            (string url, object requestData) => LoadItemAsync(item));
    }

    public static void SetupLoadItemsSuccessfull<T>(this Mock<ILoaderService> loaderMock,
        Dictionary<string,T> itemUrls,object requestData)
        where T : class
    {
        foreach(var itemUrl in itemUrls)
        loaderMock.Setup(w => w.Load(itemUrl.Key, requestData)).Returns(LoadItemAsync(itemUrl.Value));
    }

    public static void SetupLoadItemsThrowsExceptions<T>(this Mock<ILoaderService> webLoaderMock,
        Dictionary<string, T> itemUrls, 
        Func<string, Exception> getItemException,
        object requestData)
        where T : class
    {
        foreach (var itemUrl in itemUrls)
            webLoaderMock.Setup(w => w.Load(itemUrl.Key, requestData))
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

    public static void VerifyLoadUrlAndRequestHeaders(this Mock<ILoaderService> loaderMock, 
        string url, 
        object requestData)
    {
        loaderMock.Verify(l => l.Load(It.Is<string>(v => v == url), 
            It.Is<object>(r=>r == requestData)));
    }
}
