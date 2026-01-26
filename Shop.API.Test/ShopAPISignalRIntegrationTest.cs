using Alchemist.Product.SignalR;
using Alchemist.Test.Log;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.Extensions.Logging;
using Shop.API.Test.Infrastructure;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace Shop.API.Test;

public class ShopAPISignalRIntegrationTest : TestFixture<SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext>, Startup>, IAsyncLifetime
{
    record TestLogMessage(LogLevel logLevel, string categoryName, EventId eventId, string message, Exception? exception);

    private readonly List<TestLogMessage> _messages = [];

    private readonly ShopAPISignalRWebAppFactory _shopAPIFactory;

    private HttpClient _shopAPIHttpClient;

    public ShopAPISignalRIntegrationTest(SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext> webAppFactory, ITestOutputHelper outputHelper)
        : base(webAppFactory, outputHelper)
    {
        WebAppFactory.FixtureLoggingContext.LoggedMessage += Log;

        _shopAPIFactory = new ShopAPISignalRWebAppFactory(WebAppFactory.Server)
        {
            DataBase = "test_ci_db_signalr"
        };        
    }

    private void Log(LogLevel logLevel, string categoryName, EventId eventId, string message, Exception? exception)
    {
        _messages.Add(new TestLogMessage(logLevel, categoryName, eventId, message, exception));
    }
    
    [Fact]
    public async Task LogInfoSendMessageOnCreateShopAsync()
    {
        _shopAPIHttpClient = _shopAPIFactory.CreateClient();

        _messages.Clear();

        var shop = new Alchemist.Product.Data.Shop() { Name = "TestShopNew", Url = "https://testshopnew" };
        var response = await _shopAPIHttpClient.PutAsJsonAsync($"api/Shop", shop);
        Assert.True(response.IsSuccessStatusCode);

        await Task.Delay(2000);

        Assert.Equal(2, _messages.Count(m => m.categoryName.Contains(typeof(LogHubFilter).Name) && m.logLevel == LogLevel.Information));
        Assert.Empty(_messages.Where(m => m.categoryName.Contains(typeof(LogHubFilter).Name) && m.logLevel == LogLevel.Error));
    }

    [Fact]
    public async Task LogErrorSendMessageOnCreateInvalidShopAsync()
    {
        _shopAPIHttpClient = _shopAPIFactory.CreateClient();

        _messages.Clear();

        var shop = new Alchemist.Product.Data.Shop() { Name = "TestShopNew", Url = "https://testshopnew", Id = 1 };
        var response = await _shopAPIHttpClient.PutAsJsonAsync($"api/Shop", shop);
        Assert.False(response.IsSuccessStatusCode);

        Assert.Empty(_messages.Where(m => m.categoryName.Contains(typeof(LogHubFilter).Name) && m.logLevel == LogLevel.Information));        
    }

    public Task InitializeAsync()
    {
        return _shopAPIFactory.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _shopAPIFactory.DisposeAsync();
    }
}
