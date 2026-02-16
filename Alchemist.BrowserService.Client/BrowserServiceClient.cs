using BrowserDataLoader.Interfaces;
using Import.Interfaces;
using Import.LoaderSettings;
using System.Collections;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;
using ImportRequestOptions = Import.LoaderSettings.RequestOptions;
using WebLoaderRequestOptions = WebLoader.Interfaces.RequestOptions;

namespace Alchemist.BrowserService.Client;

public class BrowserServiceClient(HttpClient httpClient,
    string name,
    string host,
    IRateLimiterWebLoader ratelimiterWebLoader,
    ImportRequestOptions? hostRequestOptions = null,
    string? browserDataLoader = null,
    string? browserDataLauncher = null,
    RequestHeaders? requestHeaders = null) : ILoaderService
{
    private readonly HttpClient _httpClient = httpClient;

    private readonly string? _browserDataLoader = browserDataLoader;

    private readonly string? _browserDataLauncher = browserDataLauncher;

    private readonly ImportRequestOptions? _hostRequestOptions = hostRequestOptions;

    private readonly RequestHeaders? _requestHeaders = requestHeaders;

    private readonly string _host = host;

    private readonly IRateLimiterWebLoader _rateLimiterWebLoader = ratelimiterWebLoader;

    public string Name { get; } = name;

    private readonly Guid _connectionId = Guid.NewGuid();

    bool ILoaderService.IsStarted => _rateLimiterWebLoader.IsStarted(_connectionId);

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
        await _rateLimiterWebLoader.DisposeAsync();
    }

    async Task<object> ILoaderService.GetData(string host, CancellationToken token)
    {
        if (!string.IsNullOrWhiteSpace(_browserDataLoader))
            return await LoadCookies(host, token);

        if (_hostRequestOptions != null)
            await LoadHostPage(host, cancellationToken: token);

        return Array.Empty<ICookieData>();
    }

    public async Task<Stream> Load(string url, object? data, CancellationToken cancellationToken = default)
    {
        IEnumerable<ICookieData>? cookies = default;
        IEnumerable<string>? parameters = default;
        ImportRequestOptions? requestOptions = default;
        if (data is IEnumerable dataValues)
        {
            requestOptions = dataValues.OfType<ImportRequestOptions>().FirstOrDefault();

            parameters = dataValues.OfType<IEnumerable<string>>().FirstOrDefault();

            cookies = dataValues.OfType<IEnumerable<ICookieData>>().FirstOrDefault();
            if (cookies is null && dataValues is IEnumerable<ICookieData> cookieValues)
                cookies = cookieValues;
        }
        else if(data is not null)
            throw new InvalidOperationException($"Invalid type of {data}");

        var headers = _requestHeaders != null && cookies?.Any() == true
            ? HeadersHelper.GetHeadersForRequest(_requestHeaders, cookies)
            : [];

        try
        {
            if (requestOptions?.LoadingType == LoadingType.WaitForUrl)
                return await WaitForUrlAsync(url, requestOptions.RouteUrlFormat ?? url, parameters, requestOptions?.TimeouteMillseconds, headers, cancellationToken);

            if (!string.IsNullOrEmpty(requestOptions?.RouteUrlFormat) &&
                requestOptions?.LoadingType != LoadingType.Simple && requestOptions?.LoadingType != LoadingType.Api)
                return await LoadFromRouteUrl(url, requestOptions.RouteUrlFormat, requestOptions?.LoadingType, parameters, requestOptions?.TimeouteMillseconds, headers, cancellationToken);

            if (requestOptions?.LoadingType == LoadingType.Api)
                return await LoadFromApi(url, requestOptions, parameters, headers, cancellationToken);

            return await LoadFromUrl(url, timeoutInMilliseconds: requestOptions?.TimeouteMillseconds, headers: headers, cancellationToken: cancellationToken);
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
#if DEBUG
        catch(Exception e)
        {
            throw;
        }
#endif 
    }

    private async Task<Stream> LoadFromApi(string url,
        ImportRequestOptions? requestOptions, 
        IEnumerable<string>? parameters, 
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        var httpMethod = !string.IsNullOrEmpty(requestOptions?.HttpMethod) ? new HttpMethod(requestOptions.HttpMethod) : HttpMethod.Get;

        object? requestData = null;
        if (requestOptions?.Data is not null)
        {
            var dataFormat = requestOptions.Data.ToString();
            if (dataFormat?.StartsWith("{") == true && IsValidJson(dataFormat)) dataFormat = $"{{{dataFormat}}}";
            requestData = parameters?.Any() == true ? string.Format(dataFormat, args: [.. parameters]) : requestOptions?.Data;
        }

        return await _rateLimiterWebLoader.ExecuteAsync(_connectionId,
                (webLoader, cancellationToken) =>
                webLoader.LoadFromApiUrl(url, httpMethod, requestData,
            new WebLoaderRequestOptions { Headers = headers, TimeoutInMilliseconds = requestOptions?.TimeouteMillseconds }),
                cancellationToken);
    }

    private static bool IsValidJson(string jsonString)
    {
        try
        {
            // Attempt to parse the string into a JsonDocument
            // or a specific object type.
            JsonDocument.Parse(jsonString);
            return true;
        }
        catch (JsonException)
        {
            // If parsing fails, it's not a valid JSON string
            return false;
        }
    }

    private async Task<Stream> LoadFromRouteUrl(string url,
        string routeUrlFormat,
        LoadingType? loadingType = null,
        IEnumerable<string>? parameters = null,
        int? timeoutInMilliseconds = null,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        var routeUrl = parameters?.Any() == true
            ? string.Format(routeUrlFormat, [.. parameters])
            : routeUrlFormat;

        var (success, result) = await _rateLimiterWebLoader.ExecuteAsync(_connectionId,
                (webLoader, cancellationToken) =>
                webLoader.TryLoadFromRoute(url, (url)=>url.Contains(routeUrl, StringComparison.InvariantCultureIgnoreCase),
                    new WebLoaderRequestOptions
                    {
                        Headers = headers,
                        TimeoutInMilliseconds = timeoutInMilliseconds,
                        Parameters = new Dictionary<string, object>()
                        {
                            { "RouteType", loadingType?.ToString() ?? LoadingType.Route.ToString() }
                        }
                    }
                    , cancellationToken: cancellationToken), 
                cancellationToken);

        if (!success)
        {
            if (result is null)
                throw new LoaderServiceException($"Route {routeUrl} on page {url} not found");
            else
            {
                using var streamReader = new StreamReader(result);
                var message = await streamReader.ReadToEndAsync(cancellationToken);
                streamReader.Close();
                throw new LoaderServiceException($"Route {routeUrl} on page {url} failed. {message}");
            }
        }

        return result;
    }

    private async Task<Stream> LoadFromUrl(string url,
        int? timeoutInMilliseconds = null,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        return  await _rateLimiterWebLoader.ExecuteAsync(_connectionId,
                (webLoader, cancellationToken) =>
                webLoader.LoadFromUrl(url,
                    new WebLoaderRequestOptions
                    {
                        Headers = headers,
                        TimeoutInMilliseconds = timeoutInMilliseconds,
                        Parameters = new Dictionary<string, object>() { { "RouteUrl", url } }
                    }),
                cancellationToken);
    }

    private async Task<Stream> WaitForUrlAsync(string url,
        string routeUrlFormat,
        IEnumerable<string>? parameters = null,
        int? timeoutInMilliseconds = null,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        var routeUrl = parameters?.Any() == true
            ? string.Format(routeUrlFormat, [.. parameters])
            : routeUrlFormat;

        var result = await _rateLimiterWebLoader.ExecuteAsync(_connectionId,
                (webLoader, cancellationToken) =>
                webLoader.WaitForUrl(url, (url)=> url.Contains(routeUrl, StringComparison.InvariantCultureIgnoreCase),
                    new WebLoaderRequestOptions
                    {
                        Headers = headers,
                        TimeoutInMilliseconds = timeoutInMilliseconds
                    }),
                cancellationToken);

        return result is not null ? result : throw new LoaderServiceException($"Route {routeUrl} on page {url} not found");
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
        await _rateLimiterWebLoader.Reset(_host);

        if (!string.IsNullOrEmpty(_browserDataLoader))
            await ClearCookiesForHost(_host, token);
    }

    public async Task Start(CancellationToken token = default)
    {
        if (!_rateLimiterWebLoader.IsConnected(_connectionId))
            await _rateLimiterWebLoader.AddToPool(_connectionId, token);

        if (!await _rateLimiterWebLoader.Start(_connectionId, token))
            throw new InvalidOperationException($"Failed to start web loader for connection {_connectionId}");
    }

    public async Task Close(CancellationToken token = default)
    {
        await _rateLimiterWebLoader.Close(_connectionId, token);
    }

    async Task ILoaderService.UpdateData(string url, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_browserDataLauncher))
            await LaunchBrowser(url, cancellationToken);
        else
            await LoadHostPage(url, cancellationToken);
    }

    public async Task<Stream> LoadHostPage(string url, CancellationToken cancellationToken = default)
    {
        if (_hostRequestOptions is null || string.IsNullOrEmpty(_hostRequestOptions?.RouteUrlFormat)
            || _hostRequestOptions.LoadingType == LoadingType.Simple)
            return await LoadFromUrl(url, timeoutInMilliseconds: _hostRequestOptions?.TimeouteMillseconds, cancellationToken: cancellationToken);            

        if (_hostRequestOptions.LoadingType == LoadingType.WaitForUrl)
            return await WaitForUrlAsync(url, _hostRequestOptions.RouteUrlFormat, null, _hostRequestOptions.TimeouteMillseconds,
                cancellationToken: cancellationToken);

        return await LoadFromRouteUrl(url, _hostRequestOptions.RouteUrlFormat,
            _hostRequestOptions.LoadingType,
            timeoutInMilliseconds: _hostRequestOptions.TimeouteMillseconds,
            cancellationToken: cancellationToken);
    }
}