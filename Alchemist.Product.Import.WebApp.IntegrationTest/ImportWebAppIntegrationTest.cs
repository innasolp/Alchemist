using Alchemist.Product.Import.Model;
using Alchemist.Product.SignalR;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SignalRWebAppFactory;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace Alchemist.Product.Import.WebApp.IntegrationTest;

public class ImportWebAppIntegrationTest:TestFixture<SignalRApplicationFactory, Startup>
{
    private readonly TestImportWebAppFactory _testImportWebAppFactory;

    private readonly ShopAPIWebAppFactory _shopAPIWebAppFactory;

    private readonly SettingsAPIWebAppFactory _settingsAPIWebAppFactory;

    private readonly HttpClient _importWebAppClient;

    public ImportWebAppIntegrationTest(SignalRApplicationFactory webAppFactory, ITestOutputHelper outputHelper) 
        : base(webAppFactory, outputHelper)
    {
        _shopAPIWebAppFactory = new ShopAPIWebAppFactory(WebAppFactory.Server);
        _shopAPIWebAppFactory.CreateClient();

        _settingsAPIWebAppFactory = new SettingsAPIWebAppFactory();
        _settingsAPIWebAppFactory.CreateClient();

        _testImportWebAppFactory = new TestImportWebAppFactory();
        _importWebAppClient = _testImportWebAppFactory.CreateClient();
        _importWebAppClient.BaseAddress = new Uri (_testImportWebAppFactory.ServerAddress);
    }

    [Fact]
    public async Task LoadIndexPage()
    {
        OutputHelper.WriteLine(_testImportWebAppFactory.ServerAddress);
        var response = await _importWebAppClient.GetAsync(_testImportWebAppFactory.ServerAddress);
        Assert.True(response.IsSuccessStatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrEmpty(content));
        OutputHelper.WriteLine(content);
    }

    [Fact]
    public async Task UploadShops()
    {
        var response = await _importWebAppClient.PostAsync($"{_testImportWebAppFactory.ServerAddress}Home/LoadTab", null);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var shops = await response.Content.ReadFromJsonAsync<List<ShopModel>>();
        Assert.Equal(4, shops.Count);
    }
}