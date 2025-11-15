using Alchemist.Test.Log;
using Alchemist.Test.SettingsAPIFactory;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.AspNetCore.TestHost;

namespace Alchemist.Test.ImportSettingsWebApp.Factory;

public class ImportsettingsWebAppFullFactory(bool isApi, string? shopApiHost, int httpPort, int httpsPort,
    string settingsApiConnectionDbSection, int settingsApiHttpPort, int settingsApiHttpsPort,
   TestServer signalRTestServer
        ) : ImportSettingsWebAppFactory(isApi, shopApiHost, httpPort, httpsPort,
        SettingsApiHelper.CreateSettingsApiHttpClient(settingsApiConnectionDbSection, settingsApiHttpPort, settingsApiHttpsPort, signalRTestServer))
{ 
    public ImportsettingsWebAppFullFactory(bool isApi, string? shopApiHost, int httpPort, int httpsPort,       
       string settingsApiConnectionDbSection, int settingsApiHttpPort, int settingsApiHttpsPort)
        : this(isApi, shopApiHost, httpPort, httpsPort, 
              settingsApiConnectionDbSection, settingsApiHttpPort, settingsApiHttpsPort,
              new SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext>().Server)
    {
    }
}
