using Alchemist.Product.SignalR;
using Alchemist.Test.Log;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shop.API.Test.Infrastructure;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace Shop.API.Test;

public class SignalRLogConfigurationWebAppFactory(TestServer signalRServer, int httpPort, int httpsPort)
    : ShopApiConfigurationWebAppFactory("ConnectionStrings:DbContext2", "test_ci_db_signalr", httpPort:httpPort, httpsPort : httpsPort)
{
    private readonly TestServer _signalRServer = signalRServer;

    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        base.ConfigureWebHostBuilderContext(context, services);

        FixtureLoggingContext.ConfigureServices(services);
        services.SetSignalRHubTestSender(_signalRServer, ["events"]);
    }
}

public class ShopAPISignalRIntegrationTest : TestFixture<SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext>, Startup> //, IAsyncLifetime
{
    record TestLogMessage(LogLevel logLevel, string categoryName, EventId eventId, string message, Exception? exception);

    private readonly List<TestLogMessage> _messages = [];

    public ShopAPISignalRIntegrationTest(SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext> webAppFactory, ITestOutputHelper outputHelper)
        : base(webAppFactory, outputHelper)
    {
        WebAppFactory.FixtureLoggingContext.LoggedMessage += Log;
    }

    private void Log(LogLevel logLevel, string categoryName, EventId eventId, string message, Exception? exception)
    {
        _messages.Add(new TestLogMessage(logLevel, categoryName, eventId, message, exception));
    }
    
    [Fact]
    public async Task LogInfoSendMessageOnCreateShopAsync()
    {        
        _messages.Clear();

        var shop = new Alchemist.Product.Data.Shop() { Name = "TestShopNew", Url = "https://testshopnew" };

        var shopAPIFactory = new SignalRLogConfigurationWebAppFactory(WebAppFactory.Server, 8052, 8053);

        try
        {
            await shopAPIFactory.InitializeAsync();
            var shopAPIHttpClient = shopAPIFactory.GetHostHttpClient();
            var response = await shopAPIHttpClient.PutAsJsonAsync($"api/Shop", shop);
            Assert.True(response.IsSuccessStatusCode);

            await Task.Delay(5000);

            Assert.Equal(2, _messages.Count(m => m.categoryName.Contains(typeof(LogHubFilter).Name) && m.logLevel == LogLevel.Information));
            Assert.Empty(_messages.Where(m => m.categoryName.Contains(typeof(LogHubFilter).Name) && m.logLevel == LogLevel.Error));
        }
        finally
        {
            await shopAPIFactory.ResetDatabaseAsync();
            await (shopAPIFactory as IAsyncLifetime).DisposeAsync();
        }
    }

    [Fact]
    public async Task LogErrorSendMessageOnCreateInvalidShopAsync()
    {
        _messages.Clear();

        var shop = new Alchemist.Product.Data.Shop() { Name = "TestShopNew", Url = "https://testshopnew", Id = 1 };

        var shopAPIFactory = new SignalRLogConfigurationWebAppFactory(WebAppFactory.Server, 8054, 8055);

        try
        {
            await shopAPIFactory.InitializeAsync();

            var shopAPIHttpClient = shopAPIFactory.GetHostHttpClient();
            var response = await shopAPIHttpClient.PutAsJsonAsync($"api/Shop", shop);

            Assert.False(response.IsSuccessStatusCode);
            Assert.Empty(_messages.Where(m => m.categoryName.Contains(typeof(LogHubFilter).Name) && m.logLevel == LogLevel.Information));
        }
        finally
        {
            await shopAPIFactory.ResetDatabaseAsync();
            await (shopAPIFactory as IAsyncLifetime).DisposeAsync();
        }
    }
}