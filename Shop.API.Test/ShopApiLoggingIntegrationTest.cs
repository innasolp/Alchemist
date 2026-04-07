using Alchemist.Test.Log;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shop.API.Test.Infrastructure;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace Shop.API.Test;

public class ShopAPIConfigurationLoggingWebAppFactory : ShopApiConfigurationWebAppFactory
{
    public ShopAPIConfigurationLoggingWebAppFactory() : base("ConnectionStrings:DbContext2", "test_ci_db_logging", SignalRCommon.ConfigureSignalRMock)
    {
    }

    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        FixtureLoggingContext.ConfigureServices(services);
    }
}

public class ShopApiLoggingIntegrationTest: ShopAPIConfigurationTestFixture<ShopAPIConfigurationLoggingWebAppFactory>
{
    record TestLogMessage(LogLevel logLevel, string categoryName, EventId eventId, string message, Exception? exception);

    private readonly List<TestLogMessage> _messages = [];

    private readonly HttpClient _httpClient;

    private const string HttpLogCategory = "Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware";

    private const string HttpExceptionHandler = "GlobalExceptionHandler";

    private const string ResponseBodyEvent = "ResponseBody";

    public ShopApiLoggingIntegrationTest(ShopAPIConfigurationLoggingWebAppFactory webAppFactory, ITestOutputHelper outputHelper)
        : base(webAppFactory, outputHelper)
    {
        //WebAppFactory.DataBase = "test_ci_db_logging";
        WebAppFactory.FixtureLoggingContext.LoggedMessage += Log;
        _httpClient = WebAppFactory.CreateClient();
    }

    private void Log(LogLevel logLevel, string categoryName, EventId eventId, string message, Exception? exception)
    {
        _messages.Add(new TestLogMessage(logLevel, categoryName, eventId, message, exception));
    }

    [Fact]
    public async Task LogOnGetShopSuccessAsync()
    {
        _messages.Clear();

        var name = "TestShop";
        var response = await _httpClient.GetAsync($"api/Shop/byName?name={name}");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        Assert.Equal(1, _messages.Count(m => m.eventId.Name == ResponseBodyEvent && m.categoryName.Contains(HttpLogCategory) && m.logLevel == LogLevel.Information));
    }

    [Fact]
    public async Task LogOnCreateShopResponseInternalErrorAsync()
    {
        _messages.Clear();

        var shop = new Alchemist.Product.Data.Shop() { Name = "TestShopNew", Url = "https://testshopnew", Id = 1 };
        var response = await _httpClient.PutAsJsonAsync($"api/Shop", shop);

        Assert.Equal(1, _messages.Count(m =>m.categoryName.Contains(HttpExceptionHandler) && m.logLevel == LogLevel.Error));
    }
}
