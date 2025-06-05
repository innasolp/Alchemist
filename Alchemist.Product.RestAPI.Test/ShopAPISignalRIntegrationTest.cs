using Alchemist.Product.Data;
using Alchemist.Product.RestAPI.Test.Infrastructure;
using Alchemist.Product.SignalR;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace Alchemist.Product.RestAPI.Test;

public class ShopAPISignalRIntegrationTest : TestFixture<SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext>, Startup>
{
    record TestLogMessage(LogLevel logLevel, string categoryName, EventId eventId, string message, Exception? exception);

    private readonly List<TestLogMessage> _messages = [];

    private readonly ShopAPISignalRWebAppFactory _shopAPIFactory;

    private readonly HttpClient _shopAPIHttpClient;

    public ShopAPISignalRIntegrationTest(SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext> webAppFactory, ITestOutputHelper outputHelper)
        : base(webAppFactory, outputHelper)
    {
        WebAppFactory.FixtureLoggingContext.LoggedMessage += Log;

        _shopAPIFactory = new ShopAPISignalRWebAppFactory(WebAppFactory.Server)
        {
            DataBase = "test_ci_db_signalr"
        };
        _shopAPIHttpClient = _shopAPIFactory.CreateClient();        
    }

    private void Log(LogLevel logLevel, string categoryName, EventId eventId, string message, Exception? exception)
    {
        _messages.Add(new TestLogMessage(logLevel, categoryName, eventId, message, exception));
    }
    
    [Fact]
    public async Task LogInfoSendMessageOnCreateShopAsync()
    {
        _messages.Clear();

        var shop = new Shop() { Name = "TestShopNew", Url = "https://testshopnew" };
        var response = await _shopAPIHttpClient.PutAsJsonAsync($"api/Shop", shop);
        Assert.True(response.IsSuccessStatusCode);

        Assert.Equal(2, _messages.Count(m => m.categoryName.Contains(typeof(LogHubFilter).Name) && m.logLevel == LogLevel.Information));
        Assert.Empty(_messages.Where(m => m.categoryName.Contains(typeof(LogHubFilter).Name) && m.logLevel == LogLevel.Error));
    }

    [Fact]
    public async Task LogErrorSendMessageOnCreateInvalidShopAsync()
    {
        _messages.Clear();

        var shop = new Shop() { Name = "TestShopNew", Url = "https://testshopnew", Id = 1 };
        var response = await _shopAPIHttpClient.PutAsJsonAsync($"api/Shop", shop);
        Assert.False(response.IsSuccessStatusCode);

        Assert.Empty(_messages.Where(m => m.categoryName.Contains(typeof(LogHubFilter).Name) && m.logLevel == LogLevel.Information));        
    }
}
