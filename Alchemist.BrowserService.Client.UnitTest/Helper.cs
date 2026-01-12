using Alchemist.Import.Settings;
using Moq;
using WebLoader.Interfaces;

namespace Alchemist.BrowserService.Client.UnitTest;

internal static class Helper
{
    public static async Task<Stream> LoadAsync(Mock<IWebLoader> webLoaderMock, string url, object? data = null, Import.Settings.RequestOptions? hostRequestOptions = null, CancellationToken cancellationToken = default)
    {
        // Initially IsStarted is false; when Start() runs we make IsStarted return true.
        webLoaderMock.SetupGet(w => w.IsStarted).Returns(false);
        webLoaderMock.Setup(w => w.Start()).Returns(async () =>
        {
            webLoaderMock.SetupGet(x => x.IsStarted).Returns(true);
            await Task.CompletedTask;
            return true;
        });

        // Use the real rate limiter wrapper with mocked IWebLoader.
        var rateLimiter = new RatelimiterWebLoader(webLoaderMock.Object, new RateLimiterOptions { WindowMilliseconds = 100 });

        using var httpClient = new HttpClient(); // not used in this scenario but required by ctor
        var client = new BrowserServiceClient(httpClient, "test", "http://host", rateLimiter, hostRequestOptions: hostRequestOptions, browserDataLoader: null, browserDataLauncher: null, requestHeaders: null);

        // Act
        // Start the client which will add connection to pool and call underlying Start().
        await client.Start(CancellationToken.None);

        // Call Load with an IEnumerable containing empty cookies (method expects IEnumerable).
        return await client.Load(url, data, cancellationToken);
    }

    public static async Task<Stream> LoadHostAsync(Mock<IWebLoader> webLoaderMock, string hostUrl, Import.Settings.RequestOptions? hostRequestOptions = null, CancellationToken cancellationToken = default)
    {
        // Initially IsStarted is false; when Start() runs we make IsStarted return true.
        webLoaderMock.SetupGet(w => w.IsStarted).Returns(false);
        webLoaderMock.Setup(w => w.Start()).Returns(async () =>
        {
            webLoaderMock.SetupGet(x => x.IsStarted).Returns(true);
            await Task.CompletedTask;
            return true;
        });

        // Use the real rate limiter wrapper with mocked IWebLoader.
        var rateLimiter = new RatelimiterWebLoader(webLoaderMock.Object, new RateLimiterOptions { WindowMilliseconds = 100 });

        using var httpClient = new HttpClient(); // not used in this scenario but required by ctor
        var client = new BrowserServiceClient(httpClient, "test", "http://host", rateLimiter, hostRequestOptions: hostRequestOptions, browserDataLoader: null, browserDataLauncher: null, requestHeaders: null);

        // Act
        // Start the client which will add connection to pool and call underlying Start().
        await client.Start(CancellationToken.None);

        // Call Load with an IEnumerable containing empty cookies (method expects IEnumerable).
        return await client.LoadHostPage(hostUrl, cancellationToken);
    }
}