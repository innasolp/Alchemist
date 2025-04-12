using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;


namespace Alchemist.Product.Import.WebApp.Test;

public class ImportWebAppTest : PageTest 
{
    private readonly ProductAPIWebAppFactory _productAPIWebAppFactory;
    private readonly SettingsAPIWebAppFactory _settingsAPIWebAppFactory;
    private readonly SignalRApplicationFactory _signalRApplicationFactory;
    private readonly TestWebAppFactory<ImportWebAppProgram> _webAppFactory;

    private const string dbConnectionString = "Host=localhost;Database=test_ci_db;Username=postgres;Password=P@ssw0rd;";

    public ImportWebAppTest()
    {
        _productAPIWebAppFactory = new ProductAPIWebAppFactory("https://localhost:8051", dbConnectionString, false);

        _settingsAPIWebAppFactory = new SettingsAPIWebAppFactory(dbConnectionString, false);
        _settingsAPIWebAppFactory.WithWebHostBuilder(webHostBuilder =>
        {
            webHostBuilder.UseUrls("https://localhost:8201", "http://localhost:8200");
            webHostBuilder.PreferHostingUrls(true);
        });

        _signalRApplicationFactory = new SignalRApplicationFactory();
        _signalRApplicationFactory.WithWebHostBuilder(webHostBuilder =>
        {
            webHostBuilder.UseUrls("http://localhost:8100", "https://localhost:8101");
            webHostBuilder.PreferHostingUrls(true);            
        });

        _webAppFactory = new TestWebAppFactory<ImportWebAppProgram>();
    }

    private async Task<IBrowserContext> CreateBrowserContextAsync()
    {
        var Browser = await Playwright.Chromium.LaunchAsync(new()
        {
            Headless = false,
        });

        return await Browser.NewContextAsync(new BrowserNewContextOptions { JavaScriptEnabled = true });
    }
    
    [Fact]
    public async Task IndexPageContainsMenuDivAndShopList()
    {
        var browserContext = await CreateBrowserContextAsync();
        var page = await browserContext.NewPageAsync();

        var response = await page.GotoAsync(_webAppFactory.ServerAddress);
        Assert.True(response?.Ok);   

        var menudiv = page.Locator("#menuDiv");
        Assert.Equal(1, await menudiv.CountAsync());
        
        var shopListPartialDiv = page.Locator("#shopListPartialDiv");
        Assert.Equal(1, await shopListPartialDiv.CountAsync());
    }
}