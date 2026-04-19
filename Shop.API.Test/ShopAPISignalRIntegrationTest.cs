using Alchemist.Product.SignalR;
using Alchemist.Test.Log;
using Alchemist.Test.Server.Fixtures;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shop.API.Test.Infrastructure;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace Shop.API.Test;

public class SignalRLogConfigurationWebAppFactory : ShopApiConfigurationWebAppFactory, ILoggedContext
{
    private readonly SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext> _signalRFactory = new();

    public SignalRLogConfigurationWebAppFactory() : base("ConnectionStrings:DbContext2", "test_ci_db_signalr", httpPort:8052, httpsPort : 8053)
    {
        _signalRFactory.CreateClient();
    }

    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();

    FixtureLogContext ILoggedContext.FixtureLoggingContext => FixtureLoggingContext;

    private bool _signalRInjected = false;

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        base.ConfigureWebHostBuilderContext(context, services);

        //todo
        if (!_signalRInjected)
        {
            services.SetSignalRHubTestAckSender(_signalRFactory.Server, ["events"]);
            _signalRInjected = true;
        }
    }

    internal void SubscribeSignalRLogMessage(LogMessage logMessage)
    {
        _signalRFactory.FixtureLoggingContext.LoggedMessage += logMessage;
    }

    internal void UnsubscribeSignalRLogMessage(LogMessage logMessage)
    {
        _signalRFactory.FixtureLoggingContext.LoggedMessage -= logMessage;
    }
}

public class ShopAPISignalRIntegrationTest : LoggedContextTestFixture<SignalRLogConfigurationWebAppFactory, ShopAPIProgram> //, IAsyncLifetime
{
    public ShopAPISignalRIntegrationTest(SignalRLogConfigurationWebAppFactory webAppFactory, ITestOutputHelper outputHelper) 
        : base(webAppFactory, outputHelper) //, IAsyncLifetime
    {
        WebAppFactory.SubscribeSignalRLogMessage(Log);
    }

    [Fact]
    public async Task LogInfoSendMessageOnCreateShopAsync()
    {        
        var shop = new Alchemist.Product.Data.Shop() { Name = "TestShopNew", Url = "https://testshopnew" };

        try
        {
            var shopAPIHttpClient = WebAppFactory.GetHostHttpClient();
            var response = await shopAPIHttpClient.PutAsJsonAsync($"api/Shop", shop);
            Assert.True(response.IsSuccessStatusCode);

            await Task.Delay(2000);

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

    public override void Dispose()
    {
        WebAppFactory.UnsubscribeSignalRLogMessage(Log);

        base.Dispose();
    }
}