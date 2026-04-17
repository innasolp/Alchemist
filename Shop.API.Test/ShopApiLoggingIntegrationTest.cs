using Alchemist.Test.Log;
using Alchemist.Test.Server.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shop.API.Test.Infrastructure;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace Shop.API.Test;

public class ShopAPIConfigurationLoggingWebAppFactory : ShopApiConfigurationWebAppFactory, ILoggedContext
{
    public ShopAPIConfigurationLoggingWebAppFactory() 
        : base("ConnectionStrings:DbContext2", "test_ci_db_logging", SignalRCommon.ConfigureSignalRMock, httpPort:8048, httpsPort: 8049)
    {
    }

    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();

    FixtureLogContext ILoggedContext.FixtureLoggingContext => FixtureLoggingContext;

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        base.ConfigureWebHostBuilderContext (context, services);
        FixtureLoggingContext.ConfigureServices(services);
    }
}

public class ShopApiLoggingIntegrationTest: LoggedContextTestFixture<ShopAPIConfigurationLoggingWebAppFactory, ShopAPIProgram>
{
    record TestLogMessage(LogLevel logLevel, string categoryName, EventId eventId, string message, Exception? exception);

    private const string HttpLogCategory = "Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware";

    private const string HttpExceptionHandler = "GlobalExceptionHandler";

    private const string ResponseBodyEvent = "ResponseBody";

    public ShopApiLoggingIntegrationTest(ShopAPIConfigurationLoggingWebAppFactory webAppFactory, ITestOutputHelper outputHelper)
        : base(webAppFactory, outputHelper)
    {
        WebAppFactory.FixtureLoggingContext.LoggedMessage += Log;
    }

    [Fact]
    public async Task LogOnGetShopSuccessAsync()
    {
        var name = "TestShop";

        try
        {
            var httpClient = WebAppFactory.GetHostHttpClient(); 
            var response = await httpClient.GetAsync($"api/Shop/byName?name={name}");
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

            Assert.Equal(1, LogMessages.Count(m => m.EventId.Name == ResponseBodyEvent 
            && m.CategoryName.Contains(HttpLogCategory) 
            && m.LogLevel == LogLevel.Information));
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
        finally
        {
            // await WebAppFactory.ResetDatabaseIfAvailableAsync();
        }
    }

    [Fact]
    public async Task LogOnCreateShopResponseInternalErrorAsync()
    {
        var shop = new Alchemist.Product.Data.Shop() { Name = "TestShopNew", Url = "https://testshopnew", Id = 1 };

        try
        {
            var httpClient = WebAppFactory.GetHostHttpClient(); 
            var response = await httpClient.PutAsJsonAsync($"api/Shop", shop);

            Assert.False(response.IsSuccessStatusCode);

            Assert.Equal(1, LogMessages.Count(m => m.CategoryName.Contains(HttpExceptionHandler) && m.LogLevel == LogLevel.Error));
        }
        catch
        {
            OutputErrors();
            OutputWarnings();

            throw;
        }
        finally
        {
            // await WebAppFactory.ResetDatabaseIfAvailableAsync();
        }
    }
}
