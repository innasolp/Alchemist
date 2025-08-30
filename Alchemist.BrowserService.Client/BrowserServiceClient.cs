using Alchemist.Import.Interfaces;
using BrowserDataLoader.Interfaces;
using System.Collections;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
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

    public async Task<IEnumerable<ICookieData>> LoadCookies(string host)
    {
        var response = await _httpClient.GetAsync($"browserdata/getCookies/{_browserDataLoader}/{host}");
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return await Task.FromResult(new List<ICookieData>());

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<CookieData>>();
    }

    public async Task UpdateData(string url)
    {
        var response = await _httpClient.PostAsJsonAsync($"browserdata/launch", new ArrayList() { _browserDataLauncher, url });
        response.EnsureSuccessStatusCode();       
    }    

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await _webLoader.DisposeAsync();
    }

    async Task<object> ILoaderService.GetData(string host)
    {
        return await LoadCookies(host);        
    }

    public async Task<Stream> Load(string url, object? data)
    {
        if (data is not IEnumerable<ICookieData> cookies)
            throw new InvalidOperationException($"Invalid type of {data}");

        var headers= HeadersHelper.GetHeadersForRequest(_requestHeaders, cookies);

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
            else
                throw new LoaderServiceException(e.Message, e);
        }
    }

    public async Task Reset()
    {
        await _webLoader.Reset(_host);
    }

    public async Task Start()
    {
        await _webLoader.Start();
    }

    public async Task Close()
    {
        await _webLoader.Close();
    }
}
