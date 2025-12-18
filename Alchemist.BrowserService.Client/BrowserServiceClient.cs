using Alchemist.Import.Settings;
using BrowserDataLoader.Interfaces;
using Import.Interfaces;
using System.Collections;
using System.Net.Http.Json;
using System.Web;
using WebLoader.Interfaces;

namespace Alchemist.BrowserService.Client;

internal class BrowserServiceClient(HttpClient httpClient, string name, string host, IWebLoader webLoader,
    string? browserDataLoader = null, string? browserDataLauncher = null, RequestHeaders? requestHeaders = null) : ILoaderService
{
    private readonly HttpClient _httpClient = httpClient;

    private readonly string? _browserDataLoader = browserDataLoader;

    private readonly string? _browserDataLauncher = browserDataLauncher;

    private readonly IWebLoader _webLoader = webLoader;

    private readonly RequestHeaders? _requestHeaders = requestHeaders;

    private readonly string _host = host;

    public string Name { get; } = name;

    bool ILoaderService.IsStarted => _webLoader?.IsStarted ?? false;

    public async Task<IEnumerable<ICookieData>> LoadCookies(string host, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(_browserDataLoader))
            return [];

        var hostPath = HttpUtility.UrlEncode(host);
        var response = await _httpClient.GetAsync($"browserdata/getCookies/{_browserDataLoader}/{hostPath}", token);
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return await Task.FromResult(new List<ICookieData>());

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<CookieData>>(token);
    }

    public async Task UpdateBrowserData(string url, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(_browserDataLauncher))
            return;

        var encodedBrowserLauncher = HttpUtility.UrlEncode(_browserDataLauncher);
        var encodedHost = HttpUtility.UrlEncode(url);
        var requestUrl = $"browserdata/launch?browser={encodedBrowserLauncher}&url={encodedHost}";

        var response = await _httpClient.PostAsync(requestUrl, null, token);
        response.EnsureSuccessStatusCode();       
    }    

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await _webLoader.DisposeAsync();
    }

    async Task<object> ILoaderService.GetData(string host, CancellationToken token)
    {
        return await LoadCookies(host, token);        
    }    

    public async Task<Stream> Load(string url, object? data, CancellationToken cancellationToken = default)
    {
        IEnumerable<ICookieData>? cookies;
        RequestOptions? requestOptions = null;
        IEnumerable<string>? parameters = [];
        if (data is IEnumerable dataValues)
        {
            if (dataValues.OfType<RequestOptions>().Any())
                requestOptions = dataValues.OfType<RequestOptions>().First();

            if (dataValues.OfType<IEnumerable<string>>().Any())
                parameters = dataValues.OfType<IEnumerable<string>>().First();

            cookies = dataValues.OfType<IEnumerable<ICookieData>>().FirstOrDefault();
            if (cookies is null && dataValues is IEnumerable<ICookieData> cookieValues)
                cookies = cookieValues;
        }
        else
            throw new InvalidOperationException($"Invalid type of {data}");

        var headers = _requestHeaders!= null && cookies?.Any() == true 
            ? HeadersHelper.GetHeadersForRequest(_requestHeaders, cookies) 
            : [];

        try
        {
            if (!string.IsNullOrEmpty(requestOptions?.ApiUrlFormat))            
                return await LoadFromRouteUrl(url, requestOptions, parameters, headers, cancellationToken);            

            var httpMethod = !string.IsNullOrEmpty(requestOptions?.HttpMethod) ? new HttpMethod(requestOptions.HttpMethod) : HttpMethod.Get;

            string? requestData = null;
            if (requestOptions?.Data is not null)
            {
                var dataFormat = requestOptions.Data.ToString();
                if(dataFormat.StartsWith("{"))  dataFormat = $"{{{dataFormat}}}";
                requestData = parameters?.Any() == true ? string.Format(dataFormat, args: [.. parameters]) : dataFormat;
            }

            return await _webLoader.LoadFromUrl(url, headers, httpMethod, requestData);
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

    private async Task<Stream> LoadFromRouteUrl(string url, RequestOptions? requestOptions, IEnumerable<string> parameters, Dictionary<string, string> headers, CancellationToken cancellationToken)
    {
        var apiUrl = parameters?.Any() == true ? string.Format(requestOptions.ApiUrlFormat, [.. parameters]) : requestOptions.ApiUrlFormat;
        var (success, result) = await _webLoader.TryLoadFromRoute(url, (routeUrl) => routeUrl.Contains(apiUrl),
            headers, cancellationToken: cancellationToken);

        if (!success)
        {
            if (result is null)
                throw new LoaderServiceException($"Route {apiUrl} on page {url} not found");
            else
            {
                var streamReader = new StreamReader(result);
                var message = streamReader.ReadToEnd();
                streamReader.Close();
                throw new LoaderServiceException($"Route {apiUrl} on page {url} failed. {message}");
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
        await UpdateBrowserData(url, cancellationToken);
    }
}