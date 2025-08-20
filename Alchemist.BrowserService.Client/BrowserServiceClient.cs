using Alchemist.Import.Interfaces;
using System.Collections;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Alchemist.BrowserService.Client;

internal class BrowserServiceClient : IBrowserService
{
    private readonly HttpClient _httpClient;

    private readonly string _browserDataLoader;

    private readonly string _browserDataLauncher;

    public BrowserServiceClient(IHttpClientFactory httpClientFactory, string apiHost, string browserDataLoader, string browserDataLauncher)
    {
        _httpClient = httpClientFactory.CreateClient(apiHost);
        _httpClient.BaseAddress = new Uri(apiHost);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        _browserDataLoader = browserDataLoader;
        _browserDataLauncher = browserDataLauncher;
    }

    public async Task<IEnumerable<ICookieData>> LoadCookies(string host)
    {
        var response = await _httpClient.GetAsync($"browserdata/getCookies/{_browserDataLoader}/{host}");
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return await Task.FromResult(new List<ICookieData>());

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<CookieData>>();
    }

    public async Task UpdateCookiesForUrl(string url)
    {
        var response = await _httpClient.PostAsJsonAsync($"browserdata/launch", new ArrayList() { _browserDataLauncher, url });
        response.EnsureSuccessStatusCode();       
    }
}
