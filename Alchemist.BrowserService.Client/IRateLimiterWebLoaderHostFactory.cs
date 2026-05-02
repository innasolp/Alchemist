using Import.LoaderSettings;
using Import.Settings.Interfaces;

namespace Alchemist.BrowserService.Client;

public interface IRateLimiterWebLoaderHostFactory
{
    IRateLimiterWebLoader GetRateLimiterWebLoader(IServiceSettings webLoaderSettings, string host, RateLimiterOptions? rateLimiterOptions = null);
}