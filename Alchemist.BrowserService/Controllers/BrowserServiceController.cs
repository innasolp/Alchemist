using BrowserDataLoader.Interfaces;
using BrowserLauncher.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace Alchemist.BrowserService.Controllers;

[ApiController]
[Route("browserdata")]
public class BrowserServiceController(ILogger<BrowserServiceController> logger,
    IEnumerable<IBrowserDataLoader> browserDataLoaders,
    IEnumerable<IBrowserLauncher> browserLaunchers) : ControllerBase
{

    private readonly ILogger<BrowserServiceController> _logger = logger;

    private readonly IEnumerable<IBrowserDataLoader> _browserDataLoaders = browserDataLoaders;

    private readonly IEnumerable<IBrowserLauncher> _browserLaunchers = browserLaunchers;

    private readonly Dictionary<string, IBrowserDataLoader> _browserDataLoadersByName = [];

    private readonly Dictionary<string, IBrowserLauncher> _browserLaunchersByName = [];

    private readonly SemaphoreSlim _semaphoreSlim = new(1);

    private readonly SemaphoreSlim _launcherSemaphoreSlim = new(1);

    private IBrowserDataLoader? GetBrowserDataLoader(string browser)
    {
        if (!_browserDataLoadersByName.TryGetValue(browser, out var dataLoader))
        {
            dataLoader = _browserDataLoaders.FirstOrDefault(f => f.GetType().Name == browser
            || f.GetType().Name.Contains(browser, StringComparison.InvariantCultureIgnoreCase));

            if (dataLoader != null)
                _browserDataLoadersByName.Add(browser, dataLoader);
        }

        return dataLoader;
    }

    private IBrowserLauncher? GetBrowserLauncher(string browser)
    {
        if (!_browserLaunchersByName.TryGetValue(browser, out var launcher))
        {
            launcher = _browserLaunchers.FirstOrDefault(f => f.GetType().Name == browser
            || f.GetType().Name.Contains(browser, StringComparison.InvariantCultureIgnoreCase));

            if (launcher != null)
                _browserLaunchersByName.Add(browser, launcher);
        }

        return launcher;
    }

    [HttpGet("getCookies/{browser}/{host}", Name = nameof(GetCookies))]
    public async Task<Results<BadRequest<string>,
        NotFound<string>,
        NotFound,
        Ok<IEnumerable<ICookieData>>>>
        GetCookies(string browser, string host)
    {
        if (string.IsNullOrEmpty(browser))
            return TypedResults.BadRequest($"{nameof(browser)} is empty");

        if (string.IsNullOrEmpty(host))
            return TypedResults.BadRequest($"{nameof(host)} is empty");

        var browserDataLoader = GetBrowserDataLoader(browser);
        if (browserDataLoader == null)
            return TypedResults.NotFound(browser);

        var decodedHost = HttpUtility.UrlDecode(host);

        try
        {
            await _semaphoreSlim.WaitAsync();

            var cookies = await browserDataLoader.LoadCookies(decodedHost);
            return cookies.Any()
                ? TypedResults.Ok(cookies.Select(c => c.Convert()))
                : TypedResults.NotFound();
        }
        finally
        {
            _semaphoreSlim.Release();           
        }
    }


    [HttpPost("launch", Name = nameof(LaunchBrowser))]
    public async Task<Results<BadRequest<string>,
        NotFound<string>,
        Ok>>
        LaunchBrowser([FromQuery] string browser, [FromQuery] string url, [FromQuery]int? milliSeconds = null)
    {       

        if (string.IsNullOrEmpty(browser))
            return TypedResults.BadRequest($"{nameof(browser)} is empty");

        if (string.IsNullOrEmpty(url))
            return TypedResults.BadRequest($"{nameof(url)} is empty");

        if (milliSeconds < 0)
            return TypedResults.BadRequest($"{nameof(milliSeconds)} is negative");

        var browserLauncher = GetBrowserLauncher(browser);
        if (browserLauncher == null)
            return TypedResults.NotFound(browser);

        try
        {
            await _launcherSemaphoreSlim.WaitAsync();

            var handle = await browserLauncher.OpenUrl(url);

            await Task.Delay(milliSeconds ?? 1000);

            var errorCode = await browserLauncher.Close(handle);

            if (errorCode != 0)
                throw new InvalidOperationException($"error code = {errorCode}");

            await Task.Delay(1000);

            return TypedResults.Ok();
        }
        finally 
        {
            _launcherSemaphoreSlim.Release();
        }    
    }

    [HttpPost("clearCookies", Name = nameof(ClearCookiesForHost))]
    public async Task<Results<BadRequest<string>,
        NotFound<string>,
        NotFound,
        Ok<int>>>
        ClearCookiesForHost([FromQuery] string browser, [FromQuery] string host)
    {
        if (string.IsNullOrEmpty(browser))
            return TypedResults.BadRequest($"{nameof(browser)} is empty");

        if (string.IsNullOrEmpty(host))
            return TypedResults.BadRequest($"{nameof(host)} is empty");

        var browserDataLoader = GetBrowserDataLoader(browser);
        if (browserDataLoader == null)
            return TypedResults.NotFound(browser);

        try
        {
            await _semaphoreSlim.WaitAsync();

            var deleted = await browserDataLoader.ClearCookiesForHost(host);
            return TypedResults.Ok(deleted);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}
