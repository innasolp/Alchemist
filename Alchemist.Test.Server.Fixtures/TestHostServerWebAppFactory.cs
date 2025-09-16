using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;

namespace Alchemist.Test.Server.Fixtures;

public abstract class TestHostServerWebAppFactory<TEntryPoint> : TestWebAppFactory<TEntryPoint>
    where TEntryPoint : class
{

    private IHost _host;

    protected IHost Host => _host;

    public string ServerAddress
    {
        get
        {
            EnsureServer();
            return ClientOptions.BaseAddress.ToString();
        }
    }

    protected void EnsureServer()
    {
        if (_host is null)
        {
            // This forces WebApplicationFactory to bootstrap the server  
            using var _ = CreateDefaultClient();
        }
    }
    
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var testHost = builder.CreateTestHostUseAddressConfiguration(ConfigureHostAdresses, out _host);

        ClientOptions.BaseAddress = _host.GetBaseAddress();        

        return testHost;
    }

    protected abstract void ConfigureHostAdresses(IWebHostBuilder builder);    
}
