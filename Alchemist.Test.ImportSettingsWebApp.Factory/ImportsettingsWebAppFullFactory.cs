using Alchemist.Test.Log;
using Alchemist.Test.SettingsAPIFactory;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;

namespace Alchemist.Test.ImportSettingsWebApp.Factory;

public class ImportsettingsWebAppFullFactory(bool isApi, string? shopApiHost, int httpPort, int httpsPort,
    string settingsApiConnectionDbSection, int settingsApiHttpPort, int settingsApiHttpsPort,
   TestServer signalRTestServer
        ) : ImportSettingsWebAppFactory(isApi, shopApiHost, httpPort, httpsPort,
        GetSettingsApiHttpClient(settingsApiConnectionDbSection, settingsApiHttpPort, settingsApiHttpsPort, signalRTestServer))
{ 
    private static HttpClient GetSettingsApiHttpClient(string connectionSection, int settingsAPIHttpPort, int settingsAPIHttpsPort, TestServer signalRTestServer)
    {
        var settings = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json")
              .Build();

        var alchemyDbConnectionString = settings.GetConnectionString(connectionSection);
        var settingsAPIWebAppFactory = new SettingsAPIWebAppFactory(alchemyDbConnectionString, signalRTestServer, settingsAPIHttpPort, settingsAPIHttpsPort, false);
        return settingsAPIWebAppFactory.CreateClient();
    }

    public ImportsettingsWebAppFullFactory(bool isApi, string? shopApiHost, int httpPort, int httpsPort,       
       string settingsApiConnectionDbSection, int settingsApiHttpPort, int settingsApiHttpsPort)
        : this(isApi, shopApiHost, httpPort, httpsPort, 
              settingsApiConnectionDbSection, settingsApiHttpPort, settingsApiHttpsPort,
              new SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext>().Server)
    {
    }
}
