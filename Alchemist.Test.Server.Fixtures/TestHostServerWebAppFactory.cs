using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Alchemist.Test.Server.Fixtures;

public abstract class TestHostServerWebAppFactory<TEntryPoint> : TestWebAppFactory<TEntryPoint>, IWebHostConfigure
    where TEntryPoint : class
{
    protected IHost? _host;

    public string ServerAddress
    {
        get
        {
            EnsureServer();
            return ClientOptions.BaseAddress.ToString();
        }
    }


    private Action<IHost>? _configureHost;

    event Action<IHost> IWebHostConfigure.ConfigureHost
    {
        add
        {
            _configureHost += value;
        }
        remove
        {
            _configureHost -= value;
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

        ConfigureHost(_host);

        return testHost;
    }

    protected virtual void ConfigureHostAdresses(IWebHostBuilder builder)
    {
        builder.UseKestrel();
    }

    protected void ConfigureHost(IHost host)
    {
        _configureHost?.Invoke(host);
    }

    public virtual HttpClient GetHostHttpClient()
    {
        var httpClient = CreateClient();
        httpClient.BaseAddress = new Uri(ServerAddress);
        return httpClient;
    }
}