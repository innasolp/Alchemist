using Alchemist.Test.Server.Fixtures;
using Microsoft.VisualStudio.Threading;
using System.Net;
using Xunit.Abstractions;

namespace Alchemist.Product.Import.Backgorund.Service.IntegrationTest;

public class ImportBackgroundServiceTest : TestFixture<ImportBackgroundServiceWebAppFactory, ImportBackgroundServiceProgram>
{
    private readonly HttpClient _httpClient;

    private readonly AsyncAutoResetEvent _asyncAutoResetEvent = new AsyncAutoResetEvent();

    public ImportBackgroundServiceTest(ImportBackgroundServiceWebAppFactory webAppFactory, ITestOutputHelper outputHelper) : base(webAppFactory, outputHelper)
    {
        _httpClient = WebAppFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(WebAppFactory.ServerAddress);
    }

    [Fact]
    public async Task HelloResponseWhenStartingSuccess()
    {
        var response = await _httpClient.GetAsync($"{WebAppFactory.ServerAddress}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var hello = await response.Content.ReadAsStringAsync();
        Assert.Equal("Hello ImportBackgroundService!", hello);
    }

    [Fact]
    public async Task WaitForImportMessage()
    {        
        await WebAppFactory.SubscribeToEventAsync("product", OnHandleProductMessageAsync);
        await WebAppFactory.SubscribeToEventAsync("category", OnHandleCategoryMessageAsync);

        var response = await _httpClient.GetAsync($"{WebAppFactory.ServerAddress}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var token = new CancellationToken();
        var task = _asyncAutoResetEvent.WaitAsync(token);
        await task.WaitAsync(TimeSpan.FromMilliseconds(30000), token);

        OutputHelper.WriteLine("Event set");
    }

    private async Task OnHandleCategoryMessageAsync(object t)
    {
        _asyncAutoResetEvent.Set();
    }

    private async Task OnHandleProductMessageAsync(object t)
    {
        _asyncAutoResetEvent.Set();
    }
}