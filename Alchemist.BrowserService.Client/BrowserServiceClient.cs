using Alchemist.Import.Settings;
using BrowserDataLoader.Interfaces;
using Import.Interfaces;
using System.Collections;
using System.Net.Http.Json;
using System.Web;
using WebLoader.Interfaces;
using HostRequestOptions = Alchemist.Import.Settings.RequestOptions;
using WebLoaderRequestOptions = WebLoader.Interfaces.RequestOptions;

namespace Alchemist.BrowserService.Client;

internal class BrowserServiceClient(HttpClient httpClient, 
    string name,
    string host,
    IWebLoader webLoader,
    HostRequestOptions? hostRequestOptions = null,
    string? browserDataLoader = null, 
    string? browserDataLauncher = null,
    RequestHeaders? requestHeaders = null) : ILoaderService
{
    private readonly HttpClient _httpClient = httpClient;

    private readonly string? _browserDataLoader = browserDataLoader;

    private readonly string? _browserDataLauncher = browserDataLauncher;

    private readonly IWebLoader _webLoader = webLoader;

    private readonly HostRequestOptions? _hostRequestOptions = hostRequestOptions;

    private readonly RequestHeaders? _requestHeaders = requestHeaders;

    private readonly string _host = host;

    public string Name { get; } = name;

    bool ILoaderService.IsStarted => _webLoader?.IsStarted ?? false;

    public async Task<IEnumerable<ICookieData>> LoadCookies(string host, CancellationToken token = default)
    {
        var hostPath = HttpUtility.UrlEncode(host);
        var response = await _httpClient.GetAsync($"browserdata/getCookies/{_browserDataLoader}/{hostPath}", token);
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return await Task.FromResult(new List<ICookieData>());

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<CookieData>>(token);
    }

    public async Task LaunchBrowser(string url, CancellationToken token = default)
    {   
        var encodedBrowserLauncher = HttpUtility.UrlEncode(_browserDataLauncher);
        var encodedHost = HttpUtility.UrlEncode(url);
        var requestUrl = $"browserdata/launch?browser={encodedBrowserLauncher}&url={encodedHost}";

        var response = await _httpClient.PostAsync(requestUrl, null, token);
        response.EnsureSuccessStatusCode();       
    }    

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        _httpClient.Dispose();
        await _webLoader.DisposeAsync();
    }

    async Task<object> ILoaderService.GetData(string host, CancellationToken token)
    {
        if (!string.IsNullOrWhiteSpace(_browserDataLoader))
            return await LoadCookies(host, token);

        await LoadHostPage(host);
        return Array.Empty<ICookieData>();
    }    

    public async Task<Stream> Load(string url, object? data, CancellationToken cancellationToken = default)
    {
        IEnumerable<ICookieData>? cookies;
        IEnumerable<string>? parameters;
        ImportRequestOptions? requestOptions;
        if (data is IEnumerable dataValues)
        {
            requestOptions = dataValues.OfType<ImportRequestOptions>().FirstOrDefault();

            parameters = dataValues.OfType<IEnumerable<string>>().FirstOrDefault();

            cookies = dataValues.OfType<IEnumerable<ICookieData>>().FirstOrDefault();
            if (cookies is null && dataValues is IEnumerable<ICookieData> cookieValues)
                cookies = cookieValues;
        }
        else
            throw new InvalidOperationException($"Invalid type of {data}");

        var headers = _requestHeaders != null && cookies?.Any() == true 
            ? HeadersHelper.GetHeadersForRequest(_requestHeaders, cookies) 
            : [];

        try
        {
            if (!string.IsNullOrEmpty(requestOptions?.RouteUrlFormat))
                return await LoadFromRouteUrl(url, requestOptions.RouteUrlFormat, parameters, requestOptions.TimeouteMillseconds, headers, cancellationToken);

            if (requestOptions?.IsApi == false)
                return await _webLoader.LoadFromUrl(url, new WebLoaderRequestOptions { Headers = headers, TimeoutInMilliseconds = requestOptions.TimeouteMillseconds });
            
            return await LoadFromApi(url, requestOptions, parameters, headers);
        }
        catch (WebLoader.Common.WebLoaderException e)
        {
            if (e.NsError == WebLoader.Common.NsError.NS_ERROR_REDIRECT_LOOP)
                throw new LoaderServiceException(e.Message, e, LoaderServiceAction.Reset);
            else
                throw new LoaderServiceException(e.Message, e);
        }
        catch (HttpRequestException e)
        {
            var message = $"Request error {e.HttpRequestError}, status code {e.StatusCode}. {e.Message}";
            if (e.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                throw new LoaderServiceException(message, e, LoaderServiceAction.Wait);
            else if (e.StatusCode == System.Net.HttpStatusCode.Forbidden)
                throw new LoaderServiceException(message, e, LoaderServiceAction.Wait);
            else
                throw new LoaderServiceException(message, e);
        }
    }

    private async Task<Stream> LoadFromApi(string url, ImportRequestOptions? requestOptions, IEnumerable<string> parameters, Dictionary<string, string>? headers = null)
    {
        var httpMethod = !string.IsNullOrEmpty(requestOptions?.HttpMethod) ? new HttpMethod(requestOptions.HttpMethod) : HttpMethod.Get;

        string? requestData = null;
        if (requestOptions?.Data is not null)
        {
            var dataFormat = requestOptions.Data.ToString();
            if (dataFormat.StartsWith("{")) dataFormat = $"{{{dataFormat}}}";
            requestData = parameters?.Any() == true ? string.Format(dataFormat, args: [.. parameters]) : dataFormat;
        }

        return await _webLoader.LoadFromApiUrl(url, httpMethod, requestData,
            new WebLoaderRequestOptions { Headers = headers, TimeoutInMilliseconds = requestOptions?.TimeouteMillseconds });
    }

    private async Task<Stream> LoadFromRouteUrl(string url,
        string routeUrlFormat,
        IEnumerable<string>? parameters = null, 
        int? timeoutInMilliseconds = null,
        Dictionary<string, string>? headers = null, 
        CancellationToken cancellationToken = default)
    {
        var routeUrl = parameters?.Any() == true 
            ? string.Format(routeUrlFormat, [.. parameters])
            : routeUrlFormat;

        var (success, result) = await _webLoader.TryLoadFromRoute(url, routeUrl,
            new WebLoaderRequestOptions { Headers = headers, TimeoutInMilliseconds = timeoutInMilliseconds }
            , cancellationToken: cancellationToken);

        if (!success)
        {
            if (result is null)
                throw new LoaderServiceException($"Route {routeUrl} on page {url} not found");
            else
            {
                var streamReader = new StreamReader(result);
                var message = streamReader.ReadToEnd();
                streamReader.Close();
                throw new LoaderServiceException($"Route {routeUrl} on page {url} failed. {message}");
            }
        }

        return result;
    }

    private async Task<int> ClearCookiesForHost(string host, CancellationToken token = default)
    {
        var encodedBrowserDataLoader = HttpUtility.UrlEncode(_browserDataLoader);
        var encodedHost = HttpUtility.UrlEncode(host);
        var url = $"browserdata/clearCookies?browser={encodedBrowserDataLoader}&host={encodedHost}";

        var response = await _httpClient.PostAsync(url, null, token);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync(token);
        return int.TryParse(result, out var deleted) ? deleted : 0;    
    }

    public async Task Reset(CancellationToken token = default)
    {
        await _webLoader.Reset(_host);

        if(!string.IsNullOrEmpty(_browserDataLoader))
            await ClearCookiesForHost(_host, token);
    }

    public async Task Start(CancellationToken token = default)
    {
        await _webLoader.Start();
    }

    public async Task Close(CancellationToken token = default)
    {
        await _webLoader.Close();
    }

    async Task ILoaderService.UpdateData(string url, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_browserDataLauncher))            
            await LaunchBrowser(url, cancellationToken);
        else
            await LoadHostPage(url);
    }

    public async Task<Stream> LoadHostPage(string url)
    {
        return _hostRequestOptions?.RouteUrlFormat is null
            ? await _webLoader.LoadFromUrl(url, new WebLoaderRequestOptions { TimeoutInMilliseconds = _hostRequestOptions?.TimeouteMillseconds })
            : await _webLoader.WaitForUrl(url,
            _hostRequestOptions.RouteUrlFormat,
            new WebLoaderRequestOptions
            {
                TimeoutInMilliseconds = _hostRequestOptions.TimeouteMillseconds,
                Parameters = _hostRequestOptions.Parameters?.TryGetValue("RouteType", out var routeType) == true
                   ? new Dictionary<string, object>() { { "RouteType", routeType } }
                   : default
            });
    }
}