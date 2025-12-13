using BrowserDataLoader.Interfaces;
using Import.Interfaces;
using System.Collections;
using System.Net.Http.Json;
using System.Web;
using WebLoader.Common;
using WebLoader.Interfaces;

namespace Alchemist.BrowserService.Client;

internal class BrowserServiceClient(HttpClient httpClient, string name, string host, IWebLoader webLoader,
    string browserDataLoader, string browserDataLauncher, RequestHeaders requestHeaders) : ILoaderService
{
    private readonly HttpClient _httpClient = httpClient;

    private readonly string _browserDataLoader = browserDataLoader;

    private readonly string _browserDataLauncher = browserDataLauncher;

    private readonly IWebLoader _webLoader = webLoader;

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

    public async Task UpdateData(string url, CancellationToken token = default)
    {
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

    public async Task<Stream> Load(string url, object? data, CancellationToken token = default)
    {
        var httpMethod = HttpMethod.Get;
        string? requestData = null;
        IEnumerable<ICookieData>? cookies;
        if (data is IEnumerable dataValues)
        {
            if (dataValues.OfType<string>().Any())
            {
                httpMethod = new HttpMethod(dataValues.OfType<string>().First());
                requestData = dataValues.OfType<string>().Last();
            }

            cookies = dataValues.OfType<IEnumerable<ICookieData>>().FirstOrDefault();

            if (cookies is null && dataValues is IEnumerable<ICookieData> cookieValues)
                cookies = cookieValues;
        }  
        else
            throw new InvalidOperationException($"Invalid type of {data}");

        var headers = HeadersHelper.GetHeadersForRequest(_requestHeaders, cookies);

        try
        {
            //todo add httpmethod
            return await _webLoader.LoadFromUrl(url, headers, httpMethod, requestData);
        }
        catch(WebLoaderException e)
        {
            if (e.NsError == NsError.NS_ERROR_REDIRECT_LOOP)
                throw new LoaderServiceException(e.Message, e, LoaderServiceAction.Reset);
            else 
                throw new LoaderServiceException(e.Message, e);
        }
        catch(HttpRequestException e)
        {
            var message = $"Request error {e.HttpRequestError}, status code {e.StatusCode}. {e.Message}";
            if(e.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                throw new LoaderServiceException(message, e, LoaderServiceAction.Wait);
            else if(e.StatusCode == System.Net.HttpStatusCode.Forbidden)
                throw new LoaderServiceException(message, e, LoaderServiceAction.Wait);
            else
                throw new LoaderServiceException(message, e);
        }
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
}
