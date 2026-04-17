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

public class SignalRLogConfigurationWebAppFactory()
    : ShopApiConfigurationWebAppFactory("ConnectionStrings:DbContext2", "test_ci_db_signalr", httpPort:8052, httpsPort : 8053), ILoggedContext
{
    private readonly TestServer _signalRServer = SignalRCommon.SignalRTestServer;

    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();

    FixtureLogContext ILoggedContext.FixtureLoggingContext => FixtureLoggingContext;

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        base.ConfigureWebHostBuilderContext(context, services);

        FixtureLoggingContext.ConfigureServices(services);
        services.SetSignalRHubTestSender(_signalRServer, ["events"]);
    }
}

public class ShopAPISignalRIntegrationTest(SignalRLogConfigurationWebAppFactory webAppFactory, ITestOutputHelper outputHelper)
    : LoggedContextTestFixture<SignalRLogConfigurationWebAppFactory, ShopAPIProgram>(webAppFactory, outputHelper) //, IAsyncLifetime
{
    record TestLogMessage(LogLevel logLevel, string categoryName, EventId eventId, string message, Exception? exception);
        

    [Fact]
    public async Task LogInfoSendMessageOnCreateShopAsync()
    {        
        var shop = new Alchemist.Product.Data.Shop() { Name = "TestShopNew", Url = "https://testshopnew" };

        try
        {
            var shopAPIHttpClient = WebAppFactory.GetHostHttpClient();
            var response = await shopAPIHttpClient.PutAsJsonAsync($"api/Shop", shop);
            Assert.True(response.IsSuccessStatusCode);

            await Task.Delay(5000);

            Assert.Equal(2, LogMessages.Count(m => m.CategoryName.Contains(typeof(LogHubFilter).Name) && m.LogLevel == LogLevel.Information));
            Assert.Empty(LogMessages.Where(m => m.CategoryName.Contains(typeof(LogHubFilter).Name) && m.LogLevel == LogLevel.Error));
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
        finally
        {
            await WebAppFactory.ResetDatabaseIfAvailableAsync();
        }
            
    }

    [Fact]
    public async Task LogErrorSendMessageOnCreateInvalidShopAsync()
    {
        var shop = new Alchemist.Product.Data.Shop() { Name = "TestShopNew", Url = "https://testshopnew", Id = 1 };

        try
        {
           var shopAPIHttpClient = WebAppFactory.GetHostHttpClient();
            var response = await shopAPIHttpClient.PutAsJsonAsync($"api/Shop", shop);

            Assert.False(response.IsSuccessStatusCode);
            Assert.Empty(LogMessages.Where(m => m.CategoryName.Contains(typeof(LogHubFilter).Name) && m.LogLevel == LogLevel.Information));
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
        finally
        {
            await WebAppFactory.ResetDatabaseIfAvailableAsync();
        }
    }
}