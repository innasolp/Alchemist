using Alchemist.Import.Settings;
using Moq;
using WebLoader.Interfaces;

namespace Alchemist.BrowserService.Client.UnitTest;

internal static class Helper
{
    public static async Task<BrowserServiceClient> CreateAsync(Mock<IWebLoader> webLoaderMock,
        Import.Settings.RequestOptions? hostRequestOptions = null,
        RateLimiterOptions? rateLimiterOptions = null)
    {
        webLoaderMock.SetupGet(w => w.IsStarted).Returns(false);
        webLoaderMock.Setup(w => w.Start()).Returns(async () =>
        {
            webLoaderMock.SetupGet(x => x.IsStarted).Returns(true);
            await Task.CompletedTask;
            return true;
        });

        var rateLimiter = new RatelimiterWebLoader(webLoaderMock.Object,
            new RateLimiterOptions { WindowMilliseconds = rateLimiterOptions?.WindowMilliseconds ?? 100 });

        using var httpClient = new HttpClient(); // not used in this scenario but required by ctor
        return new BrowserServiceClient(httpClient, "test", "http://host", rateLimiter, hostRequestOptions: hostRequestOptions, browserDataLoader: null, browserDataLauncher: null, requestHeaders: null);
    }

    public static async Task<Stream> LoadAsync(Mock<IWebLoader> webLoaderMock, string url, object? data = null,
        Import.Settings.RequestOptions? hostRequestOptions = null,
        RateLimiterOptions? rateLimiterOptions = null,
        CancellationToken cancellationToken = default)
    {
        var client = await CreateAsync(webLoaderMock, hostRequestOptions, rateLimiterOptions);

        await client.Start(CancellationToken.None);

        return await client.Load(url, data, cancellationToken);
    }    

    public static async Task<Stream> LoadHostAsync(Mock<IWebLoader> webLoaderMock, string hostUrl, 
        Import.Settings.RequestOptions? hostRequestOptions = null,
        RateLimiterOptions? rateLimiterOptions = null,
        CancellationToken cancellationToken = default)
    {
        var client = await CreateAsync(webLoaderMock, hostRequestOptions, rateLimiterOptions);

        await client.Start(CancellationToken.None);

        return await client.LoadHostPage(hostUrl, cancellationToken);
    }
}