using Import.LoaderSettings;
using Import.Settings.Interfaces;
using System.Collections.Concurrent;
using WebLoader.Interfaces;

namespace Alchemist.BrowserService.Client;

internal class RateLimiterWebLoaderHostFactory(IEnumerable<IWebLoaderFactory> webLoaderFactories) : IRateLimiterWebLoaderHostFactory
{
    private readonly IEnumerable<IWebLoaderFactory> _webLoaderFactories = webLoaderFactories;

    private readonly ConcurrentDictionary<(string, IWebLoaderFactory), IRateLimiterWebLoader> _hostWebLoaders = [];

    public IRateLimiterWebLoader GetRateLimiterWebLoader(IServiceSettings webLoaderSettings, string host, RateLimiterOptions? rateLimiterOptions = null)
    {
        var webLoaderFactory = _webLoaderFactories.FirstOrDefault(f => f.GetType().Name == webLoaderSettings.ImplementationTypeName
            || f.GetType().Name.Contains(webLoaderSettings.ImplementationTypeName, StringComparison.InvariantCultureIgnoreCase))
            ?? throw new InvalidDataException($"Web loader factory for type {webLoaderSettings.ImplementationTypeName} not found");

        if (!_hostWebLoaders.TryGetValue((host, webLoaderFactory), out var ratelimiterWebLoader) || ratelimiterWebLoader == null)
        {
            var webloader = webLoaderFactory.CreateWebLoader();
            ratelimiterWebLoader = new RatelimiterWebLoader(webloader, rateLimiterOptions);
            _hostWebLoaders.TryAdd((host, webLoaderFactory), ratelimiterWebLoader);
        }

        return ratelimiterWebLoader;
    }
}