using Alchemist.Test.Log;
using Alchemist.Test.SignalRWebAppFactory;
using Microsoft.AspNetCore.TestHost;


namespace Alchemist.Product.WebApp.IntegrationTest.Infrastructure;

internal static class Common
{
    private static TestServer? _signalRTestServer;

    public static TestServer SignalRTestServer
    {
        get
        {
            _signalRTestServer ??= new SignalRLogContextWebAppFactory<FixtureLoggerFactoryContext>().Server;

            return _signalRTestServer;
        }
    }
}
