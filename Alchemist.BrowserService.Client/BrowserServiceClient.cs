using Alchemist.Import.Interfaces;
using BrowserDataLoader.Interfaces;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;
using WebLoader.Common;
using WebLoader.Interfaces;

namespace Alchemist.BrowserService.Client;

internal class BrowserServiceClient : ILoaderService
{
    private readonly HttpClient _httpClient;

    private readonly string _browserDataLoader;

    private readonly string _browserDataLauncher;

    private readonly IWebLoader _webLoader;

    private readonly RequestHeaders? _requestHeaders;

    private readonly string _host;

    public BrowserServiceClient(IHttpClientFactory httpClientFactory, string name, string apiHost, string host, IWebLoader webLoader, 
        string browserDataLoader, string browserDataLauncher, JsonObject requestHeaders)
    {
        _httpClient = httpClientFactory.CreateClient(apiHost);
        _httpClient.BaseAddress = new Uri(apiHost);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        _requestHeaders = JsonSerializer.Deserialize<RequestHeaders>(requestHeaders.ToString());

        Name = name;
        _host = host;
        _webLoader = webLoader;
        _browserDataLoader = browserDataLoader;
        _browserDataLauncher = browserDataLauncher;
    }

    public string Name { get; }

    bool ILoaderService.IsStarted => _webLoader?.IsStarted ?? false;

    public async Task<IEnumerable<ICookieData>> LoadCookies(string host, CancellationToken token = default)
    {
        var response = await _httpClient.GetAsync($"browserdata/getCookies/{_browserDataLoader}/{host}", token);
        
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

    async Task<object> ILoaderService.GetData(string host, CancellationToken token = default)
    {
        return await LoadCookies(host, token);        
    }

    public async Task<Stream> Load(string url, object? data, CancellationToken token = default)
    {
        if (data is not IEnumerable<ICookieData> cookies)
            throw new InvalidOperationException($"Invalid type of {data}");

        var headers = HeadersHelper.GetHeadersForRequest(_requestHeaders, cookies);

        try
        {
            return await _webLoader.LoadFromUrl(url, headers);
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
            if(e.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                throw new LoaderServiceException(e.Message, e, LoaderServiceAction.Wait);
            else if(e.StatusCode == System.Net.HttpStatusCode.Forbidden)
                throw new LoaderServiceException(e.Message, e, LoaderServiceAction.Wait);
            else
                throw new LoaderServiceException(e.Message, e);
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
