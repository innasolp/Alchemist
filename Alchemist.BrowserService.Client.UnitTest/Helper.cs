using Import.LoaderSettings;
using Moq;
using WebLoader.Interfaces;
using RequestOptions = Import.LoaderSettings.RequestOptions;

namespace Alchemist.BrowserService.Client.UnitTest;

internal static class Helper
{
    public static async Task<BrowserServiceClient> CreateAsync(Mock<IWebLoader> webLoaderMock,
        RequestOptions? hostRequestOptions = null,
        RateLimiterOptions? rateLimiterOptions = null)
    {
        webLoaderMock.SetupGet(w => w.IsStarted).Returns(false);
        webLoaderMock.Setup(w => w.Start()).Returns(async () =>
        {
            webLoaderMock.SetupGet(x => x.IsStarted).Returns(true);
            await Task.CompletedTask;
            return true;
        });

        var host = "http://host";

        var rateLimiter = new RatelimiterWebLoader(webLoaderMock.Object,
            new RateLimiterOptions { WindowMilliseconds = rateLimiterOptions?.WindowMilliseconds ?? 100 });

        var httpClientMock = new Mock<HttpClient>();
        return new BrowserServiceClient(httpClientMock.Object, "test", host, rateLimiter, hostRequestOptions: hostRequestOptions, browserDataLoader: null, browserDataLauncher: null, requestHeaders: null);
    }

    public static async Task<Stream> LoadAsync(Mock<IWebLoader> webLoaderMock, string url, object? data = null,
        RequestOptions? hostRequestOptions = null,
        RateLimiterOptions? rateLimiterOptions = null,
        CancellationToken cancellationToken = default)
    {
        var client = await CreateAsync(webLoaderMock, hostRequestOptions, rateLimiterOptions);

        await client.Start(CancellationToken.None);

        return await client.Load(url, data, cancellationToken);
    }    

    public static async Task<Stream> LoadHostAsync(Mock<IWebLoader> webLoaderMock, string hostUrl, 
        RequestOptions? hostRequestOptions = null,
        RateLimiterOptions? rateLimiterOptions = null,
        CancellationToken cancellationToken = default)
    {
        var client = await CreateAsync(webLoaderMock, hostRequestOptions, rateLimiterOptions);

        await client.Start(CancellationToken.None);

        return await client.LoadHostPage(hostUrl, cancellationToken);
    }
}