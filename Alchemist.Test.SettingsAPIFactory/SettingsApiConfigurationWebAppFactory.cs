using Alchemist.Product.Data;
using Alchemist.Test.DBApiWebAppFactory.Configuration;
using Alchemist.Test.Log;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Test.DbContainer.Abstractions;
using Alchemist.Test.SignalRWebAppFactory;

namespace Alchemist.Test.SettingsAPIFactory;

public class SettingsApiConfigurationWebAppFactory<TTestDbContainer>
    (string connectionStringSection, string database, int dbPort, string user, string password, int httpPort, int httpsPort, TestServer signalRServer)
    : DbApiAPIKestrelConfigurationContainerWebAppFactory<SettingsAPIProgram, AlchemyContext, TTestDbContainer>
    (connectionStringSection, database, dbPort, user, password, httpPort, httpsPort)
    where TTestDbContainer : ITestDbContainer, new()
{
    private readonly TestServer _signalRServer = signalRServer;

    public FixtureLoggerFactoryContext FixtureLoggingContext { get; } = new FixtureLoggerFactoryContext();

    protected override void FillTestData(AlchemyContext dbContext)
    {}

    protected override void ConfigureWebHostBuilderContext(WebHostBuilderContext context, IServiceCollection services)
    {
        base.ConfigureWebHostBuilderContext(context, services);

        services.SetSignalRHubTestSender(_signalRServer, ["events"]);

        FixtureLoggingContext.ConfigureServices(services);
    }
}