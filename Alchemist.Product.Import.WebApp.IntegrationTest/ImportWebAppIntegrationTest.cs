using Alchemist.Product.Import.WebApp.IntegrationTest.Infrastructure;
using Alchemist.Product.Import.WebApp.Models;
using Alchemist.Test.Server.Fixtures;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace Alchemist.Product.Import.WebApp.IntegrationTest;

public class ImportWebAppIntegrationTest : TestFixture<TestImportWebAppFactory, ImportWebAppProgram>
{
    private readonly HttpClient _importWebAppClient;    

    public ImportWebAppIntegrationTest(TestImportWebAppFactory webAppFactory, ITestOutputHelper outputHelper)
        : base(webAppFactory, outputHelper)
    {
        _importWebAppClient = WebAppFactory.CreateClient();
        _importWebAppClient.BaseAddress = new Uri(WebAppFactory.ServerAddress);
    }

    [Fact]
    public async Task LoadIndexPage()
    {
        OutputHelper.WriteLine(WebAppFactory.ServerAddress);
        var response = await _importWebAppClient.GetAsync(WebAppFactory.ServerAddress);
        Assert.True(response.IsSuccessStatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrEmpty(content));
        OutputHelper.WriteLine(content);
    }

    [Fact]
    public async Task UploadShops()
    {
        var response = await _importWebAppClient.PostAsync($"{WebAppFactory.ServerAddress}Home/UpdateShops", null);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var shops = await response.Content.ReadFromJsonAsync<List<ShopModel>>();
        Assert.Equal(4, shops.Count);
    }
}