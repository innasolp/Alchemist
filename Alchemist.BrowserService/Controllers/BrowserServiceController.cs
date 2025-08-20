using BrowserDataLoader.Interfaces;
using BrowserLauncher.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Collections;

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
            return TypedResults.BadRequest("browser is empty");

        if (string.IsNullOrEmpty(host))
            return TypedResults.BadRequest("host is empty");

        var browserDataLoader = GetBrowserDataLoader(browser);
        if (browserDataLoader == null)
            return TypedResults.NotFound(browser);

        try
        {
            await _semaphoreSlim.WaitAsync();

            var cookies = await browserDataLoader.LoadCookies(host);
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
        LaunchBrowser(ArrayList data)
    {
        if (data == null || data.Count == 0)
            return TypedResults.BadRequest("empty parameters list");

        if (data.Count < 2)
            return TypedResults.BadRequest("no second parameter");

        var browser = data[0]?.ToString();
        if (string.IsNullOrEmpty(browser))
            return TypedResults.BadRequest("browser is empty");

        var url = data[1]?.ToString();
        if (string.IsNullOrEmpty(url))
            return TypedResults.BadRequest("url is empty");

        var browserLauncher = GetBrowserLauncher(browser);
        if (browserLauncher == null)
            return TypedResults.NotFound(browser);

        try
        {
            await _semaphoreSlim.WaitAsync();

            var handle = await browserLauncher.OpenUrl(url);

            await Task.Delay(500);

            await browserLauncher.Close(handle);

            await Task.Delay(1000);

            return TypedResults.Ok();
        }
        finally 
        {
            _semaphoreSlim.Release();
        }    
    }
}
